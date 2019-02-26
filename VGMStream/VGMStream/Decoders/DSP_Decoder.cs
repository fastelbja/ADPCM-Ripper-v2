using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    class DSP_Decoder : IVGMDecoder
    {
        public int FrameSize()
        {
            // 8 bytes per frame
            return 8;
        }

        public int ShortFrameSize()
        {
            // No short frame 
            return 8;
        }

        public int SamplesPerFrame()
        {
            // 14 Samples per frame
            return 14;
        }

        public int SamplesPerShortFrame()
        {
            // No short frames on ps1/ps2
            return 14;
        }

        public string Description
        {
            get
            {
                return "Nintendo APDCM";
            }
        }

        public void Decode(VGM_Stream vgmStream, VGM_Channel vgmChannel, ref short[] vgmOutput, int vgmChannelSpacing,
                           int vgmFirstSample, int vgmSamplesToDo, int vgmChannelNumber)
        {
            int framesin = vgmFirstSample/14;
            vgmFirstSample = vgmFirstSample % 14;

            byte header = vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)(framesin *8));
            int scale = 1 << (header & 0xf);
            int coef_index = (header >> 4) & 0xf;

            int vgmSampleHistory1 = vgmChannel.adpcm_history_32bits_1;
            int vgmSampleHistory2 = vgmChannel.adpcm_history_32bits_2;

            try
            {
                int coef1 = vgmChannel.adpcm_coef[coef_index * 2];
                int coef2 = vgmChannel.adpcm_coef[coef_index * 2 + 1];

                int vgmSampleCount, i;
                int vgmSample;

                for (i = vgmFirstSample, vgmSampleCount = 0; i < vgmFirstSample + vgmSamplesToDo; i++, vgmSampleCount += vgmChannelSpacing)
                {
                    int sample_byte = vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)(framesin * 8 + 1 + i / 2));

                    vgmSample = (((i & 1) == 1) ? VGM_Utils.LOWNIBBLE_SIGNED(sample_byte)
                                                : VGM_Utils.HINIBBLE_SIGNED(sample_byte));

                    vgmSample = ((vgmSample * scale) << 11) + 1024;
                    vgmSample = vgmSample + (coef1 * vgmSampleHistory1 + coef2 * vgmSampleHistory2);
                    vgmSample = vgmSample >> 11;

                    vgmOutput[vgmStream.vgmSamplesBlockOffset + vgmSampleCount + vgmChannelNumber] = VGM_Utils.clamp16(vgmSample);

                    vgmSampleHistory2 = vgmSampleHistory1;
                    vgmSampleHistory1 = vgmSample;
                }

                vgmChannel.adpcm_history_32bits_1 = vgmSampleHistory1;
                vgmChannel.adpcm_history_32bits_2 = vgmSampleHistory2;
            }
            catch (Exception e)
            {
                Console.Write(e.Message + " in DSP_Decoder");
            }
        }

    }
}
