using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_DKPG : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".PAC")
                return null;

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for AFS marker
            if (!((fileReader.Read_32bitsBE(offset) == 0x44474b50) && (fileReader.Read_32bitsBE(offset + 4) == 0x00000100)))
                return null;

            uint fileCount = (uint)fileReader.Read_32bits(offset + 0x0C);
            uint offsetStart = (uint)fileReader.Read_32bits(offset + 0x10);
            uint fileOffset = offsetStart;

            for (uint i = 0; i < fileCount; i++)
            {
                byte[] buffer = fileReader.Read(offset+fileOffset, 0x90);
                if (MemoryReader.ReadLongBE(ref buffer,0) == 0x53415439)
                {
                    fileOffset +=  0x90;
                    uint fileSize = MemoryReader.ReadLong(ref buffer, 0x08);
                    string filename = string.Empty;
                    filename = MemoryReader.GetString(ref buffer, 0x10);

                    tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
                }
                fileOffset += (uint)MemoryReader.ReadLong(ref buffer, 0x08);
            }
            return tmpFST;
        }
    }
}
