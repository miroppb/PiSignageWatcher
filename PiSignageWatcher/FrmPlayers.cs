using System;
using System.IO;
using System.Windows.Forms;
using Dapper;
using miroppb;
using MySqlConnector;

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
            this.Close();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
			using (MySqlConnection conn = Secrets.GetConnectionString())
				conn.Execute("DELETE FROM tvs");
            foreach (DataGridViewRow row in DgvPlayers.Rows)
            {
				using (MySqlConnection conn = Secrets.GetConnectionString())
					conn.Execute("INSERT INTO tvs VALUES('" + row.Cells[0].Value + "', '" + row.Cells[1].Value + "');");
                libmiroppb.Log("INSERT INTO tvs VALUES('" + row.Cells[0].Value + "', '" + row.Cells[1].Value + "');");
            }
            MessageBox.Show("Done inserting");
            this.Close();
        }
    }
}
