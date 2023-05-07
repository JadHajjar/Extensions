using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Extensions
{
	public class Encryption
	{
		public static string Encrypt(string value, string salt)
		{
			var saltBytes = Encoding.UTF8.GetBytes(salt);
			var valueBytes = Encoding.UTF8.GetBytes(value);

			// Create a new instance of the RijndaelManaged
			// class to perform symmetric encryption.
			using (var aesAlg = Aes.Create("AesManaged"))
			{
				aesAlg.KeySize = 256;
				aesAlg.BlockSize = 128;

				var key = new Rfc2898DeriveBytes(saltBytes, saltBytes, 1000);
				aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
				aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

				// Create a encryptor to perform the stream transform.
				var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption.
				using (var msEncrypt = new MemoryStream())
				using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
				{
					csEncrypt.Write(valueBytes, 0, valueBytes.Length);
					csEncrypt.FlushFinalBlock();
					var encryptedBytes = msEncrypt.ToArray();

					// Return the encrypted data as a base64 string.
					return Convert.ToBase64String(encryptedBytes);
				}
			}
		}

		public static string Decrypt(string encryptedValue, string salt)
		{
			var saltBytes = Encoding.UTF8.GetBytes(salt);
			var encryptedBytes = Convert.FromBase64String(encryptedValue);

			// Create a new instance of the RijndaelManaged
			// class to perform symmetric decryption.
			using (var aesAlg = Aes.Create("AesManaged"))
			{
				aesAlg.KeySize = 256;
				aesAlg.BlockSize = 128;

				var key = new Rfc2898DeriveBytes(saltBytes, saltBytes, 1000);
				aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
				aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

				// Create a decryptor to perform the stream transform.
				var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for decryption.
				using (var msDecrypt = new MemoryStream(encryptedBytes))
				using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
				using (var srDecrypt = new StreamReader(csDecrypt))
				{
					// Read the decrypted bytes from the decrypting stream
					// and place them in a string.
					return srDecrypt.ReadToEnd();
				}
			}
		}
	}
}
