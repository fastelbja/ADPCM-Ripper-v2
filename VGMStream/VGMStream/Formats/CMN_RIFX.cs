using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class CMN_RIFX : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "OGG";
            }
        }

        public string Description
        {
            get
            {
                return "Audiokinetic Wwise";
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
            // Check for Mark ID ("RIFX")
            if (fileReader.Read_32bitsBE(offset) != 0x52494658)
                return 0;

            // Check for Mark ID ("WAVE" + "fmt ")
            if ((fileReader.Read_32bitsBE(offset + 0x04) == 0x57415645) && (fileReader.Read_32bitsBE(offset + 0x0C) == 0x666D7420))
            {
                for (UInt64 i = 0; i < 0x100; i+=4)
                {
                    if ((fileReader.Read_32bitsBE(offset + i) == 0x766F7262) || (fileReader.Read_32bitsBE(offset + i) == 0x63756520))
                    {
                        if (VGM_Utils.CheckSampleRate(fileReader.Read_32bitsBE(offset + 0x18)))
                        {
                            length = fileReader.Read_32bitsBE(offset + 0x04) + 0x8;
                            return length;
                        }
                    }
                }
            }
            return 0;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
        }
    }

}
