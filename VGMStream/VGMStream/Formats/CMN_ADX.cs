using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public class CMN_ADX : IVGMFormat
    {
        public string Extension
        {
            get
            {
                return "ADX";
            }
        }

        public string Description
        {
            get
            {
                return "CRI ADX Audio File";
            }
        }

        public string Filename
        {
            get
            {
                return String.Empty;
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
            if (fileReader.Read_16bitsBE(offset) != 0x8000)
                return 0;

            // Check encoding type ...
            if (fileReader.Read_8Bits(offset + 4) != 3)
                return 0;

            // Check Frame Size ...
            if (fileReader.Read_8Bits(offset + 5) != 18)
                return 0;

            // Check Bit per Samples ...
            if (fileReader.Read_8Bits(offset + 6) != 4)
                return 0;

            // Check for channel count & sample rate
            uint sampleRate = fileReader.Read_32bitsBE(offset + 0x08);
            uint channelCount = fileReader.Read_8Bits(offset + 0x07);

            if (((channelCount <= 0) || (channelCount > 8)) ||
                ((sampleRate < 11025) || (sampleRate > 48000)))
                return 0;

            // Search for the (c)CRI signature ...
            UInt64 criOffset = fileReader.Read_16bitsBE(offset + 2);
            if ((fileReader.Read_32bitsBE(offset + criOffset - 4) != 0x00002863) &&
                (fileReader.Read_32bitsBE(offset + criOffset) != 0x29435249))
                return 0;

            // Find file length ...
            UInt32 totalSamples = fileReader.Read_32bitsBE(offset + 0x0C);
            UInt64 searchOffset = offset + (totalSamples * channelCount) / 32 * 18 + criOffset + 4;

            while(fileReader.Read_16bitsBE(searchOffset)!=0x8001)
            {
                searchOffset++;
                if (!fileReader.CanRead())
                    break;
            }

            UInt64 fileLength = (searchOffset - offset) + fileReader.Read_16bitsBE(searchOffset + 2) + 4;
            return fileLength;
        }

        public void Init(StreamReader.IReader fileReader, UInt64 offset, ref VGM_Stream vgmStream, bool InitReader, UInt64 fileLength)
        {
            bool loop_flag = false;
            UInt32 loop_start_sample=0;
            UInt32 loop_start_offset;
            UInt32 loop_end_sample=0;
            UInt32 loop_end_offset;

            Int16 coef1 = 0;
            Int16 coef2 = 0;

            int i;

            int channel_count = fileReader.Read_8Bits(offset + 0x07);

            /* check version signature, read loop info */
            UInt16 version_signature = fileReader.Read_16bitsBE(offset + 0x12);
            UInt64 criOffset = (UInt64)fileReader.Read_16bitsBE(offset + 2) + 4;

            /* encryption */
            if (version_signature == 0x0408)
            {
                //if (find_key(streamFile, &xor_start, &xor_mult, &xor_add))
                //{
                //    coding_type = coding_CRI_ADX_enc;
                //    version_signature = 0x0400;
                //}
            }
            if (version_signature == 0x0300)
            {      /* type 03 */
                if (criOffset - 6 >= 0x2c)
                {   /* enough space for loop info? */
                    loop_flag = (fileReader.Read_32bitsBE(offset + 0x18) != 0);
                    loop_start_sample = (fileReader.Read_32bitsBE(offset + 0x1c));
                    loop_start_offset = fileReader.Read_32bitsBE(offset + 0x20);
                    loop_end_sample = fileReader.Read_32bitsBE(offset + 0x24);
                    loop_end_offset = fileReader.Read_32bitsBE(offset + 0x28);
                }
            }
            else if (version_signature == 0x0400)
            {

                UInt32 ainf_info_length = 0;

                if (fileReader.Read_32bitsBE(offset + 0x24) == 0x41494E46) /* AINF Header */
                    ainf_info_length = fileReader.Read_32bitsBE(offset + 0x28);

                if (criOffset - ainf_info_length - 6 >= 0x38)
                {   /* enough space for loop info? */
                    loop_flag = (fileReader.Read_32bitsBE(offset + 0x24) != 0);
                    loop_start_sample = (fileReader.Read_32bitsBE(offset + 0x28));
                    loop_start_offset = fileReader.Read_32bitsBE(offset + 0x2C);
                    loop_end_sample = fileReader.Read_32bitsBE(offset + 0x30);
                    loop_end_offset = fileReader.Read_32bitsBE(offset + 0x34);
                }
            }
            else if (version_signature == 0x0500)
            {   /* found in some SFD : Buggy Heat, appears to have no loop */
            }

            /* build the VGMSTREAM */
            VGM_Utils.allocate_vgmStream(ref vgmStream, channel_count, loop_flag);

            vgmStream.vgmChannelCount = channel_count;
            vgmStream.vgmSampleRate = (int)fileReader.Read_32bitsBE(offset + 0x08);
            vgmStream.vgmTotalSamples = (int)fileReader.Read_32bitsBE(offset + 0x0C);

            vgmStream.vgmDecoder = new ADX_Decoder();

            if (channel_count == 1)
                vgmStream.vgmLayout = new NoLayout();
            else
                vgmStream.vgmLayout = new Interleave();

            vgmStream.vgmLoopFlag = loop_flag;

            if (loop_flag)
            {
                vgmStream.vgmLoopStartSample = (int)loop_start_sample;
                vgmStream.vgmLoopEndSample = (int)loop_end_sample;
            }

            /* high-pass cutoff frequency, always 500 that I've seen */
            UInt16 cutoff =  fileReader.Read_16bitsBE(offset + 0x10);

            /* calculate filter coefficients */
            {
                double x, y, z, a, b, c;

                x = cutoff;
                y = vgmStream.vgmSampleRate;
                z = Math.Cos(2.0 * Math.PI * x / y);

                a = Math.Sqrt(2) - z;
                b = Math.Sqrt(2) - 1.0;
                c = (a - Math.Sqrt((a + b) * (a - b))) / b;

                coef1 = (Int16) Math.Floor(c * 8192);
                coef2 = (Int16) Math.Floor(c * c * -4096);
            }

            vgmStream.vgmInterleaveBlockSize = 18;

            if(InitReader) 
            {
                for (i = 0; i < channel_count; i++)
                {
                    vgmStream.vgmChannel[i].fReader = (StreamReader.IReader)Activator.CreateInstance(fileReader.GetType()); ;
                    vgmStream.vgmChannel[i].fReader.Open(fileReader.GetFilename());
                    vgmStream.vgmChannel[i].fReader.SetSessionID(fileReader.GetSessionID());

                    vgmStream.vgmChannel[i].adpcm_coef[0] = coef1;
                    vgmStream.vgmChannel[i].adpcm_coef[1] = coef2;
                    vgmStream.vgmChannel[i].startOffset = vgmStream.vgmChannel[i].currentOffset = offset + criOffset + (UInt64)(vgmStream.vgmInterleaveBlockSize * i);
                }
            }
        }
    }
}
