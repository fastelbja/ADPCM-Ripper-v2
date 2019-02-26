using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    class Blocked : IVGMLayout
    {
        public string GetDescription()
        {
            return "Blocked";
        }

        public int update(ref short[] vgmBuffer, int vgmSampleCount, VGM_Stream vgmStream)
        {
            int i;
            int samples_written=0;
            int frame_size =  vgmStream.vgmDecoder.FrameSize();
            int samples_per_frame = vgmStream.vgmDecoder.SamplesPerFrame();
            int samples_this_block;

            if (vgmStream.vgmDecodedSamples >= vgmStream.vgmTotalSamplesWithLoop)
                return 0;

            vgmStream.vgmSamplesBlockOffset = 0;

            if (frame_size == 0) 
                samples_this_block = vgmStream.vgmCurrentBlockSize * 2 * samples_per_frame;
            else 
                samples_this_block = vgmStream.vgmCurrentBlockSize / frame_size * samples_per_frame;


            while (samples_written < vgmSampleCount)
            {
                int samples_to_do;

                if (vgmStream.vgmLoopFlag && VGM_Utils.vgmstream_do_loop(vgmStream))
                {
                    if (frame_size == 0)
                        samples_this_block = vgmStream.vgmCurrentBlockSize * 2 * samples_per_frame;
                    else
                        samples_this_block = vgmStream.vgmCurrentBlockSize / frame_size * samples_per_frame;

                    continue;
                }

                samples_to_do = VGM_Utils.vgmstream_samples_to_do(samples_this_block, samples_per_frame, vgmStream);

                if (samples_written + samples_to_do > vgmSampleCount)
                    samples_to_do = vgmSampleCount - samples_written;

                if (vgmStream.vgmCurrentBlockOffset >= 0)
                {
                    for (i = 0; i < vgmStream.vgmChannelCount; i++)
                    {
                        vgmStream.vgmDecoder.Decode(vgmStream, vgmStream.vgmChannel[i], ref vgmBuffer, vgmStream.vgmChannelCount, vgmStream.vgmSamplesIntoBlock, samples_to_do, i);
                    }
                }

                samples_written += samples_to_do;

                vgmStream.vgmSamplesBlockOffset += samples_to_do * vgmStream.vgmChannelCount;
                vgmStream.vgmSamplePlayed += samples_to_do;
                vgmStream.vgmDecodedSamples += samples_to_do;
                vgmStream.vgmSamplesIntoBlock += samples_to_do;

                if (vgmStream.vgmDecodedSamples >= vgmStream.vgmTotalSamplesWithLoop)
                    return samples_written;

                if (vgmStream.vgmSamplesIntoBlock == samples_this_block)
                {
                    switch (vgmStream.vgmLayoutType)
                    {
                        case VGM_Layout_Type.CAF_Blocked:
                            BlockedFnts.CAF_Block_Update(vgmStream.vgmNextBlockOffset, ref vgmStream);
                            break;
                        case VGM_Layout_Type.THP_Blocked:
                            BlockedFnts.THP_Block_Update(vgmStream.vgmNextBlockOffset, ref vgmStream);
                            break;
                        default:
                            break;
                    }

                    if (frame_size == 0)
                        samples_this_block = vgmStream.vgmCurrentBlockSize * 2 * samples_per_frame;
                    else
                        samples_this_block = vgmStream.vgmCurrentBlockSize / frame_size * samples_per_frame;

                    vgmStream.vgmSamplesIntoBlock = 0;
                }
            }
            return samples_written;
        }
    }
}
