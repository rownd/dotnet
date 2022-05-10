# Rownd bindings for .NET Core

Use this library to integrate Rownd into your .NET Core web application.

Convenience wrappers are provided for the .NET Core Identity framework, but
you can also leverage token validation and 

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

For the purposes of getting set up quickly, we'll assume you added the app key and secret to your `appsettings.json` file as shown above.

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
        protected override async Task IsAllowedToSignIn(RowndUser rowndUser) {
            // Run any custom logic here to ensure this user should be allowed to sign in. May be async.

            // return; // if everything is fine

            // throw new Exception("You aren't allowed here!"); // if you want to prevent the user from signing in
        }

        public RowndAuthController(RowndClient client, ILogger<RowndAuthController> logger, UserManager<IdentityUser> userManager) : base(client, logger)
        {
            _addNewUsersToDatabase = true;  // If you want Rownd to add users to your own database when they're first authenticated, set this to `true`
            _userManager = userManager; // When `_addNewUsersToDatabase` is `true`, you'll need to pass in a reference to your UserManager
            
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

2. Add the following script just below the Rownd Hub script:
   ```js
    _rphConfig.push(['setPostAuthenticationApi', {
        method: 'post',
        url: '/api/auth/rownd'  // Replace this with the route you specified in the controller
    }]);
   ```

That's it! At this point, you should be able to fire up your app in a browser, sign in with Rownd, and navigate around your app.

If you run into issues, please [let us know!](https://github.com/rownd/dotnet/issues/new)
