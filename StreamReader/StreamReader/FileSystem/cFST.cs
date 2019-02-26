using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamReader
{
    public partial class FST
    {
        public List<cFile> FST_File = new List<cFile>();
        public List<cFolder> FST_Folder = new List<cFolder>();
        public List<cSession> FST_Session = new List<cSession>();

        public byte[] EmptyDateTime
        {
            get
            {
                byte[] dateStamp = { 0, 0, 0, 0, 0, 0, 0 };
                return dateStamp;
            }
        }
    }
}
