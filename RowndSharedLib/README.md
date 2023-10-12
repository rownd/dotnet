# Rownd bindings for .NET Core

Use this library to integrate Rownd into your .NET Core web application.

Convenience wrappers are provided for the .NET Core Identity framework, but
you can also leverage token validation and user profile APIs.

## Installation

From NuGet:
```bash
dotnet add package Rownd
```

## Supported versions

- .NET 6.x
- _Need a different version? [Let us know!](https://github.com/rownd/dotnet/issues/new?title=Request%20support%20for%20.NET%20X.X)_

## Usage

### Prepare configuration values

The Rownd client requires an application key (which is publishable) and an application secret (which should be kept private).
If you don't have these values, you can obtain them at [https://app.rownd.io](https://app.rownd.io).

Once you have them, you can add them to your `appsettings.json`:
```json
{
    ...
    "Rownd": {
        "AppKey": "...",
        "AppSecret": "..."
    }
}
```

Or you can set environment variables and the library will use them automatically (recommended):

```bash
export ROWND_APP_KEY="..."
export ROWND_APP_SECRET="..."
```

### Inject Rownd into your application
To get set up quickly, we'll assume you added the app key and secret to your `appsettings.json` file as shown above.

Next, add the following to your `Program.cs` file before the `builder.build()` statement:

```csharp
using Rownd;

...

builder.Services.AddSingleton<Rownd.Models.Config>(sp => {
    return new Rownd.Models.Config(builder.Configuration["Rownd:AppKey"], builder.Configuration["Rownd:AppSecret"]);
});
builder.Services.AddSingleton<RowndClient>();
```

At this point, your server should accept Rownd JWTs and validate them. If you're building a Single Page Application (SPA),
you'll want to leverage our framework-specific browser SDKs for ease of implementation.

If you're building a more traditional web application, keep reading...

### Use Rownd with .NET Core Identity cookie-based sessions

If you're adding Rownd to an existing application or building a new one that uses the default, cookie-based session handling
that comes with .NET Core Identity, you'll need to add an additional controller to your app that will accept a Rownd JWT and
set a session cookie in response.

Add a new controller that looks like this:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rownd;
using Rownd.Helpers;

namespace MyAppNamespace.Controllers
{
    [Route("/api/auth/rownd")]
    public class RowndAuthController : RowndCookieExchange
    {
        // OPTIONAL
        protected override async Task IsAllowedToSignIn(RowndUser rowndUser, Dictionary<string, dynamic> signInContext) {
            // Run any custom logic here to ensure this user should be allowed to sign in.

            // Use `signInContext` to store data you may want to use in a later phase (e.g. OnSignInSuccess())

            // return; // if everything is fine

            // throw new Exception("You aren't allowed here!"); // if you want to prevent the user from signing in
        }

        protected virtual async Task OnSuccessfulSignIn(RowndUser? rowndUser, IdentityUser? user, Dictionary<string, dynamic> signInContext) {
            // Run any custom logic here after a successful sign in.

            // Use `signInContext` to access data you stored during IsAllowedToSignIn()

            // IdentityUser `user` may be null if the user was not found in the database or if a `UserManager` instance was not provided.
        }

        public RowndAuthController(RowndClient client, ILogger<RowndAuthController> logger, UserManager<IdentityUser> userManager) : base(client, logger)
        {
            _userManager = userManager; // If provided, Rownd will attempt to match the incoming user with an existing user in the database.
            _addNewUsersToDatabase = true;  // If you want Rownd to add new users to the database when they're first authenticated, set this to `true` (requires `_userManager`)
            
            _defaultAuthenticationScheme = IdentityConstants.ApplicationScheme;  // Sets the authentication scheme (default: `IdentityConstants.ApplicationScheme`)
            _signOutRedirectUrl = "/";  // Where to redirect the user after signing out (default: "/")
            
        }
    }
}
```

Let's examine what's happening in the above code:

1. We're using the `RowndCookieExchange` base class to handle the exchange of Rownd JWTs for a session cookie. It will accept a Rownd JWT in the POST body,
   call the `HttpContext.SignInAsync()` method with the user's email address and/or phone number and a role (if present).

2. We're attaching a route to the controller (the base class is an abstract `ApiController`) that we'll use later to handle the exchange of Rownd JWTs for a session cookie.
   You can specify any route you like here, but `/api/auth/rownd` is a decent choice.

3. Using .NET dependency injection (DI), the server injects references to the RowndClient and an ILogger (which are required). If you want Rownd to add users
   to your database, then you'll also need to accept a reference to a `UserManager` instance.

4. `_addNewUsersToDatabase` is a base class instance variable and is set to `false` by default.
   If you want Rownd to add users to your database, you'll need to set this to `true`.
   Likewise, `_userManager` is a base class instance variable and is set to `null` by default. Be sure to populate this with the UserManager injected dependency
   if `_addNewUsersToDatabase` is `true`.

5. Optionally, we can override the async, virtual method `IsAllowedToSignIn()` to run custom logic identifying whether the current user
   should be able to establish an authenticated session. This might mean checking a prerequisite in another system or simply checking an
   attribute on the user's Rownd profile. If this method throws an exception, the sign-in process will stop before the session is established
    and a 403 Forbidden response will be returned. The body will contain a `message` property with the exception message.

Finally, we need to install the Rownd Hub and instruct it to call our controller API when the page loads.

1. Follow [these instructions](https://docs.rownd.io/rownd/sdk-reference/web/javascript-browser) to install the Rownd Hub. You'll want to ensure it runs
   on every page of your application, so be sure to add it as a common script or drop it directly into your layout.

2. Add the following script just below the Rownd Hub script to handle :
   ```js
    _rphConfig.push(['setPostAuthenticationApi', {
        method: 'post',
        url: '/api/auth/rownd'  // Replace this with the route you specified in the controller
    }]);
    _rphConfig.push(['setPostSignOutApi', {
        method: 'delete',
        url: '/api/auth/rownd'  // Replace this with the route you specified in the controller
    }]);
   ```

That's it! At this point, you should be able to fire up your app in a browser, sign in with Rownd, and navigate around your app.

## Rownd .NET API

### Auth APIs

**Rownd.Auth.ValidateToken(token)**<br />`public async Task<JwtSecurityToken> ValidateToken(string token)`

Provides ad-hoc Rownd token validation. Pass in a Rownd JWT to find out whether it's valid or not. Typically, all of this is handled for you as part of the request lifecycle and you can get everything you need from the `User` principal that's present in each request.

### User APIs

**Rownd.Users.ListProfiles(opts)**<br />`public async Task<ResultSet<RowndUserProfile>> ListProfiles(UserLookupOpts opts)`

Retrieves a list of user profiles matching the given criteria. The `UserLookupOpts` object is a simple class with the following properties:

```C#
public class UserLookupOpts {
    public string[]? UserLookupCriteria { get; set; }

    public string[]? UserIds { get; set; }
}
```

- `UserLookupCriteria` is an array of strings representing identifiers that may match one or more users.
   For example, providing an array with the elements `juliet@rose.com` and `+19875551212` might return
   a list of two users at most. It might return one or zero depending on whether those identifiers match
   any users. Note that only exact matches are supported. Phone numbers _must_ be in E.164 format.
- `UserIds` is an array of strings representing Rownd user IDs. Providing this array will return a list of
   users matching those IDs. Any IDs that do not match existing users will be ignored.

<hr />

**Rownd.Users.GetProfile(userId)**<br />`public async Task<RowndUserProfile> GetProfile(string userId, bool forceRefresh = false)`

Retrieves the profile for the user matching `userId` or throws if none is found. If the profile is fetched from the server, it will be cached for up to one minute in the server's memory, so subsequent calls within the same or closely-timed requests will not incur the additional latency of unnecessary network traffic.

If you need to override this behavior and force a refresh from the server, pass `true` to the second argument.

While you should look at the .NET interface for `RowndUserProfile`, the shape generally looks something like this:

```C#
string Id;
Dictionary<string, dynamic> Data;
Dictionary<string, dynamic> Meta;
```

The `Data` field will match the shape of the user profile "data types" as defined in the [Rownd platform](https://app.rownd.io/data/297860819392135684/types). Since we don't know exactly what types those values are at compile time, we leave it to you--the developer--to be aware of this and handle casting values to the appropriate type (e.g., `userProfile.Data["first_name"]?.ToString()`.

<hr />

**Rownd.Users.UpdateProfile(userProfile)**<br />`public async Task<RowndUserProfile> UpdateProfile(RowndUserProfile userProfile)`

Saves the given profile back to Rownd via its API. The in-memory cache of this user is also updated and the cache expiration for that entry is reset.

Typically, you'll retrieve the user's profile, update a field (e.g., `userProfile.Data["first_name"] = "Bob"`), and then persist the changes back to the server via `UpdateProfile()`. Changes made here will overwrite the server's copy.

<hr />

**Rownd.Users.DeleteProfile(userId)**<br />`public async Task DeleteProfile(string userId)`
Deletes the user profile matching `userId` from Rownd. If this user exists within the in-memory cache, it will be removed.

<hr />

If you run into issues with this SDK, please [let us know!](https://github.com/rownd/dotnet/issues/new)
