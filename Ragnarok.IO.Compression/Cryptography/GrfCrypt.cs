using System;
using System.Runtime.InteropServices;

namespace Ragnarok.IO.Compression.Cryptography
{
    /// <summary>
    /// The base cryptography class for Ragnarok Online grf files.
    /// </summary>
    public static class GrfCrypt
    {
        private const int GRFFILE_FLAG_MIXCRYPT = 0x02;
        private const int GRFFILE_FLAG_0x14_DES = 0x04;
        private static byte[] m_KeySchedule = new byte[0x80];

        #region DES Tables

        // Some DES tables
        private static byte[][] tables0x40 = new byte[][] {
	        // Initial Permutation (IP)
            new byte[0x40]
	        {
		        0x3A, 0x32, 0x2A, 0x22, 0x1A, 0x12, 0x0A, 0x02,
		        0x3C, 0x34, 0x2C, 0x24, 0x1C, 0x14, 0x0C, 0x04,
		        0x3E, 0x36, 0x2E, 0x26, 0x1E, 0x16, 0x0E, 0x06,
		        0x40, 0x38, 0x30, 0x28, 0x20, 0x18, 0x10, 0x08,
		        0x39, 0x31, 0x29, 0x21, 0x19, 0x11, 0x09, 0x01,
		        0x3B, 0x33, 0x2B, 0x23, 0x1B, 0x13, 0x0B, 0x03,
		        0x3D, 0x35, 0x2D, 0x25, 0x1D, 0x15, 0x0D, 0x05,
		        0x3F, 0x37, 0x2F, 0x27, 0x1F, 0x17, 0x0F, 0x07
	        },
	        // Inverse Initial Permutation (IP^-1)
            new byte[0x40]
	        {
		        0x28, 0x08, 0x30, 0x10, 0x38, 0x18, 0x40, 0x20,
		        0x27, 0x07, 0x2F, 0x0F, 0x37, 0x17, 0x3F, 0x1F,
		        0x26, 0x06, 0x2E, 0x0E, 0x36, 0x16, 0x3E, 0x1E,
		        0x25, 0x05, 0x2D, 0x0D, 0x35, 0x15, 0x3D, 0x1D,
		        0x24, 0x04, 0x2C, 0x0C, 0x34, 0x14, 0x3C, 0x1C,
		        0x23, 0x03, 0x2B, 0x0B, 0x33, 0x13, 0x3B, 0x1B,
		        0x22, 0x02, 0x2A, 0x0A, 0x32, 0x12, 0x3A, 0x1A,
		        0x21, 0x01, 0x29, 0x09, 0x31, 0x11, 0x39, 0x19
	        },
	        // 8 Selection functions (S)
            new byte[0x40]
	        {
		        0x0E, 0x00, 0x04, 0x0F, 0x0D, 0x07, 0x01, 0x04,
		        0x02, 0x0E, 0x0F, 0x02, 0x0B, 0x0D, 0x08, 0x01,
		        0x03, 0x0A, 0x0A, 0x06, 0x06, 0x0C, 0x0C, 0x0B,
		        0x05, 0x09, 0x09, 0x05, 0x00, 0x03, 0x07, 0x08,
		        0x04, 0x0F, 0x01, 0x0C, 0x0E, 0x08, 0x08, 0x02,
		        0x0D, 0x04, 0x06, 0x09, 0x02, 0x01, 0x0B, 0x07,
		        0x0F, 0x05, 0x0C, 0x0B, 0x09, 0x03, 0x07, 0x0E,
		        0x03, 0x0A, 0x0A, 0x00, 0x05, 0x06, 0x00, 0x0D
	        },new byte[0x40]{
		        0x0F, 0x03, 0x01, 0x0D, 0x08, 0x04, 0x0E, 0x07,
		        0x06, 0x0F, 0x0B, 0x02, 0x03, 0x08, 0x04, 0x0E,
		        0x09, 0x0C, 0x07, 0x00, 0x02, 0x01, 0x0D, 0x0A,
		        0x0C, 0x06, 0x00, 0x09, 0x05, 0x0B, 0x0A, 0x05,
		        0x00, 0x0D, 0x0E, 0x08, 0x07, 0x0A, 0x0B, 0x01,
		        0x0A, 0x03, 0x04, 0x0F, 0x0D, 0x04, 0x01, 0x02,
		        0x05, 0x0B, 0x08, 0x06, 0x0C, 0x07, 0x06, 0x0C,
		        0x09, 0x00, 0x03, 0x05, 0x02, 0x0E, 0x0F, 0x09
	        },new byte[0x40]{
		        0x0A, 0x0D, 0x00, 0x07, 0x09, 0x00, 0x0E, 0x09,
		        0x06, 0x03, 0x03, 0x04, 0x0F, 0x06, 0x05, 0x0A,
		        0x01, 0x02, 0x0D, 0x08, 0x0C, 0x05, 0x07, 0x0E,
		        0x0B, 0x0C, 0x04, 0x0B, 0x02, 0x0F, 0x08, 0x01,
		        0x0D, 0x01, 0x06, 0x0A, 0x04, 0x0D, 0x09, 0x00,
		        0x08, 0x06, 0x0F, 0x09, 0x03, 0x08, 0x00, 0x07,
		        0x0B, 0x04, 0x01, 0x0F, 0x02, 0x0E, 0x0C, 0x03,
		        0x05, 0x0B, 0x0A, 0x05, 0x0E, 0x02, 0x07, 0x0C
	        },new byte[0x40]{
		        0x07, 0x0D, 0x0D, 0x08, 0x0E, 0x0B, 0x03, 0x05,
		        0x00, 0x06, 0x06, 0x0F, 0x09, 0x00, 0x0A, 0x03,
		        0x01, 0x04, 0x02, 0x07, 0x08, 0x02, 0x05, 0x0C,
		        0x0B, 0x01, 0x0C, 0x0A, 0x04, 0x0E, 0x0F, 0x09,
		        0x0A, 0x03, 0x06, 0x0F, 0x09, 0x00, 0x00, 0x06,
		        0x0C, 0x0A, 0x0B, 0x01, 0x07, 0x0D, 0x0D, 0x08,
		        0x0F, 0x09, 0x01, 0x04, 0x03, 0x05, 0x0E, 0x0B,
		        0x05, 0x0C, 0x02, 0x07, 0x08, 0x02, 0x04, 0x0E
	        },new byte[0x40]{
		        0x02, 0x0E, 0x0C, 0x0B, 0x04, 0x02, 0x01, 0x0C,
		        0x07, 0x04, 0x0A, 0x07, 0x0B, 0x0D, 0x06, 0x01,
		        0x08, 0x05, 0x05, 0x00, 0x03, 0x0F, 0x0F, 0x0A,
		        0x0D, 0x03, 0x00, 0x09, 0x0E, 0x08, 0x09, 0x06,
		        0x04, 0x0B, 0x02, 0x08, 0x01, 0x0C, 0x0B, 0x07,
		        0x0A, 0x01, 0x0D, 0x0E, 0x07, 0x02, 0x08, 0x0D,
		        0x0F, 0x06, 0x09, 0x0F, 0x0C, 0x00, 0x05, 0x09,
		        0x06, 0x0A, 0x03, 0x04, 0x00, 0x05, 0x0E, 0x03
	        },new byte[0x40]{
		        0x0C, 0x0A, 0x01, 0x0F, 0x0A, 0x04, 0x0F, 0x02,
		        0x09, 0x07, 0x02, 0x0C, 0x06, 0x09, 0x08, 0x05,
		        0x00, 0x06, 0x0D, 0x01, 0x03, 0x0D, 0x04, 0x0E,
		        0x0E, 0x00, 0x07, 0x0B, 0x05, 0x03, 0x0B, 0x08,
		        0x09, 0x04, 0x0E, 0x03, 0x0F, 0x02, 0x05, 0x0C,
		        0x02, 0x09, 0x08, 0x05, 0x0C, 0x0F, 0x03, 0x0A,
		        0x07, 0x0B, 0x00, 0x0E, 0x04, 0x01, 0x0A, 0x07,
		        0x01, 0x06, 0x0D, 0x00, 0x0B, 0x08, 0x06, 0x0D
	        },new byte[0x40]{
		        0x04, 0x0D, 0x0B, 0x00, 0x02, 0x0B, 0x0E, 0x07,
		        0x0F, 0x04, 0x00, 0x09, 0x08, 0x01, 0x0D, 0x0A,
		        0x03, 0x0E, 0x0C, 0x03, 0x09, 0x05, 0x07, 0x0C,
		        0x05, 0x02, 0x0A, 0x0F, 0x06, 0x08, 0x01, 0x06,
		        0x01, 0x06, 0x04, 0x0B, 0x0B, 0x0D, 0x0D, 0x08,
		        0x0C, 0x01, 0x03, 0x04, 0x07, 0x0A, 0x0E, 0x07,
		        0x0A, 0x09, 0x0F, 0x05, 0x06, 0x00, 0x08, 0x0F,
		        0x00, 0x0E, 0x05, 0x02, 0x09, 0x03, 0x02, 0x0C
	        },new byte[0x40]{
		        0x0D, 0x01, 0x02, 0x0F, 0x08, 0x0D, 0x04, 0x08,
		        0x06, 0x0A, 0x0F, 0x03, 0x0B, 0x07, 0x01, 0x04,
		        0x0A, 0x0C, 0x09, 0x05, 0x03, 0x06, 0x0E, 0x0B,
		        0x05, 0x00, 0x00, 0x0E, 0x0C, 0x09, 0x07, 0x02,
		        0x07, 0x02, 0x0B, 0x01, 0x04, 0x0E, 0x01, 0x07,
		        0x09, 0x04, 0x0C, 0x0A, 0x0E, 0x08, 0x02, 0x0D,
		        0x00, 0x0F, 0x06, 0x0C, 0x0A, 0x09, 0x0D, 0x00,
		        0x0F, 0x03, 0x03, 0x05, 0x05, 0x06, 0x08, 0x0B
	        }
        };

