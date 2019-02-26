using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_AFS2 : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".AWB")
                return null;

            byte[] filenameBuffer = null;

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;
            UInt64 readOffset = offset;

            // check for AFS2 marker
            if (!((fileReader.Read_32bitsBE(readOffset) == 0x41465332) &&
                  (fileReader.Read_32bitsBE(readOffset + 0x04) == 0x01040200)))
                return null;

            uint fileCount = (uint)fileReader.Read_32bits(readOffset + 0x08);
            readOffset += 0x10;
            readOffset += fileCount * 2;

            uint fileOffset = (uint)fileReader.Read_32bits(readOffset);
            readOffset += 4;

            byte[] buffer = fileReader.Read(readOffset, (fileCount * 4));

            for (uint i = 0; i < fileCount; i++)
            {
                uint nextOffset = MemoryReader.ReadLong(ref buffer, (i * 8));
                uint fileSize;

                uint padding = (fileOffset / 0x10)*0x10;
                padding -= fileOffset;
                padding += 0x10;

                fileOffset += padding;
                fileSize = nextOffset;
                fileSize -= fileOffset;
                string filename = string.Empty;

                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
                fileOffset = nextOffset;
            }

            return tmpFST;
        }
    }

}
