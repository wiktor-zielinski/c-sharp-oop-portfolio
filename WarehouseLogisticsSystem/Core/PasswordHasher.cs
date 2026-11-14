using System;
using System.Security.Cryptography;
using System.Text;

namespace WarehouseLogisticsSystem.Core
{
    // Narzędzie kryptograficzne do jednokierunkowego haszowania haseł algorytmem SHA-256
    public static class PasswordHasher
    {
        public static string HashPassword(string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
                throw new ArgumentException("Hasło nie może być puste.");

            byte[] bytes = Encoding.UTF8.GetBytes(plainPassword);
            byte[] hashBytes = SHA256.HashData(bytes);
            return Convert.ToHexString(hashBytes);
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(inputPassword)) return false;
            string inputHash = HashPassword(inputPassword);
            return string.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}