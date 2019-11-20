// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.MultiCombatCarryOverData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	public class MultiCombatCarryOverData
	{
		private List<MCSystemInfo> Systems;

		public MultiCombatCarryOverData()
		{
			this.Systems = new List<MCSystemInfo>();
		}

		public void ClearData()
		{
			this.Systems.Clear();
		}

		public void AddCarryOverInfo(int systemId, int fleetId, int shipId, Matrix endShipTransform)
		{
			MCSystemInfo mcSystemInfo1 = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemId));
			if (mcSystemInfo1 != null)
			{
				MCFleetInfo mcFleetInfo1 = mcSystemInfo1.Fleets.FirstOrDefault<MCFleetInfo>((Func<MCFleetInfo, bool>)(x => x.FleetID == fleetId));
				if (mcFleetInfo1 != null)
				{
					MCShipInfo mcShipInfo = mcFleetInfo1.Ships.FirstOrDefault<MCShipInfo>((Func<MCShipInfo, bool>)(x => x.ShipID == shipId));
					if (mcShipInfo != null)
						mcShipInfo.PreviousTransform = endShipTransform;
					else
						mcFleetInfo1.Ships.Add(new MCShipInfo()
						{
							ShipID = shipId,
							PreviousTransform = endShipTransform
						});
				}
				else
				{
					MCShipInfo mcShipInfo = new MCShipInfo()
					{
						ShipID = shipId,
						PreviousTransform = endShipTransform
					};
					MCFleetInfo mcFleetInfo2 = new MCFleetInfo()
					{
						FleetID = fleetId
					};
					mcFleetInfo2.Ships.Add(mcShipInfo);
					mcSystemInfo1.Fleets.Add(mcFleetInfo2);
				}
			}
			else
			{
				MCShipInfo mcShipInfo = new MCShipInfo()
				{
					ShipID = shipId,
					PreviousTransform = endShipTransform
				};
				MCFleetInfo mcFleetInfo = new MCFleetInfo()
				{
					FleetID = fleetId
				};
				mcFleetInfo.Ships.Add(mcShipInfo);
				MCSystemInfo mcSystemInfo2 = new MCSystemInfo()
				{
					SystemID = systemId
				};
				mcSystemInfo2.Fleets.Add(mcFleetInfo);
				this.Systems.Add(mcSystemInfo2);
			}
		}

		public void SetRetreatFleetID(int systemId, int currFleetId, int retreatFleetId)
		{
			MCSystemInfo mcSystemInfo1 = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemId));
			if (mcSystemInfo1 != null)
			{
				MCFleetInfo mcFleetInfo1 = mcSystemInfo1.Fleets.FirstOrDefault<MCFleetInfo>((Func<MCFleetInfo, bool>)(x => x.FleetID == currFleetId));
				if (mcFleetInfo1 != null)
				{
					mcFleetInfo1.RetreatFleetID = retreatFleetId;
				}
				else
				{
					MCFleetInfo mcFleetInfo2 = new MCFleetInfo()
					{
						FleetID = currFleetId,
						RetreatFleetID = retreatFleetId
					};
					mcSystemInfo1.Fleets.Add(mcFleetInfo2);
				}
			}
			else
			{
				MCFleetInfo mcFleetInfo = new MCFleetInfo()
				{
					FleetID = currFleetId,
					RetreatFleetID = retreatFleetId
				};
				MCSystemInfo mcSystemInfo2 = new MCSystemInfo()
				{
					SystemID = systemId
				};
				mcSystemInfo2.Fleets.Add(mcFleetInfo);
				this.Systems.Add(mcSystemInfo2);
			}
		}

		public int GetRetreatFleetID(int systemId, int currFleetId)
		{
			MCSystemInfo mcSystemInfo = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemId));
			if (mcSystemInfo != null)
			{
				MCFleetInfo mcFleetInfo1 = mcSystemInfo.Fleets.FirstOrDefault<MCFleetInfo>((Func<MCFleetInfo, bool>)(x => x.FleetID == currFleetId));
				if (mcFleetInfo1 != null)
					return mcFleetInfo1.RetreatFleetID;
				MCFleetInfo mcFleetInfo2 = mcSystemInfo.Fleets.FirstOrDefault<MCFleetInfo>((Func<MCFleetInfo, bool>)(x => x.RetreatFleetID == currFleetId));
				if (mcFleetInfo2 != null)
					return mcFleetInfo2.RetreatFleetID;
			}
			return 0;
		}

		public void AddCarryOverCombatZoneInfo(int systemId, List<int> combatZones)
		{
			MCSystemInfo mcSystemInfo = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemId));
			if (mcSystemInfo == null)
			{
				mcSystemInfo = new MCSystemInfo()
				{
					SystemID = systemId
				};
				this.Systems.Add(mcSystemInfo);
			}
			if (mcSystemInfo == null)
				return;
			mcSystemInfo.ControlZones.Clear();
			foreach (int combatZone in combatZones)
				mcSystemInfo.ControlZones.Add(combatZone);
		}

		public Matrix? GetPreviousShipTransform(int systemId, int fleetId, int shipId)
		{
			MCSystemInfo mcSystemInfo = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemId));
			if (mcSystemInfo != null)
			{
				MCFleetInfo mcFleetInfo = mcSystemInfo.Fleets.FirstOrDefault<MCFleetInfo>((Func<MCFleetInfo, bool>)(x => x.FleetID == fleetId));
				if (mcFleetInfo != null)
				{
					MCShipInfo mcShipInfo = mcFleetInfo.Ships.FirstOrDefault<MCShipInfo>((Func<MCShipInfo, bool>)(x => x.ShipID == shipId));
					if (mcShipInfo != null)
						return new Matrix?(mcShipInfo.PreviousTransform);
				}
			}
			return new Matrix?();
		}

		public List<int> GetPreviousControlZones(int systemId)
		{
			MCSystemInfo mcSystemInfo = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemId));
			if (mcSystemInfo != null)
				return mcSystemInfo.ControlZones;
			return new List<int>();
		}

		public List<object> GetCarryOverDataList(int systemID)
		{
			List<object> objectList = new List<object>();
			MCSystemInfo mcSystemInfo = this.Systems.FirstOrDefault<MCSystemInfo>((Func<MCSystemInfo, bool>)(x => x.SystemID == systemID));
			if (mcSystemInfo == null)
				return objectList;
			objectList.Add((object)systemID);
			objectList.Add((object)mcSystemInfo.ControlZones.Count);
			foreach (int controlZone in mcSystemInfo.ControlZones)
				objectList.Add((object)controlZone);
			objectList.Add((object)mcSystemInfo.Fleets.Count);
			foreach (MCFleetInfo fleet in mcSystemInfo.Fleets)
			{
				objectList.Add((object)fleet.FleetID);
				objectList.Add((object)fleet.RetreatFleetID);
				objectList.Add((object)fleet.Ships.Count);
				foreach (MCShipInfo ship in fleet.Ships)
				{
					objectList.Add((object)ship.ShipID);
					objectList.Add((object)ship.PreviousTransform.Position.X);
					objectList.Add((object)ship.PreviousTransform.Position.Y);
					objectList.Add((object)ship.PreviousTransform.Position.Z);
					objectList.Add((object)ship.PreviousTransform.EulerAngles.X);
					objectList.Add((object)ship.PreviousTransform.EulerAngles.Y);
					objectList.Add((object)ship.PreviousTransform.EulerAngles.Z);
				}
			}
			return objectList;
		}
	}
}
