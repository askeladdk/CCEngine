namespace CCToolKit
{
	partial class frmStrEd
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
			this.olvStrings = new BrightIdeasSoftware.ObjectListView();
			this.olvColumnIndex = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
			this.olvColumnValue = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuAppend = new System.Windows.Forms.ToolStripMenuItem();
			this.mnuDeleteLast = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.olvStrings)).BeginInit();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// olvStrings
			// 
			this.olvStrings.AllColumns.Add(this.olvColumnIndex);
			this.olvStrings.AllColumns.Add(this.olvColumnValue);
			this.olvStrings.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.DoubleClick;
			this.olvStrings.CellEditUseWholeCell = false;
			this.olvStrings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvColumnIndex,
            this.olvColumnValue});
			this.olvStrings.Cursor = System.Windows.Forms.Cursors.Default;
			this.olvStrings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.olvStrings.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.olvStrings.FullRowSelect = true;
			this.olvStrings.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.olvStrings.HideSelection = false;
			this.olvStrings.Location = new System.Drawing.Point(0, 24);
			this.olvStrings.MultiSelect = false;
			this.olvStrings.Name = "olvStrings";
			this.olvStrings.ShowGroups = false;
			this.olvStrings.Size = new System.Drawing.Size(624, 396);
			this.olvStrings.TabIndex = 0;
			this.olvStrings.UseCompatibleStateImageBehavior = false;
			this.olvStrings.View = System.Windows.Forms.View.Details;
			this.olvStrings.CellEditFinishing += new BrightIdeasSoftware.CellEditEventHandler(this.olvStrings_CellEditFinishing);
			this.olvStrings.CellEditStarting += new BrightIdeasSoftware.CellEditEventHandler(this.olvStrings_CellEditStarting);
			this.olvStrings.FormatRow += new System.EventHandler<BrightIdeasSoftware.FormatRowEventArgs>(this.olvStrings_FormatRow);
			// 
			// olvColumnIndex
			// 
			this.olvColumnIndex.IsEditable = false;
			this.olvColumnIndex.Text = "Index";
			// 
			// olvColumnValue
			// 
			this.olvColumnValue.AspectName = "value";
			this.olvColumnValue.Text = "Value";
			this.olvColumnValue.Width = 500;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(624, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// mnuFile
			// 
			this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpen,
            this.mnuSave,
            this.mnuSaveAs,
            this.toolStripMenuItem1,
            this.mnuExit});
			this.mnuFile.Name = "mnuFile";
			this.mnuFile.Size = new System.Drawing.Size(37, 20);
			this.mnuFile.Text = "File";
			// 
			// mnuOpen
			// 
			this.mnuOpen.Name = "mnuOpen";
			this.mnuOpen.Size = new System.Drawing.Size(114, 22);
			this.mnuOpen.Text = "Open";
			this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
			// 
			// mnuSave
			// 
			this.mnuSave.Name = "mnuSave";
			this.mnuSave.Size = new System.Drawing.Size(114, 22);
			this.mnuSave.Text = "Save";
			this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
			// 
			// mnuSaveAs
			// 
			this.mnuSaveAs.Name = "mnuSaveAs";
			this.mnuSaveAs.Size = new System.Drawing.Size(114, 22);
			this.mnuSaveAs.Text = "Save As";
			this.mnuSaveAs.Click += new System.EventHandler(this.mnuSaveAs_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(111, 6);
			// 
			// mnuExit
			// 
			this.mnuExit.Name = "mnuExit";
			this.mnuExit.Size = new System.Drawing.Size(114, 22);
			this.mnuExit.Text = "Exit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// mnuEdit
			// 
			this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAppend,
            this.mnuDeleteLast});
			this.mnuEdit.Name = "mnuEdit";
			this.mnuEdit.Size = new System.Drawing.Size(39, 20);
			this.mnuEdit.Text = "Edit";
			// 
			// mnuAppend
			// 
			this.mnuAppend.Name = "mnuAppend";
			this.mnuAppend.Size = new System.Drawing.Size(131, 22);
			this.mnuAppend.Text = "Append";
			this.mnuAppend.Click += new System.EventHandler(this.mnuAppend_Click);
			// 
			// mnuDeleteLast
			// 
			this.mnuDeleteLast.Name = "mnuDeleteLast";
			this.mnuDeleteLast.Size = new System.Drawing.Size(131, 22);
			this.mnuDeleteLast.Text = "Delete Last";
			this.mnuDeleteLast.Click += new System.EventHandler(this.mnuDeleteLast_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 420);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(624, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.Filter = "All Files|*.*";
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.Filter = "All Files|*.*";
			// 
			// frmStrEd
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 442);
			this.Controls.Add(this.olvStrings);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.statusStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "frmStrEd";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "StrEd";
			((System.ComponentModel.ISupportInitialize)(this.olvStrings)).EndInit();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BrightIdeasSoftware.ObjectListView olvStrings;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private BrightIdeasSoftware.OLVColumn olvColumnIndex;
		private BrightIdeasSoftware.OLVColumn olvColumnValue;
		private System.Windows.Forms.ToolStripMenuItem mnuFile;
		private System.Windows.Forms.ToolStripMenuItem mnuOpen;
		private System.Windows.Forms.ToolStripMenuItem mnuSave;
		private System.Windows.Forms.ToolStripMenuItem mnuSaveAs;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem mnuExit;
		private System.Windows.Forms.ToolStripMenuItem mnuEdit;
		private System.Windows.Forms.ToolStripMenuItem mnuAppend;
		private System.Windows.Forms.ToolStripMenuItem mnuDeleteLast;
	}
}