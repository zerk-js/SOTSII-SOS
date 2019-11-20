// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GovDiplomaticEffects
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.PlayerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots
{
	internal class GovDiplomaticEffects
	{
		public float IndependantBonus;
		public float SameFactionBonus;
		public Dictionary<GovernmentInfo.GovernmentType, float> DiplomaticEffect;
		public Dictionary<GovernmentInfo.GovernmentType, float> NonPlayerDiplomaticEffect;

		public GovDiplomaticEffects()
		{
			this.DiplomaticEffect = new Dictionary<GovernmentInfo.GovernmentType, float>();
			this.NonPlayerDiplomaticEffect = new Dictionary<GovernmentInfo.GovernmentType, float>();
		}

		public void LoadDiplmaticEffects(XmlElement diploEffects)
		{
			if (diploEffects == null)
				return;
			this.DiplomaticEffect.Clear();
			this.NonPlayerDiplomaticEffect.Clear();
			foreach (GovernmentInfo.GovernmentType key in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
			{
				XmlElement diploEffect = diploEffects[key.ToString()];
				if (diploEffect != null)
					this.DiplomaticEffect.Add(key, float.Parse(diploEffect.InnerText));
			}
			XmlElement diploEffect1 = diploEffects["NonPlayer"];
			if (diploEffect1 != null)
			{
				if (diploEffect1.ChildNodes.Count == 0)
				{
					float num = float.Parse(diploEffect1.InnerText);
					foreach (GovernmentInfo.GovernmentType key in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
						this.NonPlayerDiplomaticEffect.Add(key, num + this.DiplomaticEffect[key]);
				}
				else
				{
					foreach (GovernmentInfo.GovernmentType key in Enum.GetValues(typeof(GovernmentInfo.GovernmentType)))
					{
						XmlElement xmlElement = diploEffect1[key.ToString()];
						if (xmlElement != null)
							this.NonPlayerDiplomaticEffect.Add(key, float.Parse(xmlElement.InnerText) + this.DiplomaticEffect[key]);
					}
				}
			}
			this.IndependantBonus = XmlHelper.GetData<float>(diploEffects, "Independant");
			this.SameFactionBonus = XmlHelper.GetData<float>(diploEffects, "SameFaction");
		}

		public float GetDiplomaticBonusBetweenGovernmentTypes(
		  GameDatabase gamedb,
		  AssetDatabase assetdb,
		  PlayerInfo player,
		  PlayerInfo toPlayer)
		{
			float num1 = 0.0f;
			Faction faction1 = assetdb.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == player.FactionID));
			Faction faction2 = assetdb.Factions.FirstOrDefault<Faction>((Func<Faction, bool>)(x => x.ID == toPlayer.FactionID));
			GovernmentInfo governmentInfo = gamedb.GetGovernmentInfo(toPlayer.ID);
			float num2;
			if (governmentInfo != null && this.DiplomaticEffect.TryGetValue(governmentInfo.CurrentType, out num2))
				num1 += num2;
			if (!toPlayer.isStandardPlayer)
			{
				if (faction2 != null && faction2.IsIndependent())
					num1 += this.IndependantBonus;
				else if (this.NonPlayerDiplomaticEffect.TryGetValue(governmentInfo.CurrentType, out num2))
					num1 += num2;
			}
			if (faction1 == faction2)
				num1 += this.SameFactionBonus;
			return num1;
		}
	}
}
