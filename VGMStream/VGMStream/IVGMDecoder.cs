using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public interface IVGMDecoder 
    {
        int FrameSize();
        int ShortFrameSize();
        int SamplesPerFrame();
        int SamplesPerShortFrame();
        string Description { get; }

        void Decode(VGM_Stream vgmStream, 
                    VGM_Channel vgmChannel, 
                    ref short[] vgmOutput, 
                    int vgmChannelSpacing, 
                    int vgmFirstSample, 
                    int vgmSamplesToDo,
                    int vgmChannelNumber);
    }
}
