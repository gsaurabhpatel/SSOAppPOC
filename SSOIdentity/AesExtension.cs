using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SSOIdentity
{
    class AesExtension
    {
        private static Aes aes = Aes.Create();
        private static UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

        private const int CHUNK_SIZE = 128;

        private void InitializeRijndael()
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
        }

        public AesExtension()
        {
            InitializeRijndael();

            aes.KeySize = CHUNK_SIZE;
            aes.BlockSize = CHUNK_SIZE;

            aes.GenerateKey();
            aes.GenerateIV();
        }

        public AesExtension(string base64key, string base64Iv)
        {
            InitializeRijndael();

            aes.Key = Convert.FromBase64String(base64key);
            aes.IV = Convert.FromBase64String(base64Iv);
        }

        public AesExtension(byte[] key, byte[] iv)
        {
            InitializeRijndael();

            aes.Key = key;
            aes.IV = iv;
        }

        private string Decrypt(byte[] cipher)
        {
            ICryptoTransform transform = aes.CreateDecryptor();
            byte[] decryptedValue = transform.TransformFinalBlock(cipher, 0, cipher.Length);
            return unicodeEncoding.GetString(decryptedValue);
        }

        public string DecryptFromBase64String(string base64cipher)
        {
            return Decrypt(Convert.FromBase64String(base64cipher));
        }

        private byte[] EncryptToByte(string plain)
        {
            ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] cipher = unicodeEncoding.GetBytes(plain);
            byte[] encryptedValue = encryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return encryptedValue;
        }

        public string EncryptToBase64String(string plain)
        {
            return Convert.ToBase64String(EncryptToByte(plain));
        }

        private string GetKey()
        {
            return Convert.ToBase64String(aes.Key);
        }

        private string GetIV()
        {
            return Convert.ToBase64String(aes.IV);
        }

        public override string ToString()
        {
            return "KEY:" + GetKey() + Environment.NewLine + "IV:" + GetIV();
        }
    }
}
