using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_POD2 : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {
            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for file extension ".POD"
            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".POD")
                return null;

            // check for POD2 marker
            if (fileReader.Read_32bitsBE(offset) != 0x504F4432) 
                return null;

            string archiveName = fileReader.Read_String(offset + 0x08, 0x50);
            uint fileCount = fileReader.Read_32bits(offset + 0x58);
            uint filenameOffset = 0x60 + (fileCount * 20);
            uint filenameSize = fileReader.Read_32bits(offset + 0x68) - filenameOffset;

            byte[] buffer = fileReader.Read(offset + 0x60, (fileCount * 20));
            byte[] filenameBuffer = fileReader.Read(offset + filenameOffset, filenameSize);

            for (uint i = 0; i < fileCount; i++)
            {
                uint nameOffset = MemoryReader.ReadLong(ref buffer, (i * 20));
                uint fileSize = MemoryReader.ReadLong(ref buffer, (i * 20) + 0x04);
                uint fileOffset = MemoryReader.ReadLong(ref buffer, (i * 20) + 0x08);
                string filename = MemoryReader.GetString(ref filenameBuffer, nameOffset);
                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.Filename, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }
            
            return tmpFST;
        }
    }
}
