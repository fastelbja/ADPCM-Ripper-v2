using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream 
{
    public class PS2_ADS : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "ADS";
            }
        }

        public string Description
        {
            get
            {
                return "Sony Common Audio File (SShd/SSbd)";
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
            // Check for Mark ID ("SShd")
            if (fileReader.Read_32bitsBE(offset) != 0x53536864)
                return 0;

            // Check for Mark ID ("SSbd")
            if (fileReader.Read_32bitsBE(offset+0x20) != 0x53536264)
                return 0;

            uint channel_count = fileReader.Read_32bits(offset + 0x10);
            uint sample_rate = fileReader.Read_32bits(offset + 0x0c);

            if (!VGM_Utils.CheckChannels(channel_count))
                return 0;

            if (!VGM_Utils.CheckSampleRate(sample_rate))
                return 0;

            uint fileLength = fileReader.Read_32bits(offset + 0x024);

            return (fileLength+0x28);
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {

            bool loop_flag = (fileReader.Read_32bits(offset + 0x1C) != 0xFFFFFFFF);
            int channel_count = (int)fileReader.Read_32bits(offset + 0x10);
            int sample_rate = (int)fileReader.Read_32bits(offset + 0x0c);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);

            vgmStream.vgmLoopFlag = loop_flag;
            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmSampleRate = sample_rate;
            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bits(offset + 0x24)/16*28/channel_count;
            vgmStream.vgmLayout = new Interleave();
            vgmStream.vgmInterleaveBlockSize = (int)fileReader.Read_32bits(offset + 0x14);
            vgmStream.vgmDecoder = new PSX_Decoder();

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = ((int)fileReader.Read_32bits(offset + 0x18) * 0x10) / 16 * 28 / vgmStream.vgmChannelCount;
                vgmStream.vgmLoopEndSample = ((int)fileReader.Read_32bits(offset + 0x1C) * 0x10) / 16 * 28 / vgmStream.vgmChannelCount;

                if (vgmStream.vgmLoopEndSample > vgmStream.vgmTotalSamples)
                {
                    vgmStream.vgmLoopStartSample = ((int)fileReader.Read_32bits(offset + 0x18)) / 16 * 28 / vgmStream.vgmChannelCount;
                    vgmStream.vgmLoopEndSample = ((int)fileReader.Read_32bits(offset + 0x1C)) / 16 * 28 / vgmStream.vgmChannelCount;
                }
            }

            UInt64 start_offset = offset + 0x28;

            if (!VGM_Utils.IsPS2ADPCM(fileReader, start_offset, start_offset + 0x8000))
            {
                start_offset = offset + 0x800;
                if (!VGM_Utils.IsPS2ADPCM(fileReader, start_offset, start_offset + 0x8000))
                {
                    start_offset = offset + 0x28;
                    vgmStream.vgmDecoder = new PCM16_Decoder();
                    vgmStream.vgmDecoderType = VGM_Decoder_Type.PCM16BITS;
                    vgmStream.vgmTotalSamples = (int)fileReader.Read_32bits(offset + 0x24) / 2 / channel_count;

                    if (loop_flag)
                    {
                        vgmStream.vgmLoopStartSample = (int)fileReader.Read_32bits(offset + 0x18);
                        vgmStream.vgmLoopEndSample = (int)fileReader.Read_32bits(offset + 0x1C);
                    }
                }
            }

            if (InitReader)
            {
                for (int i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = start_offset + (UInt64)(vgmStream.vgmInterleaveBlockSize * i);
                }
            }
        }
    }
}
