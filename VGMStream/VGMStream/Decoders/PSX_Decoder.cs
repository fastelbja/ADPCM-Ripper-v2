using System;
using System.Text;

namespace VGMStream
{
    public class PSX_Decoder : IVGMDecoder
    {
        protected double[,] VAG_f = new double[5,2] { 
                                         {   0.0        ,   0.0        },
                                         {  60.0 / 64.0 ,   0.0        },
		                                 { 115.0 / 64.0 , -52.0 / 64.0 },
		                                 {  98.0 / 64.0 , -55.0 / 64.0 } ,
		                                 { 122.0 / 64.0 , -60.0 / 64.0 } 
        };
        
        protected long[,] VAG_coefs = new long [5,2] {
                                         {   0 ,   0 },
                                         {  60 ,   0 },
                                         { 115 , -52 },
                                         {  98 , -55 } ,
                                         { 122 , -60 } 
        };

        public int FrameSize()
        {
            // 14 bytes per frame
            return 16; 
        }

        public int ShortFrameSize()
        {
            // No short frame on ps1/ps2, so return the same value
            return 16; 
        }

        public int SamplesPerFrame()
        {
            // 28 Samples per frame
            return 28; 
        }

        public int SamplesPerShortFrame()
        {
            // No short frames on ps1/ps2
            return 28; 
        }

        public string Description
        {
            get
            {
                return "PS2 ADPCM Decoder";
            }
        }

        public void Decode(VGM_Stream vgmStream, VGM_Channel vgmChannel, ref short[] vgmOutput, int vgmChannelSpacing, 
                           int vgmFirstSample, int vgmSamplesToDo,int vgmChannelNumber)
        {
            int i;

            double   vgmSample;
            int      vgmSampleCount;

            double      vgmSampleHistory1 = vgmChannel.adpcm_history_dbl_1;
            double      vgmSampleHistory2 = vgmChannel.adpcm_history_dbl_2;

            Int16    vgmScale;

            // Get the offset of the correct frame
            Int32    vgmFrameNum = vgmFirstSample / 28;

            Int16    vgmPredictor = (Int16)(vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)(vgmFrameNum * 16)) >> 4);
            Int16    vgmShiftFactor = (Int16)(vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)(vgmFrameNum * 16)) & 0x0F);
            byte vgmFlag = vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)((vgmFrameNum * 16) + 1));

            vgmFirstSample = vgmFirstSample % 28;

            for (i = vgmFirstSample, vgmSampleCount = 0; i < vgmFirstSample + vgmSamplesToDo; i++, vgmSampleCount += vgmChannelSpacing)
            {
                vgmSample = 0;

                if (vgmFlag < 0x07)
                {
                    Int16 sample_byte = vgmChannel.fReader.Read_8Bits(vgmChannel.currentOffset + (UInt64)((vgmFrameNum * 16) + 2 + i / 2));

                    vgmScale = (Int16)(((((i & 1) == 1) ? sample_byte >> 4 : (sample_byte & 0x0f))<<12));

                    if (vgmPredictor < 5)
                        vgmSample = (((vgmScale >> vgmShiftFactor) + vgmSampleHistory1 * VAG_f[vgmPredictor, 0] + vgmSampleHistory2 * VAG_f[vgmPredictor, 1]));
                    else
                        vgmSample = 0;
                }
                vgmOutput[vgmStream.vgmSamplesBlockOffset + vgmSampleCount + vgmChannelNumber] = VGM_Utils.clamp16(vgmSample);
                vgmSampleHistory2 = vgmSampleHistory1;
                vgmSampleHistory1 = vgmSample;
            }

            vgmChannel.adpcm_history_dbl_1 = vgmSampleHistory1;
            vgmChannel.adpcm_history_dbl_2 = vgmSampleHistory2;
		}
	}
}
