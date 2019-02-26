using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class WII_SNS : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "SNS";
            }
        }

        public string Description
        {
            get
            {
                return "WII SNS Audio File";
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

            // Check for Mark ID ("RIFF")
            if (fileReader.Read_32bitsBE(offset) != 0x52494646)
                return 0;

            // Check for Mark ID ("WAVEfmt ")
            if ((fileReader.Read_32bitsBE(offset+8) != 0x57415645) && (fileReader.Read_32bitsBE(offset+10) != 0x666D7420))
                return 0;

            // Check for RIFF Wave ID (0x5050)
            if (fileReader.Read_16bitsBE(offset + 0x14) != 0x5050)
                return 0;


            uint file_length = fileReader.Read_32bits(offset + 0x4) + 8;

            return file_length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            Int16[] coef = new Int16[16] { 0x04ab, -0x0313, 0x0789, -0x0121, 0x09a2, -0x051b, 0x0c90, -0x053f, 0x084d, -0x055c, 0x0982, -0x0209, 0x0af6, -0x0506, 0x0be6, -0x040b };
            int channel_count = fileReader.Read_8Bits(offset + 0x16);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, false);

            vgmStream.vgmDecoder = new DSP_Decoder();
            vgmStream.vgmLayout = new Interleave();

            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmLoopFlag = false;
            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bits(offset + 0x42)/8*14)/channel_count;
            vgmStream.vgmSampleRate = fileReader.Read_16bits(offset + 0x18);

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
                    vgmStream.vgmChannel[j].adpcm_coef[i] = coef[i];
                }
            }

            UInt64 start_offset = offset + 0x46;

            if (InitReader)
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());

                    vgmStream.vgmChannel[i].currentOffset = start_offset + (UInt64)(i * vgmStream.vgmInterleaveBlockSize);
                }
            }

        }
    }
}
