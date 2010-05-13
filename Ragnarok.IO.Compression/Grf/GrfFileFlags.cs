using System;

namespace Ragnarok.IO.Compression
{
    /// <summary>
    /// Provides attributes for a <see cref="GrfItem"/>.
    /// </summary>
    [Flags]
    public enum GrfFileFlags : byte
    {
        /// <summary>
        /// This flag specifies that a <see cref="GrfItem"/> entry is a directory.
        /// </summary>
        None = 0,
        /// <summary>
        /// This flag specifies that a <see cref="GrfItem"/> entry is a file. If not set, then the entry is a directory.
        /// </summary>
        File = 0x01,
        /// <summary>
        /// Flag for <see cref="GrfItem.Flags"/> to specify that the file uses mixed crypto.
        /// </summary>
        MixCrypt = 0x02,
        /// <summary>
        /// Flag for <see cref="GrfItem.Flags"/> to specify that only the first 0x14 blocks are encrypted.
        /// </summary>
        Des_0x14 = 0x04,
    }
}
