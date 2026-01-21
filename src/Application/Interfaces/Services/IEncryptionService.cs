namespace Application.Interfaces.Services;

public interface IEncryptionService
{
    string HashText(string text);
    bool VerifyText(string text, string hashed);
}
