using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KolibSoft.Rooms.Core.Protocol;
using KolibSoft.Rooms.Core.Streams;

namespace KolibSoft.Rooms.Core.Services
{

    /// <summary>
    /// Generic base implementation of a Room service.
    /// </summary>
    public abstract class RoomService : IRoomService
    {

        /// <summary>
        /// Service options.
        /// </summary>
        public RoomServiceOptions Options { get; private set; }

        /// <summary>
        /// Logs the stream errors.
        /// </summary>
        public Action<string>? Logger { get; set; }

        /// <summary>
        /// Checks if the service is active.
        /// </summary>
        public bool IsRunning => _running;

        /// <summary>
        /// Gets the current queue messages.
        /// </summary>
        protected IEnumerable<MessageContext> Messages => _messages;

        /// <summary>
        /// Checks if the instance was disposed.
        /// </summary>
        protected bool IsDisposed => _disposed;

        /// <summary>
        /// Called when a stream message was received.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="message">Message read.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        protected abstract ValueTask OnReceiveAsync(IRoomStream stream, RoomMessage message, CancellationToken token);

        /// <summary>
        /// Listen for the incoming Room stream messages.
        /// </summary>
        /// <param name="stream">Stream to listen.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public virtual async ValueTask ListenAsync(IRoomStream stream, CancellationToken token = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service not running");
            try
            {
                var ttl = TimeSpan.FromSeconds(1);
                var stopwatch = new Stopwatch();
                var rate = 0L;
                stopwatch.Start();
                while (_running && stream.IsAlive)
                {
                    var message = await stream.ReadMessageAsync(token);
                    if (stopwatch.Elapsed >= ttl)
                    {
                        rate = 0;
                        stopwatch.Restart();
                    }
                    rate += message.Content.Length;
                    if (rate > Options.MaxStreamRate)
                        await Task.Delay(TimeSpan.FromSeconds(rate / Options.MaxStreamRate), token);
                    await OnReceiveAsync(stream, message, token);
                    if (!Messages.Any(x => x.Message.Content == message.Content))
                        await message.Content.DisposeAsync();
                }
            }
            catch (Exception error)
            {
                Logger?.Invoke($"Error receiving message: {error}");
            }
        }

        /// <summary>
        /// Called when a stream message will be send.
        /// </summary>
        /// <param name="stream">Target stream.</param>
        /// <param name="message">Message to send.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        protected virtual async ValueTask OnSendAsync(IRoomStream stream, RoomMessage message, CancellationToken token)
        {
            await stream.WriteMessageAsync(message, token);
        }

        /// <summary>
        /// Enqueue a stream message.
        /// </summary>
        /// <param name="stream">Target stream.</param>
        /// <param name="message">Message to send.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void Enqueue(IRoomStream stream, RoomMessage message)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running) throw new InvalidOperationException("Service not running");
            _messages = _messages.Enqueue(new MessageContext(stream, message));
        }

        /// <summary>
        /// Send the queue messages.
        /// </summary>
        private async void Transmit()
        {
            while (_running)
                if (_messages.Any())
                {
                    _messages = _messages.Dequeue(out MessageContext context);
                    try
                    {
                        await OnSendAsync(context.Stream, context.Message, default);
                        if (!Messages.Any(x => x.Message.Content == context.Message.Content))
                            await context.Message.Content.DisposeAsync();
                    }
                    catch (Exception error)
                    {
                        Logger?.Invoke($"Error sending message: {error}");
                    }
                }
                else await Task.Delay(100);
        }

        /// <summary>
        /// Called when the service starts.
        /// </summary>
        protected virtual void OnStart() => Transmit();

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (!_running)
            {
                _running = true;
                OnStart();
            }
        }

        /// <summary>
        /// Called when the service stops.
        /// </summary>
        protected virtual void OnStop() { }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Stop()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RoomService));
            if (_running)
            {
                _running = false;
                OnStop();
            }
        }

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns></returns>
        protected virtual ValueTask OnDisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                _running = false;
                _disposed = true;
            }
            return ValueTask.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await OnDisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async void Dispose()
        {
            await OnDisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initialize the Room service.
        /// </summary>
        /// <param name="options">Service options.</param>
        protected RoomService(RoomServiceOptions? options = null)
        {
            Options = options ?? new RoomServiceOptions();
        }

        /// <summary>
        /// Internal message queue.
        /// </summary>
        private ImmutableQueue<MessageContext> _messages = ImmutableQueue.Create<MessageContext>();

        /// <summary>
        /// Internal running flag.
        /// </summary>
        private bool _running = false;

        /// <summary>
        /// Internal dispose flag.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Internal queue message context.
        /// </summary>
        protected readonly struct MessageContext
        {

            /// <summary>
            /// Target stream.
            /// </summary>
            public readonly IRoomStream Stream;

            /// <summary>
            /// Message to send.
            /// </summary>
            public readonly RoomMessage Message;

            /// <summary>
            /// Creates a new queue message context.
            /// </summary>
            /// <param name="stream">Target stream.</param>
            /// <param name="message">Message to send.</param>
            public MessageContext(IRoomStream stream, RoomMessage message)
            {
                Stream = stream;
                Message = message;
            }

        }

    }

}