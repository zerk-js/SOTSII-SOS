// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.WeaponBankShipData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameStates
{
	internal class WeaponBankShipData : IWeaponShipData
	{
		private readonly List<LogicalWeapon> _weapons = new List<LogicalWeapon>();
		private readonly List<int> _designs = new List<int>();
		public SectionShipData Section;
		public int BankIndex;
		private LogicalWeapon _selectedWeapon;
		private int _selectedDesign;

		public LogicalBank Bank { get; set; }

		public bool RequiresDesign { get; set; }

		public bool DesignIsSelectable { get; set; }

		public List<LogicalWeapon> Weapons
		{
			get
			{
				return this._weapons;
			}
		}

		public List<int> Designs
		{
			get
			{
				return this._designs;
			}
		}

		public LogicalWeapon SelectedWeapon
		{
			get
			{
				LogicalWeapon selectedWeapon = this._selectedWeapon;
				if (selectedWeapon != null)
					return selectedWeapon;
				if (this.Weapons.Count <= 0)
					return (LogicalWeapon)null;
				return this.Weapons[0];
			}
			set
			{
				this._selectedWeapon = value;
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
