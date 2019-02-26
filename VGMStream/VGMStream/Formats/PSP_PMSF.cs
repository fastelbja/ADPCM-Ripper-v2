using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PSP_PMSF : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "PMF";
            }
        }

        public string Description
        {
            get
            {
                return "PSP Movie File";
            }
        }

        public string Filename
        {
            get
            {
                return string.Empty;
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
            // Check for Mark ID ("PSMF")
            if (fileReader.Read_32bitsBE(offset) != 0x50534D46)
                return 0;

            if (   (fileReader.Read_32bitsBE(offset + 0x04) == 0x30303135) 
                || (fileReader.Read_32bitsBE(offset + 0x04) == 0x30303132)
                || (fileReader.Read_32bitsBE(offset + 0x04) == 0x30303134))
            {
                length = fileReader.Read_32bitsBE(offset + 0x0C) + 0x800;
                return length;
            }
            else return 0;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
        }

    }
}
