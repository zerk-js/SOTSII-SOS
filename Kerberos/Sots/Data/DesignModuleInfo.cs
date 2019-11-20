// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.DesignModuleInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ModuleFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class DesignModuleInfo : IIDProvider
	{
		public List<ModulePsionicInfo> PsionicAbilities = new List<ModulePsionicInfo>();
		public string MountNodeName;
		public int ModuleID;
		public int? WeaponID;
		public int? DesignID;
		public ModuleEnums.StationModuleType? StationModuleType;

		public DesignSectionInfo DesignSectionInfo { get; internal set; }

		public int ID { get; set; }

		public override string ToString()
		{
			return this.ID.ToString() + "," + this.MountNodeName ?? string.Empty + "," + (object)this.StationModuleType ?? string.Empty;
		}
	}
}
