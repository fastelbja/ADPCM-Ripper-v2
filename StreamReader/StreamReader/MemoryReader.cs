using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace StreamReader
{
    public unsafe class MemoryReader
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
        public static extern void CopyMemory(byte* dest, byte* src, int size);

        public static UInt16 ReadInt(ref byte[] buffer, UInt64 Offset)
        {
            return (UInt16)(buffer[Offset + 1] << 8 | buffer[Offset]);
        }

        public static UInt32 ReadLong(ref byte[] buffer, UInt64 Offset)
        {
            return (UInt32)(buffer[Offset + 3] << 24 | buffer[Offset + 2] << 16 | buffer[Offset + 1] << 8 | buffer[Offset]);
        }

        public static UInt16 ReadIntBE(ref byte[] buffer, UInt64 Offset)
        {
            return (UInt16)(buffer[Offset] << 8 | buffer[Offset + 1]);
        }

        public static UInt32 ReadLongBE(ref byte[] buffer, UInt64 Offset)
        {
            return (UInt32)(buffer[Offset] << 24 | buffer[Offset + 1] << 16 | buffer[Offset + 2] << 8 | buffer[Offset + 3]);
        }

        public static UInt64 ReadLongLongBE(ref byte[] buffer, UInt64 Offset)
        {
            return (UInt64)(buffer[Offset] << 128 | buffer[Offset + 1] << 64 | buffer[Offset + 2] << 32 | buffer[Offset + 3] | buffer[Offset+4] << 24 | buffer[Offset + 5] << 16 | buffer[Offset + 6] << 8 | buffer[Offset + 7]);
        }

        public static byte HINIBBLE(byte bByte)
        {
            return (byte)(((bByte) >> 4) & 0x0F);
        }

        public static byte LONIBBLE(byte bByte)
        {
            return (byte)((bByte) & 0x0F);
        }

        public static object BytesToStuct(byte[] bytes, Type type, UInt64 Offset)
        {
            object obj = null;
            int size = Marshal.SizeOf(type);
            if ((UInt64)size + Offset > (UInt64)bytes.Length)
            {
                size =(int)((UInt64)(bytes.Length) - Offset);
            }

            if (size < 0)
                Console.Write("");
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes,(int) Offset, structPtr, size);
            try
            {
                obj = Marshal.PtrToStructure(structPtr, type);
            }
            catch (StackOverflowException e)
            {
                Console.Write(e.Message + " Stack Overflow detected in ByteToStruct");
            }
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        public static string GetString(ref byte[] buffer, UInt64 Offset)
        {
            string result = "";

            do
            {
                if (buffer[Offset] == 0)
                    break;

                result += Convert.ToChar(buffer[Offset]);
                Offset++;
            } while (1 == 1);

            return result;
        }

        public static string GetString(ref byte[] buffer, UInt64 Offset, UInt64 length)
        {
            string result = "";

            do
            {
                if (buffer[Offset] == 0)
                    break;

                result += Convert.ToChar(buffer[Offset]);
                length--;
                Offset++;

                if (length == 0)
                    break;

            } while (1 == 1);

            return result;
        }

        public static uint SwapUnsignedLong(uint x)
        {
            return ((x << 24) | ((x & 0xff00) << 8) | ((x & 0xff0000) >> 8) | (x >> 24));
        }

        public static Int16 SwapUnsignedInt(Int16 x)
        {
            return (Int16)(((x & 0x00ff) << 8) | (x & 0xff00) >> 8);
        }

        public static void SaveLogBinary(byte[] buffer, string sPath)
        {
            if (System.IO.File.Exists(sPath))
                System.IO.File.Delete(sPath);
            System.IO.FileStream fs = new System.IO.FileStream(sPath, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);
            System.IO.BinaryWriter b = new System.IO.BinaryWriter(fs);
            b.Write(buffer, 0, buffer.Length);
            b.Close();
            fs.Close();
        }

        public static void PutLong(ref byte[] buffer, UInt64 Offset, uint _value)
        {
            buffer[0 + Offset] = (byte)((_value) >> 24);
            buffer[1 + Offset] = (byte)((_value) >> 16);
            buffer[2 + Offset] = (byte)((_value) >> 8);
            buffer[3 + Offset] = (byte)(_value);
        }

        public static unsafe void BytesCopy(byte[] source, UInt64 sourceOffset, byte[] dest, UInt64 destOffset, UInt64 Length)
        {
            if ((UInt64)dest.Length < Length)
                Length = (UInt64)dest.Length;

            if (destOffset + Length > (UInt64)dest.Length)
                Length = (UInt64)(dest.Length) - destOffset;

            fixed (byte* pSrc = source, pDst = dest)
            {
                byte* ppDst = pDst + destOffset;
                byte* ppSrc = pSrc + sourceOffset;
                CopyMemory(ppDst, ppSrc, (int)Length);
            }
        }

        public static bool memcmp(byte[] buffer, byte[] bufferToCompareWith, UInt64 Length, UInt64 offsetOfBuffer)
        {
            bool result = true;

            if (((UInt64)buffer.Length + offsetOfBuffer < Length) || ((UInt64)bufferToCompareWith.Length < Length))
                return false;

            for (uint i = 0; i < Length; i++)
            {
                if (buffer[i + offsetOfBuffer] != bufferToCompareWith[i])
                    result = false;
            }
            return result;
        }
    }
}
