using System;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Utility functions to scan Room protocol components.
    /// </summary>
    public static class RoomDataUtils
    {

        /// <summary>
        /// Check if is ASCII blank.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBlank(byte c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';

        /// <summary>
        /// Check if is ASCII blank.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBlank(char c) => c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\f';

        /// <summary>
        /// Check if is ASCII sign.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsSign(byte c) => c == '-' || c == '+';

        /// <summary>
        /// Check if is ASCII sign.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsSign(char c) => c == '-' || c == '+';

        /// <summary>
        /// Check if is ASCII letter.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLetter(byte c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';

        /// <summary>
        /// Count the number of consecutive ASCII letters in a range.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="min">Minimum inclusive range bound.</param>
        /// <param name="max">Maximum inlcusive range bound.</param>
        /// <returns>The number of consecutive ASCII letters in a range or 0 otherwise.</returns>
        public static int ScanWord(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsLetter(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        /// <summary>
        /// Check if is ASCII letter.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsLetter(char c) => c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z';

        /// <summary>
        /// Count the number of consecutive ASCII letters in a range.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="min">Minimum inclusive range bound.</param>
        /// <param name="max">Maximum inlcusive range bound.</param>
        /// <returns>The number of consecutive ASCII letters in a range or 0 otherwise.</returns>
        public static int ScanWord(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsLetter(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        /// <summary>
        /// Check if is ASCII digit.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsDigit(byte c) => c >= '0' && c <= '9';

        /// <summary>
        /// Count the number of consecutive ASCII digits in a range.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="min">Minimum inclusive range bound.</param>
        /// <param name="max">Maximum inlcusive range bound.</param>
        /// <returns>The number of consecutive ASCII digits in a range or 0 otherwise.</returns>
        public static int ScanDigit(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsDigit(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        /// <summary>
        /// Check if is ASCII digit.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsDigit(char c) => c >= '0' && c <= '9';

        /// <summary>
        /// Count the number of consecutive ASCII digits in a range.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="min">Minimum inclusive range bound.</param>
        /// <param name="max">Maximum inlcusive range bound.</param>
        /// <returns>The number of consecutive ASCII digits in a range or 0 otherwise.</returns>
        public static int ScanDigit(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsDigit(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        /// <summary>
        /// Check if is ASCII hex-digit.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsHexadecimal(byte c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';

        /// <summary>
        /// Count the number of consecutive ASCII hex-digits in a range.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="min">Minimum inclusive range bound.</param>
        /// <param name="max">Maximum inlcusive range bound.</param>
        /// <returns>The number of consecutive ASCII hex-digits in a range or 0 otherwise.</returns>
        public static int ScanHexadecimal(this ReadOnlySpan<byte> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsHexadecimal(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

        /// <summary>
        /// Check if is ASCII hex-digit.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsHexadecimal(char c) => c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f';

        /// <summary>
        /// Count the number of consecutive ASCII hex-digits in a range.
        /// </summary>
        /// <param name="data">UTF-8 text.</param>
        /// <param name="min">Minimum inclusive range bound.</param>
        /// <param name="max">Maximum inlcusive range bound.</param>
        /// <returns>The number of consecutive ASCII hex-digits in a range or 0 otherwise.</returns>
        public static int ScanHexadecimal(this ReadOnlySpan<char> data, int min = 1, int max = int.MaxValue)
        {
            var index = 0;
            while (index < data.Length && IsHexadecimal(data[index]))
            {
                index++;
                if (index > max)
                    return 0;
            }
            if (index < min)
                return 0;
            return index;
        }

    }
}