        private static byte[][] tables0x30 = new byte[][] {
	        // Permuted Choice 2 (PC-2)
            new byte[0x30]
	        {
		        0x0E, 0x11, 0x0B, 0x18, 0x01, 0x05, 0x03, 0x1C,
		        0x0F, 0x06, 0x15, 0x0A, 0x17, 0x13, 0x0C, 0x04,
		        0x1A, 0x08, 0x10, 0x07, 0x1B, 0x14, 0x0D, 0x02,
		        0x29, 0x34, 0x1F, 0x25, 0x2F, 0x37, 0x1E, 0x28,
		        0x33, 0x2D, 0x21, 0x30, 0x2C, 0x31, 0x27, 0x38,
		        0x22, 0x35, 0x2E, 0x2A, 0x32, 0x24, 0x1D, 0x20
	        },
	        // Bit-selection table (E)
            new byte[0x30]
	        {
		        0x20, 0x01, 0x02, 0x03, 0x04, 0x05,
		        0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
		        0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D,
		        0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11,
		        0x10, 0x11, 0x12, 0x13, 0x14, 0x15,
		        0x14, 0x15, 0x16, 0x17, 0x18, 0x19,
		        0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D,
		        0x1C, 0x1D, 0x1E, 0x1F, 0x20, 0x01
	        }
        };

