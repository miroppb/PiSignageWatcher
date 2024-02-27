using System;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	public partial class FrmPlayers : Form
	{
		public FrmPlayers()
		{
			InitializeComponent();
		}

		private void BtnClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
