// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarSystemPlacementSlot
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_STARSYSTEMPLACEMENTSLOT)]
	internal class StarSystemPlacementSlot : GameObject, IDisposable, IActive
	{
		public SlotData _slotData = new SlotData();
		public const string StationSlotType = "station";
		public const string SystemDefenceBoatSlotType = "sdb";
		public const string AsteroidMineSlotType = "astmine";
		private bool _active;

		public Vector3 Position
		{
			get
			{
				return this._slotData.Position;
			}
		}

		public StarSystemPlacementSlot(App game, SlotData slotData)
		{
			this._slotData = slotData;
			game.AddExistingObject((IGameObject)this, (object)this._slotData.OccupantID, (object)this._slotData.Parent, (object)this._slotData.ParentDBID, (object)(int)this._slotData.SupportedTypes);
		}

		public void Dispose()
		{
			this.App.ReleaseObject((IGameObject)this);
		}

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active == value)
					return;
				this.PostSetActive(value);
				this._active = value;
			}
		}

		public void SetOccupant(IGameObject value)
		{
			this._slotData.OccupantID = value != null ? value.ObjectID : 0;
			this.PostSetProp("Occupant", this._slotData.OccupantID);
		}

		public int GetOccupantID()
		{
			return this._slotData.OccupantID;
		}

		public void SetTransform(Vector3 position, float rotation)
		{
			this._slotData.Position = position;
			this._slotData.Rotation = rotation;
			this.PostSetProp("Transform", (object)position, (object)rotation);
		}
	}
}
