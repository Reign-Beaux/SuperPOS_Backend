using Application.Interfaces.Services;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Services.Application;

public class EncryptionService : IEncryptionService
{
    public string HashText(string text)
        => BC.HashPassword(text);

    public bool VerifyText(string text, string hashed)
        => BC.Verify(text, hashed);
}
