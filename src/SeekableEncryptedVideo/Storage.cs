using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Android.Store;
using Couchbase.Lite.Store;

namespace SeekableEncryptedVideo
{
    /// <summary>
    /// Handles the storage and retrival of items
    /// </summary>
    public class Storage
    {
        private Database _db = null;
        private const string DB_NAME = "test";
        private const string PASSWORD = "password";
        private ISymmetricKey _key;

        /// <summary>
        /// Constructs a Storage
        /// </summary>
        public Storage()
        {
            _key = new SeekableSymmetricKey(PASSWORD);

            var options = new DatabaseOptions
            {
                EncryptionKey = _key,
                Create = true,
                StorageType = DatabaseOptions.SQLITE_STORAGE
            };

            //_db = Manager.SharedInstance.GetDatabase(DB_NAME);
            _db = Manager.SharedInstance.OpenDatabase(DB_NAME, options);
        }

        /// <summary>
        /// Inserts a video attachment
        /// </summary>
        /// <param name="videoStream"> the video to insert</param>
        /// <returns> true when the operation has completed</returns>
        public Task<bool> InsertVideo(Stream videoStream)
        {
            Task<bool> task = Task.Run(() =>
            {

                var doc = _db.GetDocument("video");

                var rev = doc.CurrentRevision;
                if (rev != null)
                {
                    return true;
                }
                rev = doc.PutProperties(new Dictionary<string, object>());
                var newRev = rev.CreateRevision();
                newRev.SetAttachment("video.mp4", "video/mp4", videoStream);
                var savedRev = newRev.Save();
                return savedRev != null;
            });
            return task;
        }

        /// <summary>
        /// Gets a stream for the video attachment
        /// </summary>
        /// <returns></returns>
        public Stream GetVideo()
        {
            var doc = _db.GetDocument("video");
            var rev = doc.CurrentRevision;
            var att = rev.GetAttachment("video.mp4");
            return att.ContentStream;
        }

        public byte[] Encrypt(byte[] data)
        {
            return _key.EncryptData(data);
        }

        public byte[] Decrypt(byte[] data)
        {
            return _key.DecryptData(data);
        }
    }
}