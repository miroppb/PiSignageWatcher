using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using miroppb;

namespace PiSignageWatcher
{
    public partial class FrmPlayers : Form
    {
        public SQLiteConnection dbConnection = new SQLiteConnection();

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
            ExecuteNonQuery("DELETE FROM tvs");
            foreach (DataGridViewRow row in DgvPlayers.Rows)
            {
                ExecuteNonQuery("INSERT INTO tvs VALUES('" + row.Cells[0].Value + "', '" + row.Cells[1].Value + "');");
                libmiroppb.Log("INSERT INTO tvs VALUES('" + row.Cells[0].Value + "', '" + row.Cells[1].Value + "');");
            }
            MessageBox.Show("Done inserting");
            this.Close();
        }

        public int ExecuteNonQuery(string sql)
        {
            try
            {
                SQLiteConnection conn = new SQLiteConnection(dbConnection);
                conn.Open();
                SQLiteCommand comm = new SQLiteCommand(conn);
                comm.CommandText = sql;
                int rowsUpdated = comm.ExecuteNonQuery();
                conn.Close();
                return rowsUpdated;
            }
            catch (Exception ex)
            {
                StreamWriter w = new StreamWriter("error.txt"); w.WriteLine(ex.Message.ToString()); w.WriteLine(sql); w.Close();
                return -1;
            }
        }
    }
}
