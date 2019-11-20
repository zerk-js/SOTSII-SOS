// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GovernmentEffects
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots
{
	internal class GovernmentEffects
	{
		private Dictionary<GovernmentInfo.GovernmentType, GovEffectCollection> DiplomaticEffects;

		public GovernmentEffects()
		{
			this.DiplomaticEffects = new Dictionary<GovernmentInfo.GovernmentType, GovEffectCollection>();
		}

		public void LoadFromFile(XmlDocument govEffects)
		{
			if (govEffects == null)
				return;
			XmlElement govEffect = govEffects[nameof(GovernmentEffects)];
			if (govEffect == null)
				return;
			this.DiplomaticEffects.Clear();
			foreach (GovernmentInfo.GovernmentType key in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
			{
				XmlElement xmlElement = govEffect[key.ToString()];
				if (xmlElement != null)
				{
					this.DiplomaticEffects.Add(key, new GovEffectCollection()
					{
						DiplomaticEffects = new GovDiplomaticEffects(),
						MoralEffects = new GovMoralEffects(),
						StratEffects = new GovStratModifiers()
					});
					this.DiplomaticEffects[key].DiplomaticEffects.LoadDiplmaticEffects(xmlElement["DiplomaticEffects"]);
					this.DiplomaticEffects[key].MoralEffects.LoadMoralEffects(xmlElement["MoralEffects"]);
					this.DiplomaticEffects[key].StratEffects.LoadStratEffects(xmlElement["StratModifiers"]);
				}
			}
		}

		public float GetDiplomacyBonus(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  PlayerInfo player,
		  PlayerInfo toPlayer)
		{
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(player.ID);
			GovEffectCollection effectCollection;
			if (governmentInfo != null && this.DiplomaticEffects.TryGetValue(governmentInfo.CurrentType, out effectCollection))
				return effectCollection.DiplomaticEffects.GetDiplomaticBonusBetweenGovernmentTypes(gamedb, assetdb, player, toPlayer);
			return 0.0f;
		}

		public int GetMoralTotal(
		  GameDatabase gamedb,
		  GovernmentInfo.GovernmentType gt,
		  MoralEvent me,
		  int player,
		  int moral)
		{
			GovEffectCollection effectCollection;
			if (this.DiplomaticEffects.TryGetValue(gt, out effectCollection))
				return effectCollection.MoralEffects.GetResultingMoral(gamedb, me, player, moral);
			return moral;
		}

		public int GetStratModifierTotal(
		  GameDatabase gamedb,
		  StratModifiers sm,
		  int playerId,
		  int modValue)
		{
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(playerId);
			GovEffectCollection effectCollection;
			if (governmentInfo != null && this.DiplomaticEffects.TryGetValue(governmentInfo.CurrentType, out effectCollection))
				return effectCollection.StratEffects.GetResultingStratModifierValue(gamedb, sm, playerId, modValue);
			return modValue;
		}

		public float GetStratModifierTotal(
		  GameDatabase gamedb,
		  StratModifiers sm,
		  int playerId,
		  float modValue)
		{
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(playerId);
			GovEffectCollection effectCollection;
			if (governmentInfo != null && this.DiplomaticEffects.TryGetValue(governmentInfo.CurrentType, out effectCollection))
				return effectCollection.StratEffects.GetResultingStratModifierValue(gamedb, sm, playerId, modValue);
			return modValue;
		}

		public static bool IsPlayerAtWar(GameDatabase gamedb, int player)
		{
			bool flag = false;
			foreach (int otherPlayerID in gamedb.GetStandardPlayerIDs().ToList<int>())
			{
				if (otherPlayerID != player && gamedb.GetDiplomacyStateBetweenPlayers(player, otherPlayerID) == DiplomacyState.WAR)
				{
					flag = true;
					break;
				}
			}
			return flag;
		}
	}
}
