using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_ARC_Lumines : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for ARC marker
            if ((fileReader.Read_32bitsBE(offset) != 0x41524300))
                return null;

            //string archiveName = fileReader.Read_String(offset + 0x08, 0x50);
            uint fileCount = (uint)fileReader.Read_32bits(offset + 0x0C);
            uint offset_adding = (uint)fileReader.Read_32bits(offset + 0x18);
            uint offset_name = (uint)fileReader.Read_32bits(offset + 0x10);

            if((offset_adding==0) || (offset_adding>file.FileSize))
                return null;

            if((offset_name==0) || (offset_name>offset_adding))
                return null;

            byte[] buffer_name = fileReader.Read(offset + offset_name, offset_adding - offset_name);

            // long filenameOffset = fileReader.Read_32bits(offset + 0x08 + (fileCount * 8));

            // if (filenameOffset != 0)
            //{
            //    uint filenameSize = fileReader.Read_32bits(offset + 0x08 + (fileCount * 8) + 4);
            //    filenameBuffer = fileReader.Read(offset + filenameOffset, filenameSize);
            //}

            for (uint i = 0; i < fileCount; i++)
            {
                UInt64 fileOffset = MemoryReader.ReadLong(ref buffer_name, (i * 0x18));
                uint fileSize = MemoryReader.ReadLong(ref buffer_name, (i * 0x18) + 0x04);
                string filename = string.Empty;

                if (fileOffset != 0)
                {
                    fileOffset += offset_adding;
                    uint filenameOffset = MemoryReader.ReadLong(ref buffer_name, (i * 0x18) + 0x10);
                    filename = MemoryReader.GetString(ref buffer_name, (fileCount * 0x18)+filenameOffset, 0xff);
                    tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + fileOffset, fileSize, tmpFST.EmptyDateTime, false));
                }
            }

            return tmpFST;
        }
    }
}
