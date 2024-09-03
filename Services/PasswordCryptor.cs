namespace SportWeb.Services
{
    public interface IPasswordCryptor
    {
        string Hash(string password);

        bool Verify(string password, string hashedPassword);
    }

    public class PasswordCryptor : IPasswordCryptor
    {
        // Метод для хеширования пароля
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Метод для проверки пароля
        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}