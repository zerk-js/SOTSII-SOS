// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DesignSectionInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Data
{
	internal class DesignSectionInfo : IIDProvider
	{
		public string FilePath;
		public ShipSectionAsset ShipSectionAsset;
		public List<WeaponBankInfo> WeaponBanks;
		public List<DesignModuleInfo> Modules;
		public List<int> Techs;

		public DesignInfo DesignInfo { get; set; }

		public int ID { get; set; }

		public int GetMinStructure(GameDatabase db, AssetDatabase ab)
		{
			int num = 0;
			if (this.Modules != null)
			{
				foreach (DesignModuleInfo module in this.Modules)
				{
					string mPath = db.GetModuleAsset(module.ModuleID);
					LogicalModule logicalModule = ab.Modules.FirstOrDefault<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == mPath));
					if (logicalModule != null)
						num += (int)logicalModule.StructureBonus;
				}
				num = -num;
			}
			return num;
		}
	}
}
