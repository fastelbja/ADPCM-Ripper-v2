using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_CLU : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {
            UInt64 offset = file.FileStartOffset;

            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".PSP")
                return null;

            if ((fileReader.Read_32bitsBE(offset) != 0x01000010) && (fileReader.Read_32bitsBE(offset) != 0x434C5500))
                return null;

            FST tmpFST = new FST();

            uint fileCount = (uint)fileReader.Read_32bits(offset + 0x0C);
            uint bufferSize = (uint)fileReader.Read_32bits(offset + 0x08);
            byte[] buffer = fileReader.Read(offset + 0x20, bufferSize);

            for (uint i = 0; i < fileCount; i++)
            {
                uint fileOffset = MemoryReader.ReadLong(ref buffer, (i * 0x10)+0x08);
                uint fileSize = MemoryReader.ReadLong(ref buffer, (i * 0x10) + 0x04);
                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, string.Empty, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }

            return tmpFST;
        }
    }
}
