using System;
using System.Security.Cryptography;
using System.Text;

namespace ePiggy.Authentication
{
    public static class HashingProcessor
    {
        public static string CreateSalt(int size)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }


        public static string GenerateHash(string input, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(input + salt);
            var sHa256ManagedString = new SHA256Managed();
            var hash = sHa256ManagedString.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool AreEqual(string plainTextInput, string hashedInput, string salt)
        {
            var newHashedPin = GenerateHash(plainTextInput, salt);
            return newHashedPin.Equals(hashedInput);
        }
    }
}
