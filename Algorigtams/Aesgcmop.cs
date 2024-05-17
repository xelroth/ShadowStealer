using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Backuper.payload.Components.Algorigtams
{
    //AES GCM from https://github.com/dvsekhvalnov/jose-jwt
    internal class AesGcm
    {
        public byte[] Decrypt(byte[] key, byte[] iv, byte[] aad, byte[] cipherText, byte[] authTag)
        {
            IntPtr hAlg = OpenAlgorithmProvider(BCryptop.BCRYPT_AES_ALGORITHM, BCryptop.MS_PRIMITIVE_PROVIDER, BCryptop.BCRYPT_CHAIN_MODE_GCM);
            IntPtr hKey, keyDataBuffer = ImportKey(hAlg, key, out hKey);

            byte[] plainText;

            BCryptop.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO authInfo = new BCryptop.BCRYPT_AUTHENTICATED_CIPHER_MODE_INFO(iv, aad, authTag);
            using (authInfo)
            {
                byte[] ivData = new byte[MaxAuthTagSize(hAlg)];

                int plainTextSize = 0;

                uint status = BCryptop.BCryptDecrypt(hKey, cipherText, cipherText.Length, ref authInfo, ivData, ivData.Length, null, 0, ref plainTextSize, 0x0);

                if (status != BCryptop.ERROR_SUCCESS)
                    throw new CryptographicException(string.Format("BCrypt.BCryptDecrypt() (get size) failed with status code: {0}", status));

                plainText = new byte[plainTextSize];

                status = BCryptop.BCryptDecrypt(hKey, cipherText, cipherText.Length, ref authInfo, ivData, ivData.Length, plainText, plainText.Length, ref plainTextSize, 0x0);

                if (status == BCryptop.STATUS_AUTH_TAG_MISMATCH)
                    throw new CryptographicException("BCrypt.BCryptDecrypt(): authentication tag mismatch");

                if (status != BCryptop.ERROR_SUCCESS)
                    throw new CryptographicException(string.Format("BCrypt.BCryptDecrypt() failed with status code:{0}", status));
            }

            BCryptop.BCryptDestroyKey(hKey);
            Marshal.FreeHGlobal(keyDataBuffer);
            BCryptop.BCryptCloseAlgorithmProvider(hAlg, 0x0);

            return plainText;
        }

        private int MaxAuthTagSize(IntPtr hAlg)
        {
            byte[] tagLengthsValue = GetProperty(hAlg, BCryptop.BCRYPT_AUTH_TAG_LENGTH);

            return BitConverter.ToInt32(new[] { tagLengthsValue[4], tagLengthsValue[5], tagLengthsValue[6], tagLengthsValue[7] }, 0);
        }

        private IntPtr OpenAlgorithmProvider(string alg, string provider, string chainingMode)
        {
            IntPtr hAlg = IntPtr.Zero;

            uint status = BCryptop.BCryptOpenAlgorithmProvider(out hAlg, alg, provider, 0x0);

            if (status != BCryptop.ERROR_SUCCESS)
                throw new CryptographicException(string.Format("BCrypt.BCryptOpenAlgorithmProvider() failed with status code:{0}", status));

            byte[] chainMode = Encoding.Unicode.GetBytes(chainingMode);
            status = BCryptop.BCryptSetAlgorithmProperty(hAlg, BCryptop.BCRYPT_CHAINING_MODE, chainMode, chainMode.Length, 0x0);

            if (status != BCryptop.ERROR_SUCCESS)
                throw new CryptographicException(string.Format("BCrypt.BCryptSetAlgorithmProperty(BCrypt.BCRYPT_CHAINING_MODE, BCrypt.BCRYPT_CHAIN_MODE_GCM) failed with status code:{0}", status));

            return hAlg;
        }

        private IntPtr ImportKey(IntPtr hAlg, byte[] key, out IntPtr hKey)
        {
            byte[] objLength = GetProperty(hAlg, BCryptop.BCRYPT_OBJECT_LENGTH);

            int keyDataSize = BitConverter.ToInt32(objLength, 0);

            IntPtr keyDataBuffer = Marshal.AllocHGlobal(keyDataSize);

            byte[] keyBlob = Concat(BCryptop.BCRYPT_KEY_DATA_BLOB_MAGIC, BitConverter.GetBytes(0x1), BitConverter.GetBytes(key.Length), key);

            uint status = BCryptop.BCryptImportKey(hAlg, IntPtr.Zero, BCryptop.BCRYPT_KEY_DATA_BLOB, out hKey, keyDataBuffer, keyDataSize, keyBlob, keyBlob.Length, 0x0);

            if (status != BCryptop.ERROR_SUCCESS)
                throw new CryptographicException(string.Format("BCrypt.BCryptImportKey() failed with status code:{0}", status));

            return keyDataBuffer;
        }

        private byte[] GetProperty(IntPtr hAlg, string name)
        {
            int size = 0;

            uint status = BCryptop.BCryptGetProperty(hAlg, name, null, 0, ref size, 0x0);

            if (status != BCryptop.ERROR_SUCCESS)
                throw new CryptographicException(string.Format("BCrypt.BCryptGetProperty() (get size) failed with status code:{0}", status));

            byte[] value = new byte[size];

            status = BCryptop.BCryptGetProperty(hAlg, name, value, value.Length, ref size, 0x0);

            if (status != BCryptop.ERROR_SUCCESS)
                throw new CryptographicException(string.Format("BCrypt.BCryptGetProperty() failed with status code:{0}", status));

            return value;
        }

        public byte[] Concat(params byte[][] arrays)
        {
            int len = 0;

            foreach (byte[] array in arrays)
            {
                if (array == null)
                    continue;
                len += array.Length;
            }

            byte[] result = new byte[len - 1 + 1];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                if (array == null)
                    continue;
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }
    }
}