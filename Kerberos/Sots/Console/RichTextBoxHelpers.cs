// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Console.RichTextBoxHelpers
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Kerberos.Sots.Console
{
	internal static class RichTextBoxHelpers
	{
		private const int PFM_SPACEBEFORE = 64;
		private const int PFM_SPACEAFTER = 128;
		private const int PFM_LINESPACING = 256;
		private const int EM_SETPARAFORMAT = 1095;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SendMessage(
		  IntPtr hWnd,
		  int msg,
		  int wParam,
		  ref RichTextBoxHelpers.PARAFORMAT2 lParam);

		public static void InitializeForConsole(RichTextBox richTextBox)
		{
			RichTextBoxHelpers.PARAFORMAT2 lParam = new RichTextBoxHelpers.PARAFORMAT2();
			lParam.cbSize = Marshal.SizeOf((object)lParam);
			lParam.dwMask |= 192U;
			lParam.dySpaceBefore = 28;
			lParam.dySpaceAfter = 28;
			RichTextBoxHelpers.SendMessage(richTextBox.Handle, 1095, 0, ref lParam);
		}

		public struct PARAFORMAT2
		{
			public int cbSize;
			public uint dwMask;
			public short wNumbering;
			public short wReserved;
			public int dxStartIndent;
			public int dxRightIndent;
			public int dxOffset;
			public short wAlignment;
			public short cTabCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public int[] rgxTabs;
			public int dySpaceBefore;
			public int dySpaceAfter;
			public int dyLineSpacing;
			public short sStyle;
			public byte bLineSpacingRule;
			public byte bOutlineLevel;
			public short wShadingWeight;
			public short wShadingStyle;
			public short wNumberingStart;
			public short wNumberingStyle;
			public short wNumberingTab;
			public short wBorderSpace;
			public short wBorderWidth;
			public short wBorders;
		}
	}
}
