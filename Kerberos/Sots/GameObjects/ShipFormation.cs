// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.ShipFormation
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	internal class ShipFormation
	{
		private FormationDefinition m_formation;
		private float m_BackLineOffsetDist;
		private List<Ship> m_ships;
		private List<Ship> m_shipsOnBackLine;
		private List<FormationPatternData> m_formationPattern;
		private bool m_destSet;
		private Vector3 m_destination;
		private Vector3 m_facing;
		private bool m_destroyed;
		private bool m_requiresReset;
		private bool m_isVFormation;

		public float BackLineOffsetDist
		{
			get
			{
				return this.m_BackLineOffsetDist;
			}
			set
			{
				this.m_BackLineOffsetDist = value;
			}
		}

		public List<Ship> Ships
		{
			get
			{
				return this.m_ships;
			}
		}

		public List<Ship> ShipsOnBackLine
		{
			get
			{
				return this.m_shipsOnBackLine;
			}
		}

		public bool DestinationSet
		{
			get
			{
				return this.m_destSet;
			}
		}

		public Vector3 Destination
		{
			get
			{
				return this.m_destination;
			}
		}

		public bool HasReceivedAnUpdate
		{
			get
			{
				if (this.m_formation == null)
					return false;
				return this.m_formation.HasReceivedAnUpdate;
			}
		}

		public Vector3 Facing
		{
			get
			{
				return this.m_facing;
			}
		}

		public ShipFormation(App game)
		{
			this.m_facing = -Vector3.UnitZ;
			this.m_destination = Vector3.Zero;
			this.m_ships = new List<Ship>();
			this.m_shipsOnBackLine = new List<Ship>();
			this.m_formationPattern = new List<FormationPatternData>();
			this.m_destSet = false;
			this.m_requiresReset = false;
			this.m_isVFormation = false;
			this.m_formation = game.AddObject<FormationDefinition>();
			this.m_formation.ParentShipFormation = this;
			this.m_destroyed = false;
			this.m_BackLineOffsetDist = 750f;
		}

		public void AddShip(Ship ship)
		{
			if (this.m_ships.Contains(ship))
				return;
			this.m_ships.Add(ship);
			this.m_requiresReset = true;
			this.m_destSet = false;
		}

		public void RemoveShip(int shipID)
		{
			Ship ship = this.m_ships.FirstOrDefault<Ship>((Func<Ship, bool>)(x => x.ObjectID == shipID));
			if (ship == null)
				return;
			this.RemoveShip(ship);
		}

		public void RemoveShip(Ship ship)
		{
			this.m_ships.Remove(ship);
			this.m_shipsOnBackLine.Remove(ship);
			foreach (FormationPatternData formationPatternData in this.m_formationPattern)
			{
				if (formationPatternData.Ship == ship)
				{
					this.m_formationPattern.Remove(formationPatternData);
					this.m_requiresReset = true;
					this.m_destSet = false;
					break;
				}
			}
		}

		public void AddShipToBackLine(Ship ship)
		{
			if (this.m_shipsOnBackLine.Contains(ship))
				return;
			this.m_shipsOnBackLine.Add(ship);
			this.m_requiresReset = true;
			this.m_destSet = false;
		}

		public static Vector3 GetCenterOfMass(List<Ship> ships)
		{
			Vector3 zero = Vector3.Zero;
			if (ships.Count > 0)
			{
				foreach (Ship ship in ships)
					zero += ship.Maneuvering.Position;
				zero /= (float)ships.Count;
			}
			return zero;
		}

		public void CreateFormationPattern(Vector3 pos, Vector3 facing, bool vFormation)
		{
			if ((double)facing.LengthSquared <= 9.99999974737875E-06 || !facing.IsFinite() || this.m_ships.Count == 0)
				return;
			Matrix world = Matrix.CreateWorld(ShipFormation.GetCenterOfMass(this.m_ships), facing, Vector3.UnitY);
			this.m_formationPattern = !vFormation ? FormationPatternCreator.CreateLineAbreastPattern(this.m_ships, world) : FormationPatternCreator.CreateVFormationPattern(this.m_ships, world);
			float val2 = 0.0f;
			foreach (FormationPatternData formationPatternData in this.m_formationPattern)
				val2 = Math.Max(formationPatternData.Position.Z, val2);
			List<FormationPatternData> lineAbreastPattern = FormationPatternCreator.CreateLineAbreastPattern(this.m_shipsOnBackLine, Matrix.CreateWorld(ShipFormation.GetCenterOfMass(this.m_shipsOnBackLine), facing, Vector3.UnitY));
			foreach (FormationPatternData formationPatternData in lineAbreastPattern)
			{
				Vector3 position = formationPatternData.Position;
				position.Z += this.m_BackLineOffsetDist + val2;
				formationPatternData.IsLead = formationPatternData.IsLead && this.m_formationPattern.Count == 0;
				formationPatternData.Position = position;
			}
			this.m_formationPattern.AddRange((IEnumerable<FormationPatternData>)lineAbreastPattern);
		}

		public Vector3 GetCurrentPosition()
		{
			if (this.m_formation != null)
				return this.m_formation.GetFormationPosition();
			return Vector3.Zero;
		}

		public void SetDestination(App game, Vector3 pos, Vector3 facing, bool vFormation)
		{
			if (this.m_destroyed)
				return;
			bool flag = false;
			if (this.m_requiresReset || (double)Vector3.Dot(facing, this.m_facing) < 0.25 || this.m_isVFormation != vFormation)
			{
				this.CreateFormationPattern(pos, facing, vFormation);
				this.m_requiresReset = false;
				this.m_isVFormation = vFormation;
				flag = true;
			}
			if (this.m_formationPattern.Count <= 0)
				return;
			if (flag)
			{
				List<object> objectList = new List<object>();
				objectList.Add((object)pos);
				objectList.Add((object)facing);
				objectList.Add((object)(this.m_formation != null ? this.m_formation.ObjectID : 0));
				objectList.Add((object)this.m_formationPattern.Count);
				foreach (FormationPatternData formationPatternData in this.m_formationPattern)
				{
					objectList.Add((object)(formationPatternData.Ship != null ? formationPatternData.Ship.ObjectID : 0));
					objectList.Add((object)formationPatternData.Position);
					objectList.Add((object)formationPatternData.IsLead);
				}
				game.PostApplyFormationPattern(objectList.ToArray());
			}
			else
				this.m_formation.PostAddGoal(pos, facing);
			this.m_destination = pos;
			this.m_facing = facing;
			this.m_destSet = true;
		}

		public void RemoveShip(App game, Ship ship)
		{
			if (!this.m_ships.Contains(ship))
				return;
			game.PostRemoveShipsFromFormation(new List<object>()
	  {
		(object) 1,
		(object) ship.ObjectID
	  }.ToArray());
			this.RemoveShip(ship);
		}

		public void ClearShips(App game)
		{
			if (this.m_ships.Count <= 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)this.m_ships.Count);
			foreach (Ship ship in this.m_ships)
				objectList.Add((object)ship.ObjectID);
			game.PostRemoveShipsFromFormation(objectList.ToArray());
			List<Ship> shipList = new List<Ship>();
			shipList.AddRange((IEnumerable<Ship>)this.m_ships);
			foreach (Ship ship in shipList)
				this.RemoveShip(ship);
		}

		public void Destroy(App game)
		{
			if (this.m_formation != null)
			{
				this.m_formation.ParentShipFormation = (ShipFormation)null;
				game.ReleaseObject((IGameObject)this.m_formation);
			}
			this.m_formation = (FormationDefinition)null;
		}
	}
}
