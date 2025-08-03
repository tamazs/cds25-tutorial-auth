# Authentication

## Theory

When a user login to a web application they need to prove their identity
somehow.
That process is called authentication.

### Factors of authentication

There are several ways a system can authenticate users.
These are divided into 3 categories.

- Knowledge: Something the user knows. Examples:
  - Password
  - Passphrase
  - Personal identification number (PIN)
  - Security question
- Ownership: Something the user has. Examples:
  - ID card
  - Security token
  - Cell phone. Proof via:
    - SMS
    - Authenticator app
- Inherence: Something the user is or does. Examples:
  - Fingerprint
  - Face
  - Voice

When factors from more than one of these categories are combined it's called
multifactor authentication (MFA) and provides stronger guarantees about the
users' authenticity than just one factor alone.

Not all means of authentication give equal guarantees.
Some a stronger than others.
Here are examples of factors that can be weak on some situations.

Security questions was once common to use for password reset.
Examples are, "your moms maiden name", "name of first pet", "street you grew up
on" etc.
Questions like these are a fairly weak form of authentication as the answer can
often be found on social networks.

SMS code are also sometimes used for authentication.
It should never be used as a single factor of authentication due to
insecurities in the SMS system, such as messages not being encrypted.

Password based authentication is still the most widely used form of
authentication, so that is what we will focus on here.

