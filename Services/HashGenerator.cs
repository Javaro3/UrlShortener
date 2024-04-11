using System.Security.Cryptography;
using System.Text;

namespace Services {
    public class HashGenerator {
        public static string GenerateHash(string url, int length = 8) {
            using (SHA256 sha256Hash = SHA256.Create()) {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(url));

                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length && builder.Length < length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString().Substring(0, Math.Min(builder.Length, length));
            }
        }
    }
}
