using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Api;
using Api.Etc;
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
        await using var factory = CreateWebApplicationFactory(mock.Object);
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
        await using var factory = CreateWebApplicationFactory(mock.Object);
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
        mock.Setup(x => x.Authenticate(requestBody)).Throws<AuthenticationError>();
        await using var factory = CreateWebApplicationFactory(mock.Object);
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
        await using var factory = CreateWebApplicationFactory(mock.Object);
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
        await using var factory = CreateWebApplicationFactory(mock.Object);
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
        await using var factory = CreateWebApplicationFactory(mock.Object);
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
        await using var factory = CreateWebApplicationFactory(mock.Object);
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", requestBody);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}