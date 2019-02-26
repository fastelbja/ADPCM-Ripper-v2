using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PS2_NPSF : IVGMFormat
    {
        private string m_Filename;

        public string Extension
        {
            get
            {
                return "NPSF";
            }
        }

        public string Description
        {
            get
            {
                return "NPSF (Namco Production Sound File ?)";
            }
        }

        public string Filename
        {
            get
            {
                return m_Filename;
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
            // Check for Mark ID ("NPSF")
            if (fileReader.Read_32bitsBE(offset) != 0x4E505346)
                return 0;

            uint channel_count = fileReader.Read_32bits(offset + 0x0C);
            uint sample_rate = fileReader.Read_32bits(offset + 0x18);

            if (!VGM_Utils.CheckChannels(channel_count))
                return 0;

            if (!VGM_Utils.CheckSampleRate(sample_rate))
                return 0;

            if (fileReader.Read_32bits(offset + 0x10) != 0x800)
                return 0;

            uint fileLength = fileReader.Read_32bits(offset + 0x08);
            m_Filename = fileReader.Read_String(offset + 0x34,0x20);

            if (channel_count == 1)
                return (fileLength + 0x800);
            else
            {
                uint blockCount = fileLength / 0x800;
                if ((fileLength % 0x800) != 0)
                    blockCount++;

                fileLength = (blockCount * channel_count * 0x800) + 0x800;
            }
            return fileLength;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {

            bool loop_flag = (fileReader.Read_32bits(offset + 0x14) != 0xFFFFFFFF);
            int channel_count = (int)fileReader.Read_32bits(offset + 0x0C);
            int sample_rate = (int)fileReader.Read_32bits(offset + 0x18);
            m_Filename = fileReader.Read_String(offset + 0x34, 0x20);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);

            vgmStream.vgmLoopFlag = loop_flag;
            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmSampleRate = sample_rate;
            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bits(offset + 0x08) * 28 / 16);

            if(channel_count==1)
                vgmStream.vgmLayout = new NoLayout();
            else
                vgmStream.vgmLayout = new Interleave();

            vgmStream.vgmInterleaveBlockSize = (int)(fileReader.Read_32bits(offset + 0x04) / 2);
            vgmStream.vgmDecoder = new PSX_Decoder();

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)fileReader.Read_32bits(offset + 0x14);
                vgmStream.vgmLoopEndSample = (int)(fileReader.Read_32bits(offset + 0x08) * 28 / 16);
            }

            UInt64 start_offset = offset + fileReader.Read_32bits(offset + 0x10);

            if (InitReader)
            {
                for (int i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = start_offset +(UInt64)(vgmStream.vgmInterleaveBlockSize * i);
                }
            }
        }
    }
}
