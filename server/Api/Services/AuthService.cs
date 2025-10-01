using System.ComponentModel.DataAnnotations;
using Api.Etc;
using Api.Mappers;
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;

public interface IAuthService
{
    AuthUserInfo Authenticate(LoginRequest request);
    Task<AuthUserInfo> Register(RegisterRequest request);
}

public class AuthService(
    ILogger<AuthService> _logger,
    IPasswordHasher<User> _passwordHasher,
    IRepository<User> _userRepository
) : IAuthService
{
    public AuthUserInfo Authenticate(LoginRequest request)
    {
        try
        {
            var entity = _userRepository.Query().Single(u => u.Email == request.Email);
            var isAuthenticated = _passwordHasher.VerifyHashedPassword(
                entity,
                entity.PasswordHash,
                request.Password
            );
            if (isAuthenticated == PasswordVerificationResult.Success)
            {
                return entity.ToDto();
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Authenticate error: {Message}", e);
        }

        throw new AuthenticationError();
    }

    public async Task<AuthUserInfo> Register(RegisterRequest request)
    {
        if (_userRepository.Query().Any(u => u.Email == request.Email))
        {
            throw new ValidationException("Email already exists.");
        }
        var entity = new User()
        {
            UserName = request.UserName,
            Email = request.Email,
            Role = Role.Reader,
        };
        entity.PasswordHash = _passwordHasher.HashPassword(entity, request.Password);
        await _userRepository.Add(entity);
        return entity.ToDto();
    }
}