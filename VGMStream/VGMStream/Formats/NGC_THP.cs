using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class NGC_THP : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "THP";
            }
        }

        public string Description
        {
            get
            {
                return "Nintendo Movie (Audio only)";
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
                return true;
            }
        }

        public UInt64 IsFormat(StreamReader.IReader fileReader, UInt64 offset, UInt64 length)
        {
            // Check for Mark ID ("THP")
            if (fileReader.Read_32bitsBE(offset) != 0x54485000)
                return 0;

            // check if thp is correct ...
            //if (fileReader.Read_32bitsBE(offset + 0x1C) + 0x60 != length)
            //    return 0;

            // check if thp has audio 
            if (fileReader.Read_32bitsBE(offset + 0x0C) == 0)
                return 0;

            return length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            UInt64 start_offset;
            UInt64 i;

            int thpVersion = fileReader.Read_8Bits(offset + 0x06);

            /* fill in the vital statistics */
            start_offset = fileReader.Read_32bitsBE(offset + 0x28);

            // Get info from the first block
            UInt64 componentTypeOffset = offset + fileReader.Read_32bitsBE(offset + 0x20);
            uint numComponents = fileReader.Read_32bitsBE(componentTypeOffset);
            UInt64 componentDataOffset = componentTypeOffset + 0x14;
            componentTypeOffset += 4;

            for (i = 0; i < numComponents; i++)
            {
                if (fileReader.Read_8Bits(componentTypeOffset + i) == 1) // audio block
                {
                    uint channel_count = fileReader.Read_32bitsBE(componentDataOffset);

                    /* build the VGMSTREAM */
                    VGM_Utils.allocate_vgmStream(ref vgmStream, (int)channel_count, false);

                    vgmStream.vgmChannelCount = (int)channel_count;
                    vgmStream.vgmSampleRate = (int)fileReader.Read_32bitsBE(componentDataOffset + 4);
                    vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(componentDataOffset + 8);
                    break;
                }
                else
                {
                    if (thpVersion == 0x10)
                        componentDataOffset += 0x0c;
                    else
                        componentDataOffset += 0x08;
                }
            }

            vgmStream.vgmTHPNextFrameSize = fileReader.Read_32bitsBE(offset + 0x18);
            vgmStream.vgmDecoder = new DSP_Decoder();
            vgmStream.vgmLayout = new Blocked();
            vgmStream.vgmLayoutType = VGM_Layout_Type.THP_Blocked;

            if (InitReader)
            {
                for (i = 0; i < 2; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());
                }
                BlockedFnts.THP_Block_Update(offset + start_offset, ref vgmStream);
            }
        }
    }
}
