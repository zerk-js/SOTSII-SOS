// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ResearchInfoPanel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kerberos.Sots.UI
{
	internal class ResearchInfoPanel : PanelBinding
	{
		private const string UITechIcon = "techicon";
		private const string UIFamilyIcon = "familyicon";
		private const string UITechTitle = "tech_title";
		private const string UITechDescription = "tech_desc";
		private const string weaponPanel = "TechWeaponDetails";
		private string _contentPanelID;
		private int _techID;
		private WeaponInfoPanel _weaponinfopanel;

		public ResearchInfoPanel(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._contentPanelID = this.UI.Path(id, "content");
			this._weaponinfopanel = new WeaponInfoPanel(ui, "TechWeaponDetails");
		}

		private static string IconTextureToSpriteName(string texture)
		{
			return Path.GetFileNameWithoutExtension(texture);
		}

		private LogicalWeapon GetWeaponUnlockedByTech(App app, Kerberos.Sots.Data.TechnologyFramework.Tech tech)
		{
			return app.AssetDatabase.Weapons.FirstOrDefault<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => ((IEnumerable<Kerberos.Sots.Data.WeaponFramework.Tech>)x.RequiredTechs).Any<Kerberos.Sots.Data.WeaponFramework.Tech>((Func<Kerberos.Sots.Data.WeaponFramework.Tech, bool>)(y => y.Name == tech.Id))));
		}

		public void SetTech(App app, int TechID)
		{
			this._techID = TechID;
			string techid = app.GameDatabase.GetTechFileID(this._techID);
			int techId = this._techID;
			app.GameDatabase.GetPlayerTechInfo(app.LocalPlayer.ID, techId);
			Kerberos.Sots.Data.TechnologyFramework.Tech tech = app.AssetDatabase.MasterTechTree.Technologies.First<Kerberos.Sots.Data.TechnologyFramework.Tech>((Func<Kerberos.Sots.Data.TechnologyFramework.Tech, bool>)(x => x.Id == techid));
			string spriteName1 = ResearchInfoPanel.IconTextureToSpriteName(tech.Icon);
			string spriteName2 = ResearchInfoPanel.IconTextureToSpriteName(app.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == tech.Family)).Icon);
			app.UI.SetPropertyString("tech_title", "text", App.Localize("@TECH_NAME_" + tech.Id));
			app.UI.SetPropertyString("techicon", "sprite", spriteName1);
			app.UI.SetPropertyString("familyicon", "sprite", spriteName2);
			app.UI.SetText("tech_desc", App.Localize("@TECH_DESC_" + tech.Id));
			LogicalWeapon weaponUnlockedByTech = this.GetWeaponUnlockedByTech(app, tech);
			if (weaponUnlockedByTech != null)
			{
				app.UI.SetVisible("TechWeaponDetails", true);
				this._weaponinfopanel.SetWeapons(weaponUnlockedByTech, (LogicalWeapon)null);
			}
			else
				app.UI.SetVisible("TechWeaponDetails", false);
		}
	}
}
