using System.Security.Cryptography;
using System.Text;

namespace INZFS.MVC.Services
{
    public interface IApplicationNumberGenerator
    {
        public string Generate(int size);
    }
    public class ApplicationNumberGenerator : IApplicationNumberGenerator
    {
        const string PREFIX = "EEF9";
        public string Generate(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return PREFIX + "" + result.ToString().ToUpper();
        }
    }
}
