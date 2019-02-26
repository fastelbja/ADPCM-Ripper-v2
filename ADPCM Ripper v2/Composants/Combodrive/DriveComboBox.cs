namespace ZinoLib.Windows.Controls
{
	using System;
	using System.Drawing;
	using System.Collections.Specialized;
	using System.Windows.Forms;
	using System.ComponentModel;
    using System.IO;

	using ZinoLib.Win32;

	/// <summary>
	/// Summary description for DriveComboBox.
	/// </summary>
	public class DriveComboBox : ImageCombo
	{
		public DriveComboBox() : base()
		{
			base.ImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;

			base.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			base.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

			BuildDriveList();
		}


		public string SelectedDrive
		{
			get
			{
				if( base.SelectedIndex!=-1 )
					return (base.SelectedItem as ImageComboItem).ItemValue;
				else
					return null;
			}
		}

		public void BuildDriveList()
		{
			base.Items.Clear();

			ShellAPI.SHFILEINFO shInfo = new ShellAPI.SHFILEINFO();
			ShellAPI.SHGFI dwAttribs = 
				ShellAPI.SHGFI.SHGFI_ICON |
				ShellAPI.SHGFI.SHGFI_SMALLICON |
				ShellAPI.SHGFI.SHGFI_SYSICONINDEX |
				ShellAPI.SHGFI.SHGFI_DISPLAYNAME;

			ListDictionary _iconDict = new ListDictionary();			
			foreach( string drive in System.IO.Directory.GetLogicalDrives() )
			{
				IntPtr m_pHandle = ShellAPI.SHGetFileInfo(drive, ShellAPI.FILE_ATTRIBUTE_NORMAL, ref shInfo, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shInfo), dwAttribs);

				if( m_pHandle.Equals(IntPtr.Zero)==false )					
				{
					int idxIcon = 0;
					if( _iconDict.Contains(shInfo.iIcon)==false )
					{
						base.ImageList.Images.Add( System.Drawing.Icon.FromHandle(shInfo.hIcon).Clone() as System.Drawing.Icon );

						User32API.DestroyIcon(shInfo.hIcon);

						_iconDict.Add( shInfo.iIcon, _iconDict.Count );
						idxIcon = _iconDict.Count-1;
					}
					else
						idxIcon = Convert.ToInt32( _iconDict[shInfo.iIcon] );

                    DriveInfo drv_info = new DriveInfo(shInfo.szDisplayName.Substring(shInfo.szDisplayName.IndexOf("(")+1,1));
                    if (drv_info.DriveType == DriveType.CDRom)
                    {
                        ImageComboItem item = new ImageComboItem(shInfo.szDisplayName, idxIcon, false);
                        item.ItemValue = drive;
                        base.Items.Add(item);
                    }
				}
			}

			if( base.Items.Count!=0 )
				base.SelectedIndex = 0;
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new ComboBox.ObjectCollection Items
		{
			get { return base.Items; }
		}
	}
}
