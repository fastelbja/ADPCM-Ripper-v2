using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class ADXENC_Decoder : IVGMDecoder
    {
        public int FrameSize()
        {
            // 14 bytes per frame
            return 18; 
        }

        public int ShortFrameSize()
        {
            // No short frame on ps1/ps2, so return the same value
            return 18; 
        }

        public int SamplesPerFrame()
        {
            // 28 Samples per frame
            return 32; 
        }

        public int SamplesPerShortFrame()
        {
            // No short frames on ps1/ps2
            return 32; 
        }

        public string Description
        {
            get
            {
                return "CRI Encrypted ADX Decoder";
            }
        }

        public void Decode(VGM_Stream vgmStream, VGM_Channel vgmChannel, ref short[] vgmOutput, int vgmChannelSpacing,
                           int vgmFirstSample, int vgmSamplesToDo, int vgmChannelNumber)
        {
            int vgmSampleCount,i;

            int framesin = vgmFirstSample / 32;

            int scale = vgmChannel.fReader.Read_16bitsBE(vgmChannel.currentOffset + (UInt64)((framesin * 18) ^ vgmChannel.adx_xor)) + 1;
            int vgmSampleHistory1 = vgmChannel.adpcm_history_32bits_1;
            int vgmSampleHistory2 = vgmChannel.adpcm_history_32bits_2;
            int coef1 = vgmChannel.adpcm_coef[0];
            int coef2 = vgmChannel.adpcm_coef[1];
            int vgmSample;

            vgmFirstSample = vgmFirstSample % 32;

            for (i = vgmFirstSample, vgmSampleCount = 0; i < vgmFirstSample + vgmSamplesToDo; i++, vgmSampleCount += vgmChannelSpacing)
            {
                int sample_byte = vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)((framesin * 18) + 2 + i / 2));

                sample_byte = (((i & 1) == 1) ? VGM_Utils.LOWNIBBLE_SIGNED(sample_byte) : VGM_Utils.HINIBBLE_SIGNED(sample_byte));
                vgmSample = (sample_byte * scale);
                vgmSample += (((coef1 * vgmSampleHistory1) + (coef2 * vgmSampleHistory2)) >> 12);
                vgmOutput[vgmStream.vgmSamplesBlockOffset + vgmSampleCount + vgmChannelNumber] = VGM_Utils.clamp16(vgmSample);

                vgmSampleHistory2 = vgmSampleHistory1;
                vgmSampleHistory1 = vgmSample;
            }

            vgmChannel.adpcm_history_32bits_1 = vgmSampleHistory1;
            vgmChannel.adpcm_history_32bits_2 = vgmSampleHistory2;

            if ((i % 32)==0)
            {
                for (i = 0; i < vgmChannel.adx_channels; i++)
                {
                    VGM_Utils.adx_next_key(ref vgmChannel);
                }
            }
        }
    }
}
