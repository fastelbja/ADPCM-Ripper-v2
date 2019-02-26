using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class X360_XMA : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "XMA";
            }
        }

        public string Description
        {
            get
            {
                return "Microsoft XMA Audio File";
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
            // Check for Mark ID ("RIFF")
            if (fileReader.Read_32bitsBE(offset) != 0x52494646)
                return 0;

            // Check for Mark ID ("WAVE" + "fmt ")
            if ((fileReader.Read_32bitsBE(offset + 0x04) == 0x57415645) && (fileReader.Read_32bitsBE(offset + 0x0C) == 0x666D7420))
            {
                if ((fileReader.Read_16bits(offset + 0x14) == 0x0166) ||
                    (fileReader.Read_16bits(offset + 0x14) == 0x0165))
                {
                    if (VGM_Utils.CheckSampleRate(fileReader.Read_16bits(offset + 0x18)))
                    {
                        length = fileReader.Read_32bits(offset + 0x04) + 0x8;
                        return length;
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
