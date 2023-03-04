using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	public partial class FrmSchedule : Form
	{
		bool starting = true;
		public List<string> ValidTVs = new List<string>();
		public List<string> ValidActions = new List<string>();
		public List<int> ActionIDs = new List<int>();
		List<string> DoW = new List<string>(new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

		public FrmSchedule()
		{
			InitializeComponent();
		}

		private void FrmSchedule_Load(object sender, EventArgs e)
		{
			//get schedule
			((DataGridViewComboBoxColumn)DgvSchedule.Columns[0]).Items.AddRange(ValidTVs.ToArray());
			((DataGridViewComboBoxColumn)DgvSchedule.Columns[1]).Items.AddRange(DoW.ToArray());
			((DataGridViewComboBoxColumn)DgvSchedule.Columns[3]).Items.AddRange(ValidActions.ToArray());

			List<ClSchedule> clSchedules = null;
			using (MySqlConnection conn = Secrets.GetConnectionString())
				clSchedules = conn.Query<ClSchedule>("SELECT id, tv, day, time, action FROM schedule ORDER BY id").ToList();

			for (int c = 0; c < clSchedules.Count; c++)
			{
				ActionIDs.Add(clSchedules[c].id);
				DgvSchedule.Rows.Add();
				try { ((DataGridViewComboBoxCell)DgvSchedule.Rows[c].Cells[0]).Value = clSchedules[c].tv; } catch { }
				try { ((DataGridViewComboBoxCell)DgvSchedule.Rows[c].Cells[1]).Value = DoW[Convert.ToInt16(clSchedules[c].day)]; } catch { }
				DgvSchedule.Rows[c].Cells[2].Value = clSchedules[c].time.ToString();
				try { ((DataGridViewComboBoxCell)DgvSchedule.Rows[c].Cells[3]).Value = ValidActions[Convert.ToInt16(clSchedules[c].action)]; } catch { }
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
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				conn.Execute("INSERT INTO schedule VALUES(NULL, -1, -1, \'\', -1);");
				ClSchedule schedule = conn.Query("SELECT id FROM schedule ORDER BY id DESC LIMIT 0,1").FirstOrDefault();
				ActionIDs.Add(schedule.id);
			}
		}

		private void BtnRemove_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to remove the selected row?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				using (MySqlConnection conn = Secrets.GetConnectionString())
				{
					conn.Execute("DELETE FROM schedule WHERE id = " + ActionIDs[DgvSchedule.CurrentCell.RowIndex]);
					ActionIDs.RemoveAt(DgvSchedule.CurrentCell.RowIndex);
					DgvSchedule.Rows.RemoveAt(DgvSchedule.CurrentCell.RowIndex);
				}
			}
		}

		private void DgvSchedule_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (!starting)
			{
				if (e.ColumnIndex == 0)
				{
					string i = DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
					using (MySqlConnection conn = Secrets.GetConnectionString())
						conn.Execute("UPDATE schedule SET tv = \"" + i + "\" WHERE id = " + ActionIDs[e.RowIndex]);
				}
				else if (e.ColumnIndex == 1)
				{
					int i = DoW.IndexOf(DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
					using (MySqlConnection conn = Secrets.GetConnectionString())
						conn.Execute("UPDATE schedule SET day = " + i + " WHERE id = " + ActionIDs[e.RowIndex]);
				}
				else if (e.ColumnIndex == 3)
				{
					int i = ValidActions.IndexOf((DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Replace(" ", "")));
					using (MySqlConnection conn = Secrets.GetConnectionString())
						conn.Execute("UPDATE schedule SET action = " + i + " WHERE id = " + ActionIDs[e.RowIndex]);
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
					using (MySqlConnection conn = Secrets.GetConnectionString())
						conn.Execute("UPDATE schedule SET time = \'" + f.Dtp.Value.ToShortTimeString() + "\' WHERE id = " + ActionIDs[e.RowIndex]);
				}
			}
			//easy peasy
		}
	}
}
