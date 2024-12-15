using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	[SupportedOSPlatform("windows")]
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
