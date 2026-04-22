using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.Common.Services
{
    public class EncryptionService : IEncryptionService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 4;
        private const int MemorySize = 1024 * 64;
        private const int Parallelism = 2;

        public static string Hash(string str)
        {
            byte[] salt = new byte[SaltSize];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(str))
            {
                Salt = salt,
                Iterations = Iterations,
                MemorySize = MemorySize,
                DegreeOfParallelism = Parallelism
            };

            byte[] hash = argon2.GetBytes(HashSize);

            // Combine salt + hash for storage
            byte[] combined = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, combined, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, combined, SaltSize, HashSize);

            return Convert.ToBase64String(combined);
        }

        public static bool Verify(string str, string storedHash)
        {
            byte[] combined = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[HashSize];

            Buffer.BlockCopy(combined, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(combined, SaltSize, hash, 0, HashSize);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(str))
            {
                Salt = salt,
                Iterations = Iterations,
                MemorySize = MemorySize,
                DegreeOfParallelism = Parallelism
            };

            byte[] computedHash = argon2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, hash);
        }
    }
}
