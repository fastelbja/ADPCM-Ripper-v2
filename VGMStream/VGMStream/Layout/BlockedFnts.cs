using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    class BlockedFnts
    {
        public static void CAF_Block_Update(UInt64 NextBlockOffset, ref VGM_Stream vgmStream)
        {
            int i;

            vgmStream.vgmCurrentBlockOffset = NextBlockOffset;
            vgmStream.vgmCurrentBlockSize = (int)vgmStream.vgmChannel[0].fReader.Read_32bitsBE(vgmStream.vgmCurrentBlockOffset + 0x14);
            vgmStream.vgmNextBlockOffset = vgmStream.vgmCurrentBlockOffset + (UInt64)vgmStream.vgmChannel[0].fReader.Read_32bitsBE(vgmStream.vgmCurrentBlockOffset + 0x04);

            for (i = 0; i < vgmStream.vgmChannelCount; i++)
            {
                vgmStream.vgmChannel[i].currentOffset = vgmStream.vgmCurrentBlockOffset + vgmStream.vgmChannel[0].fReader.Read_32bitsBE(NextBlockOffset + (UInt64)(0x10 + (8 * i)));
            }

            /* coeffs */
            for (i = 0; i < 16; i++)
            {
                vgmStream.vgmChannel[0].adpcm_coef[i] = (Int16)vgmStream.vgmChannel[0].fReader.Read_16bitsBE(NextBlockOffset +(UInt64)(0x34 + (2 * i)));
                vgmStream.vgmChannel[1].adpcm_coef[i] = (Int16)vgmStream.vgmChannel[0].fReader.Read_16bitsBE(NextBlockOffset +(UInt64)(0x60 + (2 * i)));
            }
        }

        public static void THP_Block_Update(UInt64 NextBlockOffset, ref VGM_Stream vgmStream)
        {
            int i, j;
            UInt64 start_offset;
            uint nextFrameSize;

            vgmStream.vgmCurrentBlockOffset = NextBlockOffset;
            nextFrameSize = vgmStream.vgmChannel[0].fReader.Read_32bitsBE(vgmStream.vgmCurrentBlockOffset);

            vgmStream.vgmNextBlockOffset = vgmStream.vgmCurrentBlockOffset + (UInt64)vgmStream.vgmTHPNextFrameSize;
            vgmStream.vgmTHPNextFrameSize = nextFrameSize;

            start_offset = vgmStream.vgmCurrentBlockOffset
                           + vgmStream.vgmChannel[0].fReader.Read_32bitsBE(vgmStream.vgmCurrentBlockOffset + 0x08) + 0x10;
            vgmStream.vgmCurrentBlockSize = (int)vgmStream.vgmChannel[0].fReader.Read_32bitsBE(start_offset);

            start_offset += 8;

            for (i = 0; i < vgmStream.vgmChannelCount; i++)
            {
                // get coeff
                for (j = 0; j < 16; j++)
                {
                    vgmStream.vgmChannel[i].adpcm_coef[j] = (Int16)vgmStream.vgmChannel[0].fReader.Read_16bitsBE(start_offset + (UInt64)((i * 0x20) + (j * 2)));
                }
                // and sample history ...
                vgmStream.vgmChannel[i].adpcm_history_16bits_1 = (short)vgmStream.vgmChannel[0].fReader.Read_16bitsBE(start_offset + (UInt64)((0x20 * vgmStream.vgmChannelCount) + (i * 4)));
                vgmStream.vgmChannel[i].adpcm_history_16bits_2 = (short)vgmStream.vgmChannel[0].fReader.Read_16bitsBE(start_offset + (UInt64)((0x20 * vgmStream.vgmChannelCount) + (i * 4) + 2));
                vgmStream.vgmChannel[i].currentOffset = start_offset + (UInt64)(0x24 * vgmStream.vgmChannelCount) + (UInt64)(i * vgmStream.vgmCurrentBlockSize);
            }

        }
    }
}
