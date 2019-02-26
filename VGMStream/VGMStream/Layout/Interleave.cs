using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class Interleave : IVGMLayout
    {
        public string GetDescription()
        {
            return "Interleave";
        }

        public int update(ref short[] vgmBuffer, int vgmSampleCount, VGM_Stream vgmStream)
        {
            int samples_written = 0;
            int i;

            int frame_size = vgmStream.vgmDecoder.FrameSize();
            int samples_per_frame = vgmStream.vgmDecoder.SamplesPerFrame();
            int samples_this_block;

            vgmStream.vgmSamplesBlockOffset = 0;

            if (vgmStream.vgmDecodedSamples >= vgmStream.vgmTotalSamplesWithLoop)
                return 0;

            samples_this_block = vgmStream.vgmInterleaveBlockSize / frame_size * samples_per_frame;

            if ((vgmStream.vgmLayoutType == VGM_Layout_Type.Interleave_With_Shortblock) &&
                (vgmStream.vgmSamplePlayed - vgmStream.vgmSamplesIntoBlock + samples_this_block > vgmStream.vgmTotalSamples))
            {
                frame_size = vgmStream.vgmDecoder.ShortFrameSize();
                samples_per_frame = vgmStream.vgmDecoder.SamplesPerShortFrame();

                samples_this_block = vgmStream.vgmInterleaveShortBlockSize / frame_size * samples_per_frame;
            }

            while (samples_written < vgmSampleCount)
            {
                int samples_to_do;

                if (vgmStream.vgmLoopFlag && VGM_Utils.vgmstream_do_loop(vgmStream))
                {
                    /* we assume that the loop is not back into a short block */
                    if (vgmStream.vgmLayoutType == VGM_Layout_Type.Interleave_With_Shortblock)
                    {
                        frame_size = vgmStream.vgmDecoder.FrameSize();
                        samples_per_frame = vgmStream.vgmDecoder.SamplesPerFrame();
                        samples_this_block = vgmStream.vgmInterleaveBlockSize / frame_size * samples_per_frame;
                    }
                    continue;
                }

                samples_to_do = VGM_Utils.vgmstream_samples_to_do(samples_this_block, samples_per_frame, vgmStream);

                if (samples_written + samples_to_do > vgmSampleCount)
                    samples_to_do = vgmSampleCount - samples_written;

                for (i = 0; i < vgmStream.vgmChannelCount; i++)
                {
                    vgmStream.vgmDecoder.Decode(vgmStream, vgmStream.vgmChannel[i], ref vgmBuffer, vgmStream.vgmChannelCount, vgmStream.vgmSamplesIntoBlock, samples_to_do,i);
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
                    if ((vgmStream.vgmLayoutType == VGM_Layout_Type.Interleave_With_Shortblock) &&
                        (vgmStream.vgmSamplePlayed + samples_this_block > vgmStream.vgmTotalSamples))
                    {
                        frame_size = vgmStream.vgmDecoder.ShortFrameSize();
                        samples_per_frame = vgmStream.vgmDecoder.SamplesPerShortFrame();

                        samples_this_block = vgmStream.vgmInterleaveShortBlockSize / frame_size * samples_per_frame;
                        for (i = 0; i < vgmStream.vgmChannelCount; i++)
                            vgmStream.vgmChannel[i].currentOffset += (UInt64)((vgmStream.vgmInterleaveBlockSize * (vgmStream.vgmChannelCount - i)) + (vgmStream.vgmInterleaveShortBlockSize * i));
                    }
                    else
                    {
                        for (i = 0; i < vgmStream.vgmChannelCount; i++)
                            vgmStream.vgmChannel[i].currentOffset += (UInt64)(vgmStream.vgmInterleaveBlockSize * vgmStream.vgmChannelCount);
                    }
                    vgmStream.vgmSamplesIntoBlock = 0;
                }
            }

            return samples_written;
        }
    }
}
