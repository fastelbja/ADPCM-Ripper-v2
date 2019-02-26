using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace VGMStream
{
    /// <summary>
    /// VGMStream Main class
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class VGM_Stream
    {
        public VGM_Channel[] vgmChannel;
        public VGM_Channel[] vgmChannelAtStart;
        public VGM_Channel[] vgmChannelAtLoop;

        public int vgmChannelCount;

        public IVGMDecoder vgmDecoder;
        public VGM_Decoder_Type vgmDecoderType;

        public IVGMLayout vgmLayout;
        public VGM_Layout_Type vgmLayoutType;

        public int vgmTotalSamples;
        public int vgmTotalSamplesWithLoop;

        public int vgmDecodedSamples;

        public int vgmSampleRate;

        public bool vgmLoopFlag;
        public int vgmLoopStartSample;
        public int vgmLoopEndSample;
        
        public bool vgmHitLoop;

        public int vgmSamplePlayed;
        public int vgmSamplesIntoBlock;
        public int vgmSamplesBlockOffset;

        public int vgmSamplePlayedInLoop;
        public int vgmSamplesIntoBlockInLoop;

        public int vgmInterleaveBlockSize;
        public int vgmInterleaveShortBlockSize;

        public int vgmCurrentBlockSize;
        public UInt64 vgmCurrentBlockOffset;
        public UInt64 vgmNextBlockOffset;

        public int vgmCurrentBlockSizeInLoop;
        public UInt64 vgmCurrentBlockOffsetInLoop;
        public UInt64 vgmNextBlockOffsetInLoop;

        public Int64 vgmTHPNextFrameSize;

        public VGM_Stream vgmStreamCopy;
    }
}
