namespace PiSignageWatcher
{
	partial class FrmSchedule
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			groupBox2 = new System.Windows.Forms.GroupBox();
			BtnRemove = new System.Windows.Forms.Button();
			BtnAdd = new System.Windows.Forms.Button();
			DgvSchedule = new System.Windows.Forms.DataGridView();
			PlayerColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			DayColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			TimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			ActionColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			SubActionColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			BtnSave = new System.Windows.Forms.Button();
			groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)DgvSchedule).BeginInit();
			SuspendLayout();
			// 
			// groupBox2
			// 
			groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			groupBox2.Controls.Add(BtnRemove);
			groupBox2.Controls.Add(BtnAdd);
			groupBox2.Controls.Add(DgvSchedule);
			groupBox2.Location = new System.Drawing.Point(14, 14);
			groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			groupBox2.Name = "groupBox2";
			groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
			groupBox2.Size = new System.Drawing.Size(620, 208);
			groupBox2.TabIndex = 19;
			groupBox2.TabStop = false;
			groupBox2.Text = "Schedule";
			// 
			// BtnRemove
			// 
			BtnRemove.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			BtnRemove.Location = new System.Drawing.Point(566, 75);
			BtnRemove.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			BtnRemove.Name = "BtnRemove";
			BtnRemove.Size = new System.Drawing.Size(47, 27);
			BtnRemove.TabIndex = 17;
			BtnRemove.Text = "Rem";
			BtnRemove.UseVisualStyleBackColor = true;
			BtnRemove.Click += BtnRemove_Click;
			// 
			// BtnAdd
			// 
			BtnAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			BtnAdd.Location = new System.Drawing.Point(566, 42);
			BtnAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			BtnAdd.Name = "BtnAdd";
			BtnAdd.Size = new System.Drawing.Size(47, 27);
			BtnAdd.TabIndex = 16;
			BtnAdd.Text = "Add";
			BtnAdd.UseVisualStyleBackColor = true;
			BtnAdd.Click += BtnAdd_Click;
			// 
			// DgvSchedule
			// 
			DgvSchedule.AllowUserToAddRows = false;
			DgvSchedule.AllowUserToDeleteRows = false;
			DgvSchedule.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			DgvSchedule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DgvSchedule.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { PlayerColumn, DayColumn, TimeColumn, ActionColumn, SubActionColumn });
			DgvSchedule.Location = new System.Drawing.Point(4, 18);
			DgvSchedule.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			DgvSchedule.Name = "DgvSchedule";
			DgvSchedule.RowHeadersWidth = 51;
			DgvSchedule.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			DgvSchedule.Size = new System.Drawing.Size(556, 182);
			DgvSchedule.TabIndex = 15;
			DgvSchedule.CellDoubleClick += DgvSchedule_CellDoubleClick;
			DgvSchedule.CellValueChanged += DgvSchedule_CellValueChanged;
			// 
			// PlayerColumn
			// 
			PlayerColumn.DataPropertyName = "player";
			PlayerColumn.HeaderText = "Player";
			PlayerColumn.MinimumWidth = 6;
			PlayerColumn.Name = "PlayerColumn";
			PlayerColumn.Width = 125;
			// 
			// DayColumn
			// 
			DayColumn.DataPropertyName = "day";
			DayColumn.HeaderText = "Day";
			DayColumn.MinimumWidth = 6;
			DayColumn.Name = "DayColumn";
			DayColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			DayColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// TimeColumn
			// 
			TimeColumn.DataPropertyName = "time";
			dataGridViewCellStyle1.Format = "HH:mm tt";
			TimeColumn.DefaultCellStyle = dataGridViewCellStyle1;
			TimeColumn.HeaderText = "Time";
			TimeColumn.MinimumWidth = 6;
			TimeColumn.Name = "TimeColumn";
			TimeColumn.Width = 75;
			// 
			// ActionColumn
			// 
			ActionColumn.DataPropertyName = "action";
			ActionColumn.HeaderText = "Action";
			ActionColumn.MinimumWidth = 6;
			ActionColumn.Name = "ActionColumn";
			// 
			// SubActionColumn
			// 
			SubActionColumn.HeaderText = "SubAction";
			SubActionColumn.MinimumWidth = 6;
			SubActionColumn.Name = "SubActionColumn";
			SubActionColumn.Width = 125;
			// 
			// BtnSave
			// 
			BtnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			BtnSave.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnSave.Location = new System.Drawing.Point(300, 228);
			BtnSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			BtnSave.Name = "BtnSave";
			BtnSave.Size = new System.Drawing.Size(88, 27);
			BtnSave.TabIndex = 20;
			BtnSave.Text = "Save";
			BtnSave.UseVisualStyleBackColor = true;
			BtnSave.Click += BtnSave_Click;
			// 
			// FrmSchedule
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(649, 273);
			Controls.Add(BtnSave);
			Controls.Add(groupBox2);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			MinimumSize = new System.Drawing.Size(626, 310);
			Name = "FrmSchedule";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Edit Schedule";
			Load += FrmSchedule_Load;
			groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)DgvSchedule).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button BtnRemove;
		private System.Windows.Forms.Button BtnAdd;
		private System.Windows.Forms.DataGridView DgvSchedule;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.DataGridViewComboBoxColumn PlayerColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn DayColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn TimeColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn ActionColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn SubActionColumn;
	}
}