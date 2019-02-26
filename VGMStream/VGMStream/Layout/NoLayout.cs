using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    class NoLayout : IVGMLayout
    {
        public string GetDescription()
        {
            return "No Layout";
        }

        public int update(ref short[] vgmBuffer, int vgmSampleCount, VGM_Stream vgmStream)
        {
            int i;
            int samples_written = 0;

            int samples_this_block = vgmStream.vgmTotalSamples;
            int samples_per_frame = vgmStream.vgmDecoder.SamplesPerFrame();

            vgmStream.vgmSamplesBlockOffset = 0;

            if (vgmStream.vgmDecodedSamples >= vgmStream.vgmTotalSamplesWithLoop)
                return 0;

            while (samples_written < vgmSampleCount)
            {
                int samples_to_do;

                if (vgmStream.vgmLoopFlag && VGM_Utils.vgmstream_do_loop(vgmStream))
                {
                    continue;
                }

                samples_to_do = VGM_Utils.vgmstream_samples_to_do(samples_this_block, samples_per_frame, vgmStream);

                if (samples_written + samples_to_do > vgmSampleCount)
                    samples_to_do = vgmSampleCount - samples_written;

                for (i = 0; i < vgmStream.vgmChannelCount; i++)
                {
                    vgmStream.vgmDecoder.Decode(vgmStream, vgmStream.vgmChannel[i], ref vgmBuffer, vgmStream.vgmChannelCount, vgmStream.vgmSamplesIntoBlock, samples_to_do, i);
                }
                vgmStream.vgmSamplesBlockOffset += samples_to_do * vgmStream.vgmChannelCount;
                samples_written += samples_to_do;
                vgmStream.vgmSamplePlayed += samples_to_do;
                vgmStream.vgmDecodedSamples += samples_to_do;

                if (vgmStream.vgmDecodedSamples >= vgmStream.vgmTotalSamplesWithLoop)
                    return samples_written;

                vgmStream.vgmSamplesIntoBlock += samples_to_do;
            }

            return samples_written;

        }
    }
}
