using System;
using System.IO;

namespace KolibSoft.Rooms.Core.Streams
{

    /// <summary>
    /// Room stream options.
    /// </summary>
    public sealed class RoomStreamOptions
    {

        /// <summary>
        /// Read buffer size.
        /// </summary>
        public int ReadBuffering { get; set; } = DefaultReadBuffering;

        /// <summary>
        /// Write buffer size.
        /// </summary>
        public int WriteBuffering { get; set; } = DefaultWriteBuffering;

        /// <summary>
        /// Max Room message verb length.
        /// </summary>
        public int MaxVerbLength { get; set; } = DefaultMaxVerbLength;

        /// <summary>
        /// Max Room message channel length.
        /// </summary>
        public int MaxChannelLength { get; set; } = DefaultMaxChannelLength;

        /// <summary>
        /// Max Room message count length.
        /// </summary>
        public int MaxCountLength { get; set; } = DefaultMaxCountLength;

        /// <summary>
        /// Max Room message content length.
        /// </summary>
        public int MaxContentLength { get; set; } = DefaultMaxContentLength;

        /// <summary>
        /// Max memory stream size before switch to file stream.
        /// </summary>
        public int MaxFastBuffering { get; set; } = DefaultMaxFastBuffering;

        /// <summary>
        /// Folder path to store temp file streams.
        /// </summary>
        public string TempContentFolderPath { get; set; } = DefaultTempContentFolderPath;

        /// <summary>
        /// Creates a memory stream or a file stream to manage content read/write operations.
        /// </summary>
        /// <param name="count">Content length.</param>
        /// <returns>Memory stream or file stream.</returns>
        public Stream CreateContentStream(long count)
        {
            if (count < 1) return Stream.Null;
            if (count <= MaxFastBuffering) return new MemoryStream((int)count);
            var path = Path.Combine(TempContentFolderPath, $"{DateTime.UtcNow.Ticks}");
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }

        /// <summary>
        /// Default read buffer size.
        /// </summary>
        public const int DefaultReadBuffering = 1024;

        /// <summary>
        /// Default write buffer size.
        /// </summary>
        public const int DefaultWriteBuffering = 1024;

        /// <summary>
        /// Default max Room verb length.
        /// </summary>
        public const int DefaultMaxVerbLength = 128;

        /// <summary>
        /// Default max Room channel length.
        /// </summary>
        public const int DefaultMaxChannelLength = 32;

        /// <summary>
        /// Default max Room count length.
        /// </summary>
        public const int DefaultMaxCountLength = 32;

        /// <summary>
        /// Default max Room content length.
        /// </summary>
        public const int DefaultMaxContentLength = 4 * 1024 * 1024;

        /// <summary>
        /// Default max memory stream size before switch to file stream.
        /// </summary>
        public const int DefaultMaxFastBuffering = 1024 * 1024;

        /// <summary>
        /// Deafult folder path to store temp file streams.
        /// </summary>
        public static readonly string DefaultTempContentFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), "Content");

    }

}