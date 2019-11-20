// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestAssetsState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	internal class TestAssetsState : GameState
	{
		private Random _rand = new Random();
		private const float DistFromCenter = 0.0f;
		private GameObjectSet _crits;
		private CombatInput _combatInput;
		private CombatGrid _combatGrid;
		private Sky _sky;
		private OrbitCameraController _camera;
		private Ship _ship;
		private Ship _ship2;

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this.App.NewGame();
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.InSystem, 0);
			this._camera = this._crits.Add<OrbitCameraController>();
			this._camera.TargetPosition = new Vector3(0.0f, 0.0f, 0.0f);
			this._combatInput = this._crits.Add<CombatInput>();
			this._combatGrid = this._crits.Add<CombatGrid>();
			CreateShipParams createShipParams = new CreateShipParams();
			createShipParams.player = this.App.LocalPlayer;
			createShipParams.sections = this.App.AssetDatabase.ShipSections.Where<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x =>
		   {
			   if (!(x.Faction == "morrigi") || x.Class != ShipClass.Cruiser)
				   return false;
			   if (!x.FileName.Contains("cr_cmd.section") && !x.FileName.Contains("cr_eng_fusion.section"))
				   return x.FileName.Contains("cr_mis_armor.section");
			   return true;
		   }));
			createShipParams.turretHousings = this.App.AssetDatabase.TurretHousings;
			createShipParams.weapons = this.App.AssetDatabase.Weapons;
			createShipParams.preferredWeapons = this.App.AssetDatabase.Weapons;
			createShipParams.modules = this.App.AssetDatabase.Modules;
			createShipParams.preferredModules = this.App.AssetDatabase.Modules;
			createShipParams.psionics = this.App.AssetDatabase.Psionics;
			createShipParams.faction = this.App.AssetDatabase.GetFaction("morrigi");
			createShipParams.shipName = "BOOGER";
			createShipParams.inputID = this._combatInput.ObjectID;
			this._ship = Ship.CreateShip(this.App, createShipParams);
			this._crits.Add((IGameObject)this._ship);
			this._ship2 = Ship.CreateShip(this.App, createShipParams);
			this._ship2.Position = new Vector3(1000f, 0.0f, 0.0f);
			this._crits.Add((IGameObject)this._ship2);
		}

		protected override void OnEnter()
		{
			this.App.UI.SetScreen("Combat");
			this._camera.DesiredDistance = 30f;
			this._combatGrid.GridSize = 5000f;
			this._combatGrid.CellSize = 500f;
			this._combatInput.CameraID = this._camera.ObjectID;
			this._combatInput.CombatGridID = this._combatGrid.ObjectID;
			this._crits.Activate();
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._crits == null)
				return;
			this._crits.Dispose();
		}

		protected override void OnUpdate()
		{
		}

		public override bool IsReady()
		{
			if (this._crits != null)
				return this._crits.IsReady();
			return false;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public TestAssetsState(App game)
		  : base(game)
		{
		}
	}
}
