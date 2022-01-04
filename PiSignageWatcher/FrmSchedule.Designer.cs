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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.BtnRemove = new System.Windows.Forms.Button();
            this.BtnAdd = new System.Windows.Forms.Button();
            this.DgvSchedule = new System.Windows.Forms.DataGridView();
            this.Miner = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ClmTime1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ClmTime2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ClmAction1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.BtnSave = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvSchedule)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BtnRemove);
            this.groupBox2.Controls.Add(this.BtnAdd);
            this.groupBox2.Controls.Add(this.DgvSchedule);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(498, 180);
            this.groupBox2.TabIndex = 19;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Schedule";
            // 
            // BtnRemove
            // 
            this.BtnRemove.Location = new System.Drawing.Point(452, 65);
            this.BtnRemove.Name = "BtnRemove";
            this.BtnRemove.Size = new System.Drawing.Size(40, 23);
            this.BtnRemove.TabIndex = 17;
            this.BtnRemove.Text = "Rem";
            this.BtnRemove.UseVisualStyleBackColor = true;
            // 
            // BtnAdd
            // 
            this.BtnAdd.Location = new System.Drawing.Point(452, 36);
            this.BtnAdd.Name = "BtnAdd";
            this.BtnAdd.Size = new System.Drawing.Size(40, 23);
            this.BtnAdd.TabIndex = 16;
            this.BtnAdd.Text = "Add";
            this.BtnAdd.UseVisualStyleBackColor = true;
            // 
            // DgvSchedule
            // 
            this.DgvSchedule.AllowUserToAddRows = false;
            this.DgvSchedule.AllowUserToDeleteRows = false;
            this.DgvSchedule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DgvSchedule.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Miner,
            this.ClmTime1,
            this.ClmTime2,
            this.ClmAction1});
            this.DgvSchedule.Location = new System.Drawing.Point(3, 16);
            this.DgvSchedule.Name = "DgvSchedule";
            this.DgvSchedule.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.DgvSchedule.Size = new System.Drawing.Size(443, 158);
            this.DgvSchedule.TabIndex = 15;
            // 
            // Miner
            // 
            this.Miner.HeaderText = "Miner";
            this.Miner.Name = "Miner";
            this.Miner.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Miner.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ClmTime1
            // 
            this.ClmTime1.HeaderText = "Day";
            this.ClmTime1.Name = "ClmTime1";
            this.ClmTime1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ClmTime1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ClmTime2
            // 
            this.ClmTime2.HeaderText = "Time";
            this.ClmTime2.Name = "ClmTime2";
            this.ClmTime2.ReadOnly = true;
            // 
            // ClmAction1
            // 
            this.ClmAction1.HeaderText = "Action";
            this.ClmAction1.Name = "ClmAction1";
            this.ClmAction1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ClmAction1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // BtnSave
            // 
            this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSave.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnSave.Location = new System.Drawing.Point(224, 198);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(75, 23);
            this.BtnSave.TabIndex = 20;
            this.BtnSave.Text = "Save";
            this.BtnSave.UseVisualStyleBackColor = true;
            // 
            // FrmSchedule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 237);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.groupBox2);
            this.Name = "FrmSchedule";
            this.Text = "FrmSchedule";
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DgvSchedule)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button BtnRemove;
        private System.Windows.Forms.Button BtnAdd;
        private System.Windows.Forms.DataGridView DgvSchedule;
        private System.Windows.Forms.DataGridViewComboBoxColumn Miner;
        private System.Windows.Forms.DataGridViewComboBoxColumn ClmTime1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClmTime2;
        private System.Windows.Forms.DataGridViewComboBoxColumn ClmAction1;
        private System.Windows.Forms.Button BtnSave;
    }
}