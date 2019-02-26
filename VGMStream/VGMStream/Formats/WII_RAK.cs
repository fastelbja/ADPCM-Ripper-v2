using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class WII_RAK : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "RAK";
            }
        }

        public string Description
        {
            get
            {
                return "WII RAKI Audio File";
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

            // Check for Mark ID ("RAKI")
            if (fileReader.Read_32bitsBE(offset) != 0x52414B49)
                return 0;

            // Check for Mark ID ("Cafeadpc")
            if ((fileReader.Read_32bitsBE(offset + 8) != 0x43616665) && (fileReader.Read_32bitsBE(offset + 10) != 0x61647063))
                return 0;

            uint file_length = fileReader.Read_32bitsBE(offset + 0x4C) + fileReader.Read_32bitsBE(offset + 0x14);

            if (fileReader.Read_32bitsBE(offset + 0x44) == 0x6461744C)
                file_length = (fileReader.Read_32bitsBE(offset + 0x58) * 2) + 0x1C + 0x1C +  fileReader.Read_32bitsBE(offset + 0x14);;

            return file_length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            UInt64 startOffset =0;
            UInt64[] start_offset = new UInt64[2] {0x140,0x148};

            if (fileReader.Read_32bitsBE(offset + 0x44) == 0x6461744C) // datL
            {
                startOffset = 0x5e;
                start_offset[0] = fileReader.Read_32bitsBE(offset + 0x48);
                start_offset[1] = fileReader.Read_32bitsBE(offset + 0x54);
            }
            else
                startOffset = 0x52;

            int channel_count = fileReader.Read_16bitsBE(offset + startOffset);

            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, false);

            vgmStream.vgmDecoder = new DSP_Decoder();
            vgmStream.vgmLayout = new Interleave();
            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmLoopFlag = false;
            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bitsBE(offset + startOffset + 0x14) / 8 * 14) / channel_count;

            vgmStream.vgmSampleRate = fileReader.Read_16bitsBE(offset + startOffset + 0x4);

            if (channel_count == 1)
                vgmStream.vgmLayout = new NoLayout();
            else
            {
                vgmStream.vgmLayout = new Interleave();
                vgmStream.vgmLayoutType = VGM_Layout_Type.Interleave_With_Shortblock;
            }

            vgmStream.vgmInterleaveBlockSize = 0x08;

            int i, j;

            for (j = 0; j < vgmStream.vgmChannelCount; j++)
            {
                for (i = 0; i < 16; i++)
                {
                    vgmStream.vgmChannel[j].adpcm_coef[i] = (Int16)fileReader.Read_16bitsBE(offset + startOffset + (UInt64)(0x2C  + (i * 2) + (j * 0x60)));
                }
            }

            if (InitReader)
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());

                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = offset + start_offset[i];
                }
            }

        }
    }
}
