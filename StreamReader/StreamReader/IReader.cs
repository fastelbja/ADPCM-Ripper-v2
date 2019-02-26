using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public interface IReader 
    {
        string GetFilename();
        FST GetFileSystem();
        bool CanRead();
        bool Open(string FileName);
        byte[] Read(UInt64 Offset, UInt64 Length);
        UInt16 Read_16bitsBE(UInt64 Offset);
        UInt16 Read_16bits(UInt64 Offset);
        UInt32 Read_32bitsBE(UInt64 Offset);
        UInt32 Read_32bits(UInt64 Offset);
        UInt64 GetLength();
        void SetSessionID(byte Session);
        byte GetSessionID();
        string GetDescription();

        string Read_String(UInt64 Offset, UInt64 Length);
        byte Read_8Bits(UInt64 Offset);
        void Close();

    }
}
