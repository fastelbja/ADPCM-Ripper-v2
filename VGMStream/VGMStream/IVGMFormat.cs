using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public interface IVGMFormat
    {
        string Extension{ get; }
        string Description{ get; }
        string Filename { get; }
        bool IsPlayable { get; }

        UInt64 IsFormat(StreamReader.IReader fileReader, UInt64 offset, UInt64 Length);
        void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream,bool InitReader, UInt64 Filelength);
    }
}
