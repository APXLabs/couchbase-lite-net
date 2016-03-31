using System;
using System.IO;
using Javax.Crypto;

namespace Couchbase.Lite.Store
{
    /// <summary>
    /// Stream Wrapper for CipherOutputStream
    /// </summary>
    internal class JavaCipherOutputStream : Stream
    {
        private CipherOutputStream _stream;
        private long _bytesRead;

        /// <summary>
        /// Creates a wrapper for a CipherOutputStream
        /// </summary>
        /// <param name="stream">Stream to wrap</param>
        public JavaCipherOutputStream(CipherOutputStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            _stream = stream;
        }

        /// <summary>
        /// Always returns false
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Always returns false
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true if the stream is available for writing
        /// </summary>
        public override bool CanWrite
        {
            get { return _stream != null; }
        }

        /// <summary>
        /// Returns the current length of the stream
        /// </summary>
        public override long Length
        {
            get { return _bytesRead; }
        }

        /// <summary>
        /// Returns the write position
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if attempting to set a position</exception>
        public override long Position
        {
            get { return _bytesRead; }
            set
            {
                throw new NotSupportedException("This stream does not support seeking");
            }
        }

        /// <summary>
        /// Flushes the stream
        /// </summary>
        public override void Flush()
        {
            if (_stream == null)
                throw new ObjectDisposedException("stream", "Stream has been closed");

            _stream.Flush();
        }

        /// <summary>
        /// Read bytes. Throws since this class does not support reading.
        /// </summary>
        /// <param name="buffer">Unused</param>
        /// <param name="offset">Unused</param>
        /// <param name="count">Unused</param>
        /// <returns>N/A</returns>
        /// <exception cref="NotSupportedException">Always throws</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Read only stream");
        }

        /// <summary>
        /// Seek the stream. This class does not support seeking. Will always throw
        /// </summary>
        /// <param name="offset">Unused</param>
        /// <param name="origin">Unused</param>
        /// <returns>N/A</returns>
        /// <exception cref="NotSupportedException">Always throws</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Stream is not seekable");
        }

        /// <summary>
        /// Sets length of stream. This class does not support setting the length. Always throws
        /// </summary>
        /// <param name="value">Unused</param>
        /// <exception cref="NotSupportedException">Always throws</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("Read only stream");
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="ObjectDisposedException">Write was called after the stream has closed</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_stream == null)
                throw new ObjectDisposedException("stream", "Stream has been closed");

            _bytesRead += count;
            _stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Releases all resources used by the stream
        /// </summary>
        /// <param name="disposing">True to dispose all resources</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_stream != null)
                {
                    _stream.Flush();
                    _stream.Close();
                    _stream = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}