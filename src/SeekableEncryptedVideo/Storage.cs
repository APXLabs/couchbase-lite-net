using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Store;

namespace SeekableEncryptedVideo
{
    /// <summary>
    /// Handles the storage and retrival of items
    /// </summary>
    public class Storage
    {
        private Database _db = null;

        /// <summary>
        /// Constructs a Storage
        /// </summary>
        public Storage()
        {

            var options = new DatabaseOptions
            {
                EncryptionKey = new SymmetricKey("fooo"),
                Create = true,
                StorageType = DatabaseOptions.FORESTDB_STORAGE
            };


            _db = Manager.SharedInstance.OpenDatabase("test", options);
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
    }
}