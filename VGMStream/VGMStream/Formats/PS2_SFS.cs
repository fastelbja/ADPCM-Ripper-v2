using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PS2_SFS : IVGMFormat
    {
        private string m_Filename = "";

        public string Extension
        {
            get
            {
                return "SFS";
            }
        }

        public string Description
        {
            get
            {
                return "Baroque SFS File with STER Header";
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
            // Check for Mark ID ("STER")
            if(fileReader.Read_32bitsBE(offset)!=0x53544552)
                return 0;

            // 0x04 = Length of each channels in byte
            // 0x08 = Length of each channels in byte (in Little Endian Format)
            if (fileReader.Read_32bits(offset + 0x04) != fileReader.Read_32bitsBE(offset + 0x0C))
                return 0;

            // Check for Sample Rate
            uint sampleRate = fileReader.Read_32bitsBE(offset + 0x10);

            if ((sampleRate <11025) || (sampleRate> 48000))
                return 0;

            // others data must be equal to 0
            if ((fileReader.Read_32bits(offset + 0x14)!=0) ||
                (fileReader.Read_32bits(offset + 0x18)!=0) ||
                (fileReader.Read_32bits(offset + 0x1C)!=0))
                return 0;

            // Try to find file Length
            UInt64 startOffset = 0x30;
            UInt64 fileLength = fileReader.Read_32bits(offset + 0x04)*2;

            //if (!VGM_Utils.IsPS2ADPCM(fileReader, startOffset + offset, startOffset + offset + fileLength))
            //    return 0;

            // Filename is stored at offset +0x20
            m_Filename = fileReader.Read_String(offset + 0x20, 0x10);
            return fileLength+startOffset;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            int i;

            bool loop_flag = (fileReader.Read_32bits(offset + 0x08) != 0xFFFFFFFF);
            int channel_count = 2;

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream,channel_count, loop_flag);

            /* fill in the vital statistics */
            UInt64 start_offset = offset + 0x30;
            
            vgmStream.vgmChannelCount=channel_count;
            vgmStream.vgmSampleRate = (int)fileReader.Read_32bitsBE(offset + 0x10);
            vgmStream.vgmDecoder = new PSX_Decoder();
            vgmStream.vgmLayout = new Interleave();
            vgmStream.vgmLoopFlag = loop_flag;

            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bits(offset + 0x04) * 28 / 16);
            
            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)(fileReader.Read_32bits(offset + 0x08) * 28 / 16/ vgmStream.vgmChannelCount);
                vgmStream.vgmLoopEndSample = (int)(fileReader.Read_32bits(offset + 0x04) * 28 / 16);
            }

            vgmStream.vgmInterleaveBlockSize = 0x10;

            if (InitReader)
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = start_offset + (UInt64)(vgmStream.vgmInterleaveBlockSize * i);

                }
            }
        }
    }
}
