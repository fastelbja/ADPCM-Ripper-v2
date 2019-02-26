using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PS2_VPK : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "VPK";
            }
        }

        public string Description
        {
            get
            {
                return "VPK";
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
            // Check for Mark ID (" KPV")
            if (fileReader.Read_32bitsBE(offset) != 0x204B5056)
                return 0;

            uint channel_count = fileReader.Read_32bits(offset + 0x14);
            uint sample_rate = fileReader.Read_32bits(offset + 0x10);

            if (!VGM_Utils.CheckChannels(channel_count))
                return 0;

            if (!VGM_Utils.CheckSampleRate(sample_rate))
                return 0;

            uint fileLength = fileReader.Read_32bits(offset + 0x04);
            uint interleave = fileReader.Read_32bits(offset + 0x0C)/channel_count;
            int blockCount = (int)(fileLength / interleave);

            if ((fileLength % interleave) != 0)
                blockCount++;

            fileLength = (uint)(blockCount * interleave * channel_count);

            return fileLength + fileReader.Read_32bits(offset + 0x08);
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {

            bool loop_flag = (fileReader.Read_32bits(offset + 0x7FC) != 0);
            int channel_count = (int)fileReader.Read_32bits(offset + 0x14);
            int sample_rate = (int)fileReader.Read_32bits(offset + 0x10);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);

            vgmStream.vgmLoopFlag = loop_flag;
            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmSampleRate = sample_rate;
            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bits(offset + 0x04) * 28 / 16);
            vgmStream.vgmLayout = new Interleave();
            vgmStream.vgmInterleaveBlockSize = (int)(fileReader.Read_32bits(offset + 0x0C) / 2);
            vgmStream.vgmDecoder = new PSX_Decoder();

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)fileReader.Read_32bits(offset + 0x7FC) * 28 / 16;
                vgmStream.vgmLoopEndSample = vgmStream.vgmTotalSamples;
            }

            UInt64 start_offset = offset + fileReader.Read_32bits(offset + 08);

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
