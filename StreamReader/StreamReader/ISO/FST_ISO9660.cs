using System;
using System.Text;
using System.Runtime.InteropServices;

namespace StreamReader
{
    public partial class IsoStreamReader : IReader
    {
        private bool _isCVM;

        private void ISO9660_GetFST(string sPath,bool isCVM = false)
        {

            byte[] buffer;
            string VolID;
            string SessionName;

            _isCVM = isCVM;

            DirectoryRecord FileInfo = new DirectoryRecord();
            FileInfo.Recording_Date_and_Time = new byte[6];

            // Initiliaze the Primary Descriptor Structure
            InitISOPrimaryDescriptor();

            buffer = new byte[m_TrackSize];

            // Get pointer of the output buffer
            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr dest = gch.AddrOfPinnedObject();

            buffer = ReadCDTracks(16);

            if (buffer != null)
            {
                primary_descriptor = (ISO_PRIMARY_DESCRIPTOR)MemoryReader.BytesToStuct(buffer, primary_descriptor.GetType(), 0);
                VolID = new String(primary_descriptor.Volume_Identifier);
                SessionName = VolID.Trim();

                m_FST.FST_Session.Add(new FST.cSession(1, SessionName, 0, sPath));
                m_FST.FST_Folder.Add(new FST.cFolder(1, 
                                                     0, 
                                                     (uint)(primary_descriptor.RootDirectory.Location_of_Extent_LE + m_StartLBA), 
                                                     "\\", 
                                                     (UInt64)primary_descriptor.RootDirectory.Data_Length_LE));

                ISO_GetFile(m_FST, sPath, SessionName);
                
                // if there a dual session ??
                if(!sPath.Contains("\\\\")) 
                {
                    if ((Int64)m_FileReader.GetLength() > (Int64)(0xffffffff))
                    {
                        if ((Int64)((primary_descriptor.Volume_Space_Size_LE * m_TrackSize) & 0x0000000fffffffff) < ((Int64)m_FileReader.GetLength() - 5000))
                        {
                            FST newFST = m_FST;
                            m_FST = new FST();

                            // Add the dual session
                            m_SessionID = 2;
                            m_StartLBA += (primary_descriptor.Volume_Space_Size_LE - 16);

                            buffer = ReadCDTracks(16);

                            primary_descriptor = (ISO_PRIMARY_DESCRIPTOR)MemoryReader.BytesToStuct(buffer, primary_descriptor.GetType(), 0);
                            VolID = new String(primary_descriptor.Volume_Identifier);
                            SessionName = VolID.Trim();

                            m_FST.FST_Session.Add(new FST.cSession(2, SessionName, 0, sPath));
                            m_FST.FST_Folder.Add(new FST.cFolder(2,
                                                                 0,
                                                                 (uint)(primary_descriptor.RootDirectory.Location_of_Extent_LE),
                                                                 "\\",
                                                                 (UInt64)primary_descriptor.RootDirectory.Data_Length_LE));

                            ISO_GetFile(m_FST, sPath, SessionName);

                            foreach (FST.cSession session in m_FST.FST_Session)
                            {
                                newFST.FST_Session.Add(new FST.cSession(session.SessionID,
                                                                        session.SessionName,
                                                                        session.RootID,
                                                                        session.FilePath));
                            }

                            foreach (FST.cFolder directory in m_FST.FST_Folder)
                            {
                                newFST.FST_Folder.Add(new FST.cFolder(directory.SessionID,
                                                                     directory.ParentID,
                                                                     directory.FolderID,
                                                                     directory.FolderName,
                                                                     directory.FolderLength));
                            }

                            foreach (FST.cFile files in m_FST.FST_File)
                            {
                                newFST.FST_File.Add(new FST.cFile(files.SessionID,
                                                                 files.ParentDirectory,
                                                                 files.ParentFile,
                                                                 files.Filename,
                                                                 files.FileOwner,
                                                                 files.FilePath,
                                                                 files.FileStartOffset,
                                                                 files.FileSize,
                                                                 m_FST.EmptyDateTime,
                                                                 files.OffsetIsLBA));
                            }

                            m_FST = newFST;
                        }
                    }
                }
            }
            else
                return;
        }

        private void ISO_GetFile(FST PreviousFST, string sPath, string SessionName)
        {
            byte[] FI_Data;
            byte[] buffer;
            UInt64 bufferOffset = 0;
            uint oneSectorSize = m_SectorSize[(int)m_CurrentSectorType];

            FST CurrentFST = new FST();

            DirectoryRecord FileInfo = new DirectoryRecord();
            FileInfo.Recording_Date_and_Time = new byte[6];

            foreach (FST.cFolder f in PreviousFST.FST_Folder)
            {
                if (f.SessionID == m_SessionID)
                {
                    bufferOffset = 0;
                    buffer = new byte[f.FolderLength];
                    buffer = ReadCDBuffer(f.FolderID, f.FolderLength);

                    do
                    {
                        if (bufferOffset >= (UInt64)buffer.Length)
                            break;

                        FileInfo = (DirectoryRecord)MemoryReader.BytesToStuct(buffer, FileInfo.GetType(), bufferOffset);

                        if (FileInfo.Directory_Record_Length != 0)
                        {
                            if (FileInfo.File_Identifier_Length != 0)
                            {
                                FI_Data = new byte[FileInfo.File_Identifier_Length];
                                MemoryReader.BytesCopy(buffer, bufferOffset + (UInt64)(Marshal.SizeOf(FileInfo) - 1), FI_Data, 0, FileInfo.File_Identifier_Length);

                                FileInfo.File_Identifier = Encoding.Default.GetString(FI_Data);

                                if (FileInfo.File_Identifier.Contains(";"))
                                    FileInfo.File_Identifier = FileInfo.File_Identifier.Replace(";1", "");

                                if (!FileInfo.File_Identifier.Equals("\0") && (!FileInfo.File_Identifier.Equals("")))
                                {
                                    if (FileInfo.File_Flags == 0x02)
                                    {
                                        CurrentFST.FST_Folder.Add(new FST.cFolder(f.SessionID,
                                                                                  f.FolderID,
                                                                                  (uint)(FileInfo.Location_of_Extent_LE),
                                                                                  FileInfo.File_Identifier,
                                                                                  (UInt64)FileInfo.Data_Length_LE));
                                    }
                                    else
                                        m_FST.FST_File.Add(new FST.cFile(f.SessionID,
                                                                         f.FolderID,
                                                                         0,
                                                                         FileInfo.File_Identifier,
                                                                         sPath,
                                                                         sPath,(UInt64)((FileInfo.Location_of_Extent_LE + m_StartLBA) * oneSectorSize),
                                                                         (UInt64)FileInfo.Data_Length_LE,
                                                                         FileInfo.Recording_Date_and_Time, _isCVM));
                                }
                            }

                            bufferOffset += FileInfo.Directory_Record_Length;
                        }
                        else
                        {
                            if (bufferOffset < (UInt64)((buffer.Length - Marshal.SizeOf(FileInfo))))
                                bufferOffset++;
                            else
                                break;
                        }
                    } while (1 == 1);
                }
            }

            // Add each Folder to current FST ...
            if (CurrentFST.FST_Folder.Count > 0)
            {
                for (int i = 0; i < CurrentFST.FST_Folder.Count; i++)
                {
                    m_FST.FST_Folder.Add(CurrentFST.FST_Folder[i]);
                }
                ISO_GetFile(CurrentFST, sPath, SessionName);
            }
        }
    }
}
