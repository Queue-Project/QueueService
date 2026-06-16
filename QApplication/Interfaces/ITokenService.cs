namespace QApplication.Interfaces;

public interface ITokenService
{
     string GenerateAccessToken(int userId, string userName, string role, DateTime expiresAtUtc);
    (string token, DateTime expiresAtUtc) GenerateRefreshToken();
}