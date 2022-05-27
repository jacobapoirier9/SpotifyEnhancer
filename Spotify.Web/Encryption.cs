using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Spotify.Web
{
    public static class Encryption
    {
        private const string _salt = "Th3reC4nB30nly1ne";

        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(value));
        }

        public static string FromBase64String(this string value)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(value));
        }

        public static string ToSHA256Hash(this string pass) => pass.ToSHA256Hash(_salt);
        public static string ToSHA256Hash(this string pass, string salt)
        {
            var shaM = new SHA256Managed();

            var passBytes = Encoding.UTF8.GetBytes(pass);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            var saltedBytes = new byte[saltBytes.Length + passBytes.Length];

            saltBytes.CopyTo(saltedBytes, 0);
            passBytes.CopyTo(saltedBytes, salt.Length);

            var result = shaM.ComputeHash(saltedBytes);
            return BitConverter.ToString(result);
        }

        public static string ToAES(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");

            string toReturn;
            RijndaelManaged rijndael = null;

            try
            {
                var key = new Rfc2898DeriveBytes(_salt, Encoding.UTF8.GetBytes(_salt));

                rijndael = new RijndaelManaged();
                rijndael.Key = key.GetBytes(rijndael.KeySize / 8);

                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(BitConverter.GetBytes(rijndael.IV.Length), 0, sizeof(int));
                    memoryStream.Write(rijndael.IV, 0, rijndael.IV.Length);
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(plainText);
                    }
                    toReturn = Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            finally
            {
                if (rijndael != null)
                    rijndael.Clear();
            }

            return toReturn;
        }

        public static string FromAES(this string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

            RijndaelManaged rijndael = null;
            string plaintext = null;

            try
            {
                var key = new Rfc2898DeriveBytes(_salt, Encoding.UTF8.GetBytes(_salt));

                byte[] bytes = Convert.FromBase64String(cipherText);
                using (var memoryStream = new MemoryStream(bytes))
                {
                    rijndael = new RijndaelManaged();
                    rijndael.Key = key.GetBytes(rijndael.KeySize / 8);
                    rijndael.IV = ReadByteArray(memoryStream);
                    ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);

                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cryptoStream))
                    {
                        plaintext = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return cipherText;
            }
            finally
            {
                if (rijndael != null)
                    rijndael.Clear();
            }

            return plaintext;
        }

        private static byte[] ReadByteArray(Stream stream)
        {
            var rawLength = new byte[sizeof(int)];
            if (stream.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            var buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}
