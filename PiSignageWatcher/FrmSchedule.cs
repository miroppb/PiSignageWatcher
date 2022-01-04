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

namespace PiSignageWatcher
{
    public partial class FrmSchedule : Form
    {
        public SQLiteConnection dbConnection;
        bool starting = true;
        public List<string> ValidTVs = new List<string>();
        public List<string> ValidActions = new List<string>();
        public List<int> ActionIDs = new List<int>();
        List<string> DoW = new List<string>(new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

        public FrmSchedule()
        {
            InitializeComponent();
            dbConnection = new SQLiteConnection();
        }

        #region SQL code
        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                SQLiteConnection conn = new SQLiteConnection(dbConnection);
                conn.Open();
                SQLiteCommand comm = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = comm.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                StreamWriter w = new StreamWriter("error.txt"); w.WriteLine(sql); w.Close();
            }
            return dt;
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

        public int ExecuteNonQueryWithBlob(string sql, string blobFieldName, byte[] blob)
        {
            SQLiteConnection con = new SQLiteConnection(dbConnection);
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format(sql);
            SQLiteParameter param = new SQLiteParameter("@" + blobFieldName, System.Data.DbType.Binary);
            param.Value = blob;
            cmd.Parameters.Add(param);
            con.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception exc1)
            {
                MessageBox.Show(exc1.Message);
            }
            con.Close();
            return 0;
        }

        public bool Update(string tableName, Dictionary<string, string> data, string where)
        {
            string vals = "";
            bool returnCode = true;
            if (data.Count >= 1)
            {
                foreach (KeyValuePair<string, string> val in data)
                {
                    vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
                }
                vals = vals.Substring(0, vals.Length - 1);
            }
            try
            {
                this.ExecuteNonQuery(String.Format("UPDATE {0} SET {1} WHERE {2}", tableName, vals, where));
            }
            catch
            {
                returnCode = false;
            }
            return returnCode;
        }

        public bool Delete(string tableName, string where)
        {
            bool returnCode = true;
            try
            {
                this.ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1}", tableName, where));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                returnCode = false;
            }
            return returnCode;
        }

        public bool Insert(string tableName, Dictionary<string, string> data)
        {
            string columns = "";
            string values = "";
            bool returnCode = true;
            foreach (KeyValuePair<string, string> val in data)
            {
                columns += String.Format(" {0},", val.Key.ToString());
                values += String.Format(" '{0}',", val.Value);
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            try
            {
                this.ExecuteNonQuery(String.Format("INSERT INTO {0}({1}) VALUES({2});", tableName, columns, values));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                returnCode = false;
            }
            return returnCode;
        }

        public void Prepare(string sql, List<SQLiteParameter> data)
        {
            SQLiteConnection conn = new SQLiteConnection(dbConnection);
            conn.Open();
            SQLiteCommand comm = new SQLiteCommand(conn);
            comm.CommandText = sql;
            for (int c = 0; c < data.Count(); c++)
                comm.Parameters.Add(data[c]);
            comm.ExecuteNonQuery();
            conn.Close();
        }
        #endregion

        private void FrmSchedule_Load(object sender, EventArgs e)
        {
            //get schedule
            ((DataGridViewComboBoxColumn)DgvSchedule.Columns[0]).Items.AddRange(ValidTVs.ToArray());
            ((DataGridViewComboBoxColumn)DgvSchedule.Columns[1]).Items.AddRange(DoW.ToArray());
            ((DataGridViewComboBoxColumn)DgvSchedule.Columns[3]).Items.AddRange(ValidActions.ToArray());
            DataTable b = GetDataTable("SELECT id, tv, day, time, action FROM schedule ORDER BY id");
            for (int c = 0; c < b.Rows.Count; c++)
            {
                ActionIDs.Add(Convert.ToInt32(b.Rows[c].ItemArray[0].ToString()));
                DgvSchedule.Rows.Add();
                try { ((DataGridViewComboBoxCell)DgvSchedule.Rows[c].Cells[0]).Value = b.Rows[c].ItemArray[1].ToString(); } catch { }
                try { ((DataGridViewComboBoxCell)DgvSchedule.Rows[c].Cells[1]).Value = DoW[Convert.ToInt16(b.Rows[c].ItemArray[2].ToString())]; } catch { }
                DgvSchedule.Rows[c].Cells[2].Value = b.Rows[c].ItemArray[3].ToString();
                try { ((DataGridViewComboBoxCell)DgvSchedule.Rows[c].Cells[3]).Value = ValidActions[Convert.ToInt16(b.Rows[c].ItemArray[4].ToString())]; } catch { }
            }

            starting = false;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            DgvSchedule.Rows.Add();
            ExecuteNonQuery("INSERT INTO schedule VALUES(NULL, -1, -1, \'\', -1);");
            DataTable dt = GetDataTable("SELECT id FROM schedule ORDER BY id DESC LIMIT 0,1");
            ActionIDs.Add(Convert.ToInt32(dt.Rows[0].ItemArray[0].ToString()));
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove the selected row?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ExecuteNonQuery("DELETE FROM schedule WHERE id = " + ActionIDs[DgvSchedule.CurrentCell.RowIndex]);
                ActionIDs.RemoveAt(DgvSchedule.CurrentCell.RowIndex);
                DgvSchedule.Rows.RemoveAt(DgvSchedule.CurrentCell.RowIndex);
            }
        }

        private void DgvSchedule_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!starting)
            {
                if (e.ColumnIndex == 0)
                {
                    string i = DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    ExecuteNonQuery("UPDATE schedule SET miner = \"" + i + "\" WHERE id = " + ActionIDs[e.RowIndex]);
                }
                else if (e.ColumnIndex == 1)
                {
                    int i = DoW.IndexOf(DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                    ExecuteNonQuery("UPDATE schedule SET day = " + i + " WHERE id = " + ActionIDs[e.RowIndex]);
                }
                else if (e.ColumnIndex == 3)
                {
                    int i = ValidActions.IndexOf((DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Replace(" ", "")));
                    ExecuteNonQuery("UPDATE schedule SET action = " + i + " WHERE id = " + ActionIDs[e.RowIndex]);
                }
            }
        }

        private void DgvSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //find which cell, open the frmTime window
            if (e.ColumnIndex == 2)
            {
                FrmTime f = new FrmTime();
                //if the cell wasn't empty, fill the time
                try { f.Dtp.Value = DateTime.Parse(DateTime.Now.ToShortDateString() + " " + DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString()); }
                catch { f.Dtp.Value = DateTime.Now; }
                if (f.ShowDialog() == DialogResult.OK)
                {
                    //after window is closed, if OK, get the time, and insert into field
                    DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = f.Dtp.Value.ToShortTimeString();
                    //save to database
                    ExecuteNonQuery("UPDATE schedule SET time = \'" + f.Dtp.Value.ToShortTimeString() + "\' WHERE id = " + ActionIDs[e.RowIndex]);
                }
            }
            //easy peasy
        }
    }
}
