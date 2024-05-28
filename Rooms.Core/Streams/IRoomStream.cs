using System;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;

namespace KolibSoft.Rooms.Core.Streams
{

    /// <summary>
    /// Interface to read and write Room messages asynchronously.
    /// </summary>
    public interface IRoomStream : IAsyncDisposable, IDisposable
    {

        /// <summary>
        /// Checks if the stream is ready for read or write messages.
        /// </summary>
        public bool IsAlive { get; }

        /// <summary>
        /// Read a Room message.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Room message.</returns>
        public ValueTask<RoomMessage> ReadMessageAsync(CancellationToken token = default);

        /// <summary>
        /// Write a Room message.
        /// </summary>
        /// <param name="message">Room message.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public ValueTask WriteMessageAsync(RoomMessage message, CancellationToken token = default);

    }
}