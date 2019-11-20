// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapPreview
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.Strategy;
using System;

namespace Kerberos.Sots.StarMapElements
{
	internal class StarMapPreview : IDisposable
	{
		private GameSession sim;
		private Sky sky;
		private OrbitCameraController camera;
		private StarMap starmap;
		private GameObjectSet crits;
		private bool active;

		public StarMap StarMap
		{
			get
			{
				return this.starmap;
			}
		}

		public StarMapPreview(App app, GameSetup setup)
		{
			this.crits = new GameObjectSet(app);
			GameDatabase.New(setup.StarMapFile, app.AssetDatabase, true);
			this.sim = App.NewGame(app, new Random(), setup, app.AssetDatabase, setup, GameSession.Flags.NoNewGameMessage | GameSession.Flags.NoTechTree | GameSession.Flags.NoScriptModules | GameSession.Flags.NoDefaultFleets | GameSession.Flags.NoOrbitalObjects | GameSession.Flags.NoGameSetup);
			this.sky = new Sky(app, SkyUsage.StarMap, new Random().Next());
			this.crits.Add((IGameObject)this.sky);
			this.camera = new OrbitCameraController(app);
			this.camera.MinDistance = 10f;
			this.camera.MaxDistance = 100f;
			this.camera.DesiredDistance = 70f;
			this.camera.DesiredYaw = MathHelper.DegreesToRadians(45f);
			this.camera.DesiredPitch = MathHelper.DegreesToRadians(-45f);
			this.starmap = new StarMap(app, this.sim, (Sky)null);
			this.starmap.SelectEnabled = false;
			this.starmap.SetCamera(this.camera);
			this.starmap.Initialize(this.crits);
			this.starmap.ViewFilter = StarMapViewFilter.VF_TERRAIN;
			this.crits.Add((IGameObject)this.starmap);
		}

		public void Dispose()
		{
			this.starmap.Dispose();
			if (this.crits != null)
			{
				this.crits.Dispose();
				this.crits = (GameObjectSet)null;
			}
			if (this.camera != null)
			{
				this.camera.Dispose();
				this.camera = (OrbitCameraController)null;
			}
			if (this.sim == null)
				return;
			this.sim.Dispose();
			this.sim = (GameSession)null;
		}

		public void Update()
		{
			if (this.active || this.crits == null || !this.crits.IsReady())
				return;
			this.active = true;
			this.crits.Activate();
			this.starmap.Sync(this.crits);
		}
	}
}
