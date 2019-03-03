using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CCEngine.FileFormats;
using CCEngine.VFS;
using CCEngine;

namespace CCToolKit
{
	public partial class frmStrEd : Form
	{
		private class TableEntry
		{
			public string value;

			public TableEntry(string value)
			{
				this.value = value;
			}
		}

		private List<TableEntry> table = new List<TableEntry>();
		private string filename = "";

		private void Save(string filename)
		{
			Stream stream = null;
			try
			{
				string[] a = new string[this.table.Count];
				for (int i = 0; i < a.Length; i++) a[i] = this.table[i].value;
				stream = new FileStream(filename, FileMode.Create);
				StrFile.Write(stream, a);
			}
			catch (Exception exc)
			{
				MessageBox.Show(exc.Message, "Whoops",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (stream != null) stream.Close();
			}
		}

		public frmStrEd()
		{
			InitializeComponent();
		}

		private void mnuOpen_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				Stream stream = null;
				try
				{
					stream = new FileStream(openFileDialog1.FileName, FileMode.Open);
					string[] a = StrFile.Read(stream);
					List<TableEntry> table = new List<TableEntry>();
					foreach (string s in a) table.Add(new TableEntry(s));
					olvStrings.SetObjects(table);
					this.table = table;
					this.filename = openFileDialog1.FileName;
				}
				catch
				{
					MessageBox.Show("This is not a valid string table file!", "Whoops",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				finally
				{
					if (stream != null) stream.Close();
				}
			}
		}

		private void mnuSave_Click(object sender, EventArgs e)
		{
			if (this.filename.Length > 0) this.Save(this.filename);
			else this.mnuSaveAs_Click(sender, e);
		}

		private void mnuSaveAs_Click(object sender, EventArgs e)
		{
			saveFileDialog1.FileName = this.filename;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				this.filename = saveFileDialog1.FileName;
				this.Save(this.filename);
			}
		}

		private void olvStrings_FormatRow(object sender, BrightIdeasSoftware.FormatRowEventArgs e)
		{
			e.Item.SubItems[0].Text = e.DisplayIndex.ToString();
		}

		private void olvStrings_CellEditStarting(object sender, BrightIdeasSoftware.CellEditEventArgs e)
		{
			TextBox txt = new TextBox();
			txt.Text = e.Value as string;
			txt.Bounds = e.CellBounds;
			txt.SelectAll();
			e.Control = txt;
		}

		private void olvStrings_CellEditFinishing(object sender, BrightIdeasSoftware.CellEditEventArgs e)
		{
			TextBox txt = e.Control as TextBox;
			e.NewValue = txt.Text;
		}

		private void mnuAppend_Click(object sender, EventArgs e)
		{
			TableEntry te = new TableEntry("");
			this.table.Add(te);
			this.olvStrings.BeginUpdate();
			this.olvStrings.AddObject(te);
			this.olvStrings.EndUpdate();
			this.olvStrings.SelectedObject = te;
			this.olvStrings.EnsureVisible(this.olvStrings.SelectedIndex);
		}

		private void mnuDeleteLast_Click(object sender, EventArgs e)
		{
			if (this.table.Count > 0)
			{
				TableEntry te = this.table[this.table.Count - 1];
				this.table.Remove(te);
				this.olvStrings.BeginUpdate();
				this.olvStrings.RemoveObject(te);
				this.olvStrings.EndUpdate();
				if (this.table.Count > 0)
				{
					this.olvStrings.SelectedObject = this.table[this.table.Count - 1];
					this.olvStrings.EnsureVisible(this.olvStrings.SelectedIndex);
				}
			}
		}

		private void mnuExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
