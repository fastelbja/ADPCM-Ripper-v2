using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader.Container
{
    public class ARC_IPK : IContainer
    {
        public FST IsContainer(StreamReader.IReader fileReader, FST.cFile file, int index)
        {

            if (System.IO.Path.GetExtension(file.Filename).ToUpper() != ".IPK")
                return null;

            FST tmpFST = new FST();

            UInt64 offset = file.FileStartOffset;

            // check for 0x50EC12BA marker
            if (fileReader.Read_32bitsBE(offset) != 0x50EC12BA)
                return null;

            //string archiveName = fileReader.Read_String(offset + 0x08, 0x50);
            uint fileCount = (uint)fileReader.Read_32bitsBE(offset + 0x10);
            uint bufferSize = (uint)fileReader.Read_32bitsBE(offset + 0x0C)-0x30;
            byte[] buffer = fileReader.Read(offset + 0x30, bufferSize);
            
            uint buf_offset=0;
            uint nameSize = 0;
            uint folderSize = 0;
            UInt64 fileOffset = bufferSize + 0x30;

            for (uint i = 0; i < fileCount; i++)
            {
                uint var_temp = MemoryReader.ReadLongBE(ref buffer, buf_offset);
                buf_offset+=4;
                uint fileSizeUncompressed = MemoryReader.ReadLongBE(ref buffer, buf_offset);
                buf_offset += 4;
                uint fileSizeCompressed = MemoryReader.ReadLongBE(ref buffer, buf_offset);
                buf_offset += 4+8;
                UInt64 trueFileOffset = MemoryReader.ReadLongLongBE(ref buffer, buf_offset)+fileOffset;
                buf_offset += 8;

                if (var_temp == 2)
                    buf_offset += 8;

                nameSize = MemoryReader.ReadLongBE(ref buffer, buf_offset);
                buf_offset +=4;
                string filename = string.Empty;
                filename = MemoryReader.GetString(ref buffer, buf_offset, nameSize);
                buf_offset += nameSize;
                folderSize = MemoryReader.ReadLongBE(ref buffer, buf_offset);
                buf_offset+=4;
                filename = MemoryReader.GetString(ref buffer, buf_offset, folderSize) + filename;
                filename = filename.Replace("/","\\");
                if (fileSizeCompressed == 0)
                    fileSizeCompressed = fileSizeUncompressed;
                uint fileSize = fileSizeCompressed;
                buf_offset += folderSize+8;
                tmpFST.FST_File.Add(new FST.cFile(file.SessionID, (uint)index, 0, filename, file.FileOwner, file.FilePath, offset + trueFileOffset, fileSize, tmpFST.EmptyDateTime, false));
            }

            return tmpFST;
        }

    }
}
