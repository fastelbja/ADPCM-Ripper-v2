using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class IsoStreamReader : IReader
    {
        private void NGC_GetFST(string sPath, bool isWII)
        {
            // Read the first sector to grab disc info & rootDirectory offset & size
            byte[] buffer = m_FileReader.Read(0, 0x800);

            string SessionName = MemoryReader.GetString(ref buffer, 0x20);
            m_FST.FST_Session.Add(new FST.cSession(1, SessionName, 0, sPath));

            uint fstNGCSize = MemoryReader.ReadLongBE(ref buffer, 0x42C);
            UInt64 fstNGCOffset = MemoryReader.ReadLongBE(ref buffer, 0x424);

            // Ensure that we read the all FST
            byte[] fstNGC = m_FileReader.Read(fstNGCOffset, fstNGCSize);
            NGC_ParseRoot(fstNGC, 1, sPath, isWII);
        }

        private void NGC_ParseRoot(byte[] buffer, int Session, string sPath, bool isWII)
        {
            UInt64 fileNumbers = MemoryReader.ReadLongBE(ref buffer, 0x08);
            UInt64 fstSize = fileNumbers * 0x0C;
            UInt64 fstNameSize = (UInt64)buffer.Length - (fileNumbers * 0x0C);

            // Read FST Table ...
            byte[] fst = new byte[fstSize];
            MemoryReader.BytesCopy(buffer, 0, fst, 0, fstSize);

            // Read name Table
            byte[] fstName = new byte[fstNameSize];
            MemoryReader.BytesCopy(buffer, fstSize, fstName, 0, fstNameSize);

            uint LastDir = MemoryReader.ReadLongBE(ref buffer, 0x08) & 0x00ffffff;

            m_FST.FST_Folder.Add(new FST.cFolder((byte)Session, 0, 1, "\\", 0x800));

            NGC_ParseDir(buffer, fstName, Session, 0x0C, 1, LastDir * 0x0C, sPath, isWII);
        }

        public UInt64 NGC_ParseDir(byte[] buffer, byte[] fstName, int SessionID, UInt64 bufferOffset, uint ParentID, UInt64 lastOffset, string sPath, bool isWII)
        {
            byte[] dateStamp = { 0, 0, 0, 0, 0, 0, 0 };

            do
            {
                uint nameOffset = MemoryReader.ReadLongBE(ref buffer, bufferOffset) & 0x00ffffff;
                string Filename = MemoryReader.GetString(ref fstName, nameOffset);

                if (buffer[bufferOffset] == 0x01)
                {
                    uint LastDir = MemoryReader.ReadLongBE(ref buffer, bufferOffset + 0x08) & 0x00ffffff;
                    ParentID = MemoryReader.ReadLongBE(ref buffer, bufferOffset + 0x04) + 1;
                    m_FST.FST_Folder.Add(new FST.cFolder((byte)SessionID, ParentID,(uint)((bufferOffset / 0x0C) + 1), Filename, 0));
                    bufferOffset = NGC_ParseDir(buffer, fstName, SessionID, bufferOffset + 0x0C, (uint)((bufferOffset / 0x0C) + 1), LastDir * 0x0C, sPath, isWII);
                }
                else
                {
                    UInt64 fileOffset =(UInt64)(MemoryReader.ReadLongBE(ref buffer, bufferOffset + 0x04) * (isWII ? 4 : 1));
                    UInt64 fileSize = MemoryReader.ReadLongBE(ref buffer, bufferOffset + 0x08);
                    m_FST.FST_File.Add(new FST.cFile((byte)SessionID, ParentID, 0, Filename,sPath, sPath, fileOffset, fileSize, dateStamp, false));
                    bufferOffset += 0x0C;
                }
            } while (bufferOffset < lastOffset);
            return bufferOffset;
        }
    }
}
