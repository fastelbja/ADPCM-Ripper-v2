using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    class PCM8_Decoder : IVGMDecoder
    {
        public int FrameSize()
        {
            // 8 bytes per frame
            return 1;
        }

        public int ShortFrameSize()
        {
            // No short frame 
            return 1;
        }

        public int SamplesPerFrame()
        {
            // 1 Sample per frame
            return 1;
        }

        public int SamplesPerShortFrame()
        {
            // No short frames
            return 1;
        }

        public string Description
        {
            get
            {
                return "Standard 8bit PCM";
            }
        }

        public void Decode(VGM_Stream vgmStream, VGM_Channel vgmChannel, ref short[] vgmOutput, int vgmChannelSpacing,
                           int vgmFirstSample, int vgmSamplesToDo, int vgmChannelNumber)
        {
            for (int i = vgmFirstSample, vgmSampleCount = 0; i < vgmFirstSample + vgmSamplesToDo; i++, vgmSampleCount += vgmChannelSpacing)
            {
                int offset = vgmStream.vgmSamplesBlockOffset + vgmSampleCount + vgmChannelNumber;

                vgmOutput[offset] = (short)(vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)(i * 2)) * 0x100);
            }
        }
    }
}
