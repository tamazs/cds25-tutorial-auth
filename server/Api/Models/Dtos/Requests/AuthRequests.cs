using System.ComponentModel.DataAnnotations;

namespace Api.Models.Dtos.Requests;

public record RegisterRequest(
    [Required] string Email,
    [MinLength(6)] string Password,
    [Required] string Name
);

public record LoginRequest([Required] string Email, [Required] string Password);
