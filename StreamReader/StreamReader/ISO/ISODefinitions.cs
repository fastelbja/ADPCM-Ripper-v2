using System;
using System.Text;
using System.Runtime.InteropServices;

namespace StreamReader
{
    public partial class IsoStreamReader : IReader
    {
        #region ISODefinitions
        // Reference = ECMA-119 2nd Edition Decembre 1987
        // http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-119.pdf

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct AsciiTimeStamp
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] Year;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Month;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Day;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Hours;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Minutes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Seconds;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public char[] Hundredths_of_Second;
            public Byte TimeZone;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DirectoryRecord
        {
            public Byte Directory_Record_Length;
            public Byte Extended_Attribute_Record_Length;
            public uint Location_of_Extent_LE;
            public uint Location_of_Extent_BE;
            public Int32 Data_Length_LE;
            public Int32 Data_Length_BE;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public Byte[] Recording_Date_and_Time;

            public Byte File_Flags;
            public Byte File_Unit_Size;
            public Byte Interleave_Gap_Size;
            public Int16 Volume_Sequence_Number_LE;
            public Int16 Volume_Sequence_Number_BE;
            public Byte File_Identifier_Length;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
            public string File_Identifier;
        }

        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct ISO_PRIMARY_DESCRIPTOR
        {
            public Byte Volume_Descriptor_Type;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public Byte[] Standard_Identifier;

            public Byte Volume_Descriptor_Version;
            public Byte Unused_Field1;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] System_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] Volume_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] Unused_Field2;

            public Int32 Volume_Space_Size_LE;
            public Int32 Volume_Space_Size_BE;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public Byte[] Unused_Field3;

            public Int16 Volume_Set_Size_LE;
            public Int16 Volume_Set_Size_BE;
            public Int16 Volume_Sequence_Number_LE;
            public Int16 Volume_Sequence_Number_BE;
            public Int16 Logical_Block_Size_LE;
            public Int16 Logical_Block_Size_BE;

            public Int32 Path_Table_Size_LE;
            public Int32 Path_Table_Size_BE;

            public Int32 Location_L_Path_Table;
            public Int32 Location_Optional_L_Path_Table;
            public Int32 Location_M_Path_Table;
            public Int32 Location_Optional_M_Path_Table;

            // Directory Record for Root Directory
            public DirectoryRecord RootDirectory;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] Volume_Set_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] Publisher_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] Data_Preparer_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public char[] Application_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public char[] Copyright_File_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public char[] Abstract_File_Identifier;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public char[] Bibliographic_File_Identifier;

            public AsciiTimeStamp Volume_Creation_Date_Time;
            public AsciiTimeStamp Volume_Modification_Date_Time;
            public AsciiTimeStamp Volume_Expiration_Date_Time;
            public AsciiTimeStamp Volume_Effective_Date_Time;

            public Byte File_Structure_Version;
            public Byte Unused_Field4;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public char[] Application_Use;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)]
            public Byte[] Unused_Field5;
        }

        private ISO_PRIMARY_DESCRIPTOR primary_descriptor = new ISO_PRIMARY_DESCRIPTOR();


        private void InitISOPrimaryDescriptor()
        {
            primary_descriptor.Standard_Identifier = new Byte[6];
            primary_descriptor.System_Identifier = new char[32];
            primary_descriptor.Volume_Identifier = new char[32];
            primary_descriptor.Volume_Set_Identifier = new char[128];
            primary_descriptor.Publisher_Identifier = new char[128];
            primary_descriptor.Data_Preparer_Identifier = new char[128];
            primary_descriptor.Application_Identifier = new char[128];
            primary_descriptor.Copyright_File_Identifier = new char[37];
            primary_descriptor.Abstract_File_Identifier = new char[37];
            primary_descriptor.Bibliographic_File_Identifier = new char[37];
            primary_descriptor.Unused_Field2 = new Byte[8];
            primary_descriptor.Unused_Field3 = new Byte[32];
            primary_descriptor.Application_Use = new char[512];
            primary_descriptor.Unused_Field5 = new Byte[653];
            primary_descriptor.Volume_Creation_Date_Time.Year = new char[4];
            primary_descriptor.Volume_Creation_Date_Time.Month = new char[2];
            primary_descriptor.Volume_Creation_Date_Time.Day = new char[2];
            primary_descriptor.Volume_Creation_Date_Time.Hours = new char[2];
            primary_descriptor.Volume_Creation_Date_Time.Minutes = new char[2];
            primary_descriptor.Volume_Creation_Date_Time.Seconds = new char[2];
            primary_descriptor.Volume_Creation_Date_Time.Hundredths_of_Second = new char[2];
            primary_descriptor.Volume_Modification_Date_Time.Year = new char[4];
            primary_descriptor.Volume_Modification_Date_Time.Month = new char[2];
            primary_descriptor.Volume_Modification_Date_Time.Day = new char[2];
            primary_descriptor.Volume_Modification_Date_Time.Hours = new char[2];
            primary_descriptor.Volume_Modification_Date_Time.Minutes = new char[2];
            primary_descriptor.Volume_Modification_Date_Time.Seconds = new char[2];
            primary_descriptor.Volume_Modification_Date_Time.Hundredths_of_Second = new char[2];
            primary_descriptor.Volume_Expiration_Date_Time.Year = new char[4];
            primary_descriptor.Volume_Expiration_Date_Time.Month = new char[2];
            primary_descriptor.Volume_Expiration_Date_Time.Day = new char[2];
            primary_descriptor.Volume_Expiration_Date_Time.Hours = new char[2];
            primary_descriptor.Volume_Expiration_Date_Time.Minutes = new char[2];
            primary_descriptor.Volume_Expiration_Date_Time.Seconds = new char[2];
            primary_descriptor.Volume_Expiration_Date_Time.Hundredths_of_Second = new char[2];
            primary_descriptor.Volume_Effective_Date_Time.Year = new char[4];
            primary_descriptor.Volume_Effective_Date_Time.Month = new char[2];
            primary_descriptor.Volume_Effective_Date_Time.Day = new char[2];
            primary_descriptor.Volume_Effective_Date_Time.Hours = new char[2];
            primary_descriptor.Volume_Effective_Date_Time.Minutes = new char[2];
            primary_descriptor.Volume_Effective_Date_Time.Seconds = new char[2];
            primary_descriptor.Volume_Effective_Date_Time.Hundredths_of_Second = new char[2];
            primary_descriptor.RootDirectory.Recording_Date_and_Time = new byte[6];
        }
        #endregion

        #region SectorType
        public enum SectorType : int
        {
            NONE = 0,
            RAW_AUDIO = 1,
            MODE1 = 2,
            MODE2FORM1 = 3,
            MODE2FORM2 = 4,
            MODE2PLAIN = 5,
            RAW_SUBCHANNELS = 6
        }
        private SectorType m_CurrentSectorType = SectorType.NONE;
        private Byte[] SYNC_PATTERN = { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };
        private bool m_GotSyncPattern;
        private uint m_SyncPattLength;
        private uint m_TrackSkipOffset;
        private uint m_TrackSize;

        #endregion

        #region ISOType

        public enum ISO_Type : int
        {
            ISO9660 = 0,
            CDI = 1,
            XBOX = 2,
            XBOX360 = 3,
            PANA3DO = 4,
            WII = 5,
            NGC = 6,
            CVM = 7,
            XBOX_REAL = 8,
            XBOX360_XDG3 = 9,
            WIIU = 10
        }
        private ISO_Type m_ISOType;
        public string[] ISO_Description = { "Standard ISO", "Philips CD-i ISO", "XBOX ISO", "XBOX 360 ISO", "Panasonic 3DO ISO", "Nintendo WII ISO", "Nintendo Gamecube ISO", "CRI CVM File System", "Redump XBOX ISO", "XBOX360 XDG3 ISO","WII-U ISO"};
        private uint[] m_SectorSize = { 2048, 2048, 2048, 2048, 2048, 2328, 2448, 2048, 2048, 2048, 2048 };

        #endregion

        private int m_StartLBA=0;
        private byte m_SessionID = 0;

        public ISO_Type ISOType 
        {
            get { return m_ISOType; }
            set { m_ISOType = value; }
        }

        public string ISODescription
        {
            get 
            { 
                string description = ISO_Description[(int)m_ISOType];
                switch (m_CurrentSectorType)
                {
                    case SectorType.MODE1:
                        description += "(MODE1)";
                        break;
                    case SectorType.MODE2FORM1:
                        description += "(MODE2 - FORM1)";
                        break;
                    case SectorType.MODE2FORM2:
                        description += "(MODE2 - XA)";
                        break;
                    case SectorType.MODE2PLAIN:
                        description += "(MODE2)";
                        break;
                }
                return description;
            }
        }

        public void SetSessionID(byte Session)
        {
            m_SessionID = Session; 
        }

        private void CDRomGetSectorMode()
        {
            // Read First Sector of a DVD/ISO file
            byte[] buffer = new byte[2352];
            buffer = m_FileReader.Read(0, 2352);

            // Check for SyncPattern
            m_GotSyncPattern = true;

            for (int i = 0; i < SYNC_PATTERN.Length; i++)
            {
                if (buffer[i] != SYNC_PATTERN[i])
                {
                    m_GotSyncPattern = false;
                    break;
                }
            }

            if (m_GotSyncPattern)
            {
                m_CurrentSectorType = SectorType.MODE2PLAIN;

                switch (buffer[15])
                {
                    case 1:
                        m_CurrentSectorType = SectorType.MODE1;
                        break;
                    case 2:
                        if ((buffer[0x10] == buffer[0x14]) &&
                            (buffer[0x11] == buffer[0x15]) &&
                            (buffer[0x12] == buffer[0x16]) &&
                            (buffer[0x13] == buffer[0x17]))
                        {
                            if (!((buffer[0x18] & (Byte)0x20) == 0))
                                m_CurrentSectorType = SectorType.MODE2FORM2;
                            else
                                m_CurrentSectorType = SectorType.MODE2FORM1;
                        }
                        break;
                }
            }
            else
                m_CurrentSectorType = SectorType.RAW_AUDIO;

            m_SyncPattLength = 0;
            m_TrackSkipOffset = 0;

            if (m_GotSyncPattern)
            {
                switch (m_CurrentSectorType)
                {
                    case SectorType.MODE1:
                        m_SyncPattLength = 0x10;
                        m_TrackSkipOffset = 0x120;

                        if (System.Text.Encoding.ASCII.GetString(buffer, 0x10, 0x10) == "SEGA SEGAKATANA ")
                            m_TrackSkipOffset += 0x60;
                        break;
                    case SectorType.MODE2FORM1:
                    case SectorType.MODE2FORM2:
                        m_SyncPattLength = 0x18;
                        m_TrackSkipOffset = 0x118;
                        break;
                    case SectorType.MODE2PLAIN:
                        m_SyncPattLength = 0x18;
                        m_TrackSkipOffset = 0x60;
                        break;
                    default:
                        m_SyncPattLength = 0x18;
                        m_TrackSkipOffset = 0;
                        break;
                }
            }

            m_TrackSize = m_SectorSize[(int)m_CurrentSectorType] + m_SyncPattLength + m_TrackSkipOffset;
        }

        private byte[] ReadCDTracks(long Sector)
        {
            if (m_CurrentSectorType == SectorType.NONE)
                CDRomGetSectorMode();

            uint oneSectorSize = m_SectorSize[(int)m_CurrentSectorType];

            // Complete sector
            byte[] trackBuffer = new byte[m_TrackSize];

            // only for data (skip sync pattern & unneeded stuff)
            byte[] saveBuffer = new byte[oneSectorSize];

            // add the specified LBA start (for CVM and Xbox 360).
            Sector += m_StartLBA;

            UInt64 offset = (UInt64)(Sector * m_TrackSize);

            trackBuffer = m_FileReader.Read(offset, m_TrackSize);

            MemoryReader.BytesCopy(trackBuffer, m_SyncPattLength, saveBuffer, 0, oneSectorSize);
            return saveBuffer;
        }

        private byte[] ReadCDBuffer(long Sector, UInt64 length)
        {
            byte[] CFDSBuffer = new byte[length];

            uint buffOffset = 0;
            uint oneSectorSize = m_SectorSize[(int)m_CurrentSectorType];

            do
            {
                byte[] readBuffer = ReadCDTracks(Sector);
                MemoryReader.BytesCopy(readBuffer, 0, CFDSBuffer, buffOffset, oneSectorSize);

                buffOffset += oneSectorSize;
                length -= oneSectorSize;

                Sector++;

            } while (length > 0);

            return CFDSBuffer;
        }
    }
}
