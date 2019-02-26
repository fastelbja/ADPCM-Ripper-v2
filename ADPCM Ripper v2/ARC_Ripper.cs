using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StreamReader;
using StreamReader.Container;

namespace ADPCM_Ripper_v2
{
    public class ARC_Ripper
    {
        public void Do_RipContainer(IReader fileReader, FST currFST)
        {
            FST tmpFST;
            int filecount = currFST.FST_File.Count;
            int currFile=0;
            UInt64 maxOffset = 0;

            List<IContainer> fntToCall = new List<IContainer>();

            fntToCall.Add(new StreamReader.Container.ARC_POD2());
            fntToCall.Add(new StreamReader.Container.ARC_AFS());
            //fntToCall.Add(new StreamReader.Container.ARC_AFS2());
            fntToCall.Add(new StreamReader.Container.ARC_IPK());
            fntToCall.Add(new StreamReader.Container.ARC_ARC_Lumines());
            fntToCall.Add(new StreamReader.Container.ARC_HG2());
            fntToCall.Add(new StreamReader.Container.ARC_DKPG());
            //fntToCall.Add(new StreamReader.Container.ARC_FPAC());
            fntToCall.Add(new StreamReader.Container.ARC_XAST());
            fntToCall.Add(new StreamReader.Container.ARC_BIGF());
            fntToCall.Add(new StreamReader.Container.ARC_PAK_MOTORSTORM());
            //fntToCall.Add(new StreamReader.Container.ARC_CLU());

            do
            {
                foreach (IContainer arcFormat in fntToCall)
                {
                    tmpFST = arcFormat.IsContainer(fileReader, currFST.FST_File[currFile],currFile);
                    if (tmpFST != null)
                    {
                        uint FolderID = AddFilesToFolder(ref currFST, currFile, currFST.FST_File[currFile].SessionID);
                        currFST.FST_File[currFile].IsVisible = false;

                        for (int j = 0; j < tmpFST.FST_File.Count; j++)
                        {
                            currFST.FST_File.Add(new FST.cFile(currFST.FST_File[currFile].SessionID,
                                                               FolderID,
                                                               (uint)currFile,
                                                               tmpFST.FST_File[j].Filename,
                                                               tmpFST.FST_File[j].FileOwner,
                                                               tmpFST.FST_File[j].FilePath,
                                                               tmpFST.FST_File[j].FileStartOffset,
                                                               tmpFST.FST_File[j].FileSize,
                                                               tmpFST.EmptyDateTime ,
                                                               tmpFST.FST_File[j].OffsetIsLBA));
                            if (tmpFST.FST_File[j].FileStartOffset + tmpFST.FST_File[j].FileSize > maxOffset)
                                maxOffset = tmpFST.FST_File[j].FileStartOffset + tmpFST.FST_File[j].FileSize;
                        }
                    }
                }
                filecount = currFST.FST_File.Count;
                currFile++;
            } while (currFile < filecount);
        }

        public uint AddFilesToFolder(ref FST currFST,int index, byte sessionID)
        {
            uint FolderID = 0;

            foreach (FST.cFolder directory in currFST.FST_Folder)
            {
                if (directory.FolderID > FolderID)
                    FolderID = directory.FolderID;
            }
            FolderID++;

            currFST.FST_Folder.Add(new FST.cFolder(sessionID,
                                                   currFST.FST_File[index].ParentDirectory, 
                                                   FolderID,
                                                   currFST.FST_File[index].Filename,
                                                   currFST.FST_File[index].FileSize));
            return FolderID;
        }

    }
}
