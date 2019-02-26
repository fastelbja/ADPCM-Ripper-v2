using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace StreamReader
{
    public class BinaryStreamReader : IReader
    {
        private const uint STREAMFILE_DEFAULT_BUFFER_SIZE = 0x8000;
        private bool m_EOF = false;

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 0x00000001;

        const uint OPEN_EXISTING = 3;

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        static extern SafeFileHandle CreateFile
        (
            string FileName,          // file name
            uint DesiredAccess,       // access mode
            uint ShareMode,           // share mode
            uint SecurityAttributes,  // Security Attributes
            uint CreationDisposition, // how to create
            uint FlagsAndAttributes,  // file attributes
            int hTemplateFile         // handle to template file
        );

        [System.Runtime.InteropServices.DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle
        (
            SafeFileHandle hObject // handle to object
        );

        protected StreamFile streamFile;

        private string m_Filename;

        public string GetFilename()
        {
            return m_Filename;
        }

        public bool CanRead()
        {
            return !m_EOF;
        }

        public FST GetFileSystem()
        {
            return null;
        }

        public void SetSessionID(byte Session)
        {
            // unused...
        }

        public bool Open(string FileName)
        {
            SafeFileHandle fHandle;

            m_Filename = FileName;

            // open the existing file for reading       
            fHandle = CreateFile(FileName,GENERIC_READ,FILE_SHARE_READ,0,OPEN_EXISTING,0,0);
            if (!fHandle.IsInvalid)
            {
                streamFile = new StreamFile();
                streamFile.FileHandle = fHandle;
                streamFile.FileName = FileName;
                streamFile.fReader = new FileStream(fHandle, FileAccess.Read);
                return true;
            }
            return false;
        }

        private UInt64 ReadBuffer(ref byte[] readBuffer, UInt64 Offset, UInt64 Length) 
        {
            UInt64 LengthToRead = Length;
            UInt64 LengthRead;
            UInt64 TotalLengthRead = 0;

            // if the needed buffer is allready in memory ...
            if ((Offset >= streamFile.Offset) && (Offset + Length <= streamFile.Offset + streamFile.ValidSize))
            {
                MemoryReader.BytesCopy(streamFile.Buffer, (Offset - streamFile.Offset), readBuffer, 0, Length);
                return Length;
            }

            try
            {
                // if not, we have to read for it ...
                while (Length > 0)
                {
                    streamFile.fReader.Position = (Int64)Offset;
                    streamFile.Offset = Offset;

                    // we can't read more than the buffer size !
                    if (Length > (UInt64)streamFile.BufferSize)
                        LengthToRead = (UInt64)streamFile.BufferSize;
                    else
                        LengthToRead = Length;

                    LengthRead = (UInt64)streamFile.fReader.Read(streamFile.Buffer, 0, streamFile.BufferSize);

                    m_EOF = (LengthRead == 0);

                    streamFile.ValidSize = (UInt64)streamFile.BufferSize; // LengthRead;
                    Offset += LengthRead;

                    // We copy what we read into the dest buffer ...
                    MemoryReader.BytesCopy(streamFile.Buffer, 0, readBuffer, TotalLengthRead, LengthToRead);

                    // ... until we get all is needed 
                    TotalLengthRead += LengthToRead;
                    if (LengthRead < LengthToRead)
                        return TotalLengthRead;

                    Length -= LengthToRead;
                }
                return TotalLengthRead;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return 0;
            }
        }

        public byte[] Read(UInt64 Offset, UInt64 Length)
        {
            byte[] tmpBuffer = new byte[Length];

            // if the needed buffer is allready in memory ...
            if ((Offset>=streamFile.Offset) && (Offset+Length<=streamFile.Offset+streamFile.ValidSize)) 
            {
                MemoryReader.BytesCopy(streamFile.Buffer, (Offset - streamFile.Offset), tmpBuffer, 0, Length);
                return tmpBuffer;
            }

            ReadBuffer(ref tmpBuffer, Offset, Length);
            return tmpBuffer;
        }

        public UInt16 Read_16bitsBE(UInt64 Offset)
        {
            byte[] buffer = new byte[2];
            buffer = Read(Offset, 2);
            return (MemoryReader.ReadIntBE(ref buffer, 0));
        }

        public UInt16 Read_16bits(UInt64 Offset)
        {
            byte[] buffer = new byte[2];
            buffer = Read(Offset, 2);
            return (MemoryReader.ReadInt(ref buffer, 0));
        }

        public UInt32 Read_32bitsBE(UInt64 Offset)
        {
            byte[] buffer = new byte[4];
            buffer = Read(Offset, 4);
            return (MemoryReader.ReadLongBE(ref buffer, 0));
        }

        public UInt32 Read_32bits(UInt64 Offset)
        {
            byte[] buffer = new byte[4];
            buffer = Read(Offset, 4);
            return (MemoryReader.ReadLong(ref buffer, 0));
        }

        public byte Read_8Bits(UInt64 Offset)
        {
            byte[] buffer = new byte[1];
            buffer = Read(Offset, 1);
            return buffer[0];
        }


        public string Read_String(UInt64 Offset, UInt64 Length)
        {
            byte[] buffer = new byte[Length];
            int bufferOffset = 0;

            buffer = Read(Offset, Length);

            string result = "";

            do
            {
                if (buffer[bufferOffset] == 0)
                    break;

                result += Convert.ToChar(buffer[bufferOffset]);
                bufferOffset++;
            } while (--Length > 0);

            return result;
        }

        public UInt64 GetLength()
        {
            return (UInt64)streamFile.fReader.Length;
        }

        public byte GetSessionID()
        {
            return 1;
        }

        public string GetDescription()
        {
            return "Binary file ...";
        }

        public void Close()
        {
            if (streamFile != null)
            {
                // free what we have allready open...
                streamFile.fReader.Close();
                streamFile.fReader.Dispose();
                if (!streamFile.FileHandle.IsClosed)
                {
                    CloseHandle(streamFile.FileHandle);
                }
            }
        }
    }
}
