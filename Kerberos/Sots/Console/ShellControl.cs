// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.ShellControl
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kerberos.Sots.Console
{
	public class ShellControl : UserControl
	{
		private ShellTextBox shellTextBox;
		private Container components;

		public event EventCommandEntered CommandEntered;

		public ShellControl()
		{
			this.InitializeComponent();
		}

		internal void FireCommandEntered(string command)
		{
			this.OnCommandEntered(command);
		}

		protected virtual void OnCommandEntered(string command)
		{
			if (this.CommandEntered == null)
				return;
			this.CommandEntered((object)command, new CommandEnteredEventArgs(command));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
				this.components.Dispose();
			base.Dispose(disposing);
		}

		public Color ShellTextForeColor
		{
			get
			{
				if (this.shellTextBox == null)
					return Color.Gray;
				return this.shellTextBox.ForeColor;
			}
			set
			{
				if (this.shellTextBox == null)
					return;
				this.shellTextBox.ForeColor = value;
			}
		}

		public Color ShellTextBackColor
		{
			get
			{
				if (this.shellTextBox == null)
					return Color.Black;
				return this.shellTextBox.BackColor;
			}
			set
			{
				if (this.shellTextBox == null)
					return;
				this.shellTextBox.BackColor = value;
			}
		}

		public Font ShellTextFont
		{
			get
			{
				if (this.shellTextBox == null)
					return new Font("Lucida Console", 8f);
				return this.shellTextBox.Font;
			}
			set
			{
				if (this.shellTextBox == null)
					return;
				this.shellTextBox.Font = value;
			}
		}

		public void Clear()
		{
			this.shellTextBox.Clear();
		}

		public void WriteText(string text)
		{
			this.shellTextBox.WriteText(text);
		}

		public string[] GetCommandHistory()
		{
			return this.shellTextBox.GetCommandHistory();
		}

		public string Prompt
		{
			get
			{
				return this.shellTextBox.Prompt;
			}
			set
			{
				this.shellTextBox.Prompt = value;
			}
		}

		private void InitializeComponent()
		{
			this.shellTextBox = new ShellTextBox();
			this.SuspendLayout();
			this.shellTextBox.AcceptsReturn = true;
			this.shellTextBox.AcceptsTab = true;
			this.shellTextBox.BackColor = Color.Black;
			this.shellTextBox.Dock = DockStyle.Fill;
			this.shellTextBox.ForeColor = Color.LawnGreen;
			this.shellTextBox.Location = new Point(0, 0);
			this.shellTextBox.Multiline = true;
			this.shellTextBox.Name = "shellTextBox";
			this.shellTextBox.Prompt = ">>>";
			this.shellTextBox.ScrollBars = ScrollBars.Both;
			this.shellTextBox.BackColor = Color.Black;
			this.shellTextBox.Font = new Font("Lucida Console", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
			this.shellTextBox.ForeColor = Color.Gray;
			this.shellTextBox.Size = new Size(232, 216);
			this.shellTextBox.TabIndex = 0;
			this.shellTextBox.Text = "";
			this.Controls.Add((Control)this.shellTextBox);
			this.Name = nameof(ShellControl);
			this.Size = new Size(232, 216);
			this.ResumeLayout(false);
		}
	}
}
