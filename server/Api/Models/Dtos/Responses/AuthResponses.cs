namespace Api.Models.Dtos.Responses;

public record RegisterResponse(string UserName);

public record LoginResponse(string jwt);

public record AuthUserInfo(string Id, string UserName, string Role);
