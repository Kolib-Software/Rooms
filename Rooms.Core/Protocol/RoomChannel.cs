using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Room Channel UTF-8 text wrapper.
    /// </summary>
    public readonly struct RoomChannel
    {

        /// <summary>
        /// UTF-8 text data.
        /// </summary>
        public ReadOnlyMemory<byte> Data => _data;

        /// <summary>
        /// Data length.
        /// </summary>
        public int Length => _data.Count;

        /// <summary>
        /// </summary>
        /// <returns>String content representation.</returns>
        public override string ToString() => $"{Encoding.UTF8.GetString(_data)}";

        /// <summary>
        /// Verify if the internal data is valid.
        /// </summary>
        /// <returns>`true` if valid `false` otherwise.</returns>
        public bool Validate() => Verify(_data);

        /// <summary>
        /// Create a new channel with the specified data without validate it.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        public RoomChannel(ArraySegment<byte> data) => _data = data;

        /// <summary>
        /// Internal data.
        /// </summary>
        private readonly ArraySegment<byte> _data;

        /// <summary>
        /// Checks if the specified data is a valid channel.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <returns>`true` if data is a valid channel representation `false` otherwise.</returns>
        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2 || !RoomDataUtils.IsSign(data[0])) return false;
            var index = data.Slice(1).ScanHexadecimal() + 1;
            return index == data.Length;
        }

        /// <summary>
        /// Checks if the specified data is a valid channel.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <returns>`true` if data is a valid channel representation `false` otherwise.</returns>
        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length < 2 || !RoomDataUtils.IsSign(data[0])) return false;
            var index = data.Slice(1).ScanHexadecimal() + 1;
            return index == data.Length;
        }

        /// <summary>
        /// Attempts to parse the specified data into a channel.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="channel">Room channel.</param>
        /// <returns>`true` on success `false` otherwise.</returns>
        public static bool TryParse(ReadOnlySpan<byte> data, out RoomChannel channel)
        {
            if (Verify(data))
            {
                channel = new RoomChannel(data.ToArray());
                return true;
            }
            channel = default;
            return false;
        }

        /// <summary>
        /// Attempts to parse the specified data into a channel.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <param name="channel">Room channel.</param>
        /// <returns>`true` on success `false` otherwise.</returns>
        public static bool TryParse(ReadOnlySpan<char> data, out RoomChannel channel)
        {
            if (Verify(data))
            {
                channel = new RoomChannel(Encoding.UTF8.GetBytes(new string(data)));
                return true;
            }
            channel = default;
            return false;
        }

        /// <summary>
        /// Parse the specified data into a channel.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <returns>A Room channel.</returns>
        /// <exception cref="FormatException"></exception>
        public static RoomChannel Parse(ReadOnlySpan<byte> data)
        {
            if (TryParse(data, out RoomChannel channel)) return channel;
            throw new FormatException($"Room channel format is incorrect: {Encoding.UTF8.GetString(data)}");
        }

        /// <summary>
        /// Parse the specified data into a channel.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <returns>A Room channel.</returns>
        /// <exception cref="FormatException"></exception>
        public static RoomChannel Parse(ReadOnlySpan<char> data)
        {
            if (TryParse(data, out RoomChannel channel)) return channel;
            throw new FormatException($"Room channel format is incorrect: {new string(data)}");
        }

        /// <summary>
        /// Cast a number into a channel.
        /// </summary>
        /// <param name="number"></param>
        public static explicit operator RoomChannel(int number)
        {
            if (number >= 0)
            {
                var text = $"+{number:x}";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
            else
            {
                var text = $"-{-number:x}";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
        }

        /// <summary>
        /// Cast a number into a channel.
        /// </summary>
        /// <param name="number"></param>
        public static explicit operator RoomChannel(long number)
        {
            if (number >= 0)
            {
                var text = $"+{number:x}";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
            else
            {
                var text = $"-{-number:x}";
                var channel = new RoomChannel(Encoding.UTF8.GetBytes(text));
                return channel;
            }
        }

        /// <summary>
        /// Cast a channel into a number.
        /// </summary>
        /// <param name="channel"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static explicit operator int(RoomChannel channel)
        {
            if (channel.Length >= 2)
                if (channel._data[0] == '-')
                {
                    var text = Encoding.UTF8.GetString(channel._data.AsSpan().Slice(1));
                    var number = int.Parse(text, NumberStyles.HexNumber);
                    return -number;
                }
                else if (channel._data[0] == '+')
                {
                    var text = Encoding.UTF8.GetString(channel._data.AsSpan().Slice(1));
                    var number = int.Parse(text, NumberStyles.HexNumber);
                    return number;
                }
            throw new InvalidOperationException("Invalid internal data");
        }

        /// <summary>
        /// Cast a channel into a number.
        /// </summary>
        /// <param name="channel"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static explicit operator long(RoomChannel channel)
        {
            if (channel.Length >= 2)
                if (channel._data[0] == '-')
                {
                    var text = Encoding.UTF8.GetString(channel._data.AsSpan().Slice(1));
                    var number = long.Parse(text, NumberStyles.HexNumber);
                    return -number;
                }
                else if (channel._data[0] == '+')
                {
                    var text = Encoding.UTF8.GetString(channel._data.AsSpan().Slice(1));
                    var number = long.Parse(text, NumberStyles.HexNumber);
                    return number;
                }
            throw new InvalidOperationException("Invalid internal data");
        }

    }

}