using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_FPAC : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {
            FST tmpFST = new FST();

            if (!((System.IO.Path.GetExtension(file.Filename).ToUpper() == ".PSARC") ||
                  (System.IO.Path.GetExtension(file.Filename).ToUpper() == ".PAC")))
                return null;

            UInt64 offset = file.FileStartOffset;

            while (offset <= file.FileSize)
            {
                // check for FPAC marker
                if (fileReader.Read_32bitsBE(offset) == 0x46504143)
                {
                    uint sizeofInfo = fileReader.Read_32bits(offset + 0x04);

                    byte[] buffer = fileReader.Read(offset, sizeofInfo);
                    uint sizeofFile = MemoryReader.ReadLong(ref buffer, 0x08);
                    uint fileCount = MemoryReader.ReadLong(ref buffer, 0x0c);
                    uint offsetofIndex = MemoryReader.ReadLong(ref buffer, 0x10);
                    uint offsetofInfo = MemoryReader.ReadLong(ref buffer, 0x14);

                    if (fileCount != 0)
                    {
                        uint sizeofOneInfo = 0;
                        sizeofOneInfo = (sizeofInfo - 0x20) / fileCount;

                        for (uint i = 0; i < fileCount; i++)
                        {
                            uint fileSize = MemoryReader.ReadLong(ref buffer, (i * sizeofOneInfo) + offsetofInfo + 0x20 + 8);
                            uint fileOffset = MemoryReader.ReadLong(ref buffer, (i * sizeofOneInfo) + offsetofInfo + 0x20 + 4) + sizeofInfo;
                            string filename = string.Empty;

                            filename = MemoryReader.GetString(ref buffer, (i * sizeofOneInfo) + 0x20, offsetofInfo);

                            tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
                        }
                    }
                    offset += sizeofFile;
                }
                else offset += 4;
            }
            return tmpFST;
        }
    }
}
