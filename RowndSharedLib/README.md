# Rownd bindings for .NET Core

Use this library to integrate Rownd into your .NET Core web application.

Convenience wrappers are provided for the .NET Core Identity framework, but
you can also leverage token validation and 

## Installation

From NuGet:
```bash
dotnet add package Rownd --prerelease
```

## Supported versions

- .NET 6.x
- _Need a different version? [Let us know!](https://github.com/rownd/dotnet/issues/new?title=Request support for .NET X.X)_

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
you'll want to leverage our browser SDKs for ease of implementation.