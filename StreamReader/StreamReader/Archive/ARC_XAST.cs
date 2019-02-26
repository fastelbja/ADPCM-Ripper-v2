using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_XAST : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for XAST marker
            if (!((fileReader.Read_32bitsBE(offset) == 0x58415354) && (fileReader.Read_32bitsBE(offset) != 0x00000101)))
                return null;

            uint fileCount = (uint)fileReader.Read_32bits(offset + 0x0C);
            uint bufferSize = (uint)fileReader.Read_32bits(offset + 0x14);
            byte[] buffer = fileReader.Read(offset, bufferSize);

            uint buffer_offset = 0x30;

            for (uint i = 0; i < fileCount; i++)
            {
                if (MemoryReader.ReadLong(ref buffer, (i * 48) + buffer_offset) != 0xffffffff)
                {
                    UInt64 fileOffset = MemoryReader.ReadLong(ref buffer, (i * 48) + buffer_offset + 0x20);
                    uint fileSize = MemoryReader.ReadLong(ref buffer, (i * 48) + buffer_offset + 0x10);
                    UInt64 filenameOffset = MemoryReader.ReadLong(ref buffer, (i * 48) + buffer_offset + 0x04);
                    string filename = string.Empty;

                    filename = MemoryReader.GetString(ref buffer, filenameOffset, 255);

                    tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
                }
            }

            return tmpFST;
        }
    }
}
