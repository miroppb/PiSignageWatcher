namespace PiSignageWatcher
{
	partial class FrmMain
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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
			notifyIcon1 = new System.Windows.Forms.NotifyIcon(components);
			contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
			checkNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			showPlayerIDsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			reDeplotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			item1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			turnAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			onToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			offToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			rebootToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			item1ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			deployToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			item1ToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			anotherItem1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			scheduleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			contextMenuStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// notifyIcon1
			// 
			notifyIcon1.ContextMenuStrip = contextMenuStrip1;
			notifyIcon1.Icon = (System.Drawing.Icon)resources.GetObject("notifyIcon1.Icon");
			notifyIcon1.Text = "PiSignage";
			notifyIcon1.Visible = true;
			// 
			// contextMenuStrip1
			// 
			contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { checkNowToolStripMenuItem, showPlayerIDsToolStripMenuItem, reDeplotToolStripMenuItem, turnAllToolStripMenuItem, rebootToolStripMenuItem, deployToolStripMenuItem, scheduleToolStripMenuItem, exitToolStripMenuItem });
			contextMenuStrip1.Name = "contextMenuStrip1";
			contextMenuStrip1.Size = new System.Drawing.Size(211, 224);
			// 
			// checkNowToolStripMenuItem
			// 
			checkNowToolStripMenuItem.Name = "checkNowToolStripMenuItem";
			checkNowToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			checkNowToolStripMenuItem.Text = "Check Now";
			checkNowToolStripMenuItem.Click += CheckNowToolStripMenuItem_Click;
			// 
			// showPlayerIDsToolStripMenuItem
			// 
			showPlayerIDsToolStripMenuItem.Name = "showPlayerIDsToolStripMenuItem";
			showPlayerIDsToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			showPlayerIDsToolStripMenuItem.Text = "Show Player IDs";
			showPlayerIDsToolStripMenuItem.Click += ShowPlayerIDsToolStripMenuItem_Click;
			// 
			// reDeplotToolStripMenuItem
			// 
			reDeplotToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { item1ToolStripMenuItem });
			reDeplotToolStripMenuItem.Name = "reDeplotToolStripMenuItem";
			reDeplotToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			reDeplotToolStripMenuItem.Text = "Re-Deploy";
			// 
			// item1ToolStripMenuItem
			// 
			item1ToolStripMenuItem.Name = "item1ToolStripMenuItem";
			item1ToolStripMenuItem.Size = new System.Drawing.Size(130, 26);
			item1ToolStripMenuItem.Text = "Item1";
			// 
			// turnAllToolStripMenuItem
			// 
			turnAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { onToolStripMenuItem, offToolStripMenuItem });
			turnAllToolStripMenuItem.Name = "turnAllToolStripMenuItem";
			turnAllToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			turnAllToolStripMenuItem.Text = "Power All";
			// 
			// onToolStripMenuItem
			// 
			onToolStripMenuItem.Name = "onToolStripMenuItem";
			onToolStripMenuItem.Size = new System.Drawing.Size(113, 26);
			onToolStripMenuItem.Text = "On";
			onToolStripMenuItem.Click += OnToolStripMenuItem_Click;
			// 
			// offToolStripMenuItem
			// 
			offToolStripMenuItem.Name = "offToolStripMenuItem";
			offToolStripMenuItem.Size = new System.Drawing.Size(113, 26);
			offToolStripMenuItem.Text = "Off";
			offToolStripMenuItem.Click += OffToolStripMenuItem_Click;
			// 
			// rebootToolStripMenuItem
			// 
			rebootToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { item1ToolStripMenuItem1 });
			rebootToolStripMenuItem.Name = "rebootToolStripMenuItem";
			rebootToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			rebootToolStripMenuItem.Text = "Reboot";
			// 
			// item1ToolStripMenuItem1
			// 
			item1ToolStripMenuItem1.Name = "item1ToolStripMenuItem1";
			item1ToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
			item1ToolStripMenuItem1.Text = "Item1";
			// 
			// deployToolStripMenuItem
			// 
			deployToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { item1ToolStripMenuItem2 });
			deployToolStripMenuItem.Name = "deployToolStripMenuItem";
			deployToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			deployToolStripMenuItem.Text = "Deploy";
			// 
			// item1ToolStripMenuItem2
			// 
			item1ToolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { anotherItem1ToolStripMenuItem });
			item1ToolStripMenuItem2.Name = "item1ToolStripMenuItem2";
			item1ToolStripMenuItem2.Size = new System.Drawing.Size(224, 26);
			item1ToolStripMenuItem2.Text = "Item 1";
			// 
			// anotherItem1ToolStripMenuItem
			// 
			anotherItem1ToolStripMenuItem.Name = "anotherItem1ToolStripMenuItem";
			anotherItem1ToolStripMenuItem.Size = new System.Drawing.Size(191, 26);
			anotherItem1ToolStripMenuItem.Text = "Another Item 1";
			// 
			// scheduleToolStripMenuItem
			// 
			scheduleToolStripMenuItem.Name = "scheduleToolStripMenuItem";
			scheduleToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			scheduleToolStripMenuItem.Text = "Schedule";
			scheduleToolStripMenuItem.Click += ScheduleToolStripMenuItem_Click;
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
			// 
			// FrmMain
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(862, 576);
			Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
			Name = "FrmMain";
			Text = "Form1";
			contextMenuStrip1.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem checkNowToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scheduleToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showPlayerIDsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem turnAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem onToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem offToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reDeplotToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem item1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rebootToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem item1ToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem deployToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem item1ToolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem anotherItem1ToolStripMenuItem;
	}
}

