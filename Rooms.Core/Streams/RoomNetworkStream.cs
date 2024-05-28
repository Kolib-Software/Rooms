using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Streams
{

    /// <summary>
    /// Tcp client based Room stream implementation.
    /// </summary>
    public class RoomNetworkStream : RoomStream
    {

        /// <summary>
        /// Tcp client.
        /// </summary>
        public TcpClient Client { get; private set; }

        public override bool IsAlive => !IsDisposed && Client.Connected;

        protected override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token)
        {
            if (!Client.Connected) return 0;
            var stream = Client.GetStream();
            var result = await stream.ReadAsync(buffer, token);
            return result;
        }

        protected override async ValueTask<int> WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token)
        {
            if (!Client.Connected) return 0;
            var stream = Client.GetStream();
            await stream.WriteAsync(buffer, token);
            return buffer.Length;
        }

        /// <summary>
        /// Creates a new Tcp based Room stream.
        /// </summary>
        /// <param name="client">Tcp client.</param>
        /// <param name="readBuffer">Read buffer.</param>
        /// <param name="writeBuffer">Write buffer.</param>
        /// <param name="options">Stream options.</param>
        public RoomNetworkStream(TcpClient client, ArraySegment<byte> readBuffer, ArraySegment<byte> writeBuffer, RoomStreamOptions? options = null) : base(readBuffer, writeBuffer, options) => Client = client;

        /// <summary>
        /// Creates a new Tcp based Room stream.
        /// </summary>
        /// <param name="client">Tcp client.</param>
        /// <param name="options">Stream options.</param>
        public RoomNetworkStream(TcpClient client, RoomStreamOptions? options = null) : base(options) => Client = client;

    }

}
