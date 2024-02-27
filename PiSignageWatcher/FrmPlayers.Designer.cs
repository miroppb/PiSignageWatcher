namespace PiSignageWatcher
{
    partial class FrmPlayers
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
			BtnClose = new System.Windows.Forms.Button();
			DgvPlayers = new System.Windows.Forms.DataGridView();
			ClmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			ClmID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)DgvPlayers).BeginInit();
			SuspendLayout();
			// 
			// BtnClose
			// 
			BtnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			BtnClose.Location = new System.Drawing.Point(196, 181);
			BtnClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			BtnClose.Name = "BtnClose";
			BtnClose.Size = new System.Drawing.Size(88, 27);
			BtnClose.TabIndex = 0;
			BtnClose.Text = "Close";
			BtnClose.UseVisualStyleBackColor = true;
			BtnClose.Click += BtnClose_Click;
			// 
			// DgvPlayers
			// 
			DgvPlayers.AllowUserToAddRows = false;
			DgvPlayers.AllowUserToDeleteRows = false;
			DgvPlayers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			DgvPlayers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DgvPlayers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { ClmName, ClmID });
			DgvPlayers.Location = new System.Drawing.Point(14, 14);
			DgvPlayers.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			DgvPlayers.Name = "DgvPlayers";
			DgvPlayers.ReadOnly = true;
			DgvPlayers.Size = new System.Drawing.Size(429, 160);
			DgvPlayers.TabIndex = 1;
			// 
			// ClmName
			// 
			ClmName.HeaderText = "Name";
			ClmName.Name = "ClmName";
			ClmName.ReadOnly = true;
			// 
			// ClmID
			// 
			ClmID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			ClmID.HeaderText = "ID";
			ClmID.Name = "ClmID";
			ClmID.ReadOnly = true;
			// 
			// FrmPlayers
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = BtnClose;
			ClientSize = new System.Drawing.Size(458, 220);
			Controls.Add(DgvPlayers);
			Controls.Add(BtnClose);
			Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "FrmPlayers";
			ShowIcon = false;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			Text = "Players";
			((System.ComponentModel.ISupportInitialize)DgvPlayers).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClmName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ClmID;
        public System.Windows.Forms.DataGridView DgvPlayers;
    }
}