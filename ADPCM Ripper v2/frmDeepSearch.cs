using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using StreamReader;

namespace ADPCM_Ripper_v2
{
    public partial class frmDeepSearch : Form
    {
        private FST m_tmpFST;

        public FST SetFST 
        {
            set { m_tmpFST = value; }
        }
        public frmDeepSearch()
        {
            InitializeComponent();
        }

        public void UpdateProperties() 
        {
            for (int i = 0; i < m_tmpFST.FST_File.Count; i++)
            {
                cmbList.Items.Add(m_tmpFST.FST_File[i].Filename);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
