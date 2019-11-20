// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.PsionicBankData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal class PsionicBankData : IPsionicShipData
	{
		private readonly List<LogicalPsionic> _psionics = new List<LogicalPsionic>();
		private readonly List<int> _designs = new List<int>();
		public SectionShipData Section;
		public int BankIndex;
		private LogicalPsionic _selectedPsionic;
		private int _selectedDesign;

		public LogicalBank Bank { get; set; }

		public bool RequiresDesign { get; set; }

		public bool DesignIsSelectable { get; set; }

		public List<LogicalPsionic> Psionics
		{
			get
			{
				return this._psionics;
			}
		}

		public List<int> Designs
		{
			get
			{
				return this._designs;
			}
		}

		public LogicalPsionic SelectedPsionic
		{
			get
			{
				LogicalPsionic selectedPsionic = this._selectedPsionic;
				if (selectedPsionic != null)
					return selectedPsionic;
				if (this.Psionics.Count <= 0)
					return (LogicalPsionic)null;
				return this.Psionics[0];
			}
			set
			{
				this._selectedPsionic = value;
			}
		}

		public int SelectedDesign
		{
			get
			{
				if (this._selectedDesign != 0 || this.Designs.Count <= 0)
					return this._selectedDesign;
				return this.Designs[0];
			}
			set
			{
				this._selectedDesign = value;
			}
		}

		public int? FiringMode { get; set; }

		public int? FilterMode { get; set; }
	}
}
