using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class CMN_FSB4 : IVGMFormat
    {
        private string m_Filename;

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
                return "FMOD Sound Bank v4";
            }
        }

        public string Filename
        {
            get
            {
                return m_Filename;
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
            m_Filename = System.IO.Path.GetFileNameWithoutExtension(fileReader.Read_String(offset + 0x32, 0x1e));
            return length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            m_Filename = System.IO.Path.GetFileNameWithoutExtension(fileReader.Read_String(offset + 0x32, 0x1e));
        }
    }
}
