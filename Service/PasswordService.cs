using System;
using BCrypt.Net;

namespace magazin_mercerie.Service
{
    public class PasswordService : IPasswordService
    {
        private const int WorkFactor = 12; // BCrypt work factor (cost) - higher is more secure but slower

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // In case of any BCrypt verification error, return false
                return false;
            }
        }
    }
} 