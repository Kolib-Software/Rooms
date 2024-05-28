using System;
using System.IO;

namespace KolibSoft.Rooms.Core.Services
{

    /// <summary>
    /// Room service options.
    /// </summary>
    public sealed class RoomServiceOptions
    {

        /// <summary>
        /// Max amount of bytes read per second before force stream delay.
        /// </summary>
        public int MaxStreamRate { get; set; } = DefaultMaxStreamRate;

        /// <summary>
        /// Default max amount of bytes read per second before force stream delay.
        /// </summary>
        public const int DefaultMaxStreamRate = 1024 * 1024;

    }

}