        private static byte[][] tables0x20 = new byte[][] {
	        // P
            new byte[0x20]
	        {
		        0x10, 0x07, 0x14, 0x15,
		        0x1D, 0x0C, 0x1C, 0x11,
		        0x01, 0x0F, 0x17, 0x1A,
		        0x05, 0x12, 0x1F, 0x0A,
		        0x02, 0x08, 0x18, 0x0E,
		        0x20, 0x1B, 0x03, 0x09,
		        0x13, 0x0D, 0x1E, 0x06,
		        0x16, 0x0B, 0x04, 0x19
	        }
        };

        private static byte[][] tables0x1C = new byte[][] {
            new byte[0x1C]
	        {
		        0x39, 0x31, 0x29, 0x21, 0x19, 0x11, 0x09, 0x01,
		        0x3A, 0x32, 0x2A, 0x22, 0x1A, 0x12, 0x0A, 0x02,
		        0x3B, 0x33, 0x2B, 0x23, 0x1B, 0x13, 0x0B, 0x03,
		        0x3C, 0x34, 0x2C, 0x24
	        },
            new byte[0x1C]
	        {
		        0x3F, 0x37, 0x2F, 0x27, 0x1F, 0x17, 0x0F, 0x07,
		        0x3E, 0x36, 0x2E, 0x26, 0x1E, 0x16, 0x0E, 0x06,
		        0x3D, 0x35, 0x2D, 0x25, 0x1D, 0x15, 0x0D, 0x05,
		        0x1C, 0x14, 0x0C, 0x04
	        }
        };

        private static byte[][] tables0x10 = new byte[][] {
	        //! Left shifts each iteration
            new byte[0x10]
	        {
		        0x01, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
		        0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x01
	        }
        };

