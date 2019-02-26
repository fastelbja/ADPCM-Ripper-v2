using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class FST
    {
        public class cFile
        {
            private byte    m_SessionID;
            private uint    m_ParentDirectory;
            private uint    m_ParentFile;

            private string  m_Filename;
            private string  m_FileOwner;

            private string  m_FilePath;
            private UInt64   m_FileStartOffset;
            private UInt64 m_FileSize;
            private DateTime  m_FileDateTime;

            private bool    m_isChecked;
            private bool    m_isVisible;
            private bool    m_isToRip;
            private bool    m_OffsetIsLBA;

            public byte SessionID
            {
                get { return m_SessionID; }
                set { m_SessionID = value; }
            }

            public uint ParentDirectory
            {
                get { return m_ParentDirectory; }
                set { m_ParentDirectory = value; }
            }

            public uint ParentFile
            {
                get { return m_ParentFile; }
                set { m_ParentFile = value; }
            }

            public string Filename
            {
                get { return m_Filename; }
                set { m_Filename = value; }
            }

            public string FileOwner
            {
                get { return m_FileOwner; }
                set { m_FileOwner = value; }
            }

            public string FilePath
            {
                get { return m_FilePath; }
                set { m_FilePath = value; }
            }

            public UInt64 FileStartOffset
            {
                get { return m_FileStartOffset; }
                set { m_FileStartOffset = value; }
            }

            public UInt64 FileSize
            {
                get { return m_FileSize; }
                set { m_FileSize = value; }
            }

            public DateTime FileDateTime
            {
                get { return m_FileDateTime; }
                set { m_FileDateTime = value; }
            }

            public bool IsChecked
            {
                get { return m_isChecked; }
                set { m_isChecked = value; }
            }

            public bool OffsetIsLBA
            {
                get { return m_OffsetIsLBA; }
                set { m_OffsetIsLBA = value; }
            }

            public bool IsVisible
            {
                get { return m_isVisible; }
                set { m_isVisible = value; }
            }

            public bool IsToRip
            {
                get { return m_isToRip; }
                set { m_isToRip = value; }
            }

            public cFile(byte _SessionID, uint _ParentDirectory, uint _ParentFile, string _Filename, string _FileOwner,
                         string _FilePath, UInt64 _FileStartOffset, UInt64 _FileSize, byte[] _FileDateTime, bool _OffsetIsLBA)
            {
                m_SessionID = _SessionID;
                m_ParentDirectory = _ParentDirectory;
                m_ParentFile = _ParentFile;
                m_Filename = GetWindowsCompliantFilename(_Filename);
                m_FilePath = _FilePath;
                m_FileOwner = _FileOwner;
                m_FileStartOffset = _FileStartOffset;
                
                if (m_Filename == string.Empty)
                    m_Filename = System.IO.Path.GetFileNameWithoutExtension(m_FilePath) + m_FileStartOffset.ToString("x8");

                m_FileSize = _FileSize;  
                m_FileDateTime = GetDateTime(_FileDateTime);
                m_isChecked = false;
                m_isVisible= true;
                m_isToRip = true;
                m_OffsetIsLBA = _OffsetIsLBA;
            }

            private string GetWindowsCompliantFilename(string fileName)
            {
                byte[] filenameChar = Encoding.ASCII.GetBytes(fileName);
                string tmpFilename = string.Empty;

                for (int i = 0; i < filenameChar.Length; i++)
                {
                    if (((filenameChar[i] >= 'A' && filenameChar[i] <= 'Z') ||
                         (filenameChar[i] >= 'a' && filenameChar[i] <= 'z') ||
                         (filenameChar[i] >= '0' && filenameChar[i] <= '9')) ||
                         (filenameChar[i] == '\\') ||
                         (filenameChar[i] == '_') ||
                         (filenameChar[i] == '-') ||
                         (filenameChar[i] == '/') ||
                         (filenameChar[i] == '.'))
                    {
                        tmpFilename += Convert.ToChar(filenameChar[i]);
                    }
                }
                tmpFilename = tmpFilename.Replace("/", "\\");
                return tmpFilename;
            }

            public DateTime GetDateTime(byte[] FileDateTime)
            {
                DateTime dt;

                try
                {
                    if (FileDateTime[0] != 0)
                        dt = new DateTime(FileDateTime[0] + 1900, FileDateTime[1], FileDateTime[2], FileDateTime[3], FileDateTime[4], FileDateTime[5], DateTimeKind.Utc).Date;
                    else
                        dt = DateTime.Now.Date;
                    return dt;
                }
                catch (Exception e)
                {
                    return DateTime.Now;
                }
            }
        }
    }
}
