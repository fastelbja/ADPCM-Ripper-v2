using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class WII_BRSTM : IVGMFormat
    {
        private string m_Description = string.Empty;

        public string Extension
        {
            get
            {
                return "BRSTM";
            }
        }

        public string Description
        {
            get
            {
                return m_Description;
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
            if (fileReader.Read_32bitsBE(offset + 0) != 0x5253544D) /* "RSTM" */
                return 0;

            if ((fileReader.Read_32bitsBE(offset + 0x04) != 0xFEFF0100) && (fileReader.Read_32bitsBE(offset + 0x04) != 0xFEFF0001))
                return 0;

            /* get head offset, check */
            UInt64 head_offset = fileReader.Read_16bitsBE(offset + 0x0C);

            if (head_offset == 0x10)
                m_Description = "Nintendo RSTM Header v1";
            else
                m_Description = "Nintendo RSTM Header v2";

            UInt64 rl_head_offset = head_offset + offset;

            if (fileReader.Read_32bitsBE(head_offset + offset) != 0x48454144) /* "HEAD" */
                return 0;

            /* check type details */
            byte codec_number = fileReader.Read_8Bits((head_offset == 0x10) ? rl_head_offset + 0x8 : rl_head_offset + 0x20);
            bool loop_flag = (fileReader.Read_8Bits((head_offset == 0x10) ? rl_head_offset + 0x19 : rl_head_offset + 0x21) != 0);
            uint channel_count = fileReader.Read_8Bits((head_offset == 0x10) ? rl_head_offset + 0xa : rl_head_offset + 0x22);
            uint sample_rate = fileReader.Read_16bitsBE((head_offset == 0x10) ? rl_head_offset + 0xc : rl_head_offset + 0x24);

            if(!VGM_Utils.CheckChannels(channel_count))
                return 0;

            if (!VGM_Utils.CheckSampleRate(sample_rate))
                return 0;

            if (codec_number>2)
                return 0;

            return fileReader.Read_32bitsBE(offset + 0x08);
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            UInt64 head_offset = fileReader.Read_16bitsBE(offset + 0x0C);

            if (head_offset == 0x10)
                m_Description = "Nintendo RSTM Header v1";
            else
                m_Description = "Nintendo RSTM Header v2";

            UInt64 rl_head_offset = head_offset + offset;
            
            bool loop_flag = (fileReader.Read_8Bits((head_offset == 0x10) ? rl_head_offset + 0x19 : rl_head_offset + 0x21) != 0);
            bool isDSP = false;

            int codec_number = fileReader.Read_8Bits((head_offset == 0x10) ? rl_head_offset + 0x8 : rl_head_offset + 0x20);
            int channel_count = fileReader.Read_8Bits((head_offset == 0x10) ? rl_head_offset + 0xa : rl_head_offset + 0x22);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);
            
            switch (codec_number)
            {
                case 0:
                    vgmStream.vgmDecoder = new PCM8_Decoder();
                    break;
                case 1:
                    vgmStream.vgmDecoder = new PCM16_Decoder();
                    vgmStream.vgmDecoderType = VGM_Decoder_Type.PCM16BITSBE;
                    break;
                case 2:
                    isDSP = true;
                    vgmStream.vgmDecoder = new DSP_Decoder();
                    break;
            }

            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmLoopFlag = loop_flag;
            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE((head_offset == 0x10) ? rl_head_offset + 0x14 : rl_head_offset + 0x2c);
            vgmStream.vgmSampleRate = fileReader.Read_16bitsBE((head_offset == 0x10) ? rl_head_offset + 0xC : rl_head_offset + 0x24);

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)fileReader.Read_32bitsBE((head_offset == 0x10) ? rl_head_offset + 0x10 : rl_head_offset + 0x28);
                vgmStream.vgmLoopEndSample = vgmStream.vgmTotalSamples;
            }

            if (channel_count == 1)
                vgmStream.vgmLayout = new NoLayout();
            else
            {
                vgmStream.vgmLayout = new Interleave();
                vgmStream.vgmLayoutType = VGM_Layout_Type.Interleave_With_Shortblock;
            }

            vgmStream.vgmInterleaveBlockSize = (int)fileReader.Read_32bitsBE((head_offset == 0x10) ? rl_head_offset + 0x20 : rl_head_offset + 0x38);
            vgmStream.vgmInterleaveShortBlockSize = (int)fileReader.Read_32bitsBE((head_offset == 0x10) ? rl_head_offset + 0x30 : rl_head_offset + 0x48);

            if (isDSP)
            {
                int i, j;
                int coef_spacing =((head_offset == 0x10) ? 0x30 : 0x38);

                UInt64 coef_offset1 = fileReader.Read_32bitsBE(rl_head_offset + 0x1C);
                UInt64 coef_offset2 = fileReader.Read_32bitsBE(rl_head_offset + 0x10 + coef_offset1);
                UInt64 coef_offset = ((head_offset == 0x10) ? 0x38 : coef_offset2 + 0x10);
                
                for (j = 0; j < vgmStream.vgmChannelCount; j++)
                {
                    for (i = 0; i < 16; i++)
                    {
                        vgmStream.vgmChannel[j].adpcm_coef[i] = (Int16)fileReader.Read_16bitsBE(rl_head_offset + coef_offset + (UInt64)(j * coef_spacing + i * 2));
                    }
                }
            }

            UInt64 start_offset = offset + fileReader.Read_32bitsBE((head_offset == 0x10) ? rl_head_offset + 0x18 : rl_head_offset + 0x30);

            if (InitReader)
            {
                for (int i = 0; i < channel_count; i++)
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