        private static byte[] BitMap = new byte[0x08] {
            0x80, 0x40, 0x20, 0x10, 0x8, 0x4, 0x2, 0x1
        };

        #endregion
        #region DES Shortcuts

        /// <summary>
        /// Initial Permutation (IP)
        /// </summary>
        private static byte[] DES_IP
        {
            get { return tables0x40[0]; }
        }
        /// <summary>
        /// Final Permutatioin (IP^-1)
        /// </summary>
        private static byte[] DES_IP_INV
        {
            get { return tables0x40[1]; }
        }
        /// <summary>
        /// Selection functions (S)
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static byte[] DES_S(int num)
        {
            return tables0x40[2 + num];
        }
        /// <summary>
        /// Permuted-choice 2 (PC-2)
        /// </summary>
        private static byte[] DES_PC2
        {
            get { return tables0x30[0]; }
        }
        /// <summary>
        /// Bit-selection (E)
        /// </summary>
        private static byte[] DES_E
        {
            get { return tables0x30[1]; }
        }
        /// <summary>
        /// (P)
        /// </summary>
        private static byte[] DES_P
        {
            get { return tables0x20[0]; }
        }
        /// <summary>
        /// Permuted-choice 1 (PC-1) part 1
        /// </summary>
        private static byte[] DES_PC1_1
        {
            get { return tables0x1C[0]; }
        }
        /// <summary>
        /// Permuted-choice 1 (PC-1) part 2
        /// </summary>
        private static byte[] DES_PC1_2
        {
            get { return tables0x1C[1]; }
        }
        /// <summary>
        /// Left shifts per iteration
        /// </summary>
        private static byte[] DES_LSHIFTS
        {
            get { return tables0x10[0]; }
        }

        #endregion
        #region Enums

        private enum CryptoType
        {
            Encrypt,
            Decrypt
        }

        #endregion
        #region Public API

        /// <summary>
        /// The keyschedule used for the DES implementation. The default is an array filled with 0x80 0 bytes.
        /// </summary>
        public static byte[] KeySchedule
        {
            get { return m_KeySchedule; }
            set { m_KeySchedule = value; }
        }
        /// <summary>
        /// Decrypts a filename in a <see cref="GrfArchive"/> that has a version major of 1.
        /// </summary>
        /// <param name="buffer">The input buffer of the filename.</param>
        /// <param name="len">The length of the filename.</param>
        /// <returns>A <see cref="String"/> with the decrypted filename.</returns>
        public static string DecryptNameVer1(byte[] buffer, int len)
        {
            using (IntPtrEx pBuffer = new IntPtrEx(buffer),
                            pNamebuf = new IntPtrEx(new byte[len]))
            {
                GRFMixedProcess(pNamebuf, pBuffer, len, 1, KeySchedule, CryptoType.Decrypt);
                return pNamebuf;
            }
        }
        /// <summary>
        /// Encrypts a filename in a <see cref="GrfArchive"/> that has a version major of 1.
        /// </summary>
        /// <param name="name">The name to encrypt.</param>
        /// <param name="len">The length of the name.</param>
        /// <returns>A <see cref="Byte"/> array containing the encrypted filename.</returns>
        public static byte[] EncryptNameVer1(string name, int len)
        {
            byte[] namebuf = new byte[len];
            using (IntPtrEx pName = new IntPtrEx(name),
                            pNamebuf = new IntPtrEx(namebuf))
            {
                GRFMixedProcess(pNamebuf, pName, len, 1, KeySchedule, CryptoType.Encrypt);
                return namebuf;
            }
        }
        /// <summary>
        /// Decrypts the contents of a file inside the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="buffer">The encrypted contents.</param>
        /// <param name="len">The length of the contents.
        /// (Note: this is the CompressedLength, not the AlignedCompressedLength)</param>
        /// <param name="flags">The <see cref="GrfFileFlags"/> of the file.</param>
        /// <returns>A <see cref="Byte"/> array containing the decrypted contents of the file.</returns>
        public static byte[] DecryptFileBuffer(byte[] buffer, int len, GrfFileFlags flags)
        {
            byte[] dst = new byte[buffer.Length];
            using (IntPtrEx pDst = new IntPtrEx(dst),
                            pBuffer = new IntPtrEx(buffer))
            {
                GRFProcess(pDst, pBuffer, buffer.Length, (byte)flags, len, m_KeySchedule, CryptoType.Decrypt);
                return dst;
            }
        }
        /// <summary>
        /// Encrypts the contents of a file inside the <see cref="GrfArchive"/>.
        /// </summary>
        /// <param name="buffer">The unencrypted contents.</param>
        /// <param name="len">The length of the contents.
        /// (Note: this is the AlignedCompressedLength, not the CompressedLength)</param>
        /// <param name="flags">The <see cref="GrfFileFlags"/> of the file.</param>
        /// <returns>A <see cref="Byte"/> array containing the encrypted contents of the file.</returns>
        public static byte[] EncryptFileBuffer(byte[] buffer, int len, GrfFileFlags flags)
        {
            byte[] dst = new byte[buffer.Length];
            using (IntPtrEx pDst = new IntPtrEx(dst),
                            pBuffer = new IntPtrEx(buffer))
            {
                GRFProcess(pDst, pBuffer, buffer.Length, (byte)flags, len, m_KeySchedule, CryptoType.Encrypt);
                return dst;
            }
        }

