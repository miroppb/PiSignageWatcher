using Dapper;
using Dapper.Contrib.Extensions;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	public partial class FrmSchedule : Form
	{
		private readonly BindingList<ClSchedule> Schedules = new BindingList<ClSchedule>();

		bool starting = true;
		public List<string> ValidPlayers = new();
		public List<string> ValidActions = new();
		public List<string> ValidPlaylists = new();
		readonly List<string> DoW = new() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

		public FrmSchedule()
		{
			InitializeComponent();
		}

		private void FrmSchedule_Load(object sender, EventArgs e)
		{
			LoadSchedulesFromDatabase();
			SetupDataBinding();
			starting = false;
		}

		private void LoadSchedulesFromDatabase()
		{
			using MySqlConnection conn = Secrets.GetConnectionString();
			var _schedules = conn.GetAll<ClSchedule>().ToList();
			foreach (ClSchedule schedule in _schedules)
			{
				Schedules.Add(schedule);
			}
		}

		private void SetupDataBinding()
		{
			// Assuming the columns are already set up in the DataGridView designer
			var tvColumn = (DataGridViewComboBoxColumn)DgvSchedule.Columns["PlayerColumn"];
			var dayColumn = (DataGridViewComboBoxColumn)DgvSchedule.Columns["DayColumn"];
			var actionColumn = (DataGridViewComboBoxColumn)DgvSchedule.Columns["ActionColumn"];
			var subActionColumn = (DataGridViewComboBoxColumn)DgvSchedule.Columns["SubActionColumn"];

			tvColumn.DataPropertyName = "name";
			dayColumn.DataPropertyName = "day";
			actionColumn.DataPropertyName = "action";
			subActionColumn.DataPropertyName = "subaction";

			tvColumn.DataSource = ValidPlayers;
			dayColumn.DataSource = DoW;
			actionColumn.DataSource = ValidActions;
			subActionColumn.DataSource = ValidPlaylists;

			DgvSchedule.AutoGenerateColumns = false;
			DgvSchedule.DataSource = Schedules;
		}

		private void BtnSave_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void BtnAdd_Click(object sender, EventArgs e)
		{
			using MySqlConnection conn = Secrets.GetConnectionString();
			ClSchedule newSchedule = new()
			{
				name = ValidPlayers.FirstOrDefault()
			};
			newSchedule.id = (int)conn.Insert(newSchedule);
			Schedules.Add(newSchedule);
		}

		private void BtnRemove_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Are you sure you want to remove the selected row?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				using MySqlConnection conn = Secrets.GetConnectionString();
				var selectedSchedule = DgvSchedule.SelectedRows[0].DataBoundItem as ClSchedule;
				conn.Delete(selectedSchedule);
				Schedules.Remove(selectedSchedule);
			}
		}

		private void DgvSchedule_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (!starting)
			{
				var currentSchedule = DgvSchedule.Rows[e.RowIndex].DataBoundItem as ClSchedule;

				if (e.ColumnIndex == 0)
					currentSchedule.name = DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
				else if (e.ColumnIndex == 1)
					currentSchedule.day = DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
				else if (e.ColumnIndex == 3)
					currentSchedule.action = DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().Replace(" ", "");

				using MySqlConnection conn = Secrets.GetConnectionString();
				conn.Update(currentSchedule);
			}
		}

		private void DgvSchedule_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			//find which cell, open the frmTime window
			if (e.ColumnIndex == 2)
			{
				FrmTime f = new();
				//if the cell wasn't empty, fill the time
				if (DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
				{
					f.Dtp.Value = DateTime.Parse(DateTime.Now.ToShortDateString() + " " + DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
				}
				else
				{
					f.Dtp.Value = DateTime.Now;
				}
				if (f.ShowDialog() == DialogResult.OK)
				{
					//after window is closed, if OK, get the time, and insert into field
					DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = f.Dtp.Value.ToShortTimeString();
					//save to database
					DgvSchedule.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = f.Dtp.Value.ToShortTimeString();
					var currentSchedule = DgvSchedule.Rows[e.RowIndex].DataBoundItem as ClSchedule;
					using MySqlConnection conn = Secrets.GetConnectionString();
					conn.Update(currentSchedule);
				}
			}
			//easy peasy
		}
	}
}
