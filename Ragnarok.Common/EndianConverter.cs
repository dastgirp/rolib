namespace System
{
    /// <summary>
    /// Converts the endian of various integers.
    /// </summary>
    public static class EndianConverter
    {
        #region Long
#if EXCLUDE
        /// <summary>
        /// Converts a <see cref="Int64"/> to little endian notation.
        /// </summary>
        /// <param name="input">The <see cref="Int64"/> to convert.</param>
        /// <returns>The converted <see cref="Int64"/>.</returns>
        public static Int64 LittleEndian(Int64 input)
        {
            if (BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="Int64"/> to big endian notation.
        /// </summary>
        /// <param name="input">The <see cref="Int64"/> to convert.</param>
        /// <returns>The converted <see cref="Int64"/>.</returns>
        public static Int64 BigEndian(Int64 input)
        {
            if (!BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
#endif
        /// <summary>
        /// Converts a <see cref="UInt64"/> to little endian notation.
        /// </summary>
        /// <param name="input">The <see cref="UInt64"/> to convert.</param>
        /// <returns>The converted <see cref="UInt64"/>.</returns>
        public static UInt64 LittleEndian(UInt64 input)
        {
            if (BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="UInt64"/> to big endian notation.
        /// </summary>
        /// <param name="input">The <see cref="UInt64"/> to convert.</param>
        /// <returns>The converted <see cref="UInt64"/>.</returns>
        public static UInt64 BigEndian(UInt64 input)
        {
            if (!BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }

        #endregion
        #region Int
        /// <summary>
        /// Converts a <see cref="Int32"/> to little endian notation.
        /// </summary>
        /// <param name="input">The <see cref="Int32"/> to convert.</param>
        /// <returns>The converted <see cref="Int32"/>.</returns>
        public static Int32 LittleEndian(Int32 input)
        {
            if (BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="Int32"/> to big endian notation.
        /// </summary>
        /// <param name="input">The <see cref="Int32"/> to convert.</param>
        /// <returns>The converted <see cref="Int32"/>.</returns>
        public static Int32 BigEndian(Int32 input)
        {
            if (!BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="UInt32"/> to little endian notation.
        /// </summary>
        /// <param name="input">The <see cref="UInt32"/> to convert.</param>
        /// <returns>The converted <see cref="UInt32"/>.</returns>
        public static UInt32 LittleEndian(UInt32 input)
        {
            if (BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="UInt32"/> to big endian notation.
        /// </summary>
        /// <param name="input">The <see cref="UInt32"/> to convert.</param>
        /// <returns>The converted <see cref="UInt32"/>.</returns>
        public static UInt32 BigEndian(UInt32 input)
        {
            if (!BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }

        #endregion
        #region Short
        /// <summary>
        /// Converts a <see cref="Int16"/> to little endian notation.
        /// </summary>
        /// <param name="input">The <see cref="Int16"/> to convert.</param>
        /// <returns>The converted <see cref="Int16"/>.</returns>
        public static Int16 LittleEndian(Int16 input)
        {
            if (BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="Int16"/> to big endian notation.
        /// </summary>
        /// <param name="input">The <see cref="Int16"/> to convert.</param>
        /// <returns>The converted <see cref="Int16"/>.</returns>
        public static Int16 BigEndian(Int16 input)
        {
            if (!BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="UInt16"/> to little endian notation.
        /// </summary>
        /// <param name="input">The <see cref="UInt16"/> to convert.</param>
        /// <returns>The converted <see cref="UInt16"/>.</returns>
        public static UInt16 LittleEndian(UInt16 input)
        {
            if (BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }
        /// <summary>
        /// Converts a <see cref="UInt16"/> to big endian notation.
        /// </summary>
        /// <param name="input">The <see cref="UInt16"/> to convert.</param>
        /// <returns>The converted <see cref="UInt16"/>.</returns>
        public static UInt16 BigEndian(UInt16 input)
        {
            if (!BitConverter.IsLittleEndian)
                return input;

            return Swap(input);
        }

        #endregion
        #region Private API

#if EXCLUDE
        private static Int64 Swap(Int64 input)
        {
            return ((input & 0x00000000000000FF) << 56) |
                    ((input & 0x000000000000FF00) << 40) |
                    ((input & 0x0000000000FF0000) << 24) |
                    ((input & 0x00000000FF000000) << 8) |
                    ((input & 0x000000FF00000000) >> 8) |
                    ((input & 0x0000FF0000000000) >> 24) |
                    ((input & 0x00FF000000000000) >> 40) |
                    ((input & 0xFF00000000000000) >> 56);
        }
#endif

        private static Int32 Swap(Int32 input)
        {
            return (Int32)(((input & (uint)0x000000FF) << 24) |
                    ((input & (uint)0x0000FF00) << 8) |
                    ((input & (uint)0x00FF0000) >> 8) |
                    ((input & (uint)0xFF000000) >> 24));
        }

        private static Int16 Swap(Int16 input)
        {
            return (Int16)(((input & 0x00FF) << 8) |
                    ((input & 0xFF00) >> 8));
        }

        private static UInt64 Swap(UInt64 input)
        {
            return ((input & 0x00000000000000FF) << 56) |
                    ((input & 0x000000000000FF00) << 40) |
                    ((input & 0x0000000000FF0000) << 24) |
                    ((input & 0x00000000FF000000) << 8) |
                    ((input & 0x000000FF00000000) >> 8) |
                    ((input & 0x0000FF0000000000) >> 24) |
                    ((input & 0x00FF000000000000) >> 40) |
                    ((input & 0xFF00000000000000) >> 56);
        }

        private static UInt32 Swap(UInt32 input)
        {
            return ((input & 0x000000FF) << 24) |
                    ((input & 0x0000FF00) << 8) |
                    ((input & 0x00FF0000) >> 8) |
                    ((input & 0xFF000000) >> 24);
        }

        private static UInt16 Swap(UInt16 input)
        {
            return (UInt16)(((input & 0x00FF) << 8) |
                    ((input & 0xFF00) >> 8));
        }

        #endregion
    }
}
