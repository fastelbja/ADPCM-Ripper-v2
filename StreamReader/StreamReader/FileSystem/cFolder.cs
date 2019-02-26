using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class FST
    {
        public class cFolder
        {
            private byte    m_SessionID;
            private uint    m_ParentID;
            private uint    m_FolderID;
            private string  m_Foldername;
            private UInt64  m_FolderLength;
            private bool    m_Processed;

            public byte SessionID
            {
                get { return m_SessionID; }
                set { m_SessionID = value; }
            }

            public uint ParentID
            {
                get { return m_ParentID; }
                set { m_ParentID = value; }
            }

            public uint FolderID
            {
                get { return m_FolderID; }
                set { m_FolderID = value; }
            }

            public string FolderName
            {
                get { return m_Foldername; }
                set { m_Foldername = value; }
            }

            public UInt64 FolderLength
            {
                get { return m_FolderLength; }
                set { m_FolderLength = value; }
            }

            public bool Processed
            {
                get { return m_Processed; }
                set { m_Processed = value; }
            }

            public cFolder(byte SessionID, uint ParentID, uint FolderID, string FolderName, UInt64 FolderLength)
            {
                this.SessionID = SessionID;
                this.ParentID = ParentID;
                this.FolderID = FolderID;
                this.FolderName = FolderName;
                this.FolderLength = FolderLength;
                this.Processed = false;
            }
        }
    }
}
