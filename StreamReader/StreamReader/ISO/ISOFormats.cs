using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class IsoStreamReader : IReader
    {
        public bool IsStandardISO(string sPath)
        {
            byte[] trackBuffer;
            bool isCVM = false;

            // CVM Cri File System is a standard ISO format with informations on header ...
            if (System.IO.Path.GetExtension(sPath).ToUpper() == ".CVM")
            {
                isCVM = true;
                m_StartLBA = 3;
            }
            
            trackBuffer = ReadCDTracks(16);

            InitISOPrimaryDescriptor();
            primary_descriptor = (ISO_PRIMARY_DESCRIPTOR)MemoryReader.BytesToStuct(trackBuffer, primary_descriptor.GetType(), 0);

            // Check Standard Identifier
            if (Encoding.Default.GetString(primary_descriptor.Standard_Identifier) == "CD001")
            {
                this.ISOType = ISO_Type.ISO9660;

                if (isCVM)
                    this.ISOType = ISO_Type.CVM;

                return true;
            }
            return false;
        }

        public bool IsNGCISO(string sPath)
        {
            byte[] buffer = m_FileReader.Read(0, 0x800);

            if (buffer.Length == 0x800)
            {
                if ((buffer[0] != 0) && (MemoryReader.ReadLong(ref buffer, 0x1C) == 0x3D9F33C2))
                {
                    this.ISOType = ISO_Type.NGC;
                    return true;
                }
            }
            return false;
        }


        public bool IsWII(string sPath)
        {
            byte[] buffer = m_FileReader.Read(0, 0x800);

            if (buffer.Length == 0x800)
            {
                if (((buffer[0] == 'R') || (buffer[0] == 'S')) && (MemoryReader.ReadLong(ref buffer, 0x18) == 0xA39E1C5D))
                {
                    this.ISOType = ISO_Type.WII;
                    InitWII(sPath);
                    return true;
                }
            }
            return false;
        }

        public bool IsWIIU(string sPath)
        {
            byte[] buffer = m_FileReader.Read(0, 0x800);

            if (buffer.Length == 0x800)
            {
                if (((buffer[0] == 'W') || (buffer[0] == 'U')) && (MemoryReader.ReadLong(ref buffer, 0x18) == 0x00000000))
                {
                    this.ISOType = ISO_Type.WIIU;
                    InitWII(sPath);
                    return true;
                }
            }
            return false;
        }

        public bool IsXBOX1(string sPath)
        {
            if (IsXBOX(sPath, 32))
            {
                m_ISOType = ISO_Type.XBOX;
                return true;
            }
            else
                return false;
        }

        public bool IsXBOX1_Real(string sPath)
        {
            if (IsXBOX(sPath, 0x30620))
            {
                m_ISOType = ISO_Type.XBOX_REAL;
                return true;
            }
            else
                return false;
        }

        public bool IsXBOX360(string sPath)
        {
            if (IsXBOX(sPath, 32 + 0x1FB20))
            {
                m_ISOType = ISO_Type.XBOX360;
                return true;
            }
            else
                return false;
        }

        public bool IsXBOX360XDG3(string sPath)
        {
            if (IsXBOX(sPath, 32 + 0x4100))
            {
                m_ISOType = ISO_Type.XBOX360_XDG3;
                return true;
            }
            else
                return false;
        }
        // one function is used for Xbox & Xbox 360 depending of the firstSector parameter
        private bool IsXBOX(string sPath, int firstSector)
        {
            byte[] buffer = ReadCDTracks(firstSector);

            string xboxID = Encoding.Default.GetString(buffer, 0, 0x14);

            // Check Standard Identifier
            if (xboxID == "MICROSOFT*XBOX*MEDIA")
                return true;
            else
                return false;
        }

    }
}
