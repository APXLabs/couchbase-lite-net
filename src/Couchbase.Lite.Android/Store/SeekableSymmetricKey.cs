using System;
using System.Collections.Generic;
using System.IO;
using Couchbase.Lite.Store;
using Javax.Crypto;
using Javax.Crypto.Spec;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Couchbase.Lite.Android.Store
{

    /// <summary>
    /// Encryption strategy for the cache that will use a version of AES for seekable media
    /// </summary>
    /// <remarks>Uses AES/CTR/NoPadding</remarks>
    public class SeekableSymmetricKey : ISymmetricKey
    {
        private const string CRYPTO_ALGORITHM_NAME = "AES/CTR/NoPadding";
        private const int INITIALIZATION_VECTOR_LENGTH = 16;

        private readonly byte[] _password;
        private readonly byte[] _initializationVector;

        #region Properties

        /// <summary>
        /// The SymmetricKey's key data; can be used to reconstitute it.
        /// </summary>
        public byte[] KeyData
        {
            get
            {
                return _password;
            }
        }

        /// <summary>
        /// The key data encoded as hex.
        /// </summary>
        public string HexData
        {
            get
            {
                return BitConverter.ToString(KeyData).Replace("-", String.Empty).ToLower();
            }
        }

        #endregion


        /// <summary>
        /// Constructs an AesStreamingEncryptionStrategy
        /// </summary>
        /// <param name="password">Password for crypto</param>
        /// <param name="initializationVector">Unique initialization vector for this session</param>
        public SeekableSymmetricKey(byte[] password, byte[] initializationVector)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            if (password.Length != 16 && password.Length != 24 && password.Length != 32)
                throw new ArgumentException("Password must be 128/192/256 bits (16/24/32 bytes)", "password");

            if (initializationVector == null)
                throw new ArgumentNullException("initializationVector");

            if (initializationVector.Length != INITIALIZATION_VECTOR_LENGTH)
                throw new ArgumentException(String.Format("Initialization vector must be 16 bytes and is {0}", initializationVector.Length), "initializationVector");

            _password = password;
            _initializationVector = initializationVector;
        }


        /// <summary>
        /// Encrypts a data blob.
        /// </summary>
        public byte[] EncryptData(byte[] data)
        {
            byte[] encrypted = null;

            using (var ms = new MemoryStream())
            using (var cs = CreateStream(ms))
            {
                cs.Write(data, 0, data.Length);
                cs.Flush();
                encrypted = ms.ToArray();
            }

            return encrypted;
        }

        /// <summary>
        /// Decrypts data encoded by encryptData.
        /// </summary>
        public byte[] DecryptData(byte[] encryptedData)
        {
            var buffer = new List<byte>();
            using (var ms = new MemoryStream(encryptedData))
            using (var cs = DecryptStream(ms))
            {
                int next;
                while ((next = cs.ReadByte()) != -1)
                {
                    buffer.Add((byte)next);
                }
            }

            return buffer.ToArray();
        }

        /// <summary>
        /// Retreives cipher algorithm
        /// </summary>
        /// <returns>Cipher algorithm to use for media playback</returns>
        public string GetAlgorithm()
        {
            return CRYPTO_ALGORITHM_NAME;
        }

        /// <summary>
        /// Creates a stream that will encrypt data before passing it to the underlying destination stream
        /// </summary>
        /// <param name="destinationStream"> the stream where encrypted data will be delivered</param>
        /// <returns> the encryption wrapped destination stream</returns>
        public Stream CreateStream(Stream destinationStream)
        {
            if (destinationStream == null)
                throw new ArgumentNullException("destinationStream");
            Cipher cipher = Cipher.GetInstance(CRYPTO_ALGORITHM_NAME);
            IvParameterSpec ivSpec = new IvParameterSpec(_initializationVector);
            cipher.Init(Javax.Crypto.CipherMode.EncryptMode, new SecretKeySpec(_password, CRYPTO_ALGORITHM_NAME), ivSpec);

            CipherOutputStream outputStream = new CipherOutputStream(destinationStream, cipher);
            return new JavaCipherOutputStream(outputStream);
        }

        /// <summary>
        /// Creates a stream that will decrypt data from the source
        /// </summary>
        /// <param name="sourceStream"> the stream where encrypted data will be read from</param>
        /// <returns> the decrypted wrapped source stream</returns>
        public Stream DecryptStream(Stream sourceStream)
        {

            if (sourceStream == null)
                throw new ArgumentNullException("sourceStream");

            IBufferedCipher cipher = CipherUtilities.GetCipher(CRYPTO_ALGORITHM_NAME);
            ICipherParameters cipherParams = new ParametersWithIV(new KeyParameter(_password), _initializationVector);
            cipher.Init(false, cipherParams);

            return new CipherStream(sourceStream, cipher, null);
        }
    }
}