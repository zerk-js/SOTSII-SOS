// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.Console
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Kerberos.Sots.Console
{
	public class Console : Form
	{
		private readonly Kerberos.Sots.Console.Console.ConsoleProfile[] _profiles;
		private readonly Timer _postTextTimer;
		private readonly StringBuilder _textAdded;
		private readonly List<Kerberos.Sots.Console.Console.TextRun> _textRuns;
		private readonly Dictionary<string, Kerberos.Sots.Console.Console.CategoryInfo> _categoryInfos;
		private readonly Dictionary<Kerberos.Sots.Engine.LogLevel, ToolStripMenuItem> _logLevelItems;
		private int MaxSize;
		private IContainer components;
		private ShellControl shellControl1;
		private RichTextBox richTextBox1;
		private TableLayoutPanel tableLayoutPanel1;
		private MenuStrip menuStrip1;
		private ToolStripMenuItem logToolStripMenuItem;
		private ListView listView1;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ToolStripMenuItem verbosityToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem openLiveLogFileToolStripMenuItem;
		private ToolStripMenuItem locateLiveLogFileToolStripMenuItem;
		private TextBox logPathTextBox;
		private ToolStripMenuItem profilesToolStripMenuItem;

		public event EventCommandEntered CommandEntered
		{
			add
			{
				this.shellControl1.CommandEntered += value;
			}
			remove
			{
				this.shellControl1.CommandEntered -= value;
			}
		}

		public Console()
		{
			this._profiles = new Kerberos.Sots.Console.Console.ConsoleProfile[5]
			{
		new Kerberos.Sots.Console.Console.ConsoleProfile()
		{
		  Name = "AI Debugging",
		  Categories = new List<string>()
		  {
			"ai",
			"con",
			"log"
		  },
		  LogLevel = Kerberos.Sots.Engine.LogLevel.Normal
		},
		new Kerberos.Sots.Console.Console.ConsoleProfile()
		{
		  Name = "Ship Design Debugging",
		  Categories = new List<string>()
		  {
			"ai",
			"design",
			"con",
			"log"
		  },
		  LogLevel = Kerberos.Sots.Engine.LogLevel.Verbose
		},
		new Kerberos.Sots.Console.Console.ConsoleProfile()
		{
		  Name = "Asset Debugging",
		  Categories = new List<string>()
		  {
			"asset",
			"con",
			"log"
		  },
		  LogLevel = Kerberos.Sots.Engine.LogLevel.Verbose
		},
		new Kerberos.Sots.Console.Console.ConsoleProfile()
		{
		  Name = "Combat Debugging",
		  Categories = new List<string>()
		  {
			"combat",
			"con",
			"log"
		  },
		  LogLevel = Kerberos.Sots.Engine.LogLevel.Verbose
		},
		new Kerberos.Sots.Console.Console.ConsoleProfile()
		{
		  Name = "Game Flow Debugging",
		  Categories = new List<string>()
		  {
			"con",
			"state",
			"game",
			"log"
		  },
		  LogLevel = Kerberos.Sots.Engine.LogLevel.Normal
		}
			};
			this._textAdded = new StringBuilder();
			this._textRuns = new List<Kerberos.Sots.Console.Console.TextRun>();
			this._categoryInfos = new Dictionary<string, Kerberos.Sots.Console.Console.CategoryInfo>();
			this._logLevelItems = new Dictionary<Kerberos.Sots.Engine.LogLevel, ToolStripMenuItem>();
			this.MaxSize = 10000000;
			this.InitializeComponent();
			this._postTextTimer = new Timer();
			this._postTextTimer.Interval = 500;
			this._postTextTimer.Tick += new EventHandler(this.PostTextTimerTick);
			this._postTextTimer.Start();
			RichTextBoxHelpers.InitializeForConsole(this.richTextBox1);
			this.richTextBox1.SelectionHangingIndent = 72;
			int num = 1;
			foreach (Kerberos.Sots.Engine.LogLevel index in (Kerberos.Sots.Engine.LogLevel[])Enum.GetValues(typeof(Kerberos.Sots.Engine.LogLevel)))
			{
				this._logLevelItems[index] = new ToolStripMenuItem(string.Format("(&{0}) {1}", (object)num, (object)index), (Image)null, new EventHandler(this.OnMenuClick));
				++num;
			}
			this.verbosityToolStripMenuItem.DropDownItems.AddRange((ToolStripItem[])this._logLevelItems.Values.ToArray<ToolStripMenuItem>());
			this.SynchronizeWithLogLevel();
			foreach (Kerberos.Sots.Console.Console.ConsoleProfile profile in this._profiles)
			{
				profile.MenuItem = new ToolStripMenuItem(profile.Name, (Image)null, new EventHandler(this.OnMenuClick));
				this.profilesToolStripMenuItem.DropDownItems.Add((ToolStripItem)profile.MenuItem);
			}
			this.logPathTextBox.Text = App.Log.FilePath;
			foreach (FieldInfo field in typeof(LogCategories).GetFields())
				this.RegisterCategory(field.GetRawConstantValue() as string);
			this.ApplyProfile(this._profiles[0]);
		}

		private void OnMenuClick(object sender, EventArgs eventArgs)
		{
			if (sender == this.openLiveLogFileToolStripMenuItem)
				ShellHelper.ShellOpen(App.Log.FilePath);
			else if (sender == this.locateLiveLogFileToolStripMenuItem)
			{
				ShellHelper.ShellExplore(App.Log.FilePath);
			}
			else
			{
				foreach (KeyValuePair<Kerberos.Sots.Engine.LogLevel, ToolStripMenuItem> logLevelItem in this._logLevelItems)
				{
					if (sender == logLevelItem.Value)
					{
						App.Log.Level = logLevelItem.Key;
						this.SynchronizeWithLogLevel();
						return;
					}
				}
				foreach (Kerberos.Sots.Console.Console.ConsoleProfile profile in this._profiles)
				{
					if (sender == profile.MenuItem)
					{
						this.ApplyProfile(profile);
						break;
					}
				}
			}
		}

		private void ApplyProfile(Kerberos.Sots.Console.Console.ConsoleProfile profile)
		{
			foreach (string key in this._categoryInfos.Keys)
			{
				bool flag = profile.Categories.Contains(key);
				this.SetCategoryEnabled(key, flag);
			}
			App.Log.Level = profile.LogLevel;
		}

		private void SynchronizeWithLogLevel()
		{
			Kerberos.Sots.Engine.LogLevel level = App.Log.Level;
			foreach (KeyValuePair<Kerberos.Sots.Engine.LogLevel, ToolStripMenuItem> logLevelItem in this._logLevelItems)
				logLevelItem.Value.Checked = logLevelItem.Key == level;
		}

		private Kerberos.Sots.Console.Console.CategoryInfo RegisterCategory(string category)
		{
			Kerberos.Sots.Console.Console.CategoryInfo categoryInfo = new Kerberos.Sots.Console.Console.CategoryInfo()
			{
				Name = category,
				ListViewItem = new ListViewItem()
			};
			categoryInfo.ListViewItem.Text = categoryInfo.Name;
			categoryInfo.ListViewItem.SubItems.Add(new ListViewItem.ListViewSubItem(categoryInfo.ListViewItem, string.Empty));
			categoryInfo.ListViewItem.Tag = (object)categoryInfo;
			categoryInfo.ListViewItem.Checked = true;
			this.listView1.Items.Add(categoryInfo.ListViewItem);
			this._categoryInfos.Add(categoryInfo.Name, categoryInfo);
			return categoryInfo;
		}

		private void HitCategory(string category)
		{
			Kerberos.Sots.Console.Console.CategoryInfo categoryInfo;
			if (!this._categoryInfos.TryGetValue(category, out categoryInfo))
				categoryInfo = this.RegisterCategory(category);
			++categoryInfo.HitCount;
			categoryInfo.ListViewItem.SubItems[1].Text = categoryInfo.HitCount.ToString("N0");
		}

		private void SetCategoryEnabled(string category, bool value)
		{
			Kerberos.Sots.Console.Console.CategoryInfo categoryInfo;
			if (string.IsNullOrEmpty(category) || !this._categoryInfos.TryGetValue(category, out categoryInfo))
				return;
			categoryInfo.ListViewItem.Checked = value;
		}

		private bool IsCategoryEnabled(string category)
		{
			Kerberos.Sots.Console.Console.CategoryInfo categoryInfo;
			if (string.IsNullOrEmpty(category) || !this._categoryInfos.TryGetValue(category, out categoryInfo))
				return true;
			return categoryInfo.IsEnabled;
		}

		private void PostTextRuns(string textAdded, List<Kerberos.Sots.Console.Console.TextRun> textRuns)
		{
			if (this.richTextBox1.InvokeRequired)
			{
				this.richTextBox1.Invoke((Delegate)new Kerberos.Sots.Console.Console.PostTextRunsDelegate(this.PostTextRuns), (object)textAdded, (object)textRuns);
			}
			else
			{
				if (this.richTextBox1.TextLength > this.MaxSize)
					this.richTextBox1.Clear();
				bool flag = false;
				foreach (Kerberos.Sots.Console.Console.TextRun textRun in textRuns)
				{
					if (this.IsCategoryEnabled(textRun.Category))
					{
						if (!flag)
							this.richTextBox1.Select(this.richTextBox1.TextLength, 0);
						this.richTextBox1.SelectionColor = textRun.Color;
						this.richTextBox1.AppendText(textAdded.Substring(textRun.Start, textRun.Length));
					}
				}
				if (!flag)
					return;
				this.richTextBox1.ScrollToCaret();
			}
		}

		private void PostTextTimerTick(object sender, EventArgs e)
		{
			lock (this._textAdded)
			{
				if (this._textAdded.Length <= 0)
					return;
				foreach (Kerberos.Sots.Console.Console.TextRun textRun in this._textRuns)
				{
					if (textRun.RegisterHit && !string.IsNullOrEmpty(textRun.Category))
						this.HitCategory(textRun.Category);
				}
				this.PostTextRuns(this._textAdded.ToString(), new List<Kerberos.Sots.Console.Console.TextRun>((IEnumerable<Kerberos.Sots.Console.Console.TextRun>)this._textRuns));
				this._textRuns.Clear();
				this._textAdded.Length = 0;
			}
		}

		public void WriteText(string category, bool registerHit, string s, Color color)
		{
			if (s.Length == 0)
				return;
			lock (this._textAdded)
			{
				this._textRuns.Add(new Kerberos.Sots.Console.Console.TextRun()
				{
					Category = category,
					RegisterHit = registerHit,
					Start = this._textAdded.Length,
					Length = s.Length,
					Color = color
				});
				this._textAdded.Append(s);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
				this.components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Kerberos.Sots.Console.Console));
			this.richTextBox1 = new RichTextBox();
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.shellControl1 = new ShellControl();
			this.menuStrip1 = new MenuStrip();
			this.logToolStripMenuItem = new ToolStripMenuItem();
			this.verbosityToolStripMenuItem = new ToolStripMenuItem();
			this.toolStripSeparator1 = new ToolStripSeparator();
			this.openLiveLogFileToolStripMenuItem = new ToolStripMenuItem();
			this.locateLiveLogFileToolStripMenuItem = new ToolStripMenuItem();
			this.listView1 = new ListView();
			this.columnHeader1 = new ColumnHeader();
			this.columnHeader2 = new ColumnHeader();
			this.logPathTextBox = new TextBox();
			this.profilesToolStripMenuItem = new ToolStripMenuItem();
			this.tableLayoutPanel1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			this.richTextBox1.BackColor = Color.FromArgb(0, 0, 64);
			this.richTextBox1.Dock = DockStyle.Fill;
			this.richTextBox1.Font = new System.Drawing.Font("Lucida Console", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
			this.richTextBox1.ForeColor = Color.LightGray;
			this.richTextBox1.Location = new Point(113, 29);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.ReadOnly = true;
			this.richTextBox1.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
			this.richTextBox1.Size = new System.Drawing.Size(843, 318);
			this.richTextBox1.TabIndex = 1;
			this.richTextBox1.Text = "";
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.Controls.Add((Control)this.richTextBox1, 1, 1);
			this.tableLayoutPanel1.Controls.Add((Control)this.shellControl1, 1, 2);
			this.tableLayoutPanel1.Controls.Add((Control)this.menuStrip1, 0, 0);
			this.tableLayoutPanel1.Controls.Add((Control)this.listView1, 0, 1);
			this.tableLayoutPanel1.Controls.Add((Control)this.logPathTextBox, 1, 0);
			this.tableLayoutPanel1.Dock = DockStyle.Fill;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 100f));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(959, 450);
			this.tableLayoutPanel1.TabIndex = 2;
			this.shellControl1.Dock = DockStyle.Fill;
			this.shellControl1.Location = new Point(113, 353);
			this.shellControl1.Name = "shellControl1";
			this.shellControl1.Prompt = "> ";
			this.shellControl1.ShellTextBackColor = Color.Black;
			this.shellControl1.ShellTextFont = new System.Drawing.Font("Lucida Console", 10f, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
			this.shellControl1.ShellTextForeColor = Color.LightGray;
			this.shellControl1.Size = new System.Drawing.Size(843, 94);
			this.shellControl1.TabIndex = 0;
			this.menuStrip1.Items.AddRange(new ToolStripItem[1]
			{
		(ToolStripItem) this.logToolStripMenuItem
			});
			this.menuStrip1.Location = new Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(110, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			this.logToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[5]
			{
		(ToolStripItem) this.verbosityToolStripMenuItem,
		(ToolStripItem) this.profilesToolStripMenuItem,
		(ToolStripItem) this.toolStripSeparator1,
		(ToolStripItem) this.openLiveLogFileToolStripMenuItem,
		(ToolStripItem) this.locateLiveLogFileToolStripMenuItem
			});
			this.logToolStripMenuItem.Name = "logToolStripMenuItem";
			this.logToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.logToolStripMenuItem.Text = "&Log";
			this.verbosityToolStripMenuItem.Name = "verbosityToolStripMenuItem";
			this.verbosityToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.verbosityToolStripMenuItem.Text = "&Verbosity";
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
			this.openLiveLogFileToolStripMenuItem.Name = "openLiveLogFileToolStripMenuItem";
			this.openLiveLogFileToolStripMenuItem.ShortcutKeys = Keys.O | Keys.Control;
			this.openLiveLogFileToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.openLiveLogFileToolStripMenuItem.Text = "&Open Live Log File";
			this.openLiveLogFileToolStripMenuItem.Click += new EventHandler(this.OnMenuClick);
			this.locateLiveLogFileToolStripMenuItem.Name = "locateLiveLogFileToolStripMenuItem";
			this.locateLiveLogFileToolStripMenuItem.ShortcutKeys = Keys.L | Keys.Control;
			this.locateLiveLogFileToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.locateLiveLogFileToolStripMenuItem.Text = "&Locate Live Log File";
			this.locateLiveLogFileToolStripMenuItem.Click += new EventHandler(this.OnMenuClick);
			this.listView1.BackColor = Color.FromArgb(0, 0, 64);
			this.listView1.CheckBoxes = true;
			this.listView1.Columns.AddRange(new ColumnHeader[2]
			{
		this.columnHeader1,
		this.columnHeader2
			});
			this.listView1.Dock = DockStyle.Fill;
			this.listView1.ForeColor = Color.Orchid;
			this.listView1.HeaderStyle = ColumnHeaderStyle.None;
			this.listView1.Location = new Point(3, 29);
			this.listView1.Name = "listView1";
			this.tableLayoutPanel1.SetRowSpan((Control)this.listView1, 2);
			this.listView1.Size = new System.Drawing.Size(104, 418);
			this.listView1.Sorting = SortOrder.Ascending;
			this.listView1.TabIndex = 3;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = View.Details;
			this.columnHeader1.Text = "Category";
			this.columnHeader2.Text = "Count";
			this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader2.Width = 40;
			this.logPathTextBox.Dock = DockStyle.Fill;
			this.logPathTextBox.Location = new Point(113, 3);
			this.logPathTextBox.Name = "logPathTextBox";
			this.logPathTextBox.ReadOnly = true;
			this.logPathTextBox.Size = new System.Drawing.Size(843, 20);
			this.logPathTextBox.TabIndex = 4;
			this.profilesToolStripMenuItem.Name = "profilesToolStripMenuItem";
			this.profilesToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
			this.profilesToolStripMenuItem.Text = "&Profiles";
			this.AutoScaleDimensions = new SizeF(6f, 13f);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(959, 450);
			this.Controls.Add((Control)this.tableLayoutPanel1);
			this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
			this.MainMenuStrip = this.menuStrip1;
			this.Name = nameof(Console);
			this.ShowIcon = false;
			this.Text = "Konsole";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
		}

		private struct TextRun
		{
			public string Category;
			public bool RegisterHit;
			public int Start;
			public int Length;
			public Color Color;
		}

		private class CategoryInfo
		{
			public string Name;
			public int HitCount;
			public ListViewItem ListViewItem;

			public bool IsEnabled
			{
				get
				{
					return this.ListViewItem.Checked;
				}
			}
		}

		private class ConsoleProfile
		{
			public string Name;
			public List<string> Categories;
			public Kerberos.Sots.Engine.LogLevel LogLevel;
			public ToolStripMenuItem MenuItem;
		}

		private delegate void PostTextRunsDelegate(string textAdded, List<Kerberos.Sots.Console.Console.TextRun> textRuns);
	}
}
