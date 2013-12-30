using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Veasy;

namespace VeasySample
{
    public partial class MainForm : Form
    {
        private VeasyControl veasy;

        public MainForm()
        {
            InitializeComponent();

            this.veasy = new VeasyControl();
            this.veasy.Dock = DockStyle.Fill;

            this.Controls.Add(this.veasy);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.veasy.
        }
    }
}
