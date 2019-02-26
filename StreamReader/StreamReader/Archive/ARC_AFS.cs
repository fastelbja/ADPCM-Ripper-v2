using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_AFS : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".AFS")
                return null;

            byte[] filenameBuffer = null;

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for AFS marker
            if ((fileReader.Read_32bitsBE(offset) != 0x41465300) && (fileReader.Read_32bitsBE(offset) != 0x41465320))
                return null;

            //string archiveName = fileReader.Read_String(offset + 0x08, 0x50);
            uint fileCount = (uint)fileReader.Read_16bits(offset + 0x04);
            byte[] buffer = fileReader.Read(offset + 0x08, (fileCount * 8));
            UInt64 filenameOffset = fileReader.Read_32bits(offset + 0x08 + (fileCount * 8));

            if (filenameOffset != 0)
            {
                uint filenameSize = fileReader.Read_32bits(offset + 0x08 + (fileCount * 8) + 4);
                filenameBuffer = fileReader.Read(offset + filenameOffset, filenameSize);
            }

            for (uint i = 0; i < fileCount; i++)
            {
                uint fileOffset = MemoryReader.ReadLong(ref buffer, (i * 8));
                uint fileSize = MemoryReader.ReadLong(ref buffer, (i * 8) + 0x04);
                string filename = string.Empty;

                if((filenameOffset!=0) && (filenameBuffer.Length!=0))
                    filename = MemoryReader.GetString(ref filenameBuffer, (i*0x30), 0x20);

                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }

            return tmpFST;
        }
    }
}
