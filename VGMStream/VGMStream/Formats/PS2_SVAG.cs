using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PS2_SVAG : IVGMFormat
    {
        private string m_Filename = string.Empty;
        private string m_Description = string.Empty;

        public string Extension
        {
            get
            {
                return "SVAG";
            }
        }

        public string Description
        {
            get
            {
                return m_Description;
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
            // Check for Mark ID ("Svag")
            if(fileReader.Read_32bitsBE(offset)!=0x53766167)
                return 0;

            // Check for Sample Rate
            if(!VGM_Utils.CheckSampleRate(fileReader.Read_32bits(offset + 0x08)))
                return 0;

            // Check Channels count
            if(!VGM_Utils.CheckChannels(fileReader.Read_32bits(offset + 0x0C)))
                return 0;

            // Values from 0 -> 0x1C are duplicated at offset 0x400
            for (UInt64 i = 0; i < 0x1C / 4; i++)
            {
                if (fileReader.Read_32bits(offset + i) != fileReader.Read_32bits(offset + 0x400 + i))
                    return 0;
            }

            // Try to find file Length
            UInt64 startOffset = 0x800;
            UInt64 fileLength = fileReader.Read_32bits(offset + 0x04);

            //if (!VGM_Utils.IsPS2ADPCM(fileReader, startOffset + offset, startOffset + offset + fileLength))
            //    return 0;

            if (fileReader.Read_32bits(offset + 0x30) == 0x45434B2E)
                m_Description = "Konami SVAG (KCE-Tokyo)";
            else
                m_Description = "Konami SVAG (KONAMITYO)";

            return fileLength+startOffset;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            bool loop_flag = (fileReader.Read_32bits(offset + 0x14) == 1);
            int channel_count = (int)fileReader.Read_32bits(offset + 0x0C);

            if (fileReader.Read_32bits(offset + 0x30) == 0x45434B2E)
                m_Description = "Konami SVAG (KCE-Tokyo)";
            else
                m_Description = "Konami SVAG (KONAMITYO)";


            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream,channel_count, loop_flag);

            /* fill in the vital statistics */
            UInt64 start_offset = offset + 0x800;
            
            vgmStream.vgmChannelCount=channel_count;
            vgmStream.vgmSampleRate = (int)fileReader.Read_32bits(offset + 0x08);
            vgmStream.vgmDecoder = new PSX_Decoder();

            if(vgmStream.vgmChannelCount==1)
                vgmStream.vgmLayout = new NoLayout();
            else
                vgmStream.vgmLayout = new Interleave();

            vgmStream.vgmLoopFlag = loop_flag;

            vgmStream.vgmTotalSamples = (int)((fileReader.Read_32bits(offset + 0x04) * 28 / 16)/channel_count);
            
            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)(fileReader.Read_32bits(offset + 0x18)/16*28);
                vgmStream.vgmLoopEndSample = (int)(fileReader.Read_32bits(offset + 0x04) / 16 * 28 / vgmStream.vgmChannelCount);
            }

            vgmStream.vgmInterleaveBlockSize = (int)fileReader.Read_32bits(offset + 0x10);

            if (InitReader)
            {
                for (int i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = start_offset + (UInt64)(vgmStream.vgmInterleaveBlockSize * i);
                }
            }
        }
    }
}
