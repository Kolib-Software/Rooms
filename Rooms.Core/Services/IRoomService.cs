using System;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{

    /// <summary>
    /// Interface to listen and enqueue Room stream messages.
    /// </summary>
    public interface IRoomService : IAsyncDisposable, IDisposable
    {

        /// <summary>
        /// Checks if the service is active.
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// Listen for the incoming Room stream messages.
        /// </summary>
        /// <param name="stream">Stream to listen.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default);

        /// <summary>
        /// Enqueue a outcoming Room stream message.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="message"></param>
        public void Enqueue(IRoomStream stream, RoomMessage message);

        /// <summary>
        /// Starts the service.
        /// </summary>
        public void Start();

        /// <summary>
        /// Stops the service.
        /// </summary>
        public void Stop();

    }

}