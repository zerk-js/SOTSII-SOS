// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ShipHoloView
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.ShipFramework;
using System;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_SHIPHOLOVIEW)]
	internal class ShipHoloView : GameObject, IDisposable
	{
		public ShipHoloView(App game, OrbitCameraController cameraController)
		{
			game.AddExistingObject((IGameObject)this, (object)cameraController.GetObjectID());
		}

		public void SetUseViewport(bool value)
		{
			this.PostSetProp("UseViewport", value);
		}

		public void HideViewport(bool value)
		{
			this.PostSetProp(nameof(HideViewport), value);
		}

		public void SetShip(Ship value)
		{
			this.PostSetProp("Ship", value.GetObjectID());
		}

		public void AddWeaponGroupIcon(WeaponBank weaponBank)
		{
			this.PostSetProp(nameof(AddWeaponGroupIcon), weaponBank.GetObjectID());
		}

		public void AddModuleIcon(
		  Module selectedModule,
		  Section defaultShipSection,
		  string defaultModelNodeName,
		  string iconSpriteName)
		{
			this.PostSetProp(nameof(AddModuleIcon), (object)selectedModule.GetObjectID(), (object)defaultShipSection.GetObjectID(), (object)defaultModelNodeName, (object)iconSpriteName);
		}

		public void AddPsionicIcon(Module selectedModule, int psionicid, int elementid)
		{
			this.PostSetProp(nameof(AddPsionicIcon), (object)selectedModule.GetObjectID(), (object)psionicid, (object)elementid);
		}

		public void ClearSelection()
		{
			this.PostSetProp(nameof(ClearSelection));
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
