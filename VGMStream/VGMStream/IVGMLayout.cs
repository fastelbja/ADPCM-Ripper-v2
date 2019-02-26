using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VGMStream
{
    public interface IVGMLayout
    {
        string GetDescription();
        int update(ref short[] vgmBuffer, int vgmSampleCount, VGM_Stream vgmStream);
    }
}
