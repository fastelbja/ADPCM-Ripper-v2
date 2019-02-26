using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class IsoStreamReader : IReader
    {
        private int m_SkipSector;

        public void GetXBOX1FST(string sPath)
        {
            m_SkipSector = 0;
            GetXBOXFST(sPath);
        }

        public void GetXBOXRealFST(string sPath)
        {
            m_SkipSector = 0x30620-32;
            GetXBOXFST(sPath);
        }

        public void GetXBOX360FST(string sPath)
        {
            m_SkipSector = 0x1FB20;
            GetXBOXFST(sPath);
        }

        public void GetXBOX360XDG3(string sPath)
        {
            m_SkipSector = 0x4100;
            GetXBOXFST(sPath);
        }

        public void GetXBOXONE(string sPath)
        {
            m_SkipSector = 0x80000;
            GetXBOXFST(sPath);
        }

        // one function is used for Xbox & Xbox 360 depending of the SkipSector parameter
        private void GetXBOXFST(string sPath)
        {
            byte[] buffer = ReadCDTracks(32 + m_SkipSector);

            m_FST.FST_Session.Add(new FST.cSession(1, System.IO.Path.GetFileName(sPath), 0, System.IO.Path.GetFileName(sPath)));
            m_FST.FST_Folder.Add(new FST.cFolder(1, 0, 1, "\\", 0x800));
            int Sector = (int)MemoryReader.ReadLong(ref buffer, 0x14);
            int SectorSize = (int)MemoryReader.ReadLong(ref buffer, 0x18);

            buffer = ReadCDBuffer(Sector + m_SkipSector,(UInt64)(SectorSize * 2048));
            ParseXBOX(buffer, 0, 1, sPath, m_SkipSector);
        }

        private void ParseXBOX(byte[] buffer, uint Offset, uint ParentID, string sPath, int SkipSector)
        {
            if ((Offset > buffer.Length) || (buffer.Length==0))
                return;

            // Date Stamp is unused on XBOX & XBOX 360
            byte[] dateStamp = { 0, 0, 0, 0, 0, 0, 0 };

            uint skip = (uint)MemoryReader.ReadInt(ref buffer, Offset + 0);
            uint Forwards = (uint)MemoryReader.ReadInt(ref buffer, Offset + 2);

            if (skip > 0)
            {
                ParseXBOX(buffer, skip * 4, ParentID, sPath, SkipSector);
            }

            if ((buffer[Offset + 0x0C] == 0x10) || (buffer[Offset + 0x0C] == 0x90))
            {
                uint Sector = MemoryReader.ReadLong(ref  buffer, Offset + 0x04);
                long SectorSize = MemoryReader.ReadLong(ref  buffer, Offset + 0x08) * 2048;

                if (Sector != 0)
                {
                    string folderName = Encoding.Default.GetString(buffer, (int)(Offset + 0x0E), buffer[Offset + 0x0D]);
                    m_FST.FST_Folder.Add(new FST.cFolder(1, ParentID, Sector, folderName, 0));
                    byte[] newFolder = ReadCDBuffer(Sector + m_SkipSector,(UInt64)SectorSize);
                    ParseXBOX(newFolder, 0, Sector, sPath, SkipSector);
                }
            }
            else
            {
                string fileName = Encoding.Default.GetString(buffer, (int)(Offset + 0x0E), buffer[Offset + 0x0D]);
                UInt64 fileOffset =(UInt64)((MemoryReader.ReadLong(ref buffer, Offset + 0x04) + SkipSector)*0x800);
                UInt64 fileSize = MemoryReader.ReadLong(ref buffer, Offset + 0x08);
                m_FST.FST_File.Add(new FST.cFile(1, ParentID, ParentID, fileName, sPath, sPath, fileOffset, fileSize, dateStamp,true));
            }

            if (Forwards > 0)
            {
                ParseXBOX(buffer, Forwards * 4, ParentID, sPath, SkipSector);
            }
        }
    }
}
