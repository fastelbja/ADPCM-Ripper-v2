using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Microsoft.Win32;

using NAudio.Wave;

using StreamReader;
using VGMStream;

namespace ADPCM_Ripper_v2
{
    public partial class frmMain : Form
    {
        private FST m_FST;
        private IReader m_fileReader;
        private VGM_Ripper ripper;

        private VGM_Stream m_vgmStream;
        private DirectSoundOut m_dSoundOut;
        private int m_currMediaFile;
        private string m_textBoxText;

        private CListViewSorter lvwColumnSorter;
        
        private static bool m_Cancel = false;

        private cIcon m_Icon = new cIcon();

        public frmMain()
        {
            InitializeComponent();

            AudioFile audioFile = new AudioFile();
            pgAudioFile.SelectedObject = audioFile;
        }

        private void tsbOpen_Click(object sender, EventArgs e)
        {
            FileOpen();
        }

        public void FileOpen()
        {
            StopStream();

            m_fileReader = new IsoStreamReader();
            m_FST = new FST();
            m_Cancel = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                EnableControls(false);

                foreach (string fileName in openFileDialog.FileNames)
                {
                    OpenOneFile(fileName);
                }

                SearchForArchives(m_fileReader);
                DisplayAllFolders();
                m_fileReader.Close();

                SearchForADPCM(false);

                EnableControls(true);
            }
            textBox1.Text = string.Empty;
        }

        public void OpenOneFile(string sPath)
        {

            Application.DoEvents();

            m_fileReader = new IsoStreamReader();
            if (m_fileReader.Open(sPath))
            {
                //Console.Write(m_fileReader.GetLength().ToString());
                frmWait fWait = new frmWait();

                fWait.Show();
                fWait.Top = this.Top + (this.Height - fWait.Height) / 2;
                fWait.Left = this.Left + (this.Width - fWait.Width) / 2;
                m_FST = m_fileReader.GetFileSystem();
                fWait.Close();
                fWait.Dispose();
            }
            else
            {
                m_fileReader.Close();
                m_fileReader = new BinaryStreamReader();
                m_fileReader.Open(sPath);
                m_FST.FST_Session.Add(new FST.cSession(1, "Root File", 0, sPath));
                m_FST.FST_File.Add(new FST.cFile(1, 0, 1, System.IO.Path.GetFileName(sPath), sPath, sPath, 0, m_fileReader.GetLength(), m_FST.EmptyDateTime, false));
            }

            toolStripStatusISO.Text = m_fileReader.GetDescription();

        }

        public void DisplayAllFolders()
        {
            tvFolders.BeginUpdate();
            tvFolders.Nodes.Clear();

            for (int i = 0; i < m_FST.FST_Session.Count; i++)
            {
                TreeNode rootNode = new TreeNode(m_FST.FST_Session[i].SessionName);
                rootNode.Tag = -1;
                rootNode.ImageIndex = 0;
                rootNode.SelectedImageIndex = 1;
                rootNode.Expand();

                AddFolders(rootNode, m_FST.FST_Session[i].SessionID, m_FST.FST_Session[i].RootID);
                tvFolders.Nodes.Add(rootNode);
                rootNode.Expand();
            }

            tvFolders.EndUpdate();
        }

        public TreeNode AddFolders(TreeNode childNode, int SessionID, uint ParentID)
        {
            for (int i = 0; i < m_FST.FST_Folder.Count; i++)
            {
                if ((m_FST.FST_Folder[i].SessionID == SessionID) && (m_FST.FST_Folder[i].ParentID == ParentID))
                {
                    TreeNode node = new TreeNode(m_FST.FST_Folder[i].FolderName);
                    node.Tag = m_FST.FST_Folder[i].FolderID;
                    node.ImageIndex = 0;
                    node.SelectedImageIndex = 1;

                    if (m_FST.FST_Folder[i].FolderName == "\\")
                    {
                        node.Expand();
                    }
                    node = AddFolders(node, SessionID, m_FST.FST_Folder[i].FolderID);
                    childNode.Nodes.Add(node);
                    tvFolders.SelectedNode = node;
                }
            }
            return childNode;
        }

        public void DisplayFolderContent(int SessionID, uint FolderID)
        {
            ImageList img = new ImageList();
            string[] filenames = new string[m_FST.FST_File.Count];
            int imageCount = 0;
            int imageIndex = 0;
            int fileIndex = 0;

            lvFiles.BeginUpdate();
            lvFiles.Items.Clear();

            m_Icon.DefaultIcon = imageList.Images[2];

            for (int i = 0; i < m_FST.FST_File.Count; i++)
            {
                if ((m_FST.FST_File[i].SessionID == SessionID) && (m_FST.FST_File[i].ParentDirectory == FolderID) && (m_FST.FST_File[i].IsVisible))
                {

                    fileIndex = System.Array.IndexOf(filenames, System.IO.Path.GetExtension(m_FST.FST_File[i].Filename));
                    if (fileIndex == -1)
                    {
                        filenames[imageCount] = System.IO.Path.GetExtension(m_FST.FST_File[i].Filename);
                        Icon ico = new Icon(m_Icon.GetIconFromFile(filenames[imageCount]), 16, 16);
                        img.Images.Add(ico);
                        imageIndex = imageCount;
                        imageCount++;
                    }
                    else
                    {
                        imageIndex = fileIndex;
                    }

                    ListViewItem lvi = new ListViewItem(m_FST.FST_File[i].Filename, imageIndex);

                    lvi.Tag = i;

                    ListViewItem.ListViewSubItem lvSize = new ListViewItem.ListViewSubItem();
                    lvSize.Text = m_FST.FST_File[i].FileSize.ToString();
                    lvi.SubItems.Add(lvSize);

                    ListViewItem.ListViewSubItem lvOffset = new ListViewItem.ListViewSubItem();
                    lvOffset.Text = m_FST.FST_File[i].FileStartOffset.ToString();
                    lvi.SubItems.Add(lvOffset);

                    ListViewItem.ListViewSubItem lvDate = new ListViewItem.ListViewSubItem();
                    lvDate.Text = m_FST.FST_File[i].FileDateTime.ToString();
                    lvi.SubItems.Add(lvDate);

                    ListViewItem.ListViewSubItem lvempty2 = new ListViewItem.ListViewSubItem();
                    lvi.SubItems.Add(lvempty2);

                    ListViewItem.ListViewSubItem lvempty3 = new ListViewItem.ListViewSubItem();
                    lvi.SubItems.Add(lvempty3);


                    if (m_FST.FST_File[i].IsChecked)
                    {
                        lvi.Checked = true;
                    }

                    lvFiles.Items.Add(lvi);
                }
            }
            lvFiles.Sort();
            lvFiles.SmallImageList = img;
            lvFiles.EndUpdate();
        }

