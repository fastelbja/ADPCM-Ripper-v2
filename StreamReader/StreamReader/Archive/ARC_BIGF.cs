using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_BIGF : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            FST tmpFST = new FST();
            UInt64 offset = file.FileStartOffset;
            uint offsetBuffer = 0;

            bool bigEndian = true;

            // check for BIGF marker
            if (fileReader.Read_32bitsBE(offset) != 0x42494746)
                return null;

            if (fileReader.Read_32bitsBE(offset + 4) == file.FileSize)
                bigEndian = false;

            uint fileCount, filenameSize, fileOffset, fileSize;

            if (bigEndian)
            {
                fileCount = (uint)fileReader.Read_32bits(offset + 0x08);
                filenameSize = fileReader.Read_32bits(offset + 0x0C) - 0x10;
            }
            else
            {
                fileCount = (uint)fileReader.Read_32bitsBE(offset + 0x08);
                filenameSize = fileReader.Read_32bitsBE(offset + 0x0C) - 0x10;
            }

            byte[] buffer = fileReader.Read(offset + 0x10, filenameSize);

            for (uint i = 0; i < fileCount; i++)
            {
                if(bigEndian)
                {
                    fileOffset = MemoryReader.ReadLong(ref buffer, offsetBuffer);
                    fileSize = MemoryReader.ReadLong(ref buffer, offsetBuffer + 4);
                }
                else
                {
                    fileOffset = MemoryReader.ReadLongBE(ref buffer, offsetBuffer);
                    fileSize = MemoryReader.ReadLongBE(ref buffer, offsetBuffer + 4);
                }
                string filename = string.Empty;
                filename = MemoryReader.GetString(ref buffer, offsetBuffer+8);
                offsetBuffer += (uint)(8 + 1 + filename.Length);
                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }

            return tmpFST;
        }

    }
}
