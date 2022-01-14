using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace INZFS.Theme.Services
{

    public interface IUrlEncodingService
    {
        string GetHexFromString(string value);
        string GetStringFromHex(string value);
        string MaskEmail(string email);
        string Encrypt(string value);
        string Decrypt(string value);
        string Base64UrlEncode(string value);
        string Base64UrlDecode(string value);
    }


    public class UrlEncodingService : IUrlEncodingService
    {
        private readonly UrlEncoder _urlEncoder;
        private readonly IDataProtector protector;

        public UrlEncodingService(IDataProtectionProvider dataProtectionProvider, UrlEncoder urlEncoder)
        {
            _urlEncoder = urlEncoder;
            protector = dataProtectionProvider.CreateProtector(nameof(UrlEncodingService));
        }
        
        public string GetHexFromString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return String.Empty;
            }

            var bytes = Encoding.Unicode.GetBytes(value);

            var stringBuilder = new StringBuilder();
            foreach (var byteOfChar in bytes)
            {
                stringBuilder.Append(byteOfChar.ToString("X2"));
            }
            return stringBuilder.ToString(); 
        }

        public string GetStringFromHex(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return String.Empty;
            }
            var bytes = new byte[value.Length / 2];
            for (var index = 0; index < bytes.Length; index++)
            {
                bytes[index] = Convert.ToByte(value.Substring(index * 2, 2), 16);
            }
            return Encoding.Unicode.GetString(bytes); 
        }

        public string MaskEmail(string email)
        {   string pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            string result = Regex.Replace(email, pattern, m => new string('*', m.Length));
            return result;
        }

        public string Encrypt(string value)
        {
            return WebUtility.UrlEncode(protector.Protect(value));
        }

        public string Decrypt(string value)
        {
            var protectedData = WebUtility.UrlDecode(value);
            return protector.Unprotect(protectedData);
        }

        public string Base64UrlEncode(string value)
        {
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(value));
        }
        
        public string Base64UrlDecode(string value)
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value));
        }
    }
   
}
