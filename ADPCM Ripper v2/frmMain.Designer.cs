namespace ADPCM_Ripper_v2
{
    partial class frmMain
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbADPCM_Page = new System.Windows.Forms.TabPage();
            this.btnCancelSearch = new System.Windows.Forms.Button();
            this.btnCancelExtractAPDCM = new System.Windows.Forms.Button();
            this.btnADPCMInvert = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.pbPlay = new System.Windows.Forms.ProgressBar();
            this.lblPlayTimeCurrent = new System.Windows.Forms.Label();
            this.pgAudioFile = new System.Windows.Forms.PropertyGrid();
            this.btnExtractAPCM = new System.Windows.Forms.Button();
            this.pbRipStatus = new System.Windows.Forms.ProgressBar();
            this.pbExtract = new System.Windows.Forms.ProgressBar();
            this.lvResults = new System.Windows.Forms.ListView();
            this.chID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chOffset = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDelta = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbISOExplorer_Page = new System.Windows.Forms.TabPage();
            this.btnCancelExtractFile = new System.Windows.Forms.Button();
            this.pbExtractCurrent = new System.Windows.Forms.ProgressBar();
            this.pbExtractTotal = new System.Windows.Forms.ProgressBar();
            this.btnInvertSelection = new System.Windows.Forms.Button();
            this.btnExtractSelected = new System.Windows.Forms.Button();
            this.btnExtractAll = new System.Windows.Forms.Button();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.clmFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmLBA = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmProgressBar = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tvFolders = new System.Windows.Forms.TreeView();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.rbSystem = new System.Windows.Forms.RadioButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.clmID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmRegion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutADPCMRipperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusISO = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbOpen = new System.Windows.Forms.ToolStripButton();
            this.tsbSearchDeep = new System.Windows.Forms.ToolStripButton();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tbADPCM_Page.SuspendLayout();
            this.tbISOExplorer_Page.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tbADPCM_Page);
            this.tabControl1.Controls.Add(this.tbISOExplorer_Page);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(12, 58);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1035, 595);
            this.tabControl1.TabIndex = 0;
            // 
            // tbADPCM_Page
            // 
            this.tbADPCM_Page.Controls.Add(this.btnCancelSearch);
            this.tbADPCM_Page.Controls.Add(this.btnCancelExtractAPDCM);
            this.tbADPCM_Page.Controls.Add(this.btnADPCMInvert);
            this.tbADPCM_Page.Controls.Add(this.btnPause);
            this.tbADPCM_Page.Controls.Add(this.btnPrevious);
            this.tbADPCM_Page.Controls.Add(this.btnNext);
            this.tbADPCM_Page.Controls.Add(this.btnStop);
            this.tbADPCM_Page.Controls.Add(this.btnPlay);
            this.tbADPCM_Page.Controls.Add(this.pbPlay);
            this.tbADPCM_Page.Controls.Add(this.lblPlayTimeCurrent);
            this.tbADPCM_Page.Controls.Add(this.pgAudioFile);
            this.tbADPCM_Page.Controls.Add(this.btnExtractAPCM);
            this.tbADPCM_Page.Controls.Add(this.pbRipStatus);
            this.tbADPCM_Page.Controls.Add(this.pbExtract);
            this.tbADPCM_Page.Controls.Add(this.lvResults);
            this.tbADPCM_Page.Location = new System.Drawing.Point(4, 22);
            this.tbADPCM_Page.Name = "tbADPCM_Page";
            this.tbADPCM_Page.Padding = new System.Windows.Forms.Padding(3);
            this.tbADPCM_Page.Size = new System.Drawing.Size(1027, 569);
            this.tbADPCM_Page.TabIndex = 0;
            this.tbADPCM_Page.Text = "ADPCM Ripper";
            this.tbADPCM_Page.UseVisualStyleBackColor = true;
            // 
            // btnCancelSearch
            // 
            this.btnCancelSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelSearch.Location = new System.Drawing.Point(284, 250);
            this.btnCancelSearch.Name = "btnCancelSearch";
            this.btnCancelSearch.Size = new System.Drawing.Size(208, 45);
            this.btnCancelSearch.TabIndex = 64;
            this.btnCancelSearch.Text = "Cancel Search";
            this.btnCancelSearch.UseVisualStyleBackColor = true;
            this.btnCancelSearch.Visible = false;
            this.btnCancelSearch.Click += new System.EventHandler(this.btnCancelSearch_Click);
            // 
            // btnCancelExtractAPDCM
            // 
            this.btnCancelExtractAPDCM.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelExtractAPDCM.Location = new System.Drawing.Point(284, 251);
            this.btnCancelExtractAPDCM.Name = "btnCancelExtractAPDCM";
            this.btnCancelExtractAPDCM.Size = new System.Drawing.Size(208, 45);
            this.btnCancelExtractAPDCM.TabIndex = 63;
            this.btnCancelExtractAPDCM.Text = "Cancel Extraction";
            this.btnCancelExtractAPDCM.UseVisualStyleBackColor = true;
            this.btnCancelExtractAPDCM.Visible = false;
            this.btnCancelExtractAPDCM.Click += new System.EventHandler(this.btnCancelExtractAPDCM_Click);
            // 
            // btnADPCMInvert
            // 
            this.btnADPCMInvert.Location = new System.Drawing.Point(643, 535);
            this.btnADPCMInvert.Name = "btnADPCMInvert";
            this.btnADPCMInvert.Size = new System.Drawing.Size(122, 28);
            this.btnADPCMInvert.TabIndex = 62;
            this.btnADPCMInvert.Text = "Invert Selection";
            this.btnADPCMInvert.UseVisualStyleBackColor = true;
            this.btnADPCMInvert.Click += new System.EventHandler(this.btnADPCMInvert_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(875, 480);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(41, 27);
            this.btnPause.TabIndex = 61;
            this.btnPause.Text = "||";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(771, 485);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(27, 22);
            this.btnPrevious.TabIndex = 60;
            this.btnPrevious.Text = "<<";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(994, 484);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(27, 22);
            this.btnNext.TabIndex = 59;
            this.btnNext.Text = ">>";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(937, 480);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(41, 27);
            this.btnStop.TabIndex = 58;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(819, 480);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(41, 27);
            this.btnPlay.TabIndex = 57;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // pbPlay
            // 
            this.pbPlay.Location = new System.Drawing.Point(771, 551);
            this.pbPlay.Name = "pbPlay";
            this.pbPlay.Size = new System.Drawing.Size(250, 12);
            this.pbPlay.TabIndex = 56;
            // 
            // lblPlayTimeCurrent
            // 
            this.lblPlayTimeCurrent.BackColor = System.Drawing.Color.Black;
            this.lblPlayTimeCurrent.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPlayTimeCurrent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblPlayTimeCurrent.Location = new System.Drawing.Point(771, 510);
            this.lblPlayTimeCurrent.Name = "lblPlayTimeCurrent";
            this.lblPlayTimeCurrent.Size = new System.Drawing.Size(250, 36);
            this.lblPlayTimeCurrent.TabIndex = 55;
            this.lblPlayTimeCurrent.Text = "00:00";
            this.lblPlayTimeCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pgAudioFile
            // 
            this.pgAudioFile.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.pgAudioFile.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.pgAudioFile.Location = new System.Drawing.Point(771, 6);
            this.pgAudioFile.Name = "pgAudioFile";
            this.pgAudioFile.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.pgAudioFile.Size = new System.Drawing.Size(250, 468);
            this.pgAudioFile.TabIndex = 54;
            // 
            // btnExtractAPCM
            // 
            this.btnExtractAPCM.Location = new System.Drawing.Point(6, 535);
            this.btnExtractAPCM.Name = "btnExtractAPCM";
            this.btnExtractAPCM.Size = new System.Drawing.Size(139, 28);
            this.btnExtractAPCM.TabIndex = 50;
            this.btnExtractAPCM.Text = "Extract Selected";
            this.btnExtractAPCM.UseVisualStyleBackColor = true;
            this.btnExtractAPCM.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // pbRipStatus
            // 
            this.pbRipStatus.Location = new System.Drawing.Point(151, 535);
            this.pbRipStatus.Name = "pbRipStatus";
            this.pbRipStatus.Size = new System.Drawing.Size(486, 11);
            this.pbRipStatus.TabIndex = 49;
            this.pbRipStatus.Click += new System.EventHandler(this.pbRipStatus_Click);
            // 
            // pbExtract
            // 
            this.pbExtract.Location = new System.Drawing.Point(151, 552);
            this.pbExtract.Name = "pbExtract";
            this.pbExtract.Size = new System.Drawing.Size(486, 11);
            this.pbExtract.TabIndex = 48;
            // 
            // lvResults
            // 
            this.lvResults.CheckBoxes = true;
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chID,
            this.chFilename,
            this.chDescription,
            this.chFormat,
            this.chOffset,
            this.chLength,
            this.chDelta});
            this.lvResults.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvResults.FullRowSelect = true;
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(6, 6);
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(759, 523);
            this.lvResults.TabIndex = 38;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            this.lvResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvResults_ColumnClick);
            this.lvResults.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvResults_ItemChecked);
            this.lvResults.SelectedIndexChanged += new System.EventHandler(this.lvResults_SelectedIndexChanged);
            // 
            // chID
            // 
            this.chID.Text = "ID";
            // 
            // chFilename
            // 
            this.chFilename.Text = "Name";
            this.chFilename.Width = 240;
            // 
            // chDescription
            // 
            this.chDescription.Text = "Description";
            this.chDescription.Width = 170;
            // 
            // chFormat
            // 
            this.chFormat.Text = "Format";
            this.chFormat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.chFormat.Width = 50;
            // 
            // chOffset
            // 
            this.chOffset.Text = "Offset";
            this.chOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chOffset.Width = 70;
            // 
            // chLength
            // 
            this.chLength.Text = "Length";
            this.chLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chLength.Width = 70;
            // 
            // chDelta
            // 
            this.chDelta.Text = "Playtime";
            this.chDelta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chDelta.Width = 70;
            // 
            // tbISOExplorer_Page
            // 
            this.tbISOExplorer_Page.Controls.Add(this.btnCancelExtractFile);
            this.tbISOExplorer_Page.Controls.Add(this.pbExtractCurrent);
            this.tbISOExplorer_Page.Controls.Add(this.pbExtractTotal);
            this.tbISOExplorer_Page.Controls.Add(this.btnInvertSelection);
            this.tbISOExplorer_Page.Controls.Add(this.btnExtractSelected);
            this.tbISOExplorer_Page.Controls.Add(this.btnExtractAll);
            this.tbISOExplorer_Page.Controls.Add(this.lvFiles);
            this.tbISOExplorer_Page.Controls.Add(this.tvFolders);
            this.tbISOExplorer_Page.Location = new System.Drawing.Point(4, 22);
            this.tbISOExplorer_Page.Name = "tbISOExplorer_Page";
            this.tbISOExplorer_Page.Padding = new System.Windows.Forms.Padding(3);
            this.tbISOExplorer_Page.Size = new System.Drawing.Size(1027, 569);
            this.tbISOExplorer_Page.TabIndex = 1;
            this.tbISOExplorer_Page.Text = "ISO Explorer";
            this.tbISOExplorer_Page.UseVisualStyleBackColor = true;
            // 
            // btnCancelExtractFile
            // 
            this.btnCancelExtractFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelExtractFile.Location = new System.Drawing.Point(523, 246);
            this.btnCancelExtractFile.Name = "btnCancelExtractFile";
            this.btnCancelExtractFile.Size = new System.Drawing.Size(208, 45);
            this.btnCancelExtractFile.TabIndex = 21;
            this.btnCancelExtractFile.Text = "Cancel Extraction";
            this.btnCancelExtractFile.UseVisualStyleBackColor = true;
            this.btnCancelExtractFile.Visible = false;
            this.btnCancelExtractFile.Click += new System.EventHandler(this.btnStopExtractFile_Click);
            // 
            // pbExtractCurrent
            // 
            this.pbExtractCurrent.Location = new System.Drawing.Point(226, 552);
            this.pbExtractCurrent.Name = "pbExtractCurrent";
            this.pbExtractCurrent.Size = new System.Drawing.Size(687, 11);
            this.pbExtractCurrent.TabIndex = 19;
            // 
            // pbExtractTotal
            // 
            this.pbExtractTotal.Location = new System.Drawing.Point(226, 535);
            this.pbExtractTotal.Name = "pbExtractTotal";
            this.pbExtractTotal.Size = new System.Drawing.Size(687, 11);
            this.pbExtractTotal.TabIndex = 18;
            // 
            // btnInvertSelection
            // 
            this.btnInvertSelection.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInvertSelection.Location = new System.Drawing.Point(919, 539);
            this.btnInvertSelection.Name = "btnInvertSelection";
            this.btnInvertSelection.Size = new System.Drawing.Size(102, 24);
            this.btnInvertSelection.TabIndex = 16;
            this.btnInvertSelection.Text = "Invert Selection";
            this.btnInvertSelection.UseVisualStyleBackColor = true;
            this.btnInvertSelection.Click += new System.EventHandler(this.btnInvertSelection_Click);
            // 
            // btnExtractSelected
            // 
            this.btnExtractSelected.Image = global::ADPCM_Ripper_v2.Properties.Resources.shell32_255;
            this.btnExtractSelected.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExtractSelected.Location = new System.Drawing.Point(6, 535);
            this.btnExtractSelected.Name = "btnExtractSelected";
            this.btnExtractSelected.Size = new System.Drawing.Size(104, 25);
            this.btnExtractSelected.TabIndex = 20;
            this.btnExtractSelected.Text = "Extract Selected";
            this.btnExtractSelected.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExtractSelected.UseVisualStyleBackColor = true;
            this.btnExtractSelected.Click += new System.EventHandler(this.cmdExtractSelected_Click);
            // 
            // btnExtractAll
            // 
            this.btnExtractAll.Image = global::ADPCM_Ripper_v2.Properties.Resources.shell32_255;
            this.btnExtractAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExtractAll.Location = new System.Drawing.Point(116, 535);
            this.btnExtractAll.Name = "btnExtractAll";
            this.btnExtractAll.Size = new System.Drawing.Size(104, 25);
            this.btnExtractAll.TabIndex = 17;
            this.btnExtractAll.Text = "Extract all files";
            this.btnExtractAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnExtractAll.UseVisualStyleBackColor = true;
            this.btnExtractAll.Click += new System.EventHandler(this.cmdExtractAll_Click);
            // 
            // lvFiles
            // 
            this.lvFiles.CheckBoxes = true;
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmFileName,
            this.clmSize,
            this.clmLBA,
            this.clmDate,
            this.clmProgressBar});
            this.lvFiles.Location = new System.Drawing.Point(226, 3);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(795, 526);
            this.lvFiles.TabIndex = 3;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            this.lvFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvFiles_ColumnClick);
            this.lvFiles.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lvFiles_ItemChecked);
            // 
            // clmFileName
            // 
            this.clmFileName.Text = "Filename";
            this.clmFileName.Width = 394;
            // 
            // clmSize
            // 
            this.clmSize.Text = "Size";
            this.clmSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.clmSize.Width = 90;
            // 
            // clmLBA
            // 
            this.clmLBA.Text = "Sector";
            this.clmLBA.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.clmLBA.Width = 90;
            // 
            // clmDate
            // 
            this.clmDate.Text = "Date";
            this.clmDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.clmDate.Width = 90;
            // 
            // clmProgressBar
            // 
            this.clmProgressBar.Text = "";
            this.clmProgressBar.Width = 100;
            // 
            // tvFolders
            // 
            this.tvFolders.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvFolders.Location = new System.Drawing.Point(6, 3);
            this.tvFolders.Name = "tvFolders";
            this.tvFolders.ShowRootLines = false;
            this.tvFolders.Size = new System.Drawing.Size(214, 526);
            this.tvFolders.TabIndex = 2;
            this.tvFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFolders_AfterSelect);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.rbSystem);
            this.tabPage1.Controls.Add(this.listView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1027, 569);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Game Specific";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(796, 536);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(228, 30);
            this.button1.TabIndex = 2;
            this.button1.Text = "Launch Extraction";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // rbSystem
            // 
            this.rbSystem.AutoSize = true;
            this.rbSystem.Checked = true;
            this.rbSystem.Location = new System.Drawing.Point(14, 17);
            this.rbSystem.Name = "rbSystem";
            this.rbSystem.Size = new System.Drawing.Size(85, 17);
            this.rbSystem.TabIndex = 1;
            this.rbSystem.TabStop = true;
            this.rbSystem.Text = "Playstation 2";
            this.rbSystem.UseVisualStyleBackColor = true;
            this.rbSystem.CheckedChanged += new System.EventHandler(this.rbSystem_CheckedChanged);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmID,
            this.clmName,
            this.clmRegion});
            this.listView1.Location = new System.Drawing.Point(128, 17);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(896, 513);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // clmID
            // 
            this.clmID.Text = "Game ID";
            this.clmID.Width = 120;
            // 
            // clmName
            // 
            this.clmName.Text = "Game Name";
            this.clmName.Width = 540;
            // 
            // clmRegion
            // 
            this.clmRegion.Text = "Region";
            this.clmRegion.Width = 150;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItem2});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1059, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutADPCMRipperToolStripMenuItem});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(24, 20);
            this.toolStripMenuItem2.Text = "?";
            // 
            // aboutADPCMRipperToolStripMenuItem
            // 
            this.aboutADPCMRipperToolStripMenuItem.Name = "aboutADPCMRipperToolStripMenuItem";
            this.aboutADPCMRipperToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.aboutADPCMRipperToolStripMenuItem.Text = "About ADPCM Ripper";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusISO});
            this.statusStrip1.Location = new System.Drawing.Point(0, 682);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.statusStrip1.Size = new System.Drawing.Size(1059, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusISO
            // 
            this.toolStripStatusISO.Name = "toolStripStatusISO";
            this.toolStripStatusISO.Size = new System.Drawing.Size(0, 17);
            this.toolStripStatusISO.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbOpen,
            this.tsbSearchDeep});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1059, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbOpen
            // 
            this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpen.Image = global::ADPCM_Ripper_v2.Properties.Resources.shell32_5;
            this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpen.Name = "tsbOpen";
            this.tsbOpen.Size = new System.Drawing.Size(23, 22);
            this.tsbOpen.Text = "Open a file";
            this.tsbOpen.Click += new System.EventHandler(this.tsbOpen_Click);
            // 
            // tsbSearchDeep
            // 
            this.tsbSearchDeep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSearchDeep.Image = global::ADPCM_Ripper_v2.Properties.Resources.shell32_23;
            this.tsbSearchDeep.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSearchDeep.Name = "tsbSearchDeep";
            this.tsbSearchDeep.Size = new System.Drawing.Size(23, 22);
            this.tsbSearchDeep.Text = "Make a deep search";
            this.tsbSearchDeep.ToolTipText = "Make a deep search";
            this.tsbSearchDeep.Click += new System.EventHandler(this.tsbSearchDeep_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(12, 659);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(1031, 20);
            this.textBox1.TabIndex = 4;
            // 
            // openFileDialog
            // 
            this.openFileDialog.Multiselect = true;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "shell32_4.ico");
            this.imageList.Images.SetKeyName(1, "shell32_5.ico");
            this.imageList.Images.SetKeyName(2, "shell32_1004.ico");
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 704);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "ADPCM Ripper v2b3.2 (c) 2010-2019 Fastelbja";
            this.tabControl1.ResumeLayout(false);
            this.tbADPCM_Page.ResumeLayout(false);
            this.tbISOExplorer_Page.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tbADPCM_Page;
        private System.Windows.Forms.TabPage tbISOExplorer_Page;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem aboutADPCMRipperToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView lvFiles;
        private System.Windows.Forms.ColumnHeader clmFileName;
        private System.Windows.Forms.ColumnHeader clmSize;
        private System.Windows.Forms.ColumnHeader clmLBA;
        private System.Windows.Forms.ColumnHeader clmDate;
        private System.Windows.Forms.ColumnHeader clmProgressBar;
        private System.Windows.Forms.TreeView tvFolders;
        private System.Windows.Forms.Button btnInvertSelection;
        private System.Windows.Forms.ToolStripButton tsbOpen;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.ProgressBar pbPlay;
        private System.Windows.Forms.Label lblPlayTimeCurrent;
        private System.Windows.Forms.PropertyGrid pgAudioFile;
        private System.Windows.Forms.Button btnExtractAPCM;
        private System.Windows.Forms.ProgressBar pbRipStatus;
        private System.Windows.Forms.ProgressBar pbExtract;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.ColumnHeader chID;
        private System.Windows.Forms.ColumnHeader chFilename;
        private System.Windows.Forms.ColumnHeader chDescription;
        private System.Windows.Forms.ColumnHeader chFormat;
        private System.Windows.Forms.ColumnHeader chOffset;
        private System.Windows.Forms.ColumnHeader chLength;
        private System.Windows.Forms.ColumnHeader chDelta;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.Button btnExtractAll;
        private System.Windows.Forms.ProgressBar pbExtractCurrent;
        private System.Windows.Forms.ProgressBar pbExtractTotal;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnADPCMInvert;
        private System.Windows.Forms.Button btnExtractSelected;
        private System.Windows.Forms.Button btnCancelExtractFile;
        private System.Windows.Forms.Button btnCancelExtractAPDCM;
        private System.Windows.Forms.Button btnCancelSearch;
        private System.Windows.Forms.ToolStripButton tsbSearchDeep;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusISO;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RadioButton rbSystem;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader clmID;
        private System.Windows.Forms.ColumnHeader clmName;
        private System.Windows.Forms.ColumnHeader clmRegion;
        private System.Windows.Forms.Button button1;


    }
}

