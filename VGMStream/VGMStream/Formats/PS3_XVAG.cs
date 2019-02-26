using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PS3_XVAG : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "XVAG";
            }
        }

        public string Description
        {
            get
            {
                return "PS3 Xvag";
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
            if (fileReader.Read_32bitsBE(offset) != 0x58564147)
                return 0;

            uint channel_count = fileReader.Read_32bitsBE(offset + 0x28);
            uint sample_rate = fileReader.Read_32bitsBE(offset + 0x3C);

            if (!VGM_Utils.CheckChannels(channel_count))
                return 0;

            if (!VGM_Utils.CheckSampleRate(sample_rate))
                return 0;

            uint fileLength = fileReader.Read_32bitsBE(offset + 0x04)+fileReader.Read_32bitsBE(offset + 0x40);
            return (fileLength);
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {

            bool loop_flag = false; //(fileReader.Read_32bits(offset + 0x7FC) != 0);
            int channel_count = (int)fileReader.Read_32bitsBE(offset + 0x28);
            int sample_rate = (int)fileReader.Read_32bitsBE(offset + 0x3C);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);

            vgmStream.vgmLoopFlag = loop_flag;
            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmSampleRate = sample_rate;
            vgmStream.vgmTotalSamples = (int)((fileReader.Read_32bitsBE(offset + 0x40) * 28 / 16)/2);
            vgmStream.vgmLayout = new Interleave();
            vgmStream.vgmInterleaveBlockSize = 0x10;
            vgmStream.vgmDecoder = new PSX_Decoder();

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)fileReader.Read_32bits(offset + 0x7FC) * 28 / 16;
                vgmStream.vgmLoopEndSample = vgmStream.vgmTotalSamples;
            }

            UInt64 start_offset = offset + fileReader.Read_32bitsBE(offset + 0x04);

            if (InitReader)
            {
                for (int i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = (start_offset + (UInt64)(vgmStream.vgmInterleaveBlockSize * i));
                }
            }
        }
    }
}