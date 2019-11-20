// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapFleet
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.PlayerFramework;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPFLEET)]
	internal class StarMapFleet : StarMapObject
	{
		public bool InTransit;
		public bool IsLoaGate;
		public bool IsVisible;
		public int SystemID;
		public int PlayerID;
		public int FleetID;

		public StarMapFleet(App game, int factionFleetModelIndex)
		{
			game.AddExistingObject((IGameObject)this);
			this.PostSetProp("FleetModel", factionFleetModelIndex);
		}

		public void SetDirection(Vector3 value)
		{
			this.PostSetProp("Direction", value);
		}

		public void SetIsInTransit(bool value)
		{
			this.PostSetProp("InTransit", value);
			this.InTransit = value;
		}

		public void SetIsLoaGate(bool value)
		{
			this.PostSetProp("IsLoaGate", value);
			this.IsLoaGate = value;
		}

		public void SetVisible(bool value)
		{
			this.PostSetProp("Visible", value);
			this.IsVisible = value;
		}

		public void SetSystemID(int systemID)
		{
			this.SystemID = systemID;
		}

		public void SetPlayer(Player value)
		{
			this.PostSetProp("Player", (IGameObject)value);
			this.PlayerID = value.ID;
		}

		public void SetSystemFleetCount(int count)
		{
			this.PostSetProp("SystemFleetCount", count);
		}

		public void SetSystemFleetIndex(int index)
		{
			this.PostSetProp("SystemFleetIndex", index);
		}
	}
}
