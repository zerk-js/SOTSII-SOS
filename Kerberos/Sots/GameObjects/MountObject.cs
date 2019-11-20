// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.MountObject
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.GameObjects
{
	internal class MountObject : AutoGameObject
	{
		private int _parentID;
		private string _nodeName;

		public int ParentID
		{
			get
			{
				return this._parentID;
			}
			set
			{
				this._parentID = value;
			}
		}

		public string NodeName
		{
			get
			{
				return this._nodeName;
			}
			set
			{
				this._nodeName = value;
			}
		}

		public class WeaponModels
		{
			public WeaponModelPaths WeaponModelPath;
			public WeaponModelPaths SubWeaponModelPath;
			public WeaponModelPaths SecondaryWeaponModelPath;
			public WeaponModelPaths SecondarySubWeaponModelPath;

			public void FillOutModelFilesWithWeapon(
			  LogicalWeapon weapon,
			  Faction faction,
			  IEnumerable<LogicalWeapon> weapons)
			{
				this.WeaponModelPath = LogicalWeapon.GetWeaponModelPaths(weapon, faction);
				LogicalWeapon subWeapon1 = weapon?.GetSubWeapon(weapons);
				LogicalWeapon secondaryWeapon = weapon?.GetSecondaryWeapon(weapons);
				LogicalWeapon subWeapon2 = secondaryWeapon?.GetSubWeapon(weapons);
				this.SubWeaponModelPath = LogicalWeapon.GetWeaponModelPaths(subWeapon1, faction);
				this.SecondaryWeaponModelPath = LogicalWeapon.GetWeaponModelPaths(secondaryWeapon, faction);
				this.SecondarySubWeaponModelPath = LogicalWeapon.GetWeaponModelPaths(subWeapon2, faction);
			}

			public void FillOutModelFilesWithWeapon(
			  LogicalWeapon weapon,
			  Faction faction,
			  string preferredMount,
			  IEnumerable<LogicalWeapon> weapons)
			{
				this.WeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(weapon, faction, preferredMount);
				LogicalWeapon subWeapon1 = weapon?.GetSubWeapon(weapons);
				LogicalWeapon secondaryWeapon = weapon?.GetSecondaryWeapon(weapons);
				LogicalWeapon subWeapon2 = secondaryWeapon?.GetSubWeapon(weapons);
				this.SubWeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(subWeapon1, faction, preferredMount);
				this.SecondaryWeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(secondaryWeapon, faction, preferredMount);
				this.SecondarySubWeaponModelPath = Ship.GetWeaponModelPathsWithFixAssetNameForDLC(subWeapon2, faction, preferredMount);
			}
		}
	}
}
