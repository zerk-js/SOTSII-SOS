// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.CombatDataHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	internal class CombatDataHelper
	{
		private List<CombatData> _combatData;

		public CombatDataHelper()
		{
			this._combatData = new List<CombatData>();
		}

		public CombatData AddCombat(int conflictID, int systemID, int turn)
		{
			this._combatData.Add(new CombatData(conflictID, systemID, turn));
			return this._combatData.Last<CombatData>();
		}

		public void AddCombat(ScriptMessageReader mr, int version)
		{
			this._combatData.Add(new CombatData(mr, version));
		}

		public CombatData GetFirstCombatInSystem(GameDatabase db, int systemID, int turn)
		{
			foreach (CombatData combatData in this._combatData)
			{
				if (combatData.SystemID == systemID && combatData.Turn == turn)
					return combatData;
			}
			int version = 0;
			ScriptMessageReader combatData1 = db.GetCombatData(systemID, turn, out version);
			if (combatData1 == null)
				return (CombatData)null;
			this.AddCombat(combatData1, version);
			return this.GetLastCombat();
		}

		public CombatData GetCombat(GameDatabase db, int combatID, int systemID, int turn)
		{
			foreach (CombatData combatData in this._combatData)
			{
				if (combatData.CombatID == combatID && combatData.SystemID == systemID && combatData.Turn == turn)
					return combatData;
			}
			int version = 0;
			ScriptMessageReader combatData1 = db.GetCombatData(systemID, combatID, turn, out version);
			if (combatData1 == null)
				return (CombatData)null;
			this.AddCombat(combatData1, version);
			return this.GetLastCombat();
		}

		public IEnumerable<CombatData> GetCombatsForPlayer(
		  GameDatabase db,
		  int playerID,
		  int turnCount)
		{
			foreach (CombatData combatData in this._combatData)
			{
				if (combatData.GetPlayers().Any<PlayerCombatData>((Func<PlayerCombatData, bool>)(x => x.PlayerID == playerID)) && (turnCount == 0 || db.GetTurnCount() - combatData.Turn <= turnCount))
					yield return combatData;
			}
		}

		public IEnumerable<CombatData> GetCombatsForPlayer(
		  GameDatabase db,
		  int playerID,
		  int systemId,
		  int turnCount)
		{
			return this.GetCombatsForPlayer(db, playerID, turnCount).Where<CombatData>((Func<CombatData, bool>)(x => x.SystemID == systemId));
		}

		public CombatData GetLastCombat()
		{
			if (this._combatData.Count > 0)
				return this._combatData.Last<CombatData>();
			return (CombatData)null;
		}

		public IEnumerable<CombatData> GetCombats(int turn)
		{
			foreach (CombatData combatData in this._combatData)
			{
				if (combatData.Turn == turn)
					yield return combatData;
			}
		}

		public void ClearCombatData()
		{
			this._combatData.Clear();
		}
	}
}
