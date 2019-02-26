using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VGMStream
{
    [StructLayout(LayoutKind.Sequential)]
    public class VGM_Channel
    {
        // Each channels will have his own File Reader
        public StreamReader.IReader fReader;

        public UInt64 startOffset;
        public UInt64 currentOffset;
        
        // Save sample history in 16 bits format
        public Int16 adpcm_history_16bits_1;
        public Int16 adpcm_history_16bits_2;

        // Save sample history in 32 bits format
        public Int32 adpcm_history_32bits_1;
        public Int32 adpcm_history_32bits_2;

        // Save sample history in doubloe format
        public double adpcm_history_dbl_1;
        public double adpcm_history_dbl_2;

        /* ADX encryption */
        public int adx_channels;
        public UInt16 adx_xor;
        public UInt16 adx_mult;
        public UInt16 adx_add;

        // DSP decoder coefficients
        public Int16[] adpcm_coef;

        public VGM_Channel()
        {
            adpcm_coef = new Int16[16];
        }
    }
}
