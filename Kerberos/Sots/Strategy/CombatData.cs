// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.CombatData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class CombatData
	{
		private int _combatID;
		private int _systemID;
		private int _turn;
		private List<PlayerCombatData> _data;

		private void Construct()
		{
			this._data = new List<PlayerCombatData>();
		}

		public CombatData(int combatID, int systemID, int turn)
		{
			this.Construct();
			this._systemID = systemID;
			this._turn = turn;
			this._combatID = combatID;
		}

		public CombatData(ScriptMessageReader mr, int version)
		{
			this.Construct();
			this._combatID = mr.ReadInteger();
			this._systemID = mr.ReadInteger();
			this._turn = mr.ReadInteger();
			int num = mr.ReadInteger();
			for (int index = 0; index < num; ++index)
				this._data.Add(new PlayerCombatData(mr, version));
		}

		public PlayerCombatData GetOrAddPlayer(int playerID)
		{
			foreach (PlayerCombatData playerCombatData in this._data)
			{
				if (playerCombatData.PlayerID == playerID)
					return playerCombatData;
			}
			this._data.Add(new PlayerCombatData(playerID));
			return this._data.Last<PlayerCombatData>();
		}

		public PlayerCombatData GetPlayer(int playerID)
		{
			foreach (PlayerCombatData playerCombatData in this._data)
			{
				if (playerCombatData.PlayerID == playerID)
					return playerCombatData;
			}
			return (PlayerCombatData)null;
		}

		public List<PlayerCombatData> GetPlayers()
		{
			return this._data;
		}

		public List<object> ToList()
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)this._combatID);
			objectList.Add((object)this._systemID);
			objectList.Add((object)this._turn);
			objectList.Add((object)this._data.Count<PlayerCombatData>());
			foreach (PlayerCombatData playerCombatData in this._data)
				objectList.AddRange((IEnumerable<object>)playerCombatData.ToList());
			return objectList;
		}

		public byte[] ToByteArray()
		{
			ScriptMessageWriter scriptMessageWriter = new ScriptMessageWriter();
			scriptMessageWriter.Write((IEnumerable)this.ToList().ToArray());
			return scriptMessageWriter.GetBuffer();
		}

		public int CombatID
		{
			get
			{
				return this._combatID;
			}
		}

		public int SystemID
		{
			get
			{
				return this._systemID;
			}
		}

		public int Turn
		{
			get
			{
				return this._turn;
			}
		}
	}
}
