using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using StreamReader;

namespace ADPCM_Ripper_v2.Games
{
    public class MK_ASSERT
    {
        public void DoExtraction(string sPath, string sOut)
        {
            byte[] buffer = new byte[0x8000];
            byte[] emptyBuffer = {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
            uint interleave = 0;
            uint bgmCount = 0;

            FileStream fIn = new FileStream(sPath,FileMode.Open,FileAccess.Read);
            BinaryReader bIn = new BinaryReader(fIn);

            FileStream fOut = null;
            BinaryWriter bOut = null;

            do
            {
                bIn.Read(buffer, 0, 0x8000);

                if (MemoryReader.memcmp(buffer, emptyBuffer, 16, 0))
                {
                    if (fOut != null)
                    {
                        fOut.Close();
                        bOut.Close();
                        bgmCount++;
                    }

                    interleave = 0;

                    fOut = new FileStream(sOut + "\\" + bgmCount.ToString("0000") + ".mib",FileMode.CreateNew,FileAccess.Write);
                    bOut = new BinaryWriter(fOut);

                    // Search for Interleave ...
                    for (uint i = 0x10; i < 0x8000; i += 0x10)
                    {
                        if (MemoryReader.memcmp(buffer, emptyBuffer, 16, i))
                        {
                            interleave = i;
                            break;
                        }
                    }
                }

                bOut.Write(buffer, 0, (int)(interleave * 2));

            } while (bIn.BaseStream.Position < bIn.BaseStream.Length);

            bOut.Close();
            fOut.Close();
            bIn.Close();
            fIn.Close();
        }
    }
}
