// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.ShellTextBox
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Kerberos.Sots.Console
{
	internal class ShellTextBox : TextBox
	{
		private string prompt = "> ";
		private CommandHistory commandHistory = new CommandHistory();
		private Container components;

		internal ShellTextBox()
		{
			this.InitializeComponent();
			this.printPrompt();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
				this.components.Dispose();
			base.Dispose(disposing);
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case 12:
				case 768:
				case 770:
					if (!this.IsCaretAtWritablePosition())
					{
						this.MoveCaretToEndOfText();
						break;
					}
					break;
				case 771:
					return;
			}
			base.WndProc(ref m);
		}

		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.BackColor = Color.Black;
			this.Dock = DockStyle.Fill;
			this.ForeColor = Color.Gray;
			this.Location = new Point(0, 0);
			this.MaxLength = 0;
			this.Multiline = true;
			this.Name = "shellTextBox";
			this.AcceptsTab = true;
			this.AcceptsReturn = true;
			this.ScrollBars = ScrollBars.Both;
			this.Size = new Size(400, 176);
			this.TabIndex = 0;
			this.Text = "";
			this.KeyPress += new KeyPressEventHandler(this.shellTextBox_KeyPress);
			this.KeyDown += new KeyEventHandler(this.ShellControl_KeyDown);
			this.Name = nameof(ShellTextBox);
			this.Size = new Size(400, 176);
			this.ResumeLayout(false);
		}

		private void printPrompt()
		{
			string text = this.Text;
			if (text.Length != 0 && text[text.Length - 1] != '\n')
				this.printLine();
			this.AddText(this.prompt);
		}

		private void printLine()
		{
			this.AddText(Environment.NewLine);
		}

		private void shellTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\b' && this.IsCaretJustBeforePrompt())
			{
				e.Handled = true;
			}
			else
			{
				if (!this.IsTerminatorKey(e.KeyChar))
					return;
				e.Handled = true;
				string textAtPrompt = this.GetTextAtPrompt();
				if (textAtPrompt.Length != 0)
				{
					this.printLine();
					((ShellControl)this.Parent).FireCommandEntered(textAtPrompt);
					this.commandHistory.Add(textAtPrompt);
				}
				this.printPrompt();
			}
		}

		private void ShellControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (!this.IsCaretAtWritablePosition() && !e.Control && !this.IsTerminatorKey(e.KeyCode))
				this.MoveCaretToEndOfText();
			if (e.KeyCode == Keys.Left && this.IsCaretJustBeforePrompt())
				e.Handled = true;
			else if (e.KeyCode == Keys.Down)
			{
				if (this.commandHistory.DoesNextCommandExist())
					this.ReplaceTextAtPrompt(this.commandHistory.GetNextCommand());
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (this.commandHistory.DoesPreviousCommandExist())
					this.ReplaceTextAtPrompt(this.commandHistory.GetPreviousCommand());
				e.Handled = true;
			}
			else
			{
				if (e.KeyCode != Keys.Right)
					return;
				string textAtPrompt = this.GetTextAtPrompt();
				string lastCommand = this.commandHistory.LastCommand;
				if (lastCommand == null || textAtPrompt.Length != 0 && !lastCommand.StartsWith(textAtPrompt) || lastCommand.Length <= textAtPrompt.Length)
					return;
				this.AddText(lastCommand[textAtPrompt.Length].ToString());
			}
		}

		private string GetCurrentLine()
		{
			if (this.Lines.Length > 0)
				return this.Lines.GetValue(this.Lines.GetLength(0) - 1) as string;
			return "";
		}

		private string GetTextAtPrompt()
		{
			string currentLine = this.GetCurrentLine();
			if (currentLine.Length < this.prompt.Length)
				return string.Empty;
			return currentLine.Substring(this.prompt.Length);
		}

		private void ReplaceTextAtPrompt(string text)
		{
			int length = this.GetCurrentLine().Length - this.prompt.Length;
			if (length == 0)
			{
				this.AddText(text);
			}
			else
			{
				this.Select(this.TextLength - length, length);
				this.SelectedText = text;
			}
		}

		private bool IsCaretAtCurrentLine()
		{
			return this.TextLength - this.SelectionStart <= this.GetCurrentLine().Length;
		}

		private void MoveCaretToEndOfText()
		{
			this.SelectionStart = this.TextLength;
			this.ScrollToCaret();
		}

		private bool IsCaretJustBeforePrompt()
		{
			if (this.IsCaretAtCurrentLine())
				return this.GetCurrentCaretColumnPosition() == this.prompt.Length;
			return false;
		}

		private int GetCurrentCaretColumnPosition()
		{
			return this.SelectionStart - this.TextLength + this.GetCurrentLine().Length;
		}

		private bool IsCaretAtWritablePosition()
		{
			if (this.IsCaretAtCurrentLine())
				return this.GetCurrentCaretColumnPosition() >= this.prompt.Length;
			return false;
		}

		private void SetPromptText(string val)
		{
			this.GetCurrentLine();
			this.Select(0, this.prompt.Length);
			this.SelectedText = val;
			this.prompt = val;
		}

		public string Prompt
		{
			get
			{
				return this.prompt;
			}
			set
			{
				this.SetPromptText(value);
			}
		}

		public string[] GetCommandHistory()
		{
			return this.commandHistory.GetCommandHistory();
		}

		public void WriteText(string text)
		{
			this.AddText(text);
		}

		private bool IsTerminatorKey(Keys key)
		{
			return key == Keys.Return;
		}

		private bool IsTerminatorKey(char keyChar)
		{
			return keyChar == '\r';
		}

		private void AddText(string text)
		{
			this.Text += text;
			this.MoveCaretToEndOfText();
		}
	}
}