You can read more about authentication
[here](https://www.microsoft.com/en-us/security/business/security-101/what-is-authentication).

### Good passwords

This continues to be the most widespread way of authenticating users in IT
systems.
There are some issues to it, however.
Mainly what is known as password-fatigue.
Let me explain.

You have probably learned that a good password should be:

- at least 6 characters long
- include upper and lower case letters
- numbers
- and special symbols

So using the following as your password should be really secure, right?

```
qwerty1234!@#$
```

Wrong!
It is not a good password as there is definitely a pattern to it (try to type
it).
Using `Password1!` is also not a good password since it is probably one of the
most commonly used passwords.
So what is a good password then?

**A good password is long, random and unique.**

Long and random to make it hard to guess.
Any pattern will weaken the password.
It also needs to be unique.
Meaning that you must use a different password for each service you sign up to.
That way if one of the services you are using has bad security and gets
compromised then you don't leave the attackers open to authenticate as you on
all the services you are using.

How do you remember all those passwords then?
You don't!
As in modern day that would be beyond the capabilities of most people.
What you need to do instead is to use a password manager.
Password managers provide a secure way to store all your passwords using
cryptography.
As a user you only need to authenticate once to unlock your password manager.
Then you use it access your passwords for all other accounts.
It means that you will end up only having to remember one password.
Though you have to guard that one password with your life.

### Password hashing

As software developers, we actually don't want to save users passwords anywhere.
Because if our system gets compromised then attackers could use the passwords
to compromised accounts on other services for our users, as most people are
still going to reuse the same couple of passwords.

Yes, you read correctly.
We actually don't want to know our users passwords.
We just want to know that they know the password they have chosen for our
system to they can prove their identity to us that way.

Sound like a conundrum, doesn't it?
How can we tell that a user knows their password without storing their
password?
The answer is hashing.

Hashing is a special kind of function.
We can take some data (such as a password), run it through a hashing function
to get some other data known as a hash.
This hash has a couple of interesting properties.

1. The output (hash) looks nothing like the input.
2. The same input will always give the same output.
3. The input can not be determined from the output.

Meaning the only way to guess the password from a hash is simply to try and run
possible passwords through the same hashing function until you get an output
that match.
This process is known as password cracking btw.

So we simply store the hash of our users passwords instead of the actual
password.
That way, when user wants to authenticate we take their password, run it
through the hash function, compare with the stored hash.
If they match, we know that they know their password, without ever having to
store it.

We can make it extra difficult for someone trying to crack passwords if we use
a hashing function that is inherently slow to compute.
A simple way to make a hashing function slower is just to feed the output into
the function again many thousands of times.
Not all hashing functions are equal, as some make cracking more computational
expensive even on specialize hardware.

The password hashing algorithms you should be using (in order of preference)
is:

1. [Argon2id](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#argon2id)
2. [scrypt](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#scrypt)
3. [bcrypt](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#bcrypt)
4. [PBKDF2](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html#pbkdf2)

You should only use an algorithm lower on the list, if no good/trusted
implementation is available, or otherwise hindered in using an algorithm higher
on the list.

Each of the listed algorithms have specific parameters that can be used to
configure how much computation is required for calculating the hash.
You should go with values for parameters as suggested in the links above.

Whenever you need to implement password hashing in the future.
Look up [OWASP - Password Storage Cheat
Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html).
Take notes or get it tattooed!

Also, please don't attempt to implement the algorithms yourself.
You will likely do it wrong.
Use trusted libraries instead.

### Salt and pepper

If several users on the same service use the same password then it would store
the same hash, right?
What if multiple services use the same hashing algorithm, wouldn't that mean if
a user uses the same password they would store the same hash, and by extent,
wouldn't that mean that if the password is cracked for one service it is
cracked for all?

Yes, that is absolutely correct.
To all the questions right above.

The really cool thing is, we have ways around those issues too.
Simply, flavor the hash with some salt and pepper.
Let me explain.

We will start with **salt**.
A salt is simply some random bit of data that is combined with password in the
hashing function.
If we use different salt for each user then we don't get the same hash if two
users have the same password.
However, it also means that we have to store a salt for each user in addition
to the hash.
Luckily, many implementations of password hashing algorithms will simply encode
the hash and a salt into one output string, that we can store in our database.

What if the database gets leaked in some way, then an attacker will in theory
be able to crack the passwords?
Even though in practice it might take trillions of years because we chose a
slow algorithm.
There is a solution to that (hypothetical) issue too.
In addition to salt we also add another piece of random data called a **pepper**.
A pepper is unique to the service itself, not to each user on our service.
And importantly, pepper is not stored in the database itself.
That way, an attacker can't crack passwords even if they got a dump of the
entire database.

## Implementation

### Chose hashing library

Making the password hashing code implement the `IPasswordHasher<T>` interface
makes it easy to swap out one implementation for another.
It is also great for interoperability with Identity.

_Identity will be explained in a later tutorial._

The big question is what algorithm and what implementation should you use.
Here are some options I've found by searching for the OWASP recommended
algorithms on [Nuget](https://www.nuget.org/).
The libraries shown are those with the highest number of total downloads that
implements the recommended algorithms.
Following the logic that if enough people use it then it's probably not a
complete disaster.

Take a look at the options.
Which one would you pick and why?

**NSec.Cryptography**

- 6,860,739 total downloads
- Algorithms: argon2id, scrypt
- [Website](https://nsec.rocks/)

Install with:

```sh
dotnet add package NSec.Cryptography
```

Implementation:

```cs
using System.Security.Cryptography;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Security;

public class NSecArgon2idPasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(128 / 8);
        var hash = GenerateHash(password, salt);
        return $"argon2id${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(
        User user,
        string hashedPassword,
        string providedPassword
    )
    {
        var parts = hashedPassword.Split('$');
        var salt = Convert.FromBase64String(parts[1]);
        var storedHash = Convert.FromBase64String(parts[2]);
        var providedHash = GenerateHash(providedPassword, salt);
        return CryptographicOperations.FixedTimeEquals(storedHash, providedHash)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }

    public byte[] GenerateHash(string password, byte[] salt)
    {
        var hashAlgo = new NSec.Cryptography.Argon2id(new NSec.Cryptography.Argon2Parameters
        {
            DegreeOfParallelism = 1,
            MemorySize = 12288,
            NumberOfPasses = 3,
        });
        return hashAlgo.DeriveBytes(password, salt, 128);
    }
}
```

**Konscious.Security.Cryptography.Argon2**

- 4,216,331 total downloads
- Algorithms: argon2id
- [Website](https://github.com/kmaragon/Konscious.Security.Cryptography)

Install with:

```sh
dotnet add package Konscious.Security.Cryptography.Argon2
```

Implementation:

```cs
using System.Security.Cryptography;
using System.Text;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Security;

public class KonciousArgon2idPasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(128 / 8);
        var hash = GenerateHash(password, salt);
        return $"argon2id${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordVerificationResult VerifyHashedPassword(
        User user,
        string hashedPassword,
        string providedPassword
    )
    {
        var parts = hashedPassword.Split('$');
        var salt = Convert.FromBase64String(parts[1]);
        var storedHash = Convert.FromBase64String(parts[2]);
        var providedHash = GenerateHash(providedPassword, salt);
        return CryptographicOperations.FixedTimeEquals(storedHash, providedHash)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }

    public byte[] GenerateHash(string password, byte[] salt)
    {
        using var hashAlgo = new Konscious.Security.Cryptography.Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = 12288,
            Iterations = 3,
            DegreeOfParallelism = 1,
        };
        return hashAlgo.GetBytes(128);
    }
}
```

**BCrypt.Net-Next**

- 30,902,998 total downloads
- Algorithms: bcrypt
- [Website](https://bcryptnet.chrismckee.uk/)

Install with:

```sh
dotnet add package BCrypt.Net-Next
```

Implementation:

```cs
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Security;

public class BcryptPasswordHasher : IPasswordHasher<User>
{
    public string HashPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public PasswordVerificationResult VerifyHashedPassword(
        User user,
        string hashedPassword,
        string providedPassword
    )
    {
        var isValid = BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
    }
}
```

**Microsoft.AspNetCore.Identity.PasswordHasher**

- 121,137,536 total downloads
- Algorithms: pbkdf2
- [Website](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-9.0)

Installed with:

```sh
dotnet add package Microsoft.AspNetCore.Identity
```

Implementation:

There is already an Implementation.
Just use the `PasswordHasher<User>` class.

### Test password hashing

Add the implementation you've chosen to a file in `server/Api/Security` folder.
Create the folder if it doesn't exist already.

It's a good idea to test the implementation.

Start by registering the implementation you've chosen.
Open `server/Api/Program.cs` and add the following inside the
`ConfigureServices` method.

```cs
builder.Services.AddScoped<IPasswordHasher<User>, YourPasswordHasher>();
```

Replace `YourPasswordHasher` with the name of the implementation you've chosen.

Here is a simple test case that hashes a password then verifies the hash.
Put it in a new file in `Test` project.

```cs
using Api;
using DataAccess.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

# pragma warning disable CS8625, CS8618
public class PasswordHasherTest
{
    IPasswordHasher<User> sut;

    [Before(Test)]
    public void Setup()
    {
        var builder = WebApplication.CreateBuilder();
        Program.ConfigureServices(builder);

        var app = builder.Build();

        sut = app.Services.GetRequiredService<IPasswordHasher<User>>();
        Console.WriteLine($"Using password hasher: {sut.GetType().Name}");
    }

    [Test]
    public async Task HashAnVerifyPassword()
    {
        var password = "S3cret!1";
        var hash = sut.HashPassword(null, password);
        var result = sut.VerifyHashedPassword(null, hash, password);
        await Assert.That(result).IsEqualTo(PasswordVerificationResult.Success);
    }
}
```

### Business logic

The business logic for authentication should be implemented in a service.
Controllers are only there to provide a web API on top.

However, before the business logic can be implemented, we need some place to
store the hash.

Open the entity representing a user found in
`server/DataAccess/Entities/User.cs`.

Uncomment these lines:

```cs
[JsonIgnore]
public string PasswordHash { get; set; } = null!;
```

The `[JsonIgnore]` part is so password hash doesn't get exposed by accident.
It tells the JSON serializer to ignore the field.

Uncomment the lines including `PasswordHasher` in `server/Api/Etc/DbSeeder.cs`.
`DbSeeder` is what creates the test data when you run the `setup.sh` script.
Adding a password hash to the users are going to be useful later on when
testing the business logic.

Users needs a way to register themselves and the need a way to login
(authenticate).
You can base your business logic implementation on this interface:

```cs
public interface IAuthService
{
    AuthUserInfo Authenticate(LoginRequest request);
    Task<AuthUserInfo> Register(RegisterRequest request);
}
```

For the concrete implementation your need to inject `IPasswordHasher<User>` and
`IRepository<User>`.
Example:

```cs
public class AuthService(
    ILogger<AuthService> _logger,
    IPasswordHasher<User> _passwordHasher,
    IRepository<User> _userRepository
) : IAuthService
{
    public AuthUserInfo Authenticate(LoginRequest request)
    {
      // Your implementation here
    }

    public async Task<AuthUserInfo> Register(RegisterRequest request)
    {
      // Your implementation here
    }
}
```

You should make an implementation of the interface following these requirements.

**Authentication()**

- Lookup the user by `request.Email` (use `IRepository<User>.Query()`).
- Verify that `request.Password` matches the hashed password.
- If you get `PasswordVerificationResult.Success` then map the `User` entity to.
  `AuthUserInfo` DTO and return it.
- In all other cases throw a `AuthenticationException`.

_You don't want to disclose to the client why authentication failed.
Just whether it was successful.
Anymore could give insights to an attacker.
Log why authentication failed instead, so you can still debug any issues._

**Register()**

- Make sure that a user with `request.email` doesn't exist already.
  - Throw a `ValidationException` if the email is already in use.
- Map the `RegisterRequest` DTO to an instance of `User` entity.
- Hash `request.Password` and assign it to `PasswordHash` field of the `User` entity.
- Add the `User` to `IRepository<User>`
- Map the returned `User` entity to `AuthUserInfo`
- Return the `AuthUserInfo`

#### Testing

Production ready code should always have automated tests.
And we will write it as a unit test.

Notice the service depends on `IPasswordHasher<User>` and `IRepository<User>`.
We need a substitute (aka test double) for those, so we can keep our test to
just a unit.
One could use a mocking framework like [Moq](https://github.com/devlooped/moq)
or write the test double by hand. Here we'll write it by hand.

Create file `server/Tests/Helpers/FakePasswordHasher.cs` with:

```cs
using Microsoft.AspNetCore.Identity;

namespace Tests.Helpers;

class FakePasswordHasher<T> : IPasswordHasher<T>
    where T : class
{
    public string HashPassword(T user, string password) => new string(password.Reverse().ToArray());

    public PasswordVerificationResult VerifyHashedPassword(
        T user,
        string hashedPassword,
        string providedPassword
    ) =>
        Object.Equals(hashedPassword, HashPassword(user, providedPassword))
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
}
```

And `server/Tests/Helpers/InMemoryRepository.cs` with:

```cs
using DataAccess.Repositories;

namespace Tests.Helpers;

class InMemoryRespository<T>(IList<T> entities) : IRepository<T>
    where T : class
{
    public async Task Add(T entity)
    {
        entities.Add(entity);
    }

    public async Task Delete(T entity)
    {
        var reference = entities.Single((t) => (t as dynamic).Id == (entity as dynamic).Id);
        entities.Remove(reference);
    }

    public async Task Update(T entity)
    {
        await Delete(entity);
        await Add(entity);
    }

    public IQueryable<T> Query()
    {
        return entities.AsQueryable();
    }
}
```

Should be needless to say that `FakePasswordHasher` should never be used in
production.
Instead of calculating an actual hash then it just reverses the string as we
just need it to return some value that is derived from the given password.

`InMemoryRespository` is an implementation of `IRepository` that doesn't touch
a database.
Instead, it just keeps a list in memory which is much faster.
It does something a bit hacky as it assumes what the entity type (such as
`User`) has a `Id` field.
It does that by casting to `dynamic` which doesn't get type checked by the
compiler.

We can then write a unit test like this:

```cs
using System.Security.Authentication;
using Api.Models.Dtos.Requests;
using Api.Services;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Tests.Helpers;
using TUnit.Assertions.AssertConditions.Throws;

namespace Tests.Services;

public class AuthServiceTest
{
    IRepository<User> userRepository = null!;
    IPasswordHasher<User> passwordHasher = null!;
    IAuthService sut = null!;

    [Before(Test)]
    public void Setup()
    {
        passwordHasher = new FakePasswordHasher<User>();
        String Hash(string password) => passwordHasher.HashPassword(null!, password);
        userRepository = new InMemoryRespository<User>(
            new List<User>
            {
                new User()
                {
                    Id = "user1",
                    UserName = "User1",
                    Email = "user1@example.com",
                    PasswordHash = Hash("fakepassword"),
                    Role = Role.Reader,
                },
                new User()
                {
                    Id = "user2",
                    UserName = "User2",
                    Email = "user2@example.com",
                    PasswordHash = Hash("fakepassword"),
                    Role = Role.Admin,
                },
            }
        );
        sut = new AuthService(
            new LoggerFactory().CreateLogger<AuthService>(),
            passwordHasher,
            userRepository
        );
    }

    [Test]
    public async Task Authenticate_Success()
    {
        var response = sut.Authenticate(new LoginRequest("user1@example.com", "fakepassword"));
        await Assert.That(response.UserName).IsEqualTo("User1");
    }

    [Test]
    public async Task Authenticate_InvalidEmail()
    {
        await Assert
            .That(() => sut.Authenticate(new LoginRequest("invalid", "fakepassword")))
            .Throws<AuthenticationException>();
    }

    [Test]
    public async Task Authenticate_InvalidPassword()
    {
        await Assert
            .That(() => sut.Authenticate(new LoginRequest("user1@example.com", "invalid")))
            .Throws<AuthenticationException>();
    }
}
```

When you are done with the implementation, make sure you remember to register
it for dependency injection.
In `Program.cs` under `ConfigureServices()` add:

```cs
builder.Services.AddScoped<IAuthService, AuthService>();
```

### Web API

Now that the business logic is implemented it's simple to add a web API layer
on top.
The web API that the client will interact with is defined in a controller.
Controllers shouldn't contain business logic, the just define the API for HTTP
requests.

Here is a controller implementation you can use.

```cs
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var userInfo = service.Authenticate(request);
        return new LoginResponse();
    }

    [HttpPost]
    [Route("register")]
    public async Task<RegisterResponse> Register([FromBody] RegisterRequest request)
    {
        var userInfo = await service.Register(request);
        return new RegisterResponse(UserName: userInfo.UserName);
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IResult> Logout()
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [Route("userinfo")]
    public async Task<AuthUserInfo> UserInfo()
    {
        throw new NotImplementedException();
    }
}
```

Implementation of `Logout()` and `UserInfo()` endpoints will be implemented in
the next part where we look at how to manage sessions.

#### Testing

You can manually give it a spin.
In terminal, run:

```sh
./setup.sh 'S3cret!'
dotnet run --project server/Api --watch
```

Open <http://localhost:5153/scalar/>

<video controls>
   <source src="./assets/manual-test-login.mp4" type="video/mp4">
</video>

Manually testing is going to be hindering as a project increase in scope, so it
is better have automated test from the get go.

Here is a quick manual test following the [Arrange, Act, Assert
pattern](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices#arrange-your-tests).

```cs
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Security.Authentication;
using Api;
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.Controllers;

public class AuthControllerTest
{
    WebApplicationFactory<Program> CreateWebApplicationFactory(IAuthService mock)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped<IAuthService>((s) => mock))
        );
    }

    [Test]
    public async Task Login_Validation()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new LoginRequest("", "");
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Login_Success()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new LoginRequest("user@example.com", "secret");
        mock.Setup(x => x.Authenticate(requestBody))
            .Returns(new AuthUserInfo(Id: "1", UserName: "User1", Role: Role.Reader));
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Login_Fail()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new LoginRequest("user@example.com", "secret");
        mock.Setup(x => x.Authenticate(requestBody)).Throws<AuthenticationException>();
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Register_Validation()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new RegisterRequest(Email: "invalid_email", "", "", "");
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_ValidationWithInvalidEmail()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new RegisterRequest(
            Email: "invalid_email",
            UserName: "User",
            Password: "secret",
            Name: "User"
        );
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_Success()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new RegisterRequest(
            Email: "user@example.com",
            UserName: "User",
            Password: "secret",
            Name: "User"
        );
        mock.Setup(x => x.Register(requestBody))
            .ReturnsAsync(new AuthUserInfo(Id: "1", UserName: "User1", Role: Role.Reader));
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Register_EmailExists()
    {
        // Arrange
        var mock = new Mock<IAuthService>();
        var requestBody = new RegisterRequest(
            Email: "user@example.com",
            UserName: "User",
            Password: "secret",
            Name: "User"
        );
        mock.Setup(x => x.Register(requestBody)).Throws<ValidationException>();
        using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}
```

This time we are using the mocking framework
[Moq](https://github.com/devlooped/moq) to make a test double for
`AuthService`.
The intent of this test is solely to verify that the controller behaves as
expected at the HTTP level.
It doesn't touch the database, nor any business logic.

It is important to both test success and failure conditions.
Further, it also tests that the model is being validated.
ASP.NET automatically validates models before an action is executed.
However, it is very easy to forget adding data validation attributes to DTOs.

There is one failing test, that is `Register_ValidationWithInvalidEmail`.
It returns `InternalServerError` instead of the expected `BadRequeset`.
We expect that validation should fail with something that is obviously not a
valid email.

Go to definition of `RegisterRequest` and add the appropriate validation
attribute to the `Email` field.
See [list of validation
attributes](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-9.0).

In addition to the tests included in this article you its also good to include
either end-to-end tests or integration tests in your testing strategy.
However, that is outside the scope of this guide.

## Known limitations

#### Password recovery

What if the user forgets their password?
It is common practice to send an email with a reset link.
Such a link must include reset token or a long one-time-password.
This is used to prove that the reset request originated from that specific
email.
It is important because we don't want to let people be able to reset other peoples
passwords as that could easily lead to account takeover.
Also, we haven't actually verified that the user has possession over the email
address that was given during registration.
Maybe they made a typo, in which case we could be giving a random person access
to their account when a password reset is initiated.

#### Multifactor authentication (MFA)

Humans are bad a remembering passwords that are secure (long and random).
So they often work around this issue by picking passwords that are memorable to
them but less secure.
Even with good passwords there is the possibility that it could be compromised.
Maybe the user got tricked into revealing their passwords, such as from a scam
email.
Because of this, it is a good idea to implement MFA.
MFA is a must for high secure applications.

## Conclusion

We have now made an implementation for register and login.
That is registering and identity for the app and verifying the identity (aka
authentication).
Tests have been written to give us reasonable confidence that the implantation
is correct, though we skipped out on integration test as that is too broad a
topic to also cover in this guide.

Save your work for the next part where we look at how to implement sessions.

[Reference solution](https://github.com/rpede/cds25-tutorial-auth/tree/00-authentication)
