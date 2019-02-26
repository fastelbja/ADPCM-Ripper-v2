using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace StreamReader
{
    public class StreamFile : IDisposable
    {
        public FileStream fReader;
        public SafeFileHandle FileHandle;
        public UInt64 Offset;
        public UInt64 ValidSize;
        public byte[] Buffer;
        public int BufferSize;
        public string FileName;

        public StreamFile()
        {
            Offset = 0;
            ValidSize = 0;
            BufferSize = 0x80000;
            Buffer = new byte[BufferSize];
        }

        public void Dispose() 
        {
            Buffer = null;
            BufferSize = 0;
        }
    }
}