        #endregion
        #region Private API

        private static void MemCpy(IntPtrEx src, IntPtrEx dst, int len)
        {
            byte[] buffer = new byte[len];
            Marshal.Copy(src, buffer, 0, len);
            Marshal.Copy(buffer, 0, dst, len);
        }

        private static void MemCpy(byte[] src, IntPtrEx dst, int len)
        {
            Marshal.Copy(src, 0, dst, len);
        }

        private static void MemCpy(IntPtrEx src, byte[] dst, int len)
        {
            Marshal.Copy(src, dst, 0, len);
        }

        private static IntPtr DESPermutation(IntPtrEx block, byte[] table)
        {
            byte[] tmpblock = new byte[8];
            byte tmp;

            for (int i = 0; i < 0x40; i++)
            {
                tmp = (byte)(table[i] - 1);
                if ((block[tmp >> 3] & BitMap[tmp & 7]) > 0)
                    tmpblock[i >> 3] |= BitMap[i & 7];
            }

            MemCpy(tmpblock, block, 8);

            return block;
        }

        private static IntPtr DESRawProcessBlock(IntPtrEx block, byte[] ks)
        {
            int tmp;
            byte[][] tmpblock = new byte[2][];
            tmpblock[0] = new byte[8];
            tmpblock[1] = new byte[8];

            for (int i = 0; i < 0x30; i++)
            {
                tmp = DES_E[i] + 0x1F;
                if ((block[tmp >> 3] & BitMap[tmp & 7]) > 0)
                    tmpblock[0][i / 6] |= BitMap[i % 6];
            }

            for (int i = 0; i < 8; i++)
                tmpblock[0][i] ^= ks[i];

            for (int i = 0; i < 8; i++)
            {
                if (i % 2 > 0)
                    tmpblock[1][i >> 1] += DES_S(i)[tmpblock[0][i] >> 2];
                else
                    tmpblock[1][i >> 1] = (byte)(DES_S(i)[tmpblock[0][i] >> 2] << 4);
            }

            tmpblock[0] = new byte[8];

            for (int i = 0; i < 0x20; i++)
            {
                tmp = DES_P[i] - 1;
                if ((tmpblock[1][tmp >> 3] & BitMap[tmp & 7]) > 0)
                    tmpblock[0][i >> 3] |= BitMap[i & 7];
            }

            block[0] ^= tmpblock[0][0];
            block[1] ^= tmpblock[0][1];
            block[2] ^= tmpblock[0][2];
            block[3] ^= tmpblock[0][3];

            return block;
        }

        private static IntPtr DESProcessBlock(byte rounds, IntPtrEx dst, IntPtrEx src, byte[] ks, CryptoType dir)
        {
            byte[] tmp = new byte[4];

            MemCpy(src, dst, 8);

            DESPermutation(dst, DES_IP);

            if (rounds > 0)
            {
                for (int i = 0; i < rounds; i++)
                {
                    //DES_RawProcessBlock(dst, ks + (dir == CryptoType.Decrypt ? 0xF - i : i) * 8);
                    DESRawProcessBlock(dst, ks);

                    MemCpy(dst, tmp, 4);
                    MemCpy(dst + 4, dst, 4);
                    MemCpy(tmp, dst + 4, 4);
                }
            }

            MemCpy(dst, tmp, 4);
            MemCpy(dst + 4, dst, 4);
            MemCpy(tmp, dst + 4, 4);

            DESPermutation(dst, DES_IP_INV);

            return dst;
        }

