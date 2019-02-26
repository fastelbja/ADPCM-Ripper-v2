using System;
using System.Text;

namespace VGMStream
{
    public class PS2_VIG : IVGMFormat
    {
        private string m_Filename = "";

        public string Extension
        {
            get { return "VIG"; }
        }

        public string Description
        {
            get { return "Konami KCES/VIG Header"; }
        }

        public string Filename
        {
            get { return m_Filename; }
        }

        public bool IsPlayable
        {
            get { return true; }
        }

        public UInt64 IsFormat(StreamReader.IReader fileReader, UInt64 offset, UInt64 length)
        {
            // Check for Mark ID
            if(fileReader.Read_32bitsBE(offset)!=0x01006408)
                return 0;

            // Check for Start Offset Value (can't be equal to 0)
            if (fileReader.Read_32bits(offset + 8) == 0)
                return 0;

            // Check for Channels Count & Sample Rate
            uint sampleRate = fileReader.Read_32bits(offset + 0x18);
            uint channelCount = fileReader.Read_32bits(offset + 0x1C);
            uint interleave = fileReader.Read_32bits(offset + 0x24);

            if (((channelCount <= 0) || (channelCount > 2)) ||
                ((sampleRate <11025) || (sampleRate> 48000)))
                return 0;

            // Check for Interleave value
            if ((interleave < 0) || (interleave > 0x10000))
                return 0;

            // Try to find file Length
            UInt64 startOffset = fileReader.Read_32bits(offset + 0x08);
            UInt64 fileLength = fileReader.Read_32bits(offset + 0x0C);

            //if (!VGM_Utils.IsPS2ADPCM(fileReader, startOffset + offset, startOffset + offset + fileLength))
            //    return 0;

            return fileLength+startOffset;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            int i;

            bool loop_flag = (fileReader.Read_32bits(offset + 0x14) != 0);
            int channel_count = (int)fileReader.Read_32bits(offset + 0x1C);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream,channel_count, loop_flag);

            /* fill in the vital statistics */
            UInt64 start_offset = offset + fileReader.Read_32bits(offset + 0x08);
            
            vgmStream.vgmChannelCount=channel_count;
            vgmStream.vgmSampleRate = (int)fileReader.Read_32bits(offset + 0x18);
            vgmStream.vgmDecoder = new PSX_Decoder();

            if (channel_count == 1)
                vgmStream.vgmLayout = new NoLayout();
            else
                vgmStream.vgmLayout = new Interleave();

            vgmStream.vgmLoopFlag = loop_flag;

            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bits(offset + 0x0C) * 28 / 16 / channel_count);
            
            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)((fileReader.Read_32bits(offset + 0x0C) - fileReader.Read_32bits(offset + 0x14)) * 28 / 16 / channel_count);
                vgmStream.vgmLoopEndSample = (int)(fileReader.Read_32bits(offset + 0x0C) * 28 / 16 / channel_count);
            }

            vgmStream.vgmInterleaveBlockSize = (int)fileReader.Read_32bits(offset + 0x24);

            if (InitReader)
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = start_offset +(UInt64)(vgmStream.vgmInterleaveBlockSize * i);

                }
            }
        }
    }
}
