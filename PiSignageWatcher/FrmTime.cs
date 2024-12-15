using System;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	[SupportedOSPlatform("windows")]
	public partial class FrmTime : Form
	{
		public FrmTime()
		{
			InitializeComponent();
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