        private void tvFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string strRoot = tvFolders.SelectedNode.FullPath;
            string SessionID = strRoot.Substring(0, 1);
            if (tvFolders.SelectedNode.Tag.ToString() != "-1")
            {
                uint FolderID = System.Convert.ToUInt32(tvFolders.SelectedNode.Tag);
                DisplayFolderContent(System.Convert.ToInt32(SessionID), FolderID);
            }
        }

        private void SearchForArchives(IReader fileReader)
        {
            ARC_Ripper archive = new ARC_Ripper();
            archive.Do_RipContainer(fileReader, m_FST);
        }

        private void cmdExtractAll_Click(object sender, EventArgs e)
        {
            DoExtraction(true);
        }

        private void DoExtraction(bool extractAll)
        {
            byte[] buffer = new byte[0x8000];

            int totalSession = m_FST.FST_Session.Count;
            int totalFolder = m_FST.FST_Folder.Count;
            int totalFile = m_FST.FST_File.Count;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                EnableControls(false);

                long fileCheckedCount=0;

                btnCancelExtractFile.Visible = true;
                m_Cancel = false;

                pbExtractTotal.Value = 0;
                pbExtractTotal.Maximum = totalSession * totalFolder * totalFile;

                for (int i = 0; i < m_FST.FST_File.Count; i++)
                {
                    if ((m_FST.FST_File[i].IsChecked) || (extractAll))
                        fileCheckedCount++;
                }

                for (int i = 0; i < totalSession; i++)
                {
                    string sPath = folderBrowserDialog.SelectedPath;

                    if (pbExtractTotal.Value < pbExtractTotal.Maximum)
                        pbExtractTotal.Value++;
                    Application.DoEvents();

                    if (m_FST.FST_Session[i].SessionName != string.Empty)
                    {
                       sPath = sPath + "\\" + m_FST.FST_Session[i].SessionName.Replace(":","_");
                        if (!Directory.Exists(sPath))
                            Directory.CreateDirectory(sPath);
                    }

                    for (int j = 0; j < totalFolder; j++)
                    {
                        if (pbExtractTotal.Value < pbExtractTotal.Maximum)
                            pbExtractTotal.Value++;
                        Application.DoEvents();

                        string folderPath = sPath;

                        if (m_FST.FST_Folder[j].SessionID == m_FST.FST_Session[i].SessionID)
                        {
                            for (int k = 0; k < totalFile; k++)
                            {
                                if ((m_Cancel) || (fileCheckedCount==0))
                                    break;

                                if (pbExtractTotal.Value < pbExtractTotal.Maximum)
                                    pbExtractTotal.Value++;
                                Application.DoEvents();

                                if ((m_FST.FST_File[k].SessionID == m_FST.FST_Session[i].SessionID) &&
                                    (m_FST.FST_File[k].ParentDirectory == m_FST.FST_Folder[j].FolderID) && 
                                    (m_FST.FST_File[k].IsChecked || extractAll) && (m_FST.FST_File[k].IsVisible))
                                {

                                    m_fileReader = new IsoStreamReader();
                                    if (!m_fileReader.Open(m_FST.FST_File[k].FileOwner))
                                    {
                                        m_fileReader.Close();
                                        m_fileReader = new BinaryStreamReader();
                                        m_fileReader.Open(m_FST.FST_File[k].FileOwner);
                                    }

                                    m_fileReader.SetSessionID(m_FST.FST_File[k].SessionID);

                                    int parentidFolder = j;
                                    int SessionID = m_FST.FST_File[k].SessionID;
                                    int l = 0;
                                    string addPath = string.Empty;

                                    if(m_FST.FST_Folder[parentidFolder].FolderName!="\\")
                                    {
                                        do
                                        {
                                            addPath = m_FST.FST_Folder[parentidFolder].FolderName + "\\" + addPath;
                                            for (l = 0; l < m_FST.FST_Folder.Count; l++)
                                            {
                                                if ((m_FST.FST_Folder[l].FolderID == m_FST.FST_Folder[parentidFolder].ParentID) && 
                                                    (m_FST.FST_Folder[l].SessionID == SessionID))
                                                    break;
                                            }

                                            parentidFolder = l;
                                        } while (m_FST.FST_Folder[parentidFolder].FolderName != "\\");
                                    }

                                    string filePath = folderPath + "\\" + addPath + m_FST.FST_File[k].Filename;

                                    UInt64 totalLength = m_FST.FST_File[k].FileSize;
                                    uint lengthToRead = 0;
                                    UInt64 offset = m_FST.FST_File[k].FileStartOffset;

                                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                                    if (File.Exists(filePath))
                                        File.Delete(filePath);

                                    FileStream fsOut = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
                                    BinaryWriter bsOut = new BinaryWriter(fsOut);

                                    pbExtractCurrent.Value = 0;
                                    pbExtractCurrent.Maximum = (int) (totalLength/1024);
                                    textBox1.Text = "Extracting " + filePath;

                                    do
                                    {
                                        if (m_Cancel)
                                            break;
                                        
                                        if (totalLength > 0x8000)
                                            lengthToRead = 0x8000;
                                        else
                                            lengthToRead = (uint)totalLength;

                                        buffer = m_fileReader.Read(offset, lengthToRead);
                                        bsOut.Write(buffer, 0, (int)lengthToRead);

                                        pbExtractCurrent.Value += (int)(lengthToRead/1024);
                                        Application.DoEvents();

                                        totalLength -= lengthToRead;
                                        offset += lengthToRead;

                                    } while (totalLength > 0);

                                    bsOut.Close();
                                    fsOut.Close();

                                    fileCheckedCount--;

                                    m_fileReader.Close();
                                }
                            }
                        }
                    }
                }
                pbExtractCurrent.Value = 0;
                pbExtractTotal.Value = 0;

                EnableControls(true);

                btnCancelExtractFile.Visible = false;

                if(m_Cancel)
                    textBox1.Text = "Extraction aborted ! ";
                else
                    textBox1.Text = "Extraction completed ! ";
            }
        }

        private void EnableControls(bool isEnable)
        {
            btnExtractAPCM.Enabled = isEnable;
            btnExtractAll.Enabled = isEnable;
            btnExtractSelected.Enabled = isEnable;
            tsbSearchDeep.Enabled = isEnable;
            tsbOpen.Enabled = isEnable;
        }

        private void UpdateProgress(UInt64 value, UInt64 max)
        {
            pbExtract.Maximum = Convert.ToInt32(max / 1024);
            if ((Convert.ToInt32(value / 1024) < Convert.ToInt32(max / 1024)) && (value >= 0))
                pbExtract.Value = Convert.ToInt32(value / 1024);

            textBox1.Text = m_textBoxText + " (" + pbExtract.Value.ToString() + "Kb / " + pbExtract.Maximum + "Kb )";
            Application.DoEvents();
        }

        public void FillTreeview()
        {
            lvResults.BeginUpdate();
            lvResults.Items.Clear();

            try
            {
                for(int i=0; i<Mediafile.MediaList.Count;i++)
                {
                    ListViewItem lvLine = lvResults.Items.Add(i.ToString("000"));
                    lvLine.Checked = true;

                    ListViewItem.ListViewSubItem lvName = new ListViewItem.ListViewSubItem();
                    ListViewItem.ListViewSubItem lvDescription = new ListViewItem.ListViewSubItem();
                    ListViewItem.ListViewSubItem lvFormat = new ListViewItem.ListViewSubItem();
                    ListViewItem.ListViewSubItem lvOffset = new ListViewItem.ListViewSubItem();
                    ListViewItem.ListViewSubItem lvLength = new ListViewItem.ListViewSubItem();
                    ListViewItem.ListViewSubItem lvPlaytime = new ListViewItem.ListViewSubItem();

                    lvName.Text = Mediafile.MediaList[i].filename.ToString();
                    lvLine.SubItems.Add(lvName);

                    lvDescription.Text = Mediafile.MediaList[i].Description.ToString();
                    lvLine.SubItems.Add(lvDescription);

                    lvFormat.Text = Mediafile.MediaList[i].Extension.ToString();
                    lvLine.SubItems.Add(lvFormat);

                    lvOffset.Text = Mediafile.MediaList[i].Offset.ToString("X");
                    lvLine.SubItems.Add(lvOffset);

                    lvLength.Text = Mediafile.MediaList[i].Size.ToString("X");
                    lvLine.SubItems.Add(lvLength);

                    if (Mediafile.MediaList[i].Channels == 0)
                        lvPlaytime.Text = "Unplayable ...";
                    else
                        lvPlaytime.Text = GetTimeFromSamples(Mediafile.MediaList[i].SampleCount*Mediafile.MediaList[i].Channels, Mediafile.MediaList[i].Frequency, Mediafile.MediaList[i].Channels);
                    
                    lvLine.SubItems.Add(lvPlaytime);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            lvResults.EndUpdate();
            Application.DoEvents();
        }

        private string GetTimeFromSamples(int samples, int sampleRate, int channels)
        {
            UInt32 hours = 0;
            UInt32 minutes = 0;
            UInt32 seconds = 0;

            if (channels == 0) return "00:00:00";

            minutes = (UInt32)(samples / (UInt32)sampleRate / channels) / 60;

            if (minutes >= 60)
            {
                hours = minutes / 60;
                minutes = minutes % 60;
            }

            seconds = (UInt32)(samples / (UInt32)sampleRate / channels) % 60;

            if ((seconds == 0) && (minutes == 0) && (hours == 0))
            {
                return "00:00";
            }
            else
            {
                if (hours != 0)
                    return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
                else
                    return minutes.ToString("00") + ":" + seconds.ToString("00");
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (lvResults.Items.Count == 0)
                return;

            if (lvResults.SelectedItems.Count == 0)
                lvResults.Items[0].Selected = true;

            m_currMediaFile = Convert.ToInt16(lvResults.SelectedItems[0].Text);
            lvResults.SelectedItems[0].Selected = true;
            lvResults.Items[lvResults.SelectedItems[0].Index].EnsureVisible();
            if (Mediafile.MediaList[m_currMediaFile].isChecked)
            {
                if(Mediafile.MediaList[m_currMediaFile].Format.IsPlayable)
                {
                    if (m_dSoundOut != null)
                    {
                        if (m_dSoundOut.PlaybackState != PlaybackState.Stopped)
                            StopStream();
                    }
                    InitPlayer();
                }
            }
        }

        private void InitPlayer()
        {
            if (lvResults.Items.Count == 0)
                return;

            StopStream();

            m_fileReader = new IsoStreamReader();
            if (!m_fileReader.Open(Mediafile.MediaList[m_currMediaFile].FileOwner))
            {
                m_fileReader.Close();
                m_fileReader = new BinaryStreamReader();
                if (!m_fileReader.Open(Mediafile.MediaList[m_currMediaFile].FileOwner))
                {
                    textBox1.Text = "Can't open " + Mediafile.MediaList[m_currMediaFile].FileOwner;
                    return;
                }
            }

            m_fileReader.SetSessionID(Mediafile.MediaList[m_currMediaFile].SessionID);

            if (Mediafile.MediaList[m_currMediaFile].Format.IsFormat(m_fileReader, Mediafile.MediaList[m_currMediaFile].Offset, Mediafile.MediaList[m_currMediaFile].Size) != 0)
            {
                m_vgmStream = new VGM_Stream();
                
                Mediafile.MediaList[m_currMediaFile].Format.Init(m_fileReader, Mediafile.MediaList[m_currMediaFile].Offset, ref m_vgmStream,true, Mediafile.MediaList[m_currMediaFile].Size);

                // don't delete this !
                // is needed to free the dsound object !
                System.Threading.Thread.Sleep(150);
                Application.DoEvents();

                if(m_dSoundOut==null)
                    m_dSoundOut = new DirectSoundOut(120);

                IWaveProvider vgm = new VGM_Decoding(m_vgmStream, m_fileReader);

                m_dSoundOut.Init(vgm);
                m_dSoundOut.Play();
                timer1.Enabled = true;
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            StopStream();
            previousListviewItem();
            InitPlayer();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (m_dSoundOut != null)
            {
                if (m_dSoundOut.PlaybackState == PlaybackState.Playing)
                {
                    textBox1.Text = "Paused ...";
                    m_dSoundOut.Pause();
                }
                else
                    m_dSoundOut.Play();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopStream();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            StopStream();
            nextListviewItem();
            InitPlayer();
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            int currMediaFile = 0;
            UInt64 currMediaLength = 0;

            uint lengthToRead = 0;
            UInt64 offset, save_offset;

            if (lvResults.Items.Count == 0)
                return;

            EnableControls(false);

            m_Cancel = false;

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                btnCancelExtractAPDCM.Visible = true;

                StopStream();

                byte[] tmpBuffer = new byte[0x8000];
                pbExtract.Maximum = Mediafile.MediaList.Count;
                pbExtract.Value = 0;

                for (currMediaFile = 0; currMediaFile < Mediafile.MediaList.Count; currMediaFile++)
                {
                    if (!Mediafile.MediaList[currMediaFile].isChecked)
                        continue;

                    if (m_Cancel)
                        break;

                    m_fileReader = new IsoStreamReader();
                    if (!m_fileReader.Open(Mediafile.MediaList[currMediaFile].FileOwner))
                    {
                        m_fileReader.Close();
                        m_fileReader = new BinaryStreamReader();
                        if (!m_fileReader.Open(Mediafile.MediaList[currMediaFile].FileOwner))
                        {
                            textBox1.Text = "Can't open " + Mediafile.MediaList[currMediaFile].FileOwner;
                            return;
                        }
                    }

                    m_fileReader.SetSessionID(Mediafile.MediaList[currMediaFile].SessionID);

                    pbExtract.Value++;
                    Application.DoEvents();

                    string sPath = folderBrowserDialog.SelectedPath;

                    if(System.IO.Path.GetExtension(Mediafile.MediaList[currMediaFile].filename).ToUpper()=="." + Mediafile.MediaList[currMediaFile].Extension.ToUpper())
                        sPath = sPath + "\\" + Mediafile.MediaList[currMediaFile].filename;//currMediaFile.ToString("000")+"_" + Mediafile.MediaList[currMediaFile].filename;
                    else
                        sPath = sPath + "\\" + Mediafile.MediaList[currMediaFile].filename + "." + Mediafile.MediaList[currMediaFile].Extension.ToLower(); //currMediaFile.ToString("000") + "_" + Mediafile.MediaList[currMediaFile].filename + "." + Mediafile.MediaList[currMediaFile].Extension.ToLower();

                    if (!(System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(sPath))))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(sPath));

                    if (System.IO.File.Exists(sPath))
                        System.IO.File.Delete(sPath);

                    FileStream fsOut = new FileStream(sPath,FileMode.CreateNew, FileAccess.Write);
                    BinaryWriter bsOut = new BinaryWriter(fsOut);

                    currMediaLength = Mediafile.MediaList[currMediaFile].Size;

                    offset = Mediafile.MediaList[currMediaFile].Offset;
                    save_offset = offset;

                    pbRipStatus.Value = 0;
                    pbRipStatus.Maximum = (int)(currMediaLength/1024/1024);

                    if (Mediafile.MediaList[currMediaFile].Format.Extension == "THP")
                        THP_Rip_Audio(m_fileReader, bsOut, offset);

                    else
                    {
                        do
                        {
                            if (m_Cancel)
                                break;

                            if (currMediaLength > 0x8000)
                                lengthToRead = 0x8000;
                            else
                                lengthToRead = (uint)currMediaLength;

                            pbRipStatus.Value += (int)lengthToRead/1024/1024;
                            Application.DoEvents();

                            tmpBuffer = m_fileReader.Read(offset, lengthToRead);
                            bsOut.Write(tmpBuffer, 0, (int)lengthToRead);

                            offset += lengthToRead;
                            currMediaLength -= lengthToRead;
                        } while (currMediaLength > 0);


                    }
                    bsOut.Close();
                    fsOut.Close();

                    if (Mediafile.MediaList[currMediaFile].Format.Extension == "PMF")
                        ExtractFromPAM(m_fileReader, sPath, save_offset, Mediafile.MediaList[currMediaFile].Size, "PMF");

                    if (Mediafile.MediaList[currMediaFile].Format.Extension == "PAM")
                        ExtractFromPAM(m_fileReader, sPath, save_offset, Mediafile.MediaList[currMediaFile].Size, "PAM");

                }
            }

            btnCancelExtractAPDCM.Visible = false;
            
            EnableControls(true);

            if(m_Cancel)
                textBox1.Text = "Extraction Aborted !";
            else
                textBox1.Text = "Extraction Complete !";

            pbRipStatus.Value = 0;
            pbExtract.Value = 0;
            Application.DoEvents();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (m_vgmStream != null)
            {
                if (m_dSoundOut != null)
                {
                    if (m_dSoundOut.PlaybackState == PlaybackState.Playing)
                    {
                        PaintVisu();
                    }
                }
            }
        }

        private void StopStream()
        {
            textBox1.Text = "";
            lblPlayTimeCurrent.Text = "00:00";
            pbPlay.Value = 0;

            if (m_dSoundOut != null)
            {
                m_dSoundOut.Pause();
                m_dSoundOut.Stop();
                m_vgmStream = null;
            }

            timer1.Enabled = false;

            if(m_fileReader!=null)
                m_fileReader.Close();

            Application.DoEvents();
        }

        private void PaintVisu()
        {

            if (m_dSoundOut != null)
            {
                pbPlay.Maximum = (m_vgmStream.vgmTotalSamplesWithLoop / m_vgmStream.vgmSampleRate) * m_vgmStream.vgmChannelCount;

                if ((int)((m_vgmStream.vgmDecodedSamples * m_vgmStream.vgmChannelCount) / m_vgmStream.vgmSampleRate) <= pbPlay.Maximum)
                    pbPlay.Value = (int)((m_vgmStream.vgmDecodedSamples * m_vgmStream.vgmChannelCount) / m_vgmStream.vgmSampleRate);
                lblPlayTimeCurrent.Text = GetTimeFromSamples(m_vgmStream.vgmDecodedSamples * m_vgmStream.vgmChannelCount, m_vgmStream.vgmSampleRate, m_vgmStream.vgmChannelCount) + " / " + GetTimeFromSamples(m_vgmStream.vgmTotalSamplesWithLoop * m_vgmStream.vgmChannelCount, m_vgmStream.vgmSampleRate, m_vgmStream.vgmChannelCount);
                textBox1.Text = "Playing " + Mediafile.MediaList[m_currMediaFile].filename + " (Sample : " + m_vgmStream.vgmDecodedSamples.ToString() + " / " + m_vgmStream.vgmTotalSamplesWithLoop + ")";
                if (m_vgmStream.vgmDecodedSamples >= m_vgmStream.vgmTotalSamplesWithLoop)
                {
                    pbPlay.Value = pbPlay.Maximum;
                    StopStream();

                    // Play the next song ...
                    nextListviewItem();
                    InitPlayer();
                }
            }
        }

        private void nextListviewItem()
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            int index = lvResults.SelectedItems[0].Index;

            index++;

            if (index >= lvResults.Items.Count)
                index = 0;

            // lvResults.Items[index].Selected = true;
            lvResults.Items[index].EnsureVisible();
            m_currMediaFile = Convert.ToInt16(lvResults.Items[index].Text);
        }

        private void previousListviewItem()
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            int index = lvResults.SelectedItems[0].Index;

            index--;

            if (index < 0)
                index = lvResults.Items.Count-1;

            lvResults.Items[index].Selected = true;
            lvResults.Items[index].EnsureVisible();
            m_currMediaFile = Convert.ToInt16(lvResults.Items[index].Text);
        }

        private void lvResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            int index = Convert.ToInt16(lvResults.Items[lvResults.SelectedItems[0].Index].Text);

            AudioFile audioFile = new AudioFile();

            audioFile.Offset = Mediafile.MediaList[index].Offset;
            audioFile.Size = Mediafile.MediaList[index].Size;
            audioFile.SampleCount = Mediafile.MediaList[index].SampleCount.ToString() + " (" + GetTimeFromSamples(Mediafile.MediaList[index].SampleCount*Mediafile.MediaList[index].Channels, Mediafile.MediaList[index].Frequency, Mediafile.MediaList[index].Channels) +")";

            audioFile.SampleRate = (uint)Mediafile.MediaList[index].Frequency;
            audioFile.Channels = (uint)Mediafile.MediaList[index].Channels;
            audioFile.Decoder = Mediafile.MediaList[index].decoder;
            audioFile.Layout = Mediafile.MediaList[index].layout;
            audioFile.Interleave = "0x" +  Mediafile.MediaList[index].Interleave.ToString("x");
            
            audioFile.Looped = (Mediafile.MediaList[index].isLooped == "Yes");
            audioFile.LoopStart = Mediafile.MediaList[index].LoopStartSample;
            audioFile.LoopTimeStart = GetTimeFromSamples(Mediafile.MediaList[index].LoopStartSample * Mediafile.MediaList[index].Channels, Mediafile.MediaList[index].Frequency, Mediafile.MediaList[index].Channels);
            audioFile.LoopEnd = Mediafile.MediaList[index].LoopEndSample * Mediafile.MediaList[index].Channels;
            audioFile.LoopTimeEnd = GetTimeFromSamples(Mediafile.MediaList[index].LoopEndSample * Mediafile.MediaList[index].Channels, Mediafile.MediaList[index].Frequency, Mediafile.MediaList[index].Channels);

            if (System.IO.Path.GetExtension(Mediafile.MediaList[index].filename).ToUpper() == "." + Mediafile.MediaList[index].Extension.ToUpper())
                audioFile.Filename = System.IO.Path.GetFileNameWithoutExtension(Mediafile.MediaList[index].filename);
            else
                audioFile.Filename = System.IO.Path.GetFileNameWithoutExtension(Mediafile.MediaList[index].filename) + "." + Mediafile.MediaList[index].Extension.ToLower();

            audioFile.Description = Mediafile.MediaList[index].Description;
            audioFile.Owner = System.IO.Path.GetFileName(Mediafile.MediaList[index].FileOwner);

            pgAudioFile.SelectedObject = audioFile;
        }

        private void btnADPCMInvert_Click(object sender, EventArgs e)
        {
            if (lvResults.Items.Count != 0)
            {
                foreach (ListViewItem lvi in lvResults.Items)
                {
                    lvi.Checked = !lvi.Checked;
                }
            }
        }

        private void lvResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(sender, e, lvResults);
        }

        private void lvFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortListView(sender, e, lvFiles);
        }

        private void SortListView(object sender, ColumnClickEventArgs e, ListView lv)
        {
            if (lvwColumnSorter == null)
                lvwColumnSorter = new CListViewSorter();
            lvwColumnSorter.SortColumn = e.Column;
            lvResults.ListViewItemSorter = lvwColumnSorter;

            if (e.Column == lvwColumnSorter.SortColumn)
            {
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                    lvwColumnSorter.Order = SortOrder.Descending;
                else
                    lvwColumnSorter.Order = SortOrder.Ascending;
            }
            else
            {
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            lvResults.Sort();
        }

        private void lvResults_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Mediafile.FileEntry f = Mediafile.MediaList[Convert.ToInt32(e.Item.SubItems[0].Text)];
            f.isChecked = e.Item.Checked;
            Mediafile.MediaList[Convert.ToInt32(e.Item.SubItems[0].Text)] = f;
        }

        private void cmdExtractSelected_Click(object sender, EventArgs e)
        {
            DoExtraction(false);
        }

        private void lvFiles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            FST.cFile f = m_FST.FST_File[Convert.ToInt32(e.Item.Tag)];
            f.IsChecked = e.Item.Checked;
            m_FST.FST_File[Convert.ToInt32(e.Item.Tag)] = f;
        }

        private void btnInvertSelection_Click(object sender, EventArgs e)
        {
            if (lvFiles.Items.Count != 0)
            {
                foreach (ListViewItem lvi in lvFiles.Items)
                {
                    lvi.Checked = !lvi.Checked;
                }
            }
        }

        private void btnStopExtractFile_Click(object sender, EventArgs e)
        {
            m_Cancel = true;
        }

        private void btnCancelExtractAPDCM_Click(object sender, EventArgs e)
        {
            m_Cancel = true;
        }

        private void btnCancelSearch_Click(object sender, EventArgs e)
        {
            ripper.Cancel = true;
        }

        private void tsbSearchDeep_Click(object sender, EventArgs e)
        {
            StopStream();
            m_Cancel = false;

            if (m_FST != null)
            {
                SearchForADPCM(true);
            }
        }

        private void SearchForADPCM(bool deepSearch)
        {
            btnCancelSearch.Visible = true;

            EnableControls(false);

            ripper = new VGM_Ripper();
            Mediafile.MediaList.Clear();

            pbRipStatus.Maximum = m_FST.FST_File.Count;
            pbRipStatus.Value = 0;

            UInt64 totalSize = 0;
            bool forceBinary = false;

            for (int i = 0; i < m_FST.FST_File.Count; i++)
                totalSize += m_FST.FST_File[i].FileSize;

            for(int i = 0; i<m_FST.FST_File.Count; i++)
            {
                if (m_Cancel)
                    break;


                if (forceBinary)
                {
                    m_fileReader.Close();
                    m_fileReader = new BinaryStreamReader();
                    m_fileReader.Open(m_FST.FST_File[0].FileOwner);
                    m_textBoxText = "Searching ADPCM in ... " + m_FST.FST_File[0].FilePath;
                    textBox1.Text = m_textBoxText;
                    ripper.DoRip(m_fileReader,
                                 0,
                                 m_fileReader.GetLength(),
                                 m_FST.FST_File[0].FilePath,
                                 m_FST.FST_File[0].FilePath,
                                 m_FST.FST_File[0].SessionID,
                                 UpdateProgress,
                                 true);
                    m_Cancel = ripper.Cancel;
                    m_fileReader.Close();
                    goto stop_rip;
                }
                else
                {
                    m_fileReader = new IsoStreamReader();
                    if (!m_fileReader.Open(m_FST.FST_File[i].FileOwner))
                    {
                        m_fileReader.Close();
                        m_fileReader = new BinaryStreamReader();
                        m_fileReader.Open(m_FST.FST_File[i].FileOwner);
                    }
                    else
                    {
                        //if (!m_FST.FST_File[0].FileOwner.Contains("\\\\"))
                        //{
                        //    if (totalSize < ((m_fileReader.GetLength() / 100) * 10))
                        //    {
                        //        if (!forceBinary)
                        //        {
                        //            forceBinary = false;
                        //            if (MessageBox.Show("This iso seems to contains hidden file, rip it as a plain binary file ?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //                forceBinary = true;
                        //            goto open_a_file;
                        //        }
                        //    }
                        //}
                    }
                }
                pbRipStatus.Value++;

                if ((m_FST.FST_File[i].IsVisible) || (deepSearch))
                {
                    m_fileReader.SetSessionID(m_FST.FST_File[i].SessionID);
                    m_textBoxText = "Searching ADPCM in ... " + m_FST.FST_File[i].Filename;
                    textBox1.Text = m_textBoxText;
                    ripper.DoRip(m_fileReader, 
                                 m_FST.FST_File[i].FileStartOffset,
                                 m_FST.FST_File[i].FileSize,
                                 m_FST.FST_File[i].FileOwner,
                                 m_FST.FST_File[i].Filename,
                                 m_FST.FST_File[i].SessionID, 
                                 UpdateProgress, 
                                 deepSearch);
                    m_Cancel = ripper.Cancel;
                }

                m_fileReader.Close();
            }
stop_rip:
            if (m_Cancel)
                textBox1.Text = "Search aborted !";
            else
                textBox1.Text = "Search completed !";

            EnableControls(true);

            pbExtract.Value = 0;
            pbRipStatus.Value = 0;
            btnCancelSearch.Visible = false;

            FillTreeview();
        }

        private void THP_Rip_Audio(IReader fReader, BinaryWriter bsOut, UInt64 offset)
        {
            UInt64 i;
            uint channel_count=1;
            bool bSaveHeader=true;
            uint offsetValue;
            int nextAudioLength=0;

            byte[] thpHeader = fReader.Read(offset, 0x60);

            UInt64 numFrames = fReader.Read_32bitsBE(offset + 0x14);

            int thpVersion = fReader.Read_8Bits(offset + 0x06);

            // Get info from the first block
            UInt64 componentTypeOffset = offset + fReader.Read_32bitsBE(offset + 0x20);
            uint numComponents = fReader.Read_32bitsBE(componentTypeOffset);
            UInt64 componentDataOffset = componentTypeOffset + 0x14;
            componentTypeOffset += 4;

            for (i = 0; i < numComponents; i++)
            {
                if (fReader.Read_8Bits(componentTypeOffset + i) == 1) // audio block
                {
                    channel_count = fReader.Read_32bitsBE(componentDataOffset);
                    break;
                }
                else
                {
                    if (thpVersion == 0x10)
                        componentDataOffset += 0x0c;
                    else
                        componentDataOffset += 0x08;
                }
            }

            // allocate frame data with the max size data found at 0x08
            byte[] currFrameData = new byte[fReader.Read_32bitsBE(offset + 0x08)];
            byte[] nextFrameData = new byte[fReader.Read_32bitsBE(offset + 0x08)];

            UInt64 nextFrameOffset = offset + fReader.Read_32bitsBE(offset + 0x28);
            uint nextFrameSize = fReader.Read_32bitsBE(offset + 0x18);
            uint currFrameSize = 0;
            UInt64 currFrameOffset = nextFrameOffset;

            pbRipStatus.Value = 0;
            pbRipStatus.Maximum = (int)numFrames;

            for(i=0; i<numFrames;i++)
            {
                pbRipStatus.Value ++;
                Application.DoEvents();

                if (m_Cancel)
                    break;
                
                currFrameData = fReader.Read(nextFrameOffset, nextFrameSize);

                uint currAudioOffset = MemoryReader.ReadLongBE(ref currFrameData, 0x08) + 0x10;
                int currAudioLength = (int)MemoryReader.ReadLongBE(ref currFrameData, currAudioOffset);
                currAudioLength *= (int)channel_count;
                currAudioLength += 0x60;

                currFrameOffset += nextFrameSize;
                currFrameSize = MemoryReader.ReadLongBE(ref currFrameData, 0);

                if (i != (numFrames - 1))
                {
                    nextFrameData = fReader.Read(currFrameOffset, currFrameSize);
                    uint nextAudioOffset = MemoryReader.ReadLongBE(ref nextFrameData, 0x08) + 0x10;
                    nextAudioLength = (int)MemoryReader.ReadLongBE(ref nextFrameData, nextAudioOffset);
                    nextAudioLength *= 2;
                    nextAudioLength += 0x60;
                }
                else
                {
                    nextAudioLength = 0;
                }

                if(bSaveHeader)
		        {
			        // Save the THPHeader
			        bSaveHeader=false;
                    MemoryReader.PutLong(ref thpHeader, 0x18, (uint)currAudioLength);
                    MemoryReader.PutLong(ref thpHeader, 0x28, 0x60);
                    bsOut.Write(thpHeader,0,0x60);
		        }

                // Write Offsets ...
		        offsetValue=MemoryReader.SwapUnsignedLong((uint)nextAudioLength);
                bsOut.Write(offsetValue);
                bsOut.Write(offsetValue);
                offsetValue = 0;
                bsOut.Write(offsetValue);
                bsOut.Write(offsetValue);

		        // Write Data
                bsOut.Write(currFrameData, (int)currAudioOffset, (int)(currAudioLength - 0x10));
		
		        nextFrameOffset=currFrameOffset;
		        nextFrameSize=currFrameSize;
            } 
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tsbOpen.Enabled)
                FileOpen();
        }

       
        private void rbSystem_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Games.MK_ASSERT mk = new Games.MK_ASSERT();

            mk.DoExtraction("D:\\-= RIP Folder =-\\sndsps2\\MSLASSET.MS4", "c:\\temp");
        }

        public void ExtractFromPAM(IReader fReader,string sPath, UInt64 offset, UInt64 length, string type_of_rip)
        {
            UInt64 lPosition = 0x800;

            if ((type_of_rip=="SFD") || (type_of_rip=="PSS"))
                lPosition = 0;

            int iDataLength;

            UInt32 mpegID;
            FileStream fSource;
            FileStream[] fDest = new FileStream[0x200];
            BinaryWriter[] bDest = new BinaryWriter[0x200];
            UInt16 iCurrentStream = 0;
            UInt16 iStreamCount = 0;
            UInt32 omaID;
            bool forceAT3=false;

            int buffLength = 0;
            int sampleCount = 0;

            bool bUpdateStreamCount = true;
            byte[] buffer;

            byte[] riffHeader = { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00 };
            byte[] wavefmtHeader = { 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20 };
            byte[] at3plusHeader = { 0x34, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x02, 0x00, 0x44, 0xAC, 0x00, 0x00, 0x95, 0x3E, 0x00, 0x00, 0xE8, 0x02, 0x00, 0x00, 0x22, 0x00, 0x00, 0x08, 0x03, 0x00, 0x00, 0x00, 0xBF, 0xAA, 0x23, 0xE9, 0x58, 0xCB, 0x71, 0x44, 0xA1, 0x19, 0xFF, 0xFA, 0x01, 0xE4, 0xCE, 0x62, 0x01, 0x00, 0x28, 0x5C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] factHeader = { 0x66, 0x61, 0x63, 0x74, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00 };
            byte[] dataHeader = { 0x64, 0x61, 0x74, 0x61, 0x00, 0x00, 0x00, 0x00 };

            byte[] emptyBuffer = new byte[0x3BB];

            for (int i = 0; i < 0x3BB; i++)
                emptyBuffer[i] = 0;

            for (iCurrentStream = 0; iCurrentStream < 20; iCurrentStream++)
            {
                if (File.Exists(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp"))
                    File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp");
                if (File.Exists(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".aa3"))
                    File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".aa3");
                if (File.Exists(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".at3"))
                    File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".at3");
                if (File.Exists(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".raw"))
                    File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".raw");
                if (File.Exists(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".adx"))
                    File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".adx");
            }
            fSource = new FileStream(sPath, FileMode.Open, FileAccess.Read);

            iCurrentStream = 0;

            pbRipStatus.Maximum = (int)(length / 1024 / 1024);
            pbRipStatus.Value = 0;
            UInt32 vidID = fReader.Read_32bits(offset);

            if (vidID == 0x464d4150)
                if (fReader.Read_32bits(offset + 4) == 0x34313030)
                    forceAT3 = true;

            if (((vidID == 0x464D4150) || (vidID == 0x464D5350)) || (type_of_rip=="SFD") || (type_of_rip=="PSS"))
            {
                do
                {
                    pbRipStatus.Value = (int)(lPosition / 1024 / 1024);
                    Application.DoEvents();

                    mpegID = fReader.Read_32bits(offset + lPosition);
                    lPosition += 4;
                    switch (mpegID)
                    {
                        case 0xBA010000:
                            if (type_of_rip=="SFD")
                                lPosition += 0x0C-4;
                            else
                                lPosition += 0x0e-4;
                            break;
                        case 0xBB010000:
                        case 0xBE010000:
                        case 0xBF010000:
                        case 0xE0010000:
                            lPosition += (UInt64)(fReader.Read_16bitsBE(offset + lPosition) + 2);
                            break;
                        case 0xB9010000:
                            goto CloseThis;
                        case 0xBD010000:
                            sampleCount += 1;

                            buffLength = (int)SwapUnsignedInt16(fReader.Read_16bits(offset + lPosition));
                            UInt16 bLength;
                            lPosition += 2;
                            bLength = fReader.Read_16bits(offset + lPosition);
                            lPosition += 2;

                            switch (bLength)
                            {
                                case 0x8181:
                                    iDataLength = 0x0F;
                                    if (bUpdateStreamCount)
                                    {
                                        lPosition += 9;
                                        iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                        lPosition -= 9;
                                        if (fDest[iCurrentStream] == null)
                                        {
                                            fDest[iCurrentStream] = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp", FileMode.CreateNew, FileAccess.Write);
                                            bDest[iCurrentStream] = new BinaryWriter(fDest[iCurrentStream]);
                                            iStreamCount++;
                                        }
                                    }
                                    else
                                    {
                                        if (iStreamCount != 1)
                                        {
                                            iCurrentStream++;
                                            if (iCurrentStream > iStreamCount)
                                                iCurrentStream = 1;
                                        }
                                    }
                                    lPosition -= 8;
                                    break;
                                case 0x0081:
                                    iDataLength = 0x07 + fReader.Read_8Bits(offset + lPosition);
                                    lPosition++;
                                    if (fReader.Read_8Bits(offset + lPosition) != 0xff)
                                    {
                                        lPosition--;
                                        iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                    }
                                    else
                                    {
                                        switch (iDataLength)
                                        {
                                            case 0x08:
                                                iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                                break;
                                            case 0x09:
                                                lPosition += 1;
                                                iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                                lPosition -= 1;
                                                break;
                                            case 0x0A:
                                                lPosition += 2;
                                                iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                                lPosition -= 2;
                                                break;
                                            case 0x0B:
                                                lPosition += 3;
                                                iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                                lPosition -= 3;
                                                break;
                                            case 0x0C:
                                                lPosition += 4;
                                                iCurrentStream = fReader.Read_8Bits(offset + lPosition);
                                                lPosition -= 4;
                                                break;
                                        }
                                    }
                                    break;
                                case 0x8083:
                                case 0x0181:
                                    iDataLength = 0x07 + fReader.Read_8Bits(offset + lPosition);
                                    switch (iDataLength)
                                    {
                                        case 0x0A:
                                            lPosition += 3;
                                            iCurrentStream =  fReader.Read_8Bits(offset + lPosition);
                                            lPosition -= 3;
                                            break;
                                        case 0x0B:
                                        case 0x0C:
                                            lPosition += 5;
                                            iCurrentStream =  fReader.Read_8Bits(offset + lPosition);
                                            lPosition -= 5;
                                            break;
                                    }
                                    break;
                                //iDataLength = 0x0A;
                                //break;
                                case 0x8081:
                                    lPosition += 6;
                                    iCurrentStream =  fReader.Read_8Bits(offset + lPosition);
                                    lPosition -= 6;
                                    iDataLength = 0x07 + fReader.Read_8Bits(offset + lPosition);
                                    lPosition -= 8;
                                    if (iDataLength != 0x0c)
                                        break;
                                    break;
                                default:
                                    iDataLength = 0x0C;
                                    break;
                            }

                            buffer = new byte[buffLength - iDataLength];

                            if (iDataLength == 7)
                                lPosition += 5;
                            else
                                lPosition += (UInt64)(iDataLength + 6);

                            buffer = fReader.Read(offset + lPosition, (uint)buffer.Length);

                            if (bDest[iCurrentStream] == null)
                            {
                                if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp"))
                                    System.IO.File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp");

                                fDest[iCurrentStream] = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp", FileMode.CreateNew, FileAccess.Write);
                                bDest[iCurrentStream] = new BinaryWriter(fDest[iCurrentStream]);
                            }
                            bDest[iCurrentStream].Write(buffer, 0, buffer.Length);

                            lPosition += (UInt64)(buffLength - iDataLength);
                            break;
                        case 0xC0010000:
                        case 0xC1010000:
                            buffLength = (int)SwapUnsignedInt16(fReader.Read_16bits(offset + lPosition));
                            iDataLength = 7;

                            if (bUpdateStreamCount)
                            {
                                iCurrentStream++;
                                fDest[iCurrentStream] = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".adx", FileMode.CreateNew, FileAccess.Write);
                                bDest[iCurrentStream] = new BinaryWriter(fDest[iCurrentStream]);
                                bUpdateStreamCount = false;
                            }

                            buffer = new byte[buffLength - iDataLength];
                            lPosition += (UInt64)(iDataLength + 6);

                            buffer = fReader.Read(offset + lPosition, (uint)buffer.Length);
                            bDest[iCurrentStream].Write(buffer, 0, buffer.Length);
                            lPosition += (UInt64)(buffLength - iDataLength);

                            break;
                        default:
                            lPosition += 0x800-4;
                            lPosition += (lPosition % 0x800);
                            break;
                    }
                } while (lPosition < length);
            }

        CloseThis:

            for (iCurrentStream = 0; iCurrentStream < iStreamCount; iCurrentStream++)
            {
                bDest[iCurrentStream].Close();
                fDest[iCurrentStream].Close();
            }

            fSource.Close();

            // Check if OMA
            for (iCurrentStream = 0; iCurrentStream < iStreamCount; iCurrentStream++)
            {
                omaID = 0;

                fSource = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp", FileMode.Open, FileAccess.Read);
                BinaryReader bSource = new BinaryReader(fSource);

                if (bSource.BaseStream.Length != 0)
                {
                    int sourceID = bSource.ReadInt32();
                    bSource.BaseStream.Position = 0;
                    omaID = bSource.ReadUInt32();

                    if (omaID != 0)
                    {
                        uint blocksize = (omaID >> 16);
                        blocksize = ((blocksize >> 8) | (blocksize << 8)) & 0x3ff;
                        blocksize = blocksize * 8 + 8;

                        if ((System.IO.Path.GetExtension(sPath).ToUpper() == ".PMF") && (!forceAT3))
                        {
                            fDest[iCurrentStream] = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".at3", FileMode.CreateNew, FileAccess.Write);
                            bDest[iCurrentStream] = new BinaryWriter(fDest[iCurrentStream]);

                            bDest[iCurrentStream].Write(riffHeader, 0, riffHeader.Length);
                            bDest[iCurrentStream].Write(wavefmtHeader, 0, wavefmtHeader.Length);
                            bDest[iCurrentStream].Write(at3plusHeader, 0, at3plusHeader.Length);
                            bDest[iCurrentStream].Write(factHeader, 0, factHeader.Length);
                            bDest[iCurrentStream].Write(dataHeader, 0, dataHeader.Length);
                        }
                        else
                        {
                            fDest[iCurrentStream] = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".aa3", FileMode.CreateNew, FileAccess.Write);
                            bDest[iCurrentStream] = new BinaryWriter(fDest[iCurrentStream]);

                            bDest[iCurrentStream].Write(riffHeader, 0, riffHeader.Length);
                            bDest[iCurrentStream].Write(wavefmtHeader, 0, wavefmtHeader.Length);
                            bDest[iCurrentStream].Write(at3plusHeader, 0, at3plusHeader.Length);
                            bDest[iCurrentStream].Write(factHeader, 0, factHeader.Length);
                            bDest[iCurrentStream].Write(dataHeader, 0, dataHeader.Length);
                        }

                        do
                        {
                            if (bSource.ReadUInt32() == omaID)
                            {
                                lPosition = (UInt64)(bSource.BaseStream.Position - 4);
                                break;
                            }
                        } while (bSource.BaseStream.Position < 0x1000);

                        byte[] bufferOMA = new byte[lPosition - 8];
                        bSource.BaseStream.Position = 8;

                        int realSize = 0;

                        do
                        {
                            realSize += bufferOMA.Length;
                            bSource.Read(bufferOMA, 0, bufferOMA.Length);
                            bDest[iCurrentStream].Write(bufferOMA, 0, bufferOMA.Length);
                            if (bSource.BaseStream.Position != bSource.BaseStream.Length)
                                bSource.BaseStream.Position += 8;
                        } while (bSource.BaseStream.Position < bSource.BaseStream.Length);

                        bDest[iCurrentStream].BaseStream.Position = 0x04;
                        bDest[iCurrentStream].Write((int)(realSize + 0x60));
                        bDest[iCurrentStream].BaseStream.Position = 0x20;
                        bDest[iCurrentStream].Write((int)(blocksize));
                        bDest[iCurrentStream].BaseStream.Position = 0x50;
                        bDest[iCurrentStream].Write((int)((realSize / blocksize) * 2048));
                        bDest[iCurrentStream].BaseStream.Position = 0x5C;
                        bDest[iCurrentStream].Write((int)(realSize));
                        fDest[iCurrentStream].Close();
                        bDest[iCurrentStream].Close();
                    }
                    //goto NextStep;
                    // }

                    if ((sourceID == 0) || (omaID == 0))
                    {
                        bSource.BaseStream.Position = 0;

                        fDest[iCurrentStream] = new FileStream(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".raw", FileMode.CreateNew, FileAccess.Write);
                        bDest[iCurrentStream] = new BinaryWriter(fDest[iCurrentStream]);

                        buffer = new byte[0x8000];
                        do
                        {
                            if (type_of_rip=="PSS")
                            {
                                if ((bSource.BaseStream.Length - bSource.BaseStream.Position) > 0x8000)
                                {
                                    bSource.Read(buffer, 0, 0x8000);
                                    bDest[iCurrentStream].Write(buffer, 0, 0x8000);
                                }
                                else
                                {
                                    int lengthToRead = (int)(bSource.BaseStream.Length - bSource.BaseStream.Position);
                                    bSource.Read(buffer, 0, lengthToRead);
                                    bDest[iCurrentStream].Write(buffer, 0, lengthToRead);
                                }
                            }
                            else
                            {
                                if ((bSource.BaseStream.Length - bSource.BaseStream.Position) > 0x8000)
                                {
                                    bSource.Read(buffer, 0, 0x8000);

                                    for (int j = 0; j < (0x8000 - 1); j += 2)
                                    {
                                        byte temp = buffer[j];
                                        buffer[j] = buffer[j + 1];
                                        buffer[j + 1] = temp;
                                    }
                                    bDest[iCurrentStream].Write(buffer, 0, 0x8000);
                                }
                                else
                                {
                                    int lengthToRead = (int)(bSource.BaseStream.Length - bSource.BaseStream.Position);
                                    bSource.Read(buffer, 0, lengthToRead);
                                    for (int j = 0; j < (lengthToRead - 1); j += 2)
                                    {
                                        byte temp = buffer[j];
                                        buffer[j] = buffer[j + 1];
                                        buffer[j + 1] = temp;
                                    }
                                    bDest[iCurrentStream].Write(buffer, 0, lengthToRead);
                                }
                            }
                        } while (bSource.BaseStream.Position < bSource.BaseStream.Length);
                    }

                }
                //NextStep:
                bSource.Close();
                fSource.Close();
                bDest[iCurrentStream].Close();
                fDest[iCurrentStream].Close();
                File.Delete(System.IO.Path.GetDirectoryName(sPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(sPath) + "_Stream" + iCurrentStream.ToString("00") + ".tmp");
            }
        }

        public UInt32 SwapUnsignedInt(UInt32 x)
        {
            return ((x << 24) | ((x & 0xff00) << 8) | ((x & 0xff0000) >> 8) | (x >> 24));
        }

        public UInt16 SwapUnsignedInt16(UInt16 x)
        {
            return (UInt16)((x << 8) | (x >> 8));
        }

        private void pbRipStatus_Click(object sender, EventArgs e)
        {

        }

        public static bool IsApplictionInstalled(string p_name)
        {
            string keyName;

            // search in: CurrentUser
            keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.CurrentUser, keyName, "DisplayName", p_name) == true)
            {
                return true;
            }

            // search in: LocalMachine_32
            keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", p_name) == true)
            {
                return true;
            }

            // search in: LocalMachine_64
            keyName = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            if (ExistsInSubKey(Registry.LocalMachine, keyName, "DisplayName", p_name) == true)
            {
                return true;
            }

            return false;
        }

        private static bool ExistsInSubKey(RegistryKey p_root, string p_subKeyName, string p_attributeName, string p_name)
        {
            RegistryKey subkey;
            string displayName;

            using (RegistryKey key = p_root.OpenSubKey(p_subKeyName))
            {
                if (key != null)
                {
                    foreach (string kn in key.GetSubKeyNames())
                    {
                        using (subkey = key.OpenSubKey(kn))
                        {
                            displayName = subkey.GetValue(p_attributeName) as string;
                            if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
