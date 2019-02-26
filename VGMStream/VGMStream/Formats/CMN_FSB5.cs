using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class CMN_FSB5: IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "FSB";
            }
        }

        public string Description
        {
            get
            {
                return "FMOD Sound Bank v5";
            }
        }

        public string Filename
        {
            get
            {
                return String.Empty;
            }
        }

        public bool IsPlayable
        {
            get
            {
                return false;
            }
        }

        public UInt64 IsFormat(StreamReader.IReader fileReader, UInt64 offset, UInt64 length)
        {
            return length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
        }
    }
}
