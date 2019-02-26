using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VGMStream
{
    public class VGM_Utils
    {
        /* signed nibbles come up a lot */
        private static int[] NibbleToInt = new int[16] { 0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1 };

        public static short clamp16(Int32 vgmSample)
        {
            if (vgmSample > 32767) return 32767;
            if (vgmSample < (-32768)) return (-32768);
            return (short)vgmSample;
        }

        public static Int16 clamp16(double vgmSample)
        {
            if (vgmSample > 32767) return (Int16)32767;
            if (vgmSample < -32768) return (Int16)(-32768);
            return (Int16)vgmSample;
        }

        /// <summary>
        /// Test if we enter a loop point
        /// </summary>
        /// <param name="vgmstream">VGM_Stream Class previously instantied</param>
        /// <returns>True if we reach a loop point (start or end)</returns>
        public static bool vgmstream_do_loop(VGM_Stream vgmstream)
        {
            int i;
            int j;

            // Loop End ?
            if (vgmstream.vgmSamplePlayed == vgmstream.vgmLoopEndSample)
            {
                for (i = 0; i < vgmstream.vgmChannelCount; i++)
                {
                    // Restore the state of each channel when loop was reached ...
                    vgmstream.vgmChannel[i].adpcm_history_16bits_1 = vgmstream.vgmChannelAtLoop[i].adpcm_history_16bits_1;
                    vgmstream.vgmChannel[i].adpcm_history_16bits_2 = vgmstream.vgmChannelAtLoop[i].adpcm_history_16bits_2;
                    vgmstream.vgmChannel[i].adpcm_history_32bits_1 = vgmstream.vgmChannelAtLoop[i].adpcm_history_32bits_1;
                    vgmstream.vgmChannel[i].adpcm_history_32bits_2 = vgmstream.vgmChannelAtLoop[i].adpcm_history_32bits_2;
                    vgmstream.vgmChannel[i].adx_channels = vgmstream.vgmChannelAtLoop[i].adx_channels;
                    vgmstream.vgmChannel[i].adx_xor = vgmstream.vgmChannelAtLoop[i].adx_xor;
                    vgmstream.vgmChannel[i].adx_mult = vgmstream.vgmChannelAtLoop[i].adx_mult;
                    vgmstream.vgmChannel[i].adx_add = vgmstream.vgmChannelAtLoop[i].adx_add;

                    for (j = 0; j < 16; j++)
                    {
                        vgmstream.vgmChannel[i].adpcm_coef[j] = vgmstream.vgmChannelAtLoop[i].adpcm_coef[j];
                    }

                    vgmstream.vgmChannel[i].currentOffset = vgmstream.vgmChannelAtLoop[i].currentOffset;
                }
                vgmstream.vgmSamplePlayed = vgmstream.vgmSamplePlayedInLoop;
                vgmstream.vgmSamplesIntoBlock = vgmstream.vgmSamplesIntoBlockInLoop;
                vgmstream.vgmCurrentBlockSize = vgmstream.vgmCurrentBlockSizeInLoop;
                vgmstream.vgmCurrentBlockOffset = vgmstream.vgmCurrentBlockOffsetInLoop;
                vgmstream.vgmNextBlockOffset = vgmstream.vgmNextBlockOffsetInLoop;
                return true;
            }

            // Loop Start ?
            if ((!vgmstream.vgmHitLoop) && (vgmstream.vgmSamplePlayed == vgmstream.vgmLoopStartSample))
            {
                for (i = 0; i < vgmstream.vgmChannelCount; i++)
                {
                    // Save the state of each channel when loop is reached
                    vgmstream.vgmChannelAtLoop[i].adpcm_history_16bits_1 = vgmstream.vgmChannel[i].adpcm_history_16bits_1;
                    vgmstream.vgmChannelAtLoop[i].adpcm_history_16bits_2 = vgmstream.vgmChannel[i].adpcm_history_16bits_2;
                    vgmstream.vgmChannelAtLoop[i].adpcm_history_32bits_1 = vgmstream.vgmChannel[i].adpcm_history_32bits_1;
                    vgmstream.vgmChannelAtLoop[i].adpcm_history_32bits_2 = vgmstream.vgmChannel[i].adpcm_history_32bits_2;

                    vgmstream.vgmChannelAtLoop[i].adx_channels = vgmstream.vgmChannel[i].adx_channels;
                    vgmstream.vgmChannelAtLoop[i].adx_xor = vgmstream.vgmChannel[i].adx_xor;
                    vgmstream.vgmChannelAtLoop[i].adx_mult = vgmstream.vgmChannel[i].adx_mult;
                    vgmstream.vgmChannelAtLoop[i].adx_add = vgmstream.vgmChannel[i].adx_add;

                    for (j = 0; j < 16; j++)
                    {
                        vgmstream.vgmChannelAtLoop[i].adpcm_coef[j] = vgmstream.vgmChannel[i].adpcm_coef[j];
                    }

                    vgmstream.vgmChannelAtLoop[i].currentOffset = vgmstream.vgmChannel[i].currentOffset;
                    vgmstream.vgmCurrentBlockSizeInLoop=vgmstream.vgmCurrentBlockSize;
                    vgmstream.vgmCurrentBlockOffsetInLoop=vgmstream.vgmCurrentBlockOffset;
                    vgmstream.vgmNextBlockOffsetInLoop = vgmstream.vgmNextBlockOffset;

                }
                vgmstream.vgmSamplePlayedInLoop = vgmstream.vgmSamplePlayed;
                vgmstream.vgmSamplesIntoBlockInLoop = vgmstream.vgmSamplesIntoBlock;
                vgmstream.vgmHitLoop = true;
            }
            return false;
        }

        /// <summary>
        /// Calculate the number of samples which need to be done
        /// </summary>
        /// <param name="samples_this_block"></param>
        /// <param name="samples_per_frame"></param>
        /// <param name="vgmstream"></param>
        /// <returns></returns>
        public static int vgmstream_samples_to_do(int samples_this_block, int samples_per_frame, VGM_Stream vgmstream)
        {
            int samples_to_do;
            int samples_left_this_block;

            samples_left_this_block = samples_this_block - vgmstream.vgmSamplesIntoBlock;
            samples_to_do = samples_left_this_block;

            /* fun loopy crap */
            /* Why did I think this would be any simpler? */
            if (vgmstream.vgmLoopFlag)
            {
                /* are we going to hit the loop end during this block? */
                if (vgmstream.vgmSamplePlayed + samples_left_this_block > vgmstream.vgmLoopEndSample)
                {
                    /* only do to just before it */
                    samples_to_do = vgmstream.vgmLoopEndSample - vgmstream.vgmSamplePlayed;
                }

                /* are we going to hit the loop start during this block? */
                if (!vgmstream.vgmHitLoop && vgmstream.vgmSamplePlayed + samples_left_this_block > vgmstream.vgmLoopStartSample)
                {
                    /* only do to just before it */
                    samples_to_do = vgmstream.vgmLoopStartSample - vgmstream.vgmSamplePlayed;
                }
            }

            /* if it's a framed encoding don't do more than one frame */
            if ((samples_per_frame > 1) && ((vgmstream.vgmSamplesIntoBlock % samples_per_frame) + samples_to_do > samples_per_frame))
                samples_to_do = samples_per_frame - (vgmstream.vgmSamplesIntoBlock % samples_per_frame);

            return samples_to_do;
        }

        /// <summary>
        /// Test if the current block contains PS2 ADPCM
        /// </summary>
        /// <param name="Reader">StreamReader.IReader Class</param>
        /// <param name="StartOffset">Start offset of the block</param>
        /// <param name="EndOffset">End offset of the block</param>
        /// <returns>True if all the block contains PS2 ADPCM</returns>
        public static bool IsPS2ADPCM(StreamReader.IReader Reader, UInt64 StartOffset, UInt64 EndOffset)
        {
            byte[] buffer = new byte[0x10];
            UInt64 offset = StartOffset;

            do {
                buffer = Reader.Read(offset, 0x10);
                offset += 0x10;
                if ((HINIBBLE(buffer[0]) > 5) || ((LONIBBLE(buffer[0])) > 0xC) || (buffer[1] > 7))
                    return false;
            } while (offset < EndOffset);
            return true;
        }

        /// <summary>
        /// Calculate the total count of samples of the stream
        /// </summary>
        /// <param name="looptimes">Number of loops</param>
        /// <param name="fadeseconds">Number of seconds used for fading</param>
        /// <param name="fadedelayseconds">Delay beween each fading</param>
        /// <param name="vgmstream">VGM Stream Class previously instantied</param>
        /// <returns>Number of samples (loops include)</returns>
        public static Int32 get_vgmstream_play_samples(double looptimes, double fadeseconds, double fadedelayseconds, VGM_Stream vgmstream)
        {
            if (vgmstream.vgmLoopFlag)
            {
                return (Int32)(vgmstream.vgmLoopStartSample + (vgmstream.vgmLoopEndSample - vgmstream.vgmLoopStartSample) * looptimes + (fadedelayseconds + fadeseconds) * vgmstream.vgmSampleRate);
            }
            else return vgmstream.vgmTotalSamples;
        }
        
        /// <summary>
        /// Allocate each structures used by VGM Stream
        /// </summary>
        /// <param name="vgmStream">VGM Stream Class previously instantied</param>
        /// <param name="channels">Channel count of the current stream</param>
        /// <param name="loop_flag">Does this loop ?</param>
        public static void allocate_vgmStream(ref VGM_Stream vgmStream, int channels, bool loop_flag)
        {
            int i;

            vgmStream = new VGM_Stream();
            vgmStream.vgmChannel = new VGM_Channel[channels];
            vgmStream.vgmChannelAtStart = new VGM_Channel[channels];

            for (i = 0; i < channels; i++)
            {
                vgmStream.vgmChannel[i] = new VGM_Channel();
                vgmStream.vgmChannelAtStart[i] = new VGM_Channel();
            }

            if (loop_flag)
            {
                vgmStream.vgmChannelAtLoop = new VGM_Channel[channels];
                for (i = 0; i < channels; i++)
                {
                    vgmStream.vgmChannelAtLoop[i] = new VGM_Channel();
                }
            }
        }

        /// <summary>
        /// Return the high nibbles of a byte (ex : HINIBBLE(0x1F)=1)
        /// </summary>
        /// <param name="bByte">a Byte</param>
        /// <returns>High nibbles of the byte</returns>
        public static byte HINIBBLE(byte bByte)
        {
            return (byte)(((bByte) >> 4) & 0x0F);
        }

        /// <summary>
        /// Return the low nibbles of a byte (ex : LONIBBLE(0x1F)=0xF)
        /// </summary>
        /// <param name="bByte">a Byte</param>
        /// <returns>Low nibbles of the byte</returns>
        public static byte LONIBBLE(byte bByte)
        {
            return (byte)((bByte) & 0x0F);
        }

        public static int HINIBBLE_SIGNED(int n)
        {
            return NibbleToInt[n >> 4];
        }

        public static int LOWNIBBLE_SIGNED(int n)
        {
            return NibbleToInt[n & 0xf];
        }

        public static bool CheckSampleRate(uint SampleRate)
        {
            return ((SampleRate >= 7999) && (SampleRate <= 48000));
        }

        public static bool CheckChannels(uint Channels)
        {
            return ((Channels >= 1) && (Channels <= 8));
        }

        public static void adx_next_key(ref VGM_Channel vgmChannel)
        {
            vgmChannel.adx_xor = (UInt16)((vgmChannel.adx_xor * vgmChannel.adx_mult + vgmChannel.adx_add) & 0x7fff);
        }

    }
}
