using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StreamReader
{
    public partial class IsoStreamReader : IReader
    {
        private IReader m_FileReader = new BinaryStreamReader();
        private FST m_FST;
        private string m_Filename;

        public string GetFilename()
        {
            return m_Filename;
        }

        public FST GetFileSystem()
        {
            m_FST = new FST();
            SetSessionID(1);

            switch (this.ISOType)
            {
                case ISO_Type.ISO9660:
                    ISO9660_GetFST(m_Filename);
                    break;
                case ISO_Type.CVM:
                    ISO9660_GetFST(m_Filename,true);
                    break;
                case ISO_Type.NGC:
                    NGC_GetFST(m_Filename,false);
                    break;
                case ISO_Type.WII:
                    GetWIIFST(m_Filename);
                    break;
                case ISO_Type.WIIU:
                    GetWIIFST(m_Filename);
                    break;
                case ISO_Type.XBOX:
                    GetXBOX1FST(m_Filename);
                    break;
                case ISO_Type.XBOX360:
                    GetXBOX360FST(m_Filename);
                    break;
                case ISO_Type.XBOX_REAL:
                    GetXBOXRealFST(m_Filename);
                    break;
                case ISO_Type.XBOX360_XDG3:
                    GetXBOX360XDG3(m_Filename);
                    break;
            }
            return m_FST;
        }

        public string GetDescription()
        {
            switch (this.ISOType)
            {
                case ISO_Type.ISO9660:
                    return "Standard ISO";
                case ISO_Type.CVM:
                    return "CRI ROFS CVM";
                case ISO_Type.NGC:
                    return "Gamecube ISO";
                case ISO_Type.WII:
                    return "WII ISO";
                case ISO_Type.XBOX:
                    return "XBOX1 ISO";
                case ISO_Type.XBOX360:
                    return "XBOX360 ISO";
                case ISO_Type.XBOX_REAL:
                    return "XBOX1 ISO (Redump)";
                case ISO_Type.XBOX360_XDG3:
                    return "XBOX360 ISO (XDG3)";

            }
            return string.Empty;
        }

        public bool Open(string FileName)
        {
            m_Filename = FileName;

            // Try to see if the open file is an ISO
            if (m_FileReader.Open(FileName))
            {
                m_StartLBA = 0;

                if (IsXBOX1(FileName) || IsXBOX1_Real(FileName) || IsXBOX360XDG3(FileName) || IsXBOX360(FileName) || IsStandardISO(FileName) || IsNGCISO(FileName) || IsWII(FileName) || IsWIIU(FileName))
                    return true;
            }
            return false;
        }

        public byte[] Read(UInt64 Offset, UInt64 Length)
        {
            uint oneSectorSize = m_SectorSize[(int)m_CurrentSectorType];
            int sector = (int)(Offset / oneSectorSize);

            uint cache_offset = (uint)(Offset % oneSectorSize);

            UInt64 cache_size = 0;

            byte[] buffer = new byte[Length];

            UInt64 buffer_offset = 0;

            while (Length != 0)
            {
                cache_size = Length;

                if (cache_size + cache_offset > oneSectorSize)
                    cache_size = oneSectorSize - cache_offset;

                if (this.ISOType == ISO_Type.WII)
                {
                    buffer = DecryptBlock(Length, m_SessionID, Offset);
                    return buffer;
                }
                else
                    MemoryReader.BytesCopy(ReadCDBuffer(sector, oneSectorSize), cache_offset, buffer, buffer_offset, cache_size);

                buffer_offset += cache_size;
                Length -= cache_size;
                cache_offset = 0;
                sector++;
            }
            return buffer;
        }

        public UInt16 Read_16bitsBE(UInt64 Offset)
        {
            byte[] buffer = new byte[2];
            buffer = this.Read(Offset, 2);
            return (MemoryReader.ReadIntBE(ref buffer, 0));
        }

        public UInt16 Read_16bits(UInt64 Offset)
        {
            byte[] buffer = new byte[2];
            buffer = this.Read(Offset, 2);
            return (MemoryReader.ReadInt(ref buffer, 0));
        }

        public UInt32 Read_32bitsBE(UInt64 Offset)
        {
            byte[] buffer = new byte[4];
            buffer = this.Read(Offset, 4);
            return (MemoryReader.ReadLongBE(ref buffer, 0));
        }

        public UInt32 Read_32bits(UInt64 Offset)
        {
            byte[] buffer = new byte[4];
            buffer = this.Read(Offset, 4);
            return (MemoryReader.ReadLong(ref buffer, 0));
        }

        public byte Read_8Bits(UInt64 Offset)
        {
            byte[] buffer = new byte[1];
            buffer = this.Read(Offset, 1);
            return buffer[0];
        }

        public string Read_String(UInt64 Offset, UInt64 Length)
        {
            byte[] buffer = new byte[Length];
            int bufferOffset = 0;

            buffer = this.Read(Offset, Length);

            string result = "";

            do
            {
                if (buffer[bufferOffset] == 0)
                    break;

                result += Convert.ToChar(buffer[bufferOffset]);
                bufferOffset++;
            } while (--Length > 0);

            return result;
        }

        public UInt64 GetLength()
        {
            return m_FileReader.GetLength();
        }

        public bool CanRead()
        {
            return m_FileReader.CanRead();
        }

        public byte GetSessionID()
        {
            return m_SessionID;
        }

        public void Close()
        {
            if(m_FileReader!=null)
                m_FileReader.Close();
        }
    }
}
