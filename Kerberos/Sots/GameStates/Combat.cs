// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.Combat
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_COMBAT)]
	internal class Combat : GameObject
	{
		private Combat()
		{
		}

		public static Combat Create(
		  App game,
		  OrbitCameraController cameraController,
		  CombatInput input,
		  CombatSensor sensor,
		  StarSystem system,
		  CombatGrid grid,
		  Vector3 origin,
		  float radius,
		  int duration,
		  Player[] players,
		  Dictionary<Player, Dictionary<Player, PlayerCombatDiplomacy>> diplomacyStates,
		  bool simulateOnly = false)
		{
			List<int> list = ((IEnumerable<Player>)players).Where<Player>((Func<Player, bool>)(x => !x.IsStandardPlayer)).Select<Player, int>((Func<Player, int>)(x => x.ObjectID)).ToList<int>();
			Combat combat = new Combat();
			List<object> objectList = new List<object>();
			objectList.Add((object)cameraController.GetObjectID());
			objectList.Add((object)input.GetObjectID());
			objectList.Add((object)sensor.GetObjectID());
			objectList.Add((object)system.GetObjectID());
			objectList.Add((object)grid.GetObjectID());
			objectList.Add((object)origin);
			objectList.Add((object)radius);
			objectList.Add((object)duration);
			objectList.Add((object)game.LocalPlayer.GetObjectID());
			objectList.Add((object)simulateOnly);
			objectList.Add((object)players.Length);
			foreach (Player player in players)
				objectList.Add((object)player.ObjectID);
			objectList.Add((object)diplomacyStates.Count);
			foreach (KeyValuePair<Player, Dictionary<Player, PlayerCombatDiplomacy>> diplomacyState in diplomacyStates)
			{
				objectList.Add((object)diplomacyState.Key.GetObjectID());
				objectList.Add((object)diplomacyState.Value.Count);
				foreach (KeyValuePair<Player, PlayerCombatDiplomacy> keyValuePair in diplomacyState.Value)
				{
					objectList.Add((object)keyValuePair.Key.GetObjectID());
					objectList.Add((object)keyValuePair.Value);
				}
			}
			objectList.Add((object)list.Count);
			foreach (int num in list)
				objectList.Add((object)num);
			game.AddExistingObject((IGameObject)combat, objectList.ToArray());
			return combat;
		}
	}
}
