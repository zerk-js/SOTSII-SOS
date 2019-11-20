// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PsionicInfoPanel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;

namespace Kerberos.Sots.UI
{
	internal class PsionicInfoPanel : PanelBinding
	{
		private const string UIModuleIcon = "moduleIcon";
		private const string UIModuleTitle = "moduleTitle";
		private const string UIModuleAbility = "moduleAbility";
		private const string UISupplyAttribute = "supplyAttribute";
		private const string UIPowerAttribute = "powerAttribute";
		private const string UICrewAttribute = "crewAttribute";
		private const string UIStructureValue = "structureValue";
		private const string UICost = "costDisplay.costValue";
		private string _contentPanelID;

		public PsionicInfoPanel(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._contentPanelID = this.UI.Path(id, "content");
		}

		public void SetPsionic(LogicalPsionic primary)
		{
			if (primary == null)
				return;
			this.UI.SetPropertyString("moduleIcon", "sprite", primary.Icon);
			this.UI.SetVisible("moduleIcon", true);
			this.UI.SetText("moduleTitle", primary.PsionicTitle ?? string.Empty);
			this.UI.SetText("moduleAbility", primary.Description ?? string.Empty);
		}
	}
}
