using System;
using System.Text;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Room Verb UTF-8 text wrapper.
    /// </summary>
    public readonly struct RoomVerb
    {

        /// /// <summary>
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
        /// Create a new verb with the specified data without validate it.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        public RoomVerb(ArraySegment<byte> data) => _data = data;

        /// <summary>
        /// Internal data.
        /// </summary>
        private readonly ArraySegment<byte> _data;

        /// <summary>
        /// Checks if the specified data is a valid verb.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <returns>`true` if data is a valid verb representation `false` otherwise.</returns>
        public static bool Verify(ReadOnlySpan<byte> data)
        {
            if (data.Length < 1) return false;
            var index = data.ScanWord();
            return index == data.Length;
        }

        /// <summary>
        /// Checks if the specified data is a valid verb.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <returns>`true` if data is a valid verb representation `false` otherwise.</returns>
        public static bool Verify(ReadOnlySpan<char> data)
        {
            if (data.Length < 1) return false;
            var index = data.ScanWord();
            return index == data.Length;
        }

        /// <summary>
        /// Attempts to parse the specified data into a verb.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="verb">Room verb.</param>
        /// <returns>`true` on success `false` otherwise.</returns>
        public static bool TryParse(ReadOnlySpan<byte> data, out RoomVerb verb)
        {
            if (Verify(data))
            {
                verb = new RoomVerb(data.ToArray());
                return true;
            }
            verb = default;
            return false;
        }

        /// <summary>
        /// Attempts to parse the specified data into a verb.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <param name="verb">Room verb.</param>
        /// <returns>`true` on success `false` otherwise.</returns>
        public static bool TryParse(ReadOnlySpan<char> data, out RoomVerb verb)
        {
            if (Verify(data))
            {
                verb = new RoomVerb(Encoding.UTF8.GetBytes(new string(data)));
                return true;
            }
            verb = default;
            return false;
        }

        /// <summary>
        /// Parse the specified data into a verb.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <returns>A Room verb.</returns>
        /// <exception cref="FormatException"></exception>
        public static RoomVerb Parse(ReadOnlySpan<byte> data)
        {
            if (TryParse(data, out RoomVerb verb)) return verb;
            throw new FormatException($"Room verb format is incorrect: {Encoding.UTF8.GetString(data)}");
        }

        /// <summary>
        /// Parse the specified data into a verb.
        /// </summary>
        /// <param name="data">UTF-16 text.</param>
        /// <returns>A Room verb.</returns>
        /// <exception cref="FormatException"></exception>
        public static RoomVerb Parse(ReadOnlySpan<char> data)
        {
            if (TryParse(data, out RoomVerb verb)) return verb;
            throw new FormatException($"Room verb format is incorrect: {new string(data)}");
        }

    }

}