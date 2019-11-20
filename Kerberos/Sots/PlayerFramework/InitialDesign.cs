// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.InitialDesign
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.PlayerFramework
{
	internal class InitialDesign
	{
		public string WeaponBiasTechFamilyID;
		public string Name;
		public string[] Sections;

		public IEnumerable<ShipSectionAsset> EnumerateShipSectionAssets(
		  AssetDatabase assetdb,
		  Faction faction)
		{
			foreach (string section in this.Sections)
			{
				string name = section;
				yield return assetdb.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
			   {
				   if (x.Faction == faction.Name)
					   return x.SectionName.Equals(name, StringComparison.InvariantCultureIgnoreCase);
				   return false;
			   }));
			}
		}
	}
}
