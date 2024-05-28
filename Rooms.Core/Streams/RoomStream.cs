using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Streams
{

    /// <summary>
    /// Generic base implementation of a Room stream.
    /// </summary>
    public abstract class RoomStream : IRoomStream
    {

        /// <summary>
        /// Stream options.
        /// </summary>
        public RoomStreamOptions Options { get; private set; } = new RoomStreamOptions();

        /// <summary>
        /// Checks if the stream is ready for read or write messages.
        /// </summary>
        public abstract bool IsAlive { get; }

        /// <summary>
        /// Checks if the instance was disposed.
        /// </summary>
        protected bool IsDisposed => _disposed;

        /// <summary>
        /// Read a chunk of data from the underline implementation.
        /// </summary>
        /// <param name="buffer">Chunk to store the read data.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Amount of bytes read.</returns>
        protected abstract ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default);

        /// <summary>
        /// Get the next available chunk of data for read operations.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Available chunk or `default` otherwise.</returns>
        private async ValueTask<ReadOnlyMemory<byte>> GetChunkAsync(CancellationToken token = default)
        {
            if (_position == _length)
            {
                _position = 0;
                _length = await ReadAsync(_readBuffer, token);
                if (_length < 1)
                    return default;
            }
            var slice = _readBuffer.AsMemory().Slice(_position, _length - _position);
            return slice;
        }

        /// <summary>
        /// Read a verb.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Room verb.</returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask<RoomVerb> ReadVerbAsync(CancellationToken token)
        {
            _data.SetLength(0);
            var done = false;
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room verb broken");
                var length = RoomDataUtils.ScanWord(chunk.Span);
                if (length < chunk.Length)
                    length += (done = RoomDataUtils.IsBlank(chunk.Span[length])) ? 1 : 0;
                _position += length;
                if (_data.Length + length > Options.MaxVerbLength) throw new IOException("Room verb too large");
                if (_position < _length || done)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length - 1));
                        var verb = new RoomVerb(_data.ToArray());
                        return verb;
                    }
                    if (length > 0)
                    {
                        var verb = new RoomVerb(chunk.Slice(0, length - 1).ToArray());
                        return verb;
                    }
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        /// <summary>
        /// Read a channel.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Room channel.</returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask<RoomChannel> ReadChannelAsync(CancellationToken token)
        {
            _data.SetLength(0);
            var done = false;
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room channel broken");
                var length = RoomDataUtils.IsSign(chunk.Span[0]) ? 1 : 0;
                if (length < chunk.Length)
                    length += RoomDataUtils.ScanHexadecimal(chunk.Slice(length).Span);
                if (length < chunk.Length)
                    length += (done = RoomDataUtils.IsBlank(chunk.Span[length])) ? 1 : 0;
                _position += length;
                if (_data.Length + length > Options.MaxChannelLength) throw new IOException("Room channel too large");
                if (_position < _length || done)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length - 1));
                        var channel = new RoomChannel(_data.ToArray());
                        return channel;
                    }
                    if (length > 0)
                    {
                        var channel = new RoomChannel(chunk.Slice(0, length - 1).ToArray());
                        return channel;
                    }
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        /// <summary>
        /// Read a count.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Room count.</returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask<RoomCount> ReadCountAsync(CancellationToken token)
        {
            _data.SetLength(0);
            var done = false;
            while (true)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room count broken");
                var length = RoomDataUtils.ScanDigit(chunk.Span);
                if (length < chunk.Length)
                    length += (done = RoomDataUtils.IsBlank(chunk.Span[length])) ? 1 : 0;
                _position += length;
                if (_data.Length + length > Options.MaxCountLength) throw new IOException("Room count too large");
                if (_position < _length || done)
                {
                    if (_data.Length > 0)
                    {
                        await _data.WriteAsync(chunk.Slice(0, length - 1));
                        var count = new RoomCount(_data.ToArray());
                        return count;
                    }
                    if (length > 0)
                    {
                        var count = new RoomCount(chunk.Slice(0, length - 1).ToArray());
                        return count;
                    }
                    return default;
                }
                await _data.WriteAsync(chunk.Slice(0, length));
            }
        }

        /// <summary>
        /// Read a content.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Room content.</returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask<Stream> ReadContentAsync(CancellationToken token)
        {
            var count = await ReadCountAsync(token);
            var _count = (long)count;
            if (_count == 0) return Stream.Null;
            if (_count > Options.MaxContentLength) throw new IOException("Room content too large");
            var content = Options.CreateContentStream(_count);
            var index = 0L;
            while (index < _count)
            {
                var chunk = await GetChunkAsync(token);
                if (_length < 1) throw new IOException("Room content broken");
                var length = (int)Math.Min(chunk.Length, _count - index);
                await content.WriteAsync(chunk.Slice(0, length), token);
                index += length;
                _position += length;
            }
            content.Seek(0, SeekOrigin.Begin);
            return content;
        }

        /// <summary>
        /// Read a message.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Room message.</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async ValueTask<RoomMessage> ReadMessageAsync(CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            var verb = await ReadVerbAsync(token);
            var channel = await ReadChannelAsync(token);
            var content = await ReadContentAsync(token);
            var message = new RoomMessage
            {
                Verb = verb.ToString(),
                Channel = (int)channel,
                Content = content
            };
            return message;
        }

        /// <summary>
        /// Write a chunk of data into the underline implementation.
        /// </summary>
        /// <param name="buffer">Chunk data to write.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Amount of bytes written.</returns>
        protected abstract ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token);

        /// <summary>
        /// Write a verb.
        /// </summary>
        /// <param name="verb">Room verb.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask WriteVerbAsync(RoomVerb verb, CancellationToken token)
        {
            if (verb.Length > Options.MaxVerbLength) throw new IOException("Room verb too large");
            var index = 0;
            while (index < verb.Length)
            {
                var length = await WriteAsync(verb.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room verb broken");
                index += length;
            }
            await WriteAsync(Blank, token);
        }

        /// <summary>
        /// Write a channel.
        /// </summary>
        /// <param name="channel">Room channel.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask WriteChannelAsync(RoomChannel channel, CancellationToken token)
        {
            if (channel.Length > Options.MaxChannelLength) throw new IOException("Room channel too large");
            var index = 0;
            while (index < channel.Length)
            {
                var length = await WriteAsync(channel.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room channel broken");
                index += length;
            }
            await WriteAsync(Blank, token);
        }

        /// <summary>
        /// Write a count.
        /// </summary>
        /// <param name="count">Room count.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask WriteCountAsync(RoomCount count, CancellationToken token)
        {
            if (count.Length > Options.MaxCountLength) throw new IOException("Room count too large");
            var index = 0;
            while (index < count.Length)
            {
                var length = await WriteAsync(count.Data.Slice(index), token);
                if (length < 1) throw new IOException("Room count broken");
                index += length;
            }
            await WriteAsync(Blank, token);
        }

        /// <summary>
        /// Write a content.
        /// </summary>
        /// <param name="content">Room content.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        private async ValueTask WriteContentAsync(Stream content, CancellationToken token)
        {
            if (content.Length > Options.MaxContentLength) throw new IOException("Room content too large");
            var count = (RoomCount)content.Length;
            await WriteCountAsync(count, token);
            content.Seek(0, SeekOrigin.Begin);
            var index = 0L;
            while (index < content.Length)
            {
                var _count = await content.ReadAsync(_writeBuffer, token);
                var slice = _writeBuffer.Slice(0, _count);
                var _index = 0;
                while (_index < slice.Count)
                {
                    var length = await WriteAsync(slice.Slice(_index), token);
                    if (length < 1) throw new IOException("Room content broken");
                    _index += length;
                }
                index += slice.Count;
            }
        }

        /// <summary>
        /// Write a message.
        /// </summary>
        /// <param name="message">Room message.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public async ValueTask WriteMessageAsync(RoomMessage message, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomStream));
            var verb = RoomVerb.Parse(message.Verb);
            var channel = (RoomChannel)message.Channel;
            var content = message.Content;
            await WriteVerbAsync(verb, token);
            await WriteChannelAsync(channel, token);
            await WriteContentAsync(content, token);
        }

        /// <summary>
        /// Dipose implementation.
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns></returns>
        protected virtual async ValueTask OnDisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    await _data.DisposeAsync();
                _readBuffer = default;
                _writeBuffer = default;
                _disposed = true;
            }
        }

        public void Dispose()
        {
            _ = OnDisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await OnDisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize the Room stream.
        /// </summary>
        /// <param name="readBuffer">Read buffer.</param>
        /// <param name="writeBuffer">Write buffer.</param>
        /// <param name="options">Strea options.</param>
        protected RoomStream(ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer, RoomStreamOptions? options = null)
        {
            _readBuffer = readBuffer;
            _writeBuffer = writeBuffer;
            Options = options ?? new RoomStreamOptions();
        }

        /// <summary>
        /// Initialize the Room stream.
        /// </summary>
        /// <param name="options">Stream options.</param>
        protected RoomStream(RoomStreamOptions? options = null)
        {
            Options = options ?? new RoomStreamOptions();
            _readBuffer = new byte[Options.ReadBuffering];
            _writeBuffer = new byte[Options.WriteBuffering];
        }

        /// <summary>
        /// Internal message buffer.
        /// </summary>
        private MemoryStream _data = new MemoryStream();

        /// <summary>
        /// Internal read buffer.
        /// </summary>
        private ArraySegment<byte> _readBuffer = default;

        /// <summary>
        /// Internal write buffer.
        /// </summary>
        private ArraySegment<byte> _writeBuffer = default;

        /// <summary>
        /// Internal reading position.
        /// </summary>
        private int _position = 0;

        /// <summary>
        /// Internal reading length.
        /// </summary>
        private int _length = 0;

        /// <summary>
        /// Internal dispose flag.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Internal blank data.
        /// </summary>
        private static readonly byte[] Blank = Encoding.UTF8.GetBytes(" ");

    }

}