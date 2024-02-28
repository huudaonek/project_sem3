using System.Security.Cryptography;
using System.Text;
namespace CoffeeLands.Helpers
{
    public static class DataEncryptionExtensions
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển đổi mảng byte thành chuỗi và chọn một phần của chuỗi để sử dụng
                string hashedPassword = BitConverter.ToString(bytes).Replace("-", "").Substring(0, 29);

                return hashedPassword;
            }
        }
    }
}
