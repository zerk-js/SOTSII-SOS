// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TechCube
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using System;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_TECHCUBE)]
	internal class TechCube : GameObject, IActive, IDisposable
	{
		private string _familyTexture = "";
		private string _techTexture = "";
		private App App;
		private bool _active;
		private float _spinSpeed;

		public string FamilyTexture
		{
			get
			{
				return this._familyTexture;
			}
			set
			{
				if (!(this._familyTexture != value))
					return;
				this._familyTexture = value;
				this.PostSetProp("SetFamilyTexture", value);
			}
		}

		public string TechTexture
		{
			get
			{
				return this._techTexture;
			}
			set
			{
				if (!(this._techTexture != value))
					return;
				this._techTexture = value;
				this.PostSetProp("SetTechTexture", value);
			}
		}

		public float SpinSpeed
		{
			get
			{
				return this._spinSpeed;
			}
			set
			{
				this._spinSpeed = value;
				this.PostSetProp(nameof(SpinSpeed), value);
			}
		}

		public void UpdateResearchProgress()
		{
			int researchingTechId = this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID);
			if (researchingTechId != 0)
			{
				PlayerTechInfo playerTechInfo = this.App.GameDatabase.GetPlayerTechInfo(this.App.LocalPlayer.ID, researchingTechId);
				this.PostSetProp("AmountResearched", (float)playerTechInfo.Progress / (float)playerTechInfo.ResearchCost);
			}
			else
				this.PostSetProp("AmountResearched", 0.0f);
		}

		public void RefreshResearchingTech()
		{
			string techID = this.App.GameDatabase.GetTechFileID(this.App.GameDatabase.GetPlayerResearchingTechID(this.App.LocalPlayer.ID));
			string str1 = "Tech\\Icons\\question_mark.bmp";
			string str2 = "Tech\\Icons\\question_mark.bmp";
			if (techID != null)
			{
				Tech tech1 = this.App.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(tech => tech.Id == techID));
				if (tech1 != null)
				{
					str1 = tech1.GetProperIconPath();
					str2 = "Tech\\Icons\\Research_Icon.bmp";
				}
			}
			else
			{
				techID = this.App.GameDatabase.GetTechFileID(this.App.GameDatabase.GetPlayerFeasibilityStudyTechId(this.App.LocalPlayer.ID));
				if (techID != null)
				{
					Tech techno = this.App.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(tech => tech.Id == techID));
					if (techno != null)
					{
						this.App.AssetDatabase.MasterTechTree.TechFamilies.First<TechFamily>((Func<TechFamily, bool>)(x => x.Id == techno.Family));
						str1 = techno.GetProperIconPath();
					}
				}
			}
			this.FamilyTexture = str2;
			this.TechTexture = str1;
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				this._active = value;
				this.PostSetActive(true);
				if (!value)
					return;
				this.App.UI.Send((object)"SetGameObject", (object)"researchCubeButton.idle", (object)this.ObjectID);
				this.App.UI.Send((object)"SetGameObject", (object)"researchCubeButton.mouse_over", (object)this.ObjectID);
				this.App.UI.Send((object)"SetGameObject", (object)"researchCubeButton.pressed", (object)this.ObjectID);
			}
		}

		public TechCube(App game)
		{
			game.AddExistingObject((IGameObject)this);
			this.App = game;
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
