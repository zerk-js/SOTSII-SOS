// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.FormationPatternCreator
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.ShipFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	internal static class FormationPatternCreator
	{
		public static Ship FindBestFitShipBySize(
		  List<Ship> remainingShips,
		  Vector3 formationPosition,
		  Matrix formationMatrix,
		  ShipClass requestedClass)
		{
			if (remainingShips.Count == 0)
				return (Ship)null;
			Vector3 vector3 = Vector3.Transform(formationPosition, formationMatrix);
			ShipClass shipClass = requestedClass;
			remainingShips.Sort((Comparison<Ship>)((x, y) => Ship.IsShipClassBigger(y.ShipClass, x.ShipClass, false).CompareTo(Ship.IsShipClassBigger(x.ShipClass, y.ShipClass, false))));
			Ship ship = (Ship)null;
			int num1 = 30;
			while (ship == null && shipClass != ShipClass.BattleRider)
			{
				ShipClass sc2 = ShipClass.BattleRider;
				float num2 = float.MaxValue;
				foreach (Ship remainingShip in remainingShips)
				{
					if (Ship.IsShipClassBigger(remainingShip.ShipClass, sc2, false))
						sc2 = remainingShip.ShipClass;
					if (remainingShip.ShipClass == shipClass)
					{
						float lengthSquared = (remainingShip.Maneuvering.Position - vector3).LengthSquared;
						if ((double)lengthSquared < (double)num2)
						{
							num2 = lengthSquared;
							ship = remainingShip;
						}
					}
					else
						break;
				}
				if (ship == null)
				{
					shipClass = sc2;
					--num1;
					if (num1 <= 0)
					{
						ship = remainingShips.FirstOrDefault<Ship>();
						break;
					}
				}
				else
					break;
			}
			return ship;
		}

		public static Vector3 GetLineAbreastPositionAtIndex(int index)
		{
			int num1 = index / 5;
			int num2 = (index % 5 + 1) / 2;
			int num3 = index % 2 == 0 ? 1 : -1;
			return new Vector3()
			{
				X = (float)num3 * 400f * (float)num2,
				Y = 0.0f,
				Z = 500f * (float)num1
			};
		}

		public static Vector3 GetVFormationPositionAtIndex(int index)
		{
			int num1 = (index + 1) / 2;
			int num2 = (index + 1) / 2;
			int num3 = index % 2 == 0 ? 1 : -1;
			return new Vector3()
			{
				X = (float)num3 * 400f * (float)num2,
				Y = 0.0f,
				Z = 500f * (float)num1
			};
		}

		public static Vector3 GetCubeFormationPositionAtIndex(int index)
		{
			switch (index)
			{
				case 0:
					return new Vector3(0.0f, 0.0f, 0.0f);
				case 1:
					return new Vector3(-750f, 0.0f, 0.0f);
				case 2:
					return new Vector3(750f, 0.0f, 0.0f);
				case 3:
					return new Vector3(0.0f, 0.0f, -750f);
				case 4:
					return new Vector3(0.0f, 0.0f, 750f);
				case 5:
					return new Vector3(-750f, 0.0f, -750f);
				case 6:
					return new Vector3(750f, 0.0f, -750f);
				case 7:
					return new Vector3(-750f, 0.0f, 750f);
				case 8:
					return new Vector3(750f, 0.0f, 750f);
				case 9:
					return new Vector3(0.0f, 300f, 0.0f);
				case 10:
					return new Vector3(0.0f, -300f, 0.0f);
				case 11:
					return new Vector3(0.0f, 300f, -750f);
				case 12:
					return new Vector3(0.0f, -300f, -750f);
				case 13:
					return new Vector3(0.0f, 300f, 750f);
				case 14:
					return new Vector3(0.0f, -300f, 750f);
				case 15:
					return new Vector3(-750f, 300f, 0.0f);
				case 16:
					return new Vector3(-750f, -300f, 0.0f);
				case 17:
					return new Vector3(750f, 300f, 0.0f);
				case 18:
					return new Vector3(750f, -300f, 0.0f);
				case 19:
					return new Vector3(-750f, 300f, -750f);
				case 20:
					return new Vector3(-750f, -300f, -750f);
				case 21:
					return new Vector3(750f, 300f, -750f);
				case 22:
					return new Vector3(750f, -300f, -750f);
				case 23:
					return new Vector3(-750f, 300f, 750f);
				case 24:
					return new Vector3(-750f, -300f, 750f);
				case 25:
					return new Vector3(750f, 300f, 750f);
				case 26:
					return new Vector3(750f, -300f, 750f);
				default:
					return Vector3.Zero;
			}
		}

		public static List<FormationPatternData> CreateLineAbreastPattern(
		  List<Ship> ships,
		  Matrix formationMat)
		{
			List<FormationPatternData> formationPatternDataList = new List<FormationPatternData>();
			int count = ships.Count;
			Vector3 zero = Vector3.Zero;
			for (int index = 0; index < count; ++index)
			{
				FormationPatternData formationPatternData = new FormationPatternData();
				formationPatternData.Position = FormationPatternCreator.GetLineAbreastPositionAtIndex(index);
				formationPatternData.Ship = (Ship)null;
				formationPatternData.IsLead = index == 0;
				zero += formationPatternData.Position;
				formationPatternDataList.Add(formationPatternData);
			}
			if (formationPatternDataList.Count > 0)
				zero /= (float)formationPatternDataList.Count;
			List<Ship> remainingShips = new List<Ship>();
			remainingShips.AddRange((IEnumerable<Ship>)ships);
			for (int index = 0; index < count; ++index)
			{
				formationPatternDataList[index].Ship = FormationPatternCreator.FindBestFitShipBySize(remainingShips, formationPatternDataList[index].Position - zero, formationMat, ShipClass.Leviathan);
				remainingShips.Remove(formationPatternDataList[index].Ship);
			}
			return formationPatternDataList;
		}

		public static List<FormationPatternData> CreateVFormationPattern(
		  List<Ship> ships,
		  Matrix formationMat)
		{
			List<FormationPatternData> formationPatternDataList = new List<FormationPatternData>();
			int count = ships.Count;
			for (int index = 0; index < count; ++index)
				formationPatternDataList.Add(new FormationPatternData()
				{
					Position = FormationPatternCreator.GetVFormationPositionAtIndex(index),
					Ship = (Ship)null,
					IsLead = index == 0
				});
			List<Ship> remainingShips = new List<Ship>();
			remainingShips.AddRange((IEnumerable<Ship>)ships);
			for (int index = 0; index < count; ++index)
			{
				formationPatternDataList[index].Ship = FormationPatternCreator.FindBestFitShipBySize(remainingShips, formationPatternDataList[index].Position, formationMat, ShipClass.Leviathan);
				remainingShips.Remove(formationPatternDataList[index].Ship);
			}
			return formationPatternDataList;
		}

		public static List<FormationPatternData> CreateCubeFormationPattern(
		  List<Ship> ships,
		  Matrix formationMat)
		{
			List<FormationPatternData> formationPatternDataList = new List<FormationPatternData>();
			int count = ships.Count;
			for (int index = 0; index < count; ++index)
				formationPatternDataList.Add(new FormationPatternData()
				{
					Position = FormationPatternCreator.GetCubeFormationPositionAtIndex(index),
					Ship = (Ship)null,
					IsLead = index == 0
				});
			List<Ship> remainingShips = new List<Ship>();
			remainingShips.AddRange((IEnumerable<Ship>)ships);
			for (int index = 0; index < count; ++index)
			{
				formationPatternDataList[index].Ship = FormationPatternCreator.FindBestFitShipBySize(remainingShips, formationPatternDataList[index].Position, formationMat, ShipClass.Leviathan);
				remainingShips.Remove(formationPatternDataList[index].Ship);
			}
			return formationPatternDataList;
		}
	}
}
