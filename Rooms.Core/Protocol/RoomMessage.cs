using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KolibSoft.Rooms.Core.Protocol
{

    /// <summary>
    /// Room message representation.
    /// </summary>
    public sealed class RoomMessage
    {

        /// <summary>
        /// Room message verb.
        /// </summary>
        public string Verb { get; set; } = string.Empty;

        /// <summary>
        /// Room message channel.
        /// </summary>
        public int Channel { get; set; } = default;

        /// <summary>
        /// Room message content.
        /// </summary>
        public Stream Content { get; set; } = Stream.Null;

    }

}