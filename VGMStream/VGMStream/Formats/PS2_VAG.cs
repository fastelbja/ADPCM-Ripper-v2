using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class PS2_VAG: IVGMFormat
    {
        private string m_Filename = "";

        public string Extension
        {
            get
            {
                return "VAG";
            }
        }

        public string Description
        {
            get
            {
                return "Sony VAG";
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
            // Check for Mark ID ("VAGx") where x can be "p","i","s"
            if((fileReader.Read_32bitsBE(offset) & 0xFFFFFF00) !=0x56414700)
                return 0;

            uint vagLength = fileReader.Read_32bitsBE(offset + 0x0C) + 0x30;

            if (fileReader.Read_32bitsBE(offset + 0x24) == 0x56414778)
            {
                uint k = 0;
                do
                {
                    k += 0x10;
                    vagLength += 0x10;
                }
                while (fileReader.Read_16bitsBE(offset + fileReader.Read_32bitsBE(offset + 0x0C) + k) != 0x0007);
            }

            if ((length != (UInt64)(0xFFFFFFFFF)) && (vagLength != length))
                return 0;

            // Check for Sample Rate
            if (!VGM_Utils.CheckSampleRate(fileReader.Read_32bitsBE(offset + 0x10)))
                return 0;

           

            if (!VGM_Utils.IsPS2ADPCM(fileReader, offset + 0x30, offset + vagLength))
                return 0;
            
            // Filename is stored at offset +0x20
            m_Filename = fileReader.Read_String(offset + 0x20, 0x10);
            return vagLength+0x30;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            int i;

            char vagID = (char)fileReader.Read_8Bits(offset + 0x03);
            uint fileVagLength = fileReader.Read_32bitsBE(offset + 0x0c);

            int channel_count = 1;
            uint interleave = 0;

            UInt64 loopStart = 0;
            UInt64 loopEnd = 0;

            bool loop_flag = false;

            switch (vagID)
            {
                case 'i':
                    channel_count = 2;
                    break;
                case 'V':
                    if (fileReader.Read_32bitsBE(offset + 0x20) == 0x53746572) // vag Stereo
                        channel_count = 2;
                    break;
                case 'p':
                    if (fileReader.Read_32bitsBE(offset + 0x24) == 0x56414778)
                    {
                        loop_flag = false;
                        channel_count = 2;
                    }
                    else
                    {
                        if(fileReader.Read_32bitsBE(offset + 0x04) <= 0x00000004)
                        {
                            loop_flag = (fileReader.Read_32bitsBE(offset + 0x14) != 0);
                            channel_count = 1;
                        }
                        else
                        {
                            /* Search for loop in VAG */
                            uint vagfileLength = fileReader.Read_32bitsBE(offset + 0x0c);
                            UInt64 readOffset = offset + 0x20;

                            do
                            {
                                readOffset += 0x10;

                                // Loop Start ...
                                if (fileReader.Read_8Bits(readOffset + 0x01) == 0x06)
                                {
                                    if (loopStart == 0) loopStart = readOffset;
                                }

                                // Loop End ...
                                if (fileReader.Read_8Bits(readOffset + 0x01) == 0x03)
                                {
                                    if (loopEnd == 0) loopEnd = readOffset;
                                }

                                // Loop from end to beginning ...
                                if ((fileReader.Read_8Bits(readOffset + 0x01) == 0x01))
                                {
                                    // Check if we have the eof tag after the loop point ...
                                    // if so we don't loop, if not present, we loop from end to start ...
                                    byte[] vagBuffer = fileReader.Read(readOffset + 0x10, 0x10);
                                    if ((vagBuffer[0] != 0) && (vagBuffer[0] != 0x0c))
                                    {
                                        if ((vagBuffer[0] == 0x00) && (vagBuffer[0] == 0x07))
                                        {
                                            loopStart = 0x40;
                                            loopEnd = readOffset;
                                        }
                                    }
                                }

                            } while (readOffset < offset + 0x20 + vagfileLength);
                            loop_flag = (loopEnd != 0);
                        }
                    }
                    break;
                default:
                    break;
            }

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream,channel_count, loop_flag);

            /* fill in the vital statistics */
            UInt64 start_offset = offset + 0x30;
            
            vgmStream.vgmChannelCount=channel_count;
            vgmStream.vgmSampleRate = (int)fileReader.Read_32bitsBE(offset + 0x10);
            vgmStream.vgmTotalSamples = (int)(fileReader.Read_32bits(offset + 0x04) * 28 / 16);

            vgmStream.vgmLayout = new NoLayout();
                            
            switch (vagID)
            {
                case 'i': // VAGi
                    vgmStream.vgmLayout = new Interleave();
                    vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(0x0C) / 16 * 28;
                    interleave = fileReader.Read_32bitsBE(offset + 0x08);
                    start_offset = offset + 0x800;
                    break;
                case 'p': // VAGp
                    interleave = 0x10; // used for loop calc

                    if (fileReader.Read_32bitsBE(offset + 0x04) == 0x00000004)
                    {
                        vgmStream.vgmChannelCount = 2;
                        vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(offset + 0x0C);
                        
                        if (loop_flag)
                        {
                            vgmStream.vgmLoopStartSample = (int)fileReader.Read_32bitsBE(offset + 0x14);
                            vgmStream.vgmLoopEndSample = (int)fileReader.Read_32bitsBE(offset + 0x18);
                        }

                        start_offset = offset + 0x80;

                        vgmStream.vgmLayout = new Interleave();

                        // Double VAG Header @ 0x0000 & 0x1000
                        if (fileReader.Read_32bitsBE(offset + 0) == fileReader.Read_32bitsBE(offset + 0x1000))
                        {
                            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(offset + 0x0C) / 16 * 28;
                            interleave = 0x1000;
                            start_offset = offset + 0;
                        }

                    }
                    else
                    {
                        if (fileReader.Read_32bitsBE(offset + 0x24) == 0x56414778) 
                        {
                            if (fileReader.Read_16bitsBE(offset +  fileReader.Read_32bitsBE(offset + 0x0C) + 0x10)!=0x0007)
                                interleave = 0x8000;

                            vgmStream.vgmLayout = new Interleave();
                            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(offset + 0x0C) / 16 * 14;
                    
                        }
                        else
                        {
                            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(offset + 0x0C) / 16 * 28;
                            start_offset = offset + 0x30;
                        } 
                    }
                    break;
                case 'V': // pGAV
                    vgmStream.vgmLayout = new Interleave();
                    interleave = 0x2000;

                    // Jak X hack ...
                    if (fileReader.Read_32bitsBE(offset + 0x1000) == 0x56414770)
                        interleave = 0x1000;

                    vgmStream.vgmSampleRate = (int)fileReader.Read_32bits(offset + 0x10);
                    vgmStream.vgmTotalSamples =(int)fileReader.Read_32bits(offset + 0x0C) / 16 * 14;
                    start_offset = offset + 0;
                    break;
            }
            
            vgmStream.vgmDecoder = new PSX_Decoder();
            vgmStream.vgmLoopFlag = loop_flag;

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)(fileReader.Read_32bits(offset + 0x08) * 28 / 16/ vgmStream.vgmChannelCount);
                vgmStream.vgmLoopEndSample = (int)(fileReader.Read_32bits(offset + 0x04) * 28 / 16);
            }

            vgmStream.vgmInterleaveBlockSize = (int)interleave;

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
