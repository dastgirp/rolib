using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ragnarok
{
    public static class FilenameEncoding
    {
        private static readonly Encoding NameEncoding = Encoding.GetEncoding(949);

        /// <summary>
        /// Encodes the name with the proper encoding.
        /// </summary>
        /// <param name="name">The name to encode.</param>
        /// <returns>A <see cref="String"/> with the properly encoded filename.</returns>
        public static string Encode(string name)
        {
            byte[] buffer = new byte[name.Length];
            using (IntPtrEx pName = new IntPtrEx(name))
            {
                Marshal.Copy(pName, buffer, 0, name.Length);
            }
            return Encode(buffer);
        }
        /// <summary>
        /// Encodes the name with the proper encoding.
        /// </summary>
        /// <param name="buffer">The raw buffer of the name to encode.</param>
        /// <returns>A <see cref="String"/> with the properly encoded filename.</returns>
        public static string Encode(byte[] buffer)
        {
            return NameEncoding.GetString(buffer).Replace('/', '\\');
        }
    }
}
