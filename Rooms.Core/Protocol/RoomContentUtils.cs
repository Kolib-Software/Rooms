using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Utility functions to read and write data into streams.
    /// </summary>
    public static class RoomContentUtils
    {

        /// <summary>
        /// Read the stream content as text.
        /// </summary>
        /// <param name="content">Stream to read.</param>
        /// <param name="encoding">Text encoding (UTF-8 by default). </param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Text content</returns>
        public static async ValueTask<string> ReadAsTextAsync(this Stream content, Encoding? encoding = null, CancellationToken token = default)
        {
            var reader = new StreamReader(content, encoding ?? Encoding.UTF8);
            var text = await reader.ReadToEndAsync();
            return text;
        }

        /// <summary>
        /// Read the stream content as json.
        /// </summary>
        /// <typeparam name="T">Json type.</typeparam>
        /// <param name="content">Stream to read.</param>
        /// <param name="options">Json serialization options.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Json object.</returns>
        public static async ValueTask<T?> ReadAsJsonAsync<T>(this Stream content, JsonSerializerOptions? options = null, CancellationToken token = default)
        {
            var json = await JsonSerializer.DeserializeAsync<T>(content, options, token);
            return json;
        }

        /// <summary>
        /// Read the stream as file.
        /// </summary>
        /// <param name="content">Stream to read.</param>
        /// <param name="path">File path to store stream content.</param>
        /// <returns>File stream.</returns>
        public static async ValueTask<FileStream> ReadAsFileAsync(this Stream content, string path)
        {
            var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            await content.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Create a stream with the text content.
        /// </summary>
        /// <param name="text">Text content.</param>
        /// <param name="encoding">Text encoding (UTF-8 by default).</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Memory stream.</returns>
        public static ValueTask<Stream> CreateAsTextAsync(string text, Encoding? encoding = null, CancellationToken token = default)
        {
            encoding ??= Encoding.UTF8;
            var stream = new MemoryStream(encoding.GetBytes(text));
            stream.Seek(0, SeekOrigin.Begin);
            return ValueTask.FromResult<Stream>(stream);
        }

        /// <summary>
        /// Create a stream with the json content.
        /// </summary>
        /// <typeparam name="T">Json type.</typeparam>
        /// <param name="json">Json object.</param>
        /// <param name="options">Json serialization options.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Memory stream.</returns>
        public static async ValueTask<Stream> CreateAsJsonAsync<T>(T? json, JsonSerializerOptions? options = null, CancellationToken token = default)
        {
            var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, json, options, token);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Create a stream with the file content.
        /// </summary>
        /// <param name="path">File path to load stream content.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>File stream.</returns>
        public static ValueTask<Stream> CreateAsFileAsync(string path, CancellationToken token = default)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            stream.Seek(0, SeekOrigin.Begin);
            return ValueTask.FromResult<Stream>(stream);
        }
    }

}