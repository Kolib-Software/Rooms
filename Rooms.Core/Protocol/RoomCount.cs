using System;
using System.Globalization;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Room Count UTF-8 text wrapper.
    /// </summary>
    public readonly struct RoomCount
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
        /// Create a new count with the specified data without validate it.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        public RoomCount(ArraySegment<byte> data) => _data = data;

        /// <summary>
        /// Internal data.
        /// </summary>
        private readonly ArraySegment<byte> _data;

        /// <summary>
        /// Checks if the specified data is a valid count.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <returns>`true` if data is a valid count representation `false` otherwise.</returns>
        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length < 1) return false;
            var index = data.ScanDigit();
            return index == data.Length;
        }

        /// <summary>
        /// Checks if the specified data is a valid count.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <returns>`true` if data is a valid count representation `false` otherwise.</returns>
        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length < 1) return false;
            var index = data.ScanDigit();
            return index == data.Length;
        }

        /// <summary>
        /// Attempts to parse the specified data into a count.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="count">Room count.</param>
        /// <returns>`true` on success `false` otherwise.</returns>
        public static bool TryParse(ReadOnlySpan<byte> data, out RoomCount count)
        {
            if (Verify(data))
            {
                count = new RoomCount(data.ToArray());
                return true;
            }
            count = default;
            return false;
        }

        /// <summary>
        /// Attempts to parse the specified data into a count.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <param name="count">Room count.</param>
        /// <returns>`true` on success `false` otherwise.</returns>
        public static bool TryParse(ReadOnlySpan<char> data, out RoomCount count)
        {
            if (Verify(data))
            {
                count = new RoomCount(Encoding.UTF8.GetBytes(new string(data)));
                return true;
            }
            count = default;
            return false;
        }

        /// <summary>
        /// Parse the specified data into a count.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <returns>A Room count.</returns>
        /// <exception cref="FormatException"></exception>
        public static RoomCount Parse(ReadOnlySpan<byte> data)
        {
            if (TryParse(data, out RoomCount count)) return count;
            throw new FormatException($"Room count format is incorrect: {Encoding.UTF8.GetString(data)}");
        }

        /// <summary>
        /// Parse the specified data into a count.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <returns>A Room count.</returns>
        /// <exception cref="FormatException"></exception>
        public static RoomCount Parse(ReadOnlySpan<char> data)
        {
            if (TryParse(data, out RoomCount count)) return count;
            throw new FormatException($"Room count format is incorrect: {new string(data)}");
        }

        /// <summary>
        /// Cast a number into a count.
        /// </summary>
        /// <param name="number"></param>
        /// <exception cref="InvalidCastException"></exception>
        public static explicit operator RoomCount(int number)
        {
            if (number < 0) throw new InvalidCastException("Negative values are not allowed");
            var text = $"{number}";
            var count = new RoomCount(Encoding.UTF8.GetBytes(text));
            return count;
        }

        /// <summary>
        /// Cast a number into a count.
        /// </summary>
        /// <param name="number"></param>
        /// <exception cref="InvalidCastException"></exception>
        public static explicit operator RoomCount(long number)
        {
            if (number < 0) throw new InvalidCastException("Negative values are not allowed");
            var text = $"{number}";
            var count = new RoomCount(Encoding.UTF8.GetBytes(text));
            return count;
        }

        /// <summary>
        /// Cast a count into a number.
        /// </summary>
        /// <param name="count"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static explicit operator int(RoomCount count)
        {
            if (count.Length >= 1)
            {
                var text = Encoding.UTF8.GetString(count._data);
                var number = int.Parse(text, NumberStyles.Integer, null);
                return number;
            }
            throw new InvalidOperationException("Invalid internal data");
        }

        /// <summary>
        /// Cast a count into a number.
        /// </summary>
        /// <param name="count"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static explicit operator long(RoomCount count)
        {
            if (count.Length >= 1)
            {
                var text = Encoding.UTF8.GetString(count._data);
                var number = long.Parse(text, NumberStyles.Integer, null);
                return number;
            }
            throw new InvalidOperationException("Invalid internal data");
        }

    }

}