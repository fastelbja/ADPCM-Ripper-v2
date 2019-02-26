using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_PAK_MOTORSTORM : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".PAK")
                return null;

            byte[] filenameBuffer = null;

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for AFS marker
            if ((fileReader.Read_16bits(offset+0x0A)  != 0xA3DA))
                return null;

            //string archiveName = fileReader.Read_String(offset + 0x08, 0x50);
            uint fileCount = (uint)fileReader.Read_32bits(offset + 0x08) ^ 0xa3da79b6;
            uint filenameSize = fileReader.Read_32bits(offset + 0x0C);
            uint folderNameSize = fileReader.Read_32bits(offset + 0x10);
            byte[] folderName = fileReader.Read(offset + 0x14, folderNameSize);
            for (int i = 0; i < folderNameSize; i++)
                folderName[i] = (byte)(folderName[i] ^ 0xC5);
            string folder = MemoryReader.GetString(ref folderName, 0);

            byte[] buffer = fileReader.Read(offset + 0x14 + folderNameSize + 1, (fileCount * 0x10));
            byte[] bufferName = fileReader.Read(offset + 0x14 + folderNameSize + 1 + (fileCount*0x10), filenameSize);
            for (int i = 0; i < filenameSize; i++)
                bufferName[i] = (byte)(bufferName[i] ^ 0xB5);

            for (uint i = 0; i < fileCount; i++)
            {
                uint fileNameOffset = MemoryReader.ReadLong(ref buffer, (i * 0x10));
                uint fileOffset = MemoryReader.ReadLong(ref buffer, (i * 0x10) + 0x04);
                uint fileSize = MemoryReader.ReadLong(ref buffer, (i * 0x10) + 0x08);
                string filename = string.Empty;
                filename = MemoryReader.GetString(ref bufferName, fileNameOffset);

                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }

            return tmpFST;
        }
    }
}
