using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class FST
    {
        public class cSession
        {
            private byte    m_SessionID;
            private string  m_SessionName;
            private uint    m_RootID;
            private string  m_filePath;

            public byte SessionID
            {
                get { return m_SessionID; }
                set { m_SessionID = value; }
            }

            public string SessionName
            {
                get { return m_SessionName; }
                set { m_SessionName = value; }
            }

            public uint RootID
            {
                get { return m_RootID; }
                set { m_RootID = value; }
            }

            public string FilePath
            {
                get { return m_filePath; }
                set { m_filePath = value; }
            }

            public cSession(byte SessionID, string SessionName, uint RootID, string FilePath) 
            {
                this.SessionID = SessionID;
                this.SessionName = SessionID.ToString().Trim()+ "_" + SessionName;
                this.RootID = RootID;
                this.FilePath = FilePath;
            }
        }
    }
}
