# Session

<!--toc:start-->

- [Session](#session)
  - [Theory](#theory)
    - [What are JWT?](#what-are-jwt)
    - [Session ID vs JWT](#session-id-vs-jwt)
  - [Server implementation](#server-implementation)
    - [Configuration](#configuration)
    - [JWT handling](#jwt-handling)
    - [Issue JWT on login](#issue-jwt-on-login)
    - [Verify JWT on incoming requests](#verify-jwt-on-incoming-requests)
  - [Client implementation](#client-implementation)
    - [Token storage](#token-storage)
  - [Limitations](#limitations)
    - [Token expiration](#token-expiration)
    - [Cross Site Scripting (XSS)](#cross-site-scripting-xss)
  - [Conclusion](#conclusion)
  <!--toc:end-->

## Theory

In the last part we implemented authentication.
Meaning, the user can prove their identity to the server.
They prove themselves by presenting a password that the server is able to
verify.
It does it without actually storing the password anywhere.
The assumption is that only the user themselves knows their password.

But what about the next HTTP request?
How can the server tell from whom it originated?
Do we need to authenticate the user on each request?
That would require them to enter their password each time they click on
something on the web app.

What we need is some sort of prof that consecutive requests originated from the
same web browser instance from which the user authenticated.
Such "prof" is called a session.
We can think of a session as authenticating the request.
Meaning that it originates from the same browser window from where the user had
entered their credentials.

Broadly speaking, there are two common ways authenticating a request.
Aka establishing a session.

1. Session ID
2. Self-contained token

A **session ID** is an identifier that is included in all requests within the session.
The server will correlate this ID with information about the authenticated user.
To make it secure, it is important the ID is extremely difficult to guess.
You see, sometimes we use auto-incremented numbers as ID in the database.
That wouldn't work as a proof of anything.
Because it is very easy to guess the previous and next ID.
For session ID's to be secure they need to be long and random.
They don't need to be memorable to humans.
In fact humans shouldn't even have to see them.
A [UUID
v4](<https://en.wikipedia.org/wiki/Universally_unique_identifier#Version_4_(random)>)
can work as session ID.
Generally a session ID is 64 or more random bits encoded into printable
characters somehow. Common encodings are
[HEX](https://en.wikipedia.org/wiki/Hexadecimal) and
[Base64](https://en.wikipedia.org/wiki/Base64).
But other schemes can also be used.
A new random session ID is created for each login.
Session IDs are commonly transmitted in the form of a cookie.
Cookies are simply small pieces of text data that the server can instruct the
browser to include on subsequent requests.
There is no client side code required to use cookies as it is all handled by
server and browser.
A session ID can also be delivered as an HTTP header managed by client-side
code.

A **Self-contained token** is a piece of data that encodes information about
the user directly in such a way that it can't be tempered with.
JWT is the de facto standard for such tokens.
JWT's are often delivered as an HTTP header manages by client-side code.
But can also be delivered as a cookie.

We can use session ID or Self-contained token (such as JWT) to manage the session.
Both can either be delivered as cookie or header.
To simplify things I will limit my further explanation to session ID delivered
as cookie and JWT delivered in HTTP header.

### What are JWT?

JWT is short for [JSON Web Token](https://en.wikipedia.org/wiki/JSON_Web_Token).
It is a token with a specific format.
A JWT consists of 3 parts.

1. **Header** JSON object indicating the signature algorithm.
2. **Payload** JSON object with claims (commonly user ID and role).
3. **Signature** a digital signature of the header and payload.

All 3 parts are [Base64](https://en.wikipedia.org/wiki/Base64) encoded and
separated by a `.` (dot).
Base64 is **_not_** encryption!

_The header in a JWT is not the same as headers in an HTTP request._

Here is an example of a JWT:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

[jwt.io](https://jwt.io/) is a useful tool when debugging code using JWTs.
You can paste a JWT into the site to see the decoded parts.

The signature is a keyed-hash over the header and payload.
It protects the integrity of the JWT.
If claims in the payload gets manipulated then the signature will no longer
match and the manipulation can therefore be detected.
This protection mechanism relies on only the server knowing the secret used to
generate the signature.
It is important to be aware that they are not encrypted.
A JWT can easily be decoded and read, but it can not be tampered with without
breaking the signature.

When using JWTs.
The server will issue a JWT after successful authentication.
The client will then include the JWT in `Authorization` HTTP header.

```
GET /admin-panel HTTP/1.1
Host: example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

When used this way it might also be referred to as a bearer token, since the
word "Bearer" appears before the token.
A bearer token can have other formats, it doesn't have to be a JWT.

<dl>
  <dt>Bearer token</dt>
  <dd>How the token is supplied in HTTP requests.</dd>
  <dt>JWT</dt>
  <dd>How the token is structured.</dd>
</dl>

### Session ID vs JWT

The big difference between cookies and bearer tokens, is that cookies are
included automatically in all request by the browser.
Whereas bearer tokens are set by JavaScript in the client app.
Each approach has its pros and cons.

In this application, the built-in cookie mechanism would be just fine.
However, since using JWT as bearer tokens are so common these days, you will
need to learn how they work and how to use them.

Here is a comparison between the two approaches:

- **Cookies** are automatically managed by the browser.
- **Bearer tokens** are managed by client-side code.
  Meaning that the front-end developer is responsible managing the token.
- **Cookie** can (if misconfigured) be susceptible to
[CSRF](https://en.wikipedia.org/wiki/Cross-site_request_forgery) attacks.
- **Bearer tokens** can be stolen if the app is vulnerable to
  [XSS](https://en.wikipedia.org/wiki/Cross-site_scripting).
- **Bearer tokens** allow for cross-origin authorization.
  Cookies don't.
- **Bearer tokens** well suited for native applications such as desktop and
mobile apps.

[Advanced reading](https://auth0.com/blog/cookies-tokens-jwt-the-aspnet-core-identity-dilemma/)

Imagine you have your front-end being served from my-awesome-frontend.com and
your back-end from my-awesome-backend.com.
This scenario is trivial with bearer tokens.
But doesn't really work with cookies.
There is a workaround though.
You can just have the client app send all requests to front-end web-server,
then have it rewrite requests intended for back-end.

## Server implementation

In our setup we will use a symmetric key for the signature.
Meaning, same key that was used to issue the JWT is also used verify it.

### Configuration

The `appsettings.Development.json` file is a convenient place to store the
signing key during development.

**IMPORTANT the secret you use during development should NEVER be used in
production!**

You can generate a key by running the following command, in git-bash on Windows
or in terminal on macOS.

```sh
openssl rand -base64 64 | tr -d '\n'
```

Copy the result and paste it into `server/Api/appsettings.Development.json`.
Should look similar to this:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "AppDb": "HOST=localhost;DB=postgres;UID=postgres;PWD=mysecret;PORT=5432;"
  },
  "JwtKey": "replace this with the key"
}
```

To use JWT in the back-end, we need to add a couple of packages.

```sh
dotnet add server/Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add server/Api package Microsoft.IdentityModel.JsonWebTokens
```

The **JwtBearer** package is used to extract session information from the
bearer token header.
**JsonWebTokens** provides classes for working with JWTs.

### JWT handling

The values stored inside the payload of JWT are referred to as claims.
We need a way to map between the apps `AuthUserInfo` DTO and the
`ClaimsPrincipal` type that ASP.NET uses internally.
We are going to add the mapping as [extensions
methods](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods).

Create `server/Api/Security/ClaimsExtension.cs` with:

```cs
using System.Security.Claims;
using Api.Models.Dtos.Responses;

namespace Api.Security;

public static class ClaimExtensions
{
    public static string GetUserId(this ClaimsPrincipal claims) =>
        claims.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    public static IEnumerable<Claim> ToClaims(this AuthUserInfo user) =>
        [new("sub", user.Id.ToString()), new("role", user.Role)];

    public static ClaimsPrincipal ToPrincipal(this AuthUserInfo user) =>
        new ClaimsPrincipal(new ClaimsIdentity(user.ToClaims()));
}
```

We need some code to create and validate JWTs.
Create a file `server/Api/Security/JwtService.cs`

```cs
using System.Security.Claims;
using Api.Models.Dtos.Responses;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

public interface ITokenService
{
    string CreateToken(AuthUserInfo user);
}

public class JwtService(IConfiguration config) : ITokenService
{
    public const string SignatureAlgorithm = SecurityAlgorithms.HmacSha512;
    public const string JwtKey = "JwtKey";

    public string CreateToken(AuthUserInfo user)
    {
        var key = Convert.FromBase64String(config.GetValue<string>(JwtKey)!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SignatureAlgorithm
            ),
            Subject = new ClaimsIdentity(user.ToClaims()),
            Expires = DateTime.UtcNow.AddDays(7),
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    public static TokenValidationParameters ValidationParameters(IConfiguration config)
    {
        var key = Convert.FromBase64String(config.GetValue<string>(JwtKey)!);
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidAlgorithms = [SignatureAlgorithm],
            ValidateIssuerSigningKey = true,
            TokenDecryptionKey = null,

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,

            // Set to 0 when validating on the same system that created the token
            ClockSkew = TimeSpan.Zero,
        };
    }
}
```

The `CreateToken` method is used to issue new JWTs.
`TokenValidationParameters` is used to tell ASP.NET how to validate them.

`SigningCredentials` is the key/secret used to create the JWT signature.
The secret is retrieved from the configuration system where it is typically
Base64 encoded.
It is important that you don't use the same secret in development as in production.
It is very important that you do **NOT** commit the production JWT secret to
GIT.
If someone possesses the secret then they can impersonate any user in the app.

We can store whatever information we want in the claims.
But we should stay away from storing secret information such as password in it,
since it isn't encrypted.
Most applications will store user ID and roles as claims.
`Subject` is the payload of the token.
`Expires` is when the token is valid until.
For security reasons, if someone malicious manage to steal the token we want it
to expire quickly to minimize the damage.

Don't forget to wire the service up for dependency injection by adding the following line to `Program.cs`:

```cs
builder.Services.AddScoped<ITokenService, JwtService>();
```

### Issue a JWT on login

Let's update `AuthController` to return a JWT on login.
Add `ITokenService tokenService` to the constructor parameters of the class.
Then replace the login method with:

```cs
[HttpPost]
[Route("login")]
public async Task<LoginResponse> Login([FromBody] LoginRequest request)
{
    var userInfo = service.Authenticate(request);
    var token = tokenService.CreateToken(userInfo);
    return new LoginResponse(token);
}
```

Then we need to update the DTO with a field for JWT, such that it can be
returned to the client.

Open `server/Api/Models/Dtos/Responses/AuthResponses.cs` and change
`LoginResponse` definition to:

```cs
public record LoginResponse(string jwt);
```

Try it!
Run the application.
Open <http://localhost:5153/scalar> and login.
You can paste the JWT into [jwt.io](https://jwt.io) to verify its content.

You can find a list of users in `README.md`.

> [!TIP]
> You can reset the database with the `setup.sh` script.
> In case you forgot the password.

### Verify JWT on incoming requests

We need to tell ASP.NET to look for a JWT Bearer on incoming request, so it can
verify and decode the token, in order to check who is logged in.
To do this we need to configure it in [Program.cs](server/Api/Program.cs).
Inside `ConfigureServices()` method, add the following lines after all the `builder.Services.AddScoped...`.

```cs
// Authentication & Authorization
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = JwtService.ValidationParameters(
            builder.Configuration
        );
        // Add this for debugging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
        };
    });
builder.Services.AddAuthorization();
```

Then inside `ConfigureApp()` method add `app.UseAuthentication();` just above
`app.UseAuthorization();`

Maybe we should also make an endpoint that does something information about the
logged-in user.
Let's add an endpoint that simply return some user information.

In `server/Api/Controllers/AuthController.cs`, replace `UserInfo` method with this:

```cs
[HttpGet]
[Route("userinfo")]
public async Task<AuthUserInfo?> UserInfo()
{
    return service.GetUserInfo(User);
}
```

Then open `server/Api/Services/AuthService.cs` and add the following to `IAuthService` interface:

```cs
AuthUserInfo? GetUserInfo(ClaimsPrincipal principal);
```

And this to `AuthService` class:

```cs
public AuthUserInfo? GetUserInfo(ClaimsPrincipal principal)
{
    var userId = principal.GetUserId();
    return _userRepository
        .Query()
        .Where(user => user.Id == userId)
        .SingleOrDefault()
        ?.ToDto();
}
```

That's it for the server.

## Client implementation

When the client has called the login endpoint with correct credentials, it will
receive a token in response body.
The client needs to hold on to the token, so it can be in a header on future
requests.
Otherwise, the user would have to re-authenticate themselves all the time,
which would be silly.

Cookies are automatically included in requests by the browser.
However, with bearer tokens, we need to write code to include them.

### Token storage

We need to be able to access the token across components.
Because we have different pages that make requests to the server.
They all need to include the token in their requests.

You could use just a normal atom from Jotai to store the token.
But then the user would have to re-authenticate if they reload the page or open
it in a new tab.
Instead of using a plain atom, we can use an
[atomWithStorage](https://jotai.org/docs/utilities/storage).

A `atomWithStorage` stores a value in the browser, meaning it can persist
across multiple tabs.
Each tab you open (even if the URL is the same) is a new instance of the
client.

Browsers have two storage mechanisms that can be used to store key/value pairs.
They are
[localStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage)
and
[sessionStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/sessionStorage).
The difference is that:

- **sessionStorage** is cleared when the browser is closed.
- **localStorage** is only cleared when user clears the browser data.

A `atomWithStorage` can use either storage mechanism.
Which one you should use depends on your security requirements.
For high-secure applications such as online-banking, you defiantly want to go
with `sessionStorage`.
If you need to keep the user authenticated for longer, then go with
`localStorage`.

Let's get started on the implementation.

Add `client/src/atoms/token.ts` with:

```ts
import { atom } from "jotai";
import { atomWithStorage, createJSONStorage } from "jotai/utils";
import { authClient } from "../api-clients";

// Storage key for JWT
export const TOKEN_KEY = "token";
export const tokenStorage = createJSONStorage<string | null>(
  () => sessionStorage,
);

export const tokenAtom = atomWithStorage<string | null>(
  TOKEN_KEY,
  null,
  tokenStorage,
);

export const userInfoAtom = atom(async (get) => {
  // Create a dependency on 'token' atom
  const token = get(tokenAtom);
  if (!token) return null;
  // Fetch user-info
  const userInfo = await authClient.userInfo();
  return userInfo;
});
```

`tokenStorage` defines what storage mechanism to use.
In this case `sessionStorage`.
`tokenAtom` can be used like any other atom, but it will persists the token in
`tokenStorage` backed by `sessionStorage`.

We also have a `userInfoAtom` atom, which when a token is present will fetch
information about the authenticated user from the userInfo endpoint.

When the backend server is started it will use
[NSwag](https://github.com/RicoSuter/NSwag) generate a TypeScript client for
the API.
That way, we don't have to write and maintain the API client code our selves.
Especially keeping DTOs in sync manually can be annoying.
There is just a tiny problem though.
The generated API client doesn't know about Bearer tokens.
To work around this, replace `client/src/api-clients.ts`
with:

```ts
import { TOKEN_KEY, tokenStorage } from "./atoms/token.ts";
import {
  AuthClient,
  BlogClient,
  DraftClient,
} from "./models/generated-client.ts";

const customFetch = async (url: RequestInfo, init?: RequestInit) => {
  const token = tokenStorage.getItem(TOKEN_KEY, null);

  if (token) {
    // Copy of existing init or new object, with copy of existing headers or
    // new object including Bearer token.
    init = {
      ...(init ?? {}),
      headers: {
        ...(init?.headers ?? {}),
        Authorization: `Bearer ${token}`,
      },
    };
  }
  return await fetch(url, init);
};

const baseUrl = undefined;
export const authClient = new AuthClient(baseUrl, { fetch: customFetch });
export const blogClient = new BlogClient(baseUrl, { fetch: customFetch });
export const draftClient = new DraftClient(baseUrl, { fetch: customFetch });
```

The generated clients, in this case AuthClient, BlogClient and DraftClient
allows us to pass a custom fetch function for making the HTTP requests.
Therefore, we can solve the problem by simply creating a wrapper around the
built-in `fetch()` function which adds the token to "Authorization" header, if
present in `tokenStorage`.
This works because rest of the client app already uses the exports from
`api-clients.ts` instead of the generated clients from
`client/src/models/generated-client.ts` directly.

> [!CAUTION]
> When using Bearer tokens in your own projects, it is important that the token
> is only ever included in calls to your own back-end.
> It should NEVER be included in calls to third party APIs.

Now that we got the low-level token plumbing in place then we can use it in a
hook providing nice interface for use in our React components.

Replace `client/src/hooks/auth.tsx` with:

```ts
import { useNavigate } from "react-router-dom";
import { authClient } from "../api-clients";
import type { AuthUserInfo, LoginRequest } from "../models/generated-client";
import { useAtom } from "jotai";
import { tokenAtom, userInfoAtom } from "../atoms/token";

type AuthHook = {
  user: AuthUserInfo | null;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
};

export const useAuth = () => {
  const [_, setJwt] = useAtom(tokenAtom);
  const [user] = useAtom(userInfoAtom);
  const navigate = useNavigate();

  const login = async (request: LoginRequest) => {
    const response = await authClient.login(request);
    setJwt(response.jwt!);
    navigate("/");
  };

  const logout = async () => {
    setJwt(null);
    navigate("/login");
  };

  return {
    user,
    login,
    logout,
  } as AuthHook;
};
```

Take a moment to understand the code.
Notice that with bearer tokens, we simply logout by "forgetting" the token.

You can see how the hook is being used in `client/src/pages/auth/login.tsx` and
`client/src/components/base/profile-dropdown.tsx`.

That's it.
Try it out!

Make sure you have both client and server running.
Then login at <http://localhost:5173/login>.

Inspect the page and see if you can spot the JWT.

![JWT in sessionStorage](./assets/jwt-in-storage.png)

![JWT in header](./assets/jwt-in-header.png)

The app now supports authentication and can maintain session across requests.

## Limitations

### Token expiration

The JWT will expire at some point.
When that happens the user needs will need to re-authenticate.
It doesn't matter whether they are using the app or not, at the moment it
expires.
It can be a bad user experience.
Imagine someone is writing a long blog post.
When they have finally done and ready to submit they can an error because the
token has expired, so they are no longer authenticated.

To work around this issue, many systems will have a second token called a
refresh token.
Upon login the client receives two tokens.
One with a short expiration called an access token.
It is used to authenticate requests.
Another token called a refresh token with a longer lifespan.
The refresh token can be used to retrieve a new access token.
It requires more logic in the client so that the refresh token will be used to
get a new access token when it is near its expiration.

Of course, just making a token that lasts forever would also solve the issue.
However, that is a terrible practice, as it means that the token can be abused
forever if stolen.
Short expiration is more secure, but requires the user to re-authenticate more
often.
It's a trade-off.

### Cross Site Scripting (XSS)

A token stored in `sessionStorage` or `localStorage` can be stolen if the site
is vulnerable to [XSS](https://owasp.org/www-community/attacks/xss/).
XSS is a type of attack where an attacker is able to inject JavaScript into the
page, such that it will be executed within the context of the page on the
victims' browser.
It can be huge problem because JavaScript can just read from `sessionStorage`
or `localStorage`.

_JS on one site can not read session or local storage on a different site.
The browser isolates the storage to a given site._

This blog application uses [Markdown](https://en.wikipedia.org/wiki/Markdown)
to provide rich-text editing for blog posts.
Rich-text allows for adding stuff like headers, bold-text, links etc. to the
text.
Many Markdown libraries have been vulnerable to XSS in the past.
If you are curious on how these kinds of vulnerabilities work, then you can
read this [Exploiting XSS via
Markdown](https://medium.com/taptuit/exploiting-xss-via-markdown-72a61e774bf8)
blog post.

Cookies on the other hand (assuming they have the
[HttpOnly](https://owasp.org/www-community/HttpOnly) flag set) prevents any
JavaScript from reading the value, thereby preventing it from being stolen by
XSS.

A session cookie configured with `Secure`, `HttpOnly` and `SameSite=strict` is
generally more secure than bearer tokens.
See [Secure cookie
configuration](https://developer.mozilla.org/en-US/docs/Web/Security/Practical_implementation_guides/Cookies).

## Conclusion

Using JWT bearer is a bit more involved than cookies.
But they are getting popular, so it is important that you understand how they
work.

A cookie is only going to be included for the same site for which it was set.

The reason bearer tokens are common is that they can be used to seamless
transition between the increasing complex and interconnected infrastructure.

[Reference solution](https://github.com/rpede/cds25-tutorial-auth/tree/01-session)
