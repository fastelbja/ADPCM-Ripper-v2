using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class NGC_CAF : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "CFN";
            }
        }

        public string Description
        {
            get
            {
                return "Baten Kaitos Audio File";
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
            UInt64 nextOffset =0;
            UInt64 endOffset = offset + length;

            // Check for Mark ID ("CAF ")
            if (fileReader.Read_32bitsBE(offset) != 0x43414620)
                return 0;

            uint blockCount = fileReader.Read_32bitsBE(offset + 0x24) - 1;

            for(int i = 0; i < blockCount; i++)
            {
                nextOffset = fileReader.Read_32bitsBE(offset + 0x4);

                if (fileReader.Read_32bitsBE(offset) != 0x43414620)
                    return 0;

                if (fileReader.Read_32bitsBE(offset + 0x08) != i)
                    return 0;

                offset += nextOffset;
            } 

            return length;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            int vgmNumSamples = 0;
            int loop_start = -1;

            UInt64 NextBlock;
            UInt64 currBlock = offset;
            int i;

            // Calculate sample length ...
            uint blockCount = fileReader.Read_32bitsBE(offset + 0x24);

            for (i = 0; i < blockCount; i++)
            {
                NextBlock = fileReader.Read_32bitsBE(currBlock + 0x04);
                vgmNumSamples += (int)(fileReader.Read_32bitsBE(currBlock + 0x14) / 8 * 14);

                if (fileReader.Read_32bitsBE(currBlock + 0x20) == fileReader.Read_32bitsBE(currBlock + 0x08))
                {
                    loop_start = (int)(vgmNumSamples - fileReader.Read_32bitsBE(currBlock + 0x14) / 8 * 14);
                }
                currBlock += NextBlock;
                
            } 

            bool loop_flag = (loop_start != -1);

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, 2, loop_flag);

            vgmStream.vgmLoopFlag = loop_flag;
            vgmStream.vgmChannelCount = 2;
            vgmStream.vgmSampleRate = 32000;
            vgmStream.vgmTotalSamples = vgmNumSamples;

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = loop_start;
                vgmStream.vgmLoopEndSample = vgmNumSamples;
            }

            vgmStream.vgmDecoder = new DSP_Decoder();
            vgmStream.vgmLayout = new Blocked();
            vgmStream.vgmLayoutType = VGM_Layout_Type.CAF_Blocked;

            if (InitReader)
            {
                for (i = 0; i < 2; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());
                }
                BlockedFnts.CAF_Block_Update(offset + 0, ref vgmStream);
            }
        }
    }
}
