// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestPhysicsState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using System;

namespace Kerberos.Sots.GameStates
{
	internal class TestPhysicsState : GameState
	{
		private GameObjectSet _crits;
		private OrbitCameraController _camera;
		private CombatInput _input;
		private Sky _sky;
		private CombatGrid _grid;
		private Random _rnd;
		private bool hack1;

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._rnd = new Random();
			this._crits = new GameObjectSet(this.App);
			this._sky = new Sky(this.App, SkyUsage.InSystem, 0);
			this._crits.Add((IGameObject)this._sky);
			this._camera = this._crits.Add<OrbitCameraController>();
			this._input = this._crits.Add<CombatInput>();
			this._grid = this._crits.Add<CombatGrid>();
		}

		protected override void OnEnter()
		{
			this._sky.Active = true;
			this._grid.GridSize = 1000f;
			this._grid.CellSize = 50f;
			this._grid.Active = true;
			this._input.Active = true;
			this._input.CameraID = this._camera.ObjectID;
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			if (this._crits == null)
				return;
			this._crits.Dispose();
		}

		protected override void OnUpdate()
		{
			int num = this.hack1 ? 1 : 0;
			this.hack1 = !this.hack1;
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

		public TestPhysicsState(App game)
		  : base(game)
		{
		}
	}
}