        private static IntPtr DESProcess(IntPtrEx dst, IntPtrEx src, int len, byte[] ks, CryptoType dir)
        {
            IntPtr orig = dst;
            for (int i = 0; i < len / 8; i++, dst += 8, src += 8)
                DESProcessBlock(1, dst, src, ks, dir);

            return orig;
        }

        private static IntPtr GRFProcess(IntPtrEx dst, IntPtrEx src, int len, byte flags, int digitsGen, byte[] ks, CryptoType dir)
        {
            int digits, i;
            if ((flags & GRFFILE_FLAG_MIXCRYPT) == GRFFILE_FLAG_MIXCRYPT)
            {
                for (i = digitsGen, digits = 0; i > 0; i /= 0xA, digits++) ;
                if (digits < 1) digits = 1;

                GRFMixedProcess(dst, src, len, (byte)digits, ks, dir);
            }
            else if ((flags & GRFFILE_FLAG_0x14_DES) == GRFFILE_FLAG_0x14_DES)
            {
                i = len / 8;
                if (i > 0x14)
                {
                    i = 0x14;
                    MemCpy(src + 0x14 * 8, dst + 0x14 * 8, len - 0x14 * 8);
                }

                DESProcess(dst, src, i * 8, ks, dir);
            }
            else
            {
                MemCpy(src, dst, len);
            }

            return dst;
        }

        private static IntPtr GRFMixedProcess(IntPtrEx dst, IntPtrEx src, int len, byte cycle, byte[] ks, CryptoType dir)
        {
            IntPtr orig = dst;
            byte j = 0, tmp;

            if (cycle < 3)
                cycle = 1;
            else if (cycle < 5)
                cycle++;
            else if (cycle < 7)
                cycle += 9;
            else
                cycle += 0xF;

            for (int i = 0; i < len / 8; i++, dst += 8, src += 8)
            {
                if (i < 0x14 || i % cycle == 0)
                {
                    DESProcessBlock(1, dst, src, ks, dir);
                }
                else
                {
                    if (j == 7)
                    {
                        if (dir == CryptoType.Decrypt)
                        {
                            // 3450162
                            MemCpy(src + 3, dst, 2);
                            // 01_____
                            dst[2] = src[6];
                            // 012____
                            MemCpy(src, dst + 3, 3);
                            // 012345_
                            dst[6] = src[5];
                        }
                        else
                        {
                            // 0123456
                            MemCpy(src, dst + 3, 2);
                            // ___01__
                            dst[6] = src[2];
                            // ___01_2
                            MemCpy(src + 3, dst, 3);
                            // 34501_2
                            dst[5] = src[6];
                            // 3450162
                        }

                        // Modify byte 7
                        if ((tmp = src[7]) <= 0x77)
                        {
                            if (tmp == 0x77)                // 0x77
                                dst[7] = 0x48;
                            else if (tmp == 0)              // 0x00
                                dst[7] = 0x2B;
                            else if ((--tmp) == 0)          // 0x01
                                dst[7] = 0x68;
                            else if ((tmp -= 0x2A) == 0)    // 0x2B
                                dst[7] = 0x00;
                            else if ((tmp -= 0x1D) == 0)    // 0x48
                                dst[7] = 0x77;
                            else if ((tmp -= 0x18) == 0)    // 0x60
                                dst[7] = 0xFF;
                            else if ((tmp -= 0x08) == 0)    // 0x68
                                dst[7] = 0x01;
                            else if ((tmp -= 0x04) == 0)    // 0x6C
                                dst[7] = 0x80;
                            else
                                dst[7] = src[7];
                        }
                        else
                        {
                            if ((tmp -= 0x80) == 0)         // 0x80
                                dst[7] = 0x6C;
                            else if ((tmp -= 0x39) == 0)      // 0xB9
                                dst[7] = 0xC0;
                            else if ((tmp -= 0x07) == 0)      // 0xC0
                                dst[7] = 0xB9;
                            else if ((tmp -= 0x2B) == 0)      // 0xEB
                                dst[7] = 0xFE;
                            else if ((tmp -= 0x13) == 0)      // 0xFE
                                dst[7] = 0xEB;
                            else if ((--tmp) == 0)          // 0xFF
                                dst[7] = 0x60;
                            else
                                dst[7] = src[7];
                        }
                        j = 0;
                    }
                    else
                    {
                        MemCpy(src, dst, 8);
                    }
                    j++;
                }
            }

            return orig;
        }

        #endregion
    }
}
