using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_HG2 : IContainer
    {

        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for AFS marker
            if (!((fileReader.Read_32bitsBE(offset) == 0x48473200) && 
                (fileReader.Read_32bitsBE(offset+4) == 0x03000000) &&
                (fileReader.Read_32bitsBE(offset+8) == 0x02000000)))
                return null;

            //string archiveName = fileReader.Read_String(offset + 0x08, 0x50);
            uint sizeOfEntireFile = fileReader.Read_32bits(offset + 0x0c);

            if ((sizeOfEntireFile == 0) || (sizeOfEntireFile > file.FileSize))
                return null;

            uint fileCount = (uint)fileReader.Read_32bits(offset + 0x20);
            uint offsetOfFilename = (uint)fileReader.Read_32bits(offset + 0x2C);
            uint offsetOfFileInfo = (uint)fileReader.Read_32bits(offset + 0x24);

            byte[] buffer = fileReader.Read(offset + offsetOfFileInfo, (fileCount * 0x0c));
            byte[] bufferOffsetName = fileReader.Read(offset + offsetOfFilename, (fileCount * 0x04));

            uint OffsetSubstract = MemoryReader.ReadLong(ref bufferOffsetName, 0);

            if (OffsetSubstract > sizeOfEntireFile)
                return null;

            byte[] bufferFileName = fileReader.Read(offset + offsetOfFilename + (fileCount * 0x04), sizeOfEntireFile - OffsetSubstract);

            for (uint i = 0; i < fileCount; i++)
            {
                uint fileOffset = MemoryReader.ReadLong(ref buffer, (i * 0x0c) + 0x08);
                uint fileSize = MemoryReader.ReadLong(ref buffer, (i * 0x0c) + 0x04);
                string filename = string.Empty;
                uint OffsetofFilename = MemoryReader.ReadLong(ref bufferOffsetName, (i * 0x04));

                filename = MemoryReader.GetString(ref bufferFileName, OffsetofFilename - OffsetSubstract, 0x255);

                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }

            return tmpFST;
        }
    }
}
