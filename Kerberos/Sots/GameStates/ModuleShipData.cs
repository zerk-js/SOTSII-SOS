// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ModuleShipData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.ShipFramework;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class ModuleShipData
	{
		public readonly List<ModuleData> Modules = new List<ModuleData>();
		public SectionShipData Section;
		public int ModuleIndex;
		public LogicalModuleMount ModuleMount;
		public static bool DebugAutoAssignModules;
		private ModuleData _selected;

		public ModuleData SelectedModule
		{
			get
			{
				if (ModuleShipData.DebugAutoAssignModules && this._selected == null)
					return this.Modules.FirstOrDefault<ModuleData>();
				return this._selected;
			}
			set
			{
				this._selected = value;
			}
		}
	}
}
