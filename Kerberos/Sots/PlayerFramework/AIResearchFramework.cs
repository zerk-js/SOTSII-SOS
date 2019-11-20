// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.AIResearchFramework
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Kerberos.Sots.PlayerFramework
{
	internal class AIResearchFramework
	{
		private const int BinaryResponseOverrideTurns = 5;
		private AIResearchFramework.AITechLog _log;
		private readonly AIResearchFramework.AITechReplacementRow[] _replacements;
		private readonly List<AIResearchFramework.AITechStyleGroup> _styleGroups;
		private readonly Dictionary<AIStance, AIResearchFramework.AIResearchRhythm> _rhythms;

		public AIResearchFramework()
		{
			this._replacements = new AIResearchFramework.AITechReplacementRow[0];
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<stylegroups><!-- Race value format = Human/Hiver/Tark/Liir/Zuul/Morrigi (% chance) --><!-- style, group, priority, % chance --><!--{ {EnergyWeapons},       1, 0.50, 40,10,30,90,20,70 },--><!--{ {BallisticWeapons},    1, 0.50, 40,80,60,10,70,20 },--><group><style costmul='0.5'><families><family value='EnergyWeapons'/></families><chances><chance faction='human'     value='50'/><chance faction='hiver'     value='10'/><chance faction='tarkas'    value='30'/><chance faction='liir_zuul' value='90'/><chance faction='zuul'      value='50'/><chance faction='morrigi'   value='90'/><chance faction='loa'       value='90'/></chances></style><style costmul='0.5'><families><family value='BallisticWeapons'/></families><chances><chance faction='human'     value='50'/><chance faction='hiver'     value='90'/><chance faction='tarkas'    value='90'/><chance faction='liir_zuul' value='10'/><chance faction='zuul'      value='50'/><chance faction='morrigi'   value='10'/><chance faction='loa'       value='10'/></chances></style></group><!--{ {ShieldTechnology},      2, 0.50, 40,10,30,70,10,60 },--><!--{ {IndustrialTechnology},  2, 0.50, 40,90,70,20,80,20 },--><group><style costmul='0.5'><families><family value='ShieldTechnology'/></families><chances><chance faction='human'     value='80'/><chance faction='hiver'     value='10'/><chance faction='tarkas'    value='50'/><chance faction='liir_zuul' value='40'/><chance faction='zuul'      value='20'/><chance faction='morrigi'   value='50'/><chance faction='loa'       value='40'/></chances></style><style costmul='0.5'><families><family value='IndustrialTechnology'/></families><chances><chance faction='human'     value='50'/><chance faction='hiver'     value='50'/><chance faction='tarkas'    value='50'/><chance faction='liir_zuul' value='50'/><chance faction='zuul'      value='50'/><chance faction='morrigi'   value='50'/><chance faction='loa'       value='50'/></chances></style></group><!--{ {Torpedos,EnergyWeapons},  3, 0.50, 60,55,70,30,50,70 },--><!--{ {BioTechnology},           3, 0.50, 20,15,20,60, 0,20 },--><group><style costmul='0.5'><families><family value='Torpedos'/><family value='EnergyWeapons'/></families><chances><chance faction='human'     value='70'/><chance faction='hiver'     value='10'/><chance faction='tarkas'    value='20'/><chance faction='liir_zuul' value='50'/><chance faction='zuul'      value='50'/><chance faction='morrigi'   value='50'/><chance faction='loa'       value='40'/></chances></style><style costmul='0.5'><families><family value='BioTechnology'/></families><chances><chance faction='human'     value='30'/><chance faction='hiver'     value='30'/><chance faction='tarkas'    value='30'/><chance faction='liir_zuul' value='30'/><chance faction='zuul'      value='30'/><chance faction='morrigi'   value='30'/><chance faction='loa'       value='30'/></chances></style></group><!--{ {RiderTechnology,Psionics},  4, 0.50, 40,35,30,90,50,70 },--><!--{ {WarheadTechnology},         4, 0.50, 40,45,50,10,40,20 },--><group><style costmul='0.5'><families><family value='RiderTechnology'/><family value='Psionics'/></families><chances><chance faction='human'     value='10'/><chance faction='hiver'     value='10'/><chance faction='tarkas'    value='10'/><chance faction='liir_zuul' value='30'/><chance faction='zuul'      value='10'/><chance faction='morrigi'   value='10'/><chance faction='loa'       value='10'/></chances></style><style costmul='0.5'><families><family value='WarheadTechnology'/></families><chances><chance faction='human'     value='10'/><chance faction='hiver'     value='50'/><chance faction='tarkas'    value='40'/><chance faction='liir_zuul' value='10'/><chance faction='zuul'      value='20'/><chance faction='morrigi'   value='10'/><chance faction='loa'       value='10'/></chances></style></group></stylegroups>");
			this._styleGroups = new List<AIResearchFramework.AITechStyleGroup>(xmlDocument["stylegroups"].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "group")).Select<XmlElement, AIResearchFramework.AITechStyleGroup>((Func<XmlElement, AIResearchFramework.AITechStyleGroup>)(y => AIResearchFramework.AITechStyleGroup.Parse(y))));
			this._rhythms = new Dictionary<AIStance, AIResearchFramework.AIResearchRhythm>();
			Dictionary<AIStance, AIResearchFramework.AIResearchRhythm> rhythms1 = this._rhythms;
			AIStance aiStance1 = AIStance.ARMING;
			AIResearchFramework.AIResearchRhythm aiResearchRhythm1 = new AIResearchFramework.AIResearchRhythm();
			aiResearchRhythm1.Beats = new AIResearchModes[8]
			{
		AIResearchModes.Weapon,
		AIResearchModes.Engine,
		AIResearchModes.Weapon,
		AIResearchModes.Expansion,
		AIResearchModes.Empire,
		AIResearchModes.Weapon,
		AIResearchModes.Engine,
		AIResearchModes.Empire
			};
			int num1 = (int)aiStance1;
			AIResearchFramework.AIResearchRhythm aiResearchRhythm2 = aiResearchRhythm1;
			rhythms1[(AIStance)num1] = aiResearchRhythm2;
			this._rhythms[AIStance.CONQUERING] = new AIResearchFramework.AIResearchRhythm()
			{
				Beats = new AIResearchModes[7]
			  {
		  AIResearchModes.Weapon,
		  AIResearchModes.Engine,
		  AIResearchModes.Weapon,
		  AIResearchModes.Weapon,
		  AIResearchModes.Empire,
		  AIResearchModes.Weapon,
		  AIResearchModes.Engine
			  }
			};
			this._rhythms[AIStance.DEFENDING] = new AIResearchFramework.AIResearchRhythm()
			{
				Beats = new AIResearchModes[7]
			  {
		  AIResearchModes.Weapon,
		  AIResearchModes.Empire,
		  AIResearchModes.Weapon,
		  AIResearchModes.Weapon,
		  AIResearchModes.Empire,
		  AIResearchModes.Weapon,
		  AIResearchModes.Engine
			  }
			};
			this._rhythms[AIStance.DESTROYING] = new AIResearchFramework.AIResearchRhythm()
			{
				Beats = new AIResearchModes[7]
			  {
		  AIResearchModes.Weapon,
		  AIResearchModes.Engine,
		  AIResearchModes.Weapon,
		  AIResearchModes.Weapon,
		  AIResearchModes.Empire,
		  AIResearchModes.Weapon,
		  AIResearchModes.Engine
			  }
			};
			this._rhythms[AIStance.EXPANDING] = new AIResearchFramework.AIResearchRhythm()
			{
				Beats = new AIResearchModes[9]
			  {
		  AIResearchModes.Expansion,
		  AIResearchModes.Weapon,
		  AIResearchModes.Empire,
		  AIResearchModes.Weapon,
		  AIResearchModes.Expansion,
		  AIResearchModes.Engine,
		  AIResearchModes.Weapon,
		  AIResearchModes.Empire,
		  AIResearchModes.Expansion
			  }
			};
			Dictionary<AIStance, AIResearchFramework.AIResearchRhythm> rhythms2 = this._rhythms;
			AIStance aiStance2 = AIStance.HUNKERING;
			AIResearchFramework.AIResearchRhythm aiResearchRhythm3 = new AIResearchFramework.AIResearchRhythm();
			aiResearchRhythm3.Beats = new AIResearchModes[8]
			{
		AIResearchModes.Empire,
		AIResearchModes.Weapon,
		AIResearchModes.Empire,
		AIResearchModes.Weapon,
		AIResearchModes.Expansion,
		AIResearchModes.Weapon,
		AIResearchModes.Engine,
		AIResearchModes.Empire
			};
			int num2 = (int)aiStance2;
			AIResearchFramework.AIResearchRhythm aiResearchRhythm4 = aiResearchRhythm3;
			rhythms2[(AIStance)num2] = aiResearchRhythm4;
		}

		private bool AIHaveReplacementForTech(StrategicAI ai, Tech tech)
		{
			bool flag = false;
			foreach (AIResearchFramework.AITechReplacementRow replacement in this._replacements)
			{
				if (tech == replacement.Old)
				{
					PlayerTechInfo playerTechInfo = AIResearchFramework.AIGetPlayerTechInfo(ai, replacement.New);
					if (playerTechInfo != null)
					{
						switch (replacement.Contexts)
						{
							case AIResearchFramework.AITechReplacementContexts.Available:
								flag = AIResearchFramework.AIIsTechAvailable(playerTechInfo.State) || AIResearchFramework.AIHaveTech(playerTechInfo.State);
								break;
							case AIResearchFramework.AITechReplacementContexts.Researched:
								flag = AIResearchFramework.AIHaveTech(playerTechInfo.State);
								break;
						}
						if (flag)
							break;
					}
				}
			}
			return flag;
		}

		private int AIGetPhase(StrategicAI ai)
		{
			return ai.Game.GameDatabase.GetPlayerTechInfos(ai.Player.ID).Count<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x =>
		   {
			   if (x.TurnResearched.HasValue)
			   {
				   int? turnResearched = x.TurnResearched;
				   if ((turnResearched.GetValueOrDefault() <= 1 ? 0 : (turnResearched.HasValue ? 1 : 0)) != 0)
					   return x.State == TechStates.Researched;
			   }
			   return false;
		   }));
		}

		private static bool AIContainsTech(AIResearchModes? mode, TechFamilies? family, Tech tech)
		{
			return (!mode.HasValue || tech.AIResearchModeEnums.Contains<AIResearchModes>(mode.Value)) && (!family.HasValue || (TechFamilies)Enum.Parse(typeof(TechFamilies), tech.Family) == family.Value);
		}

		private static bool AIIsTechAvailable(TechStates state)
		{
			switch (state)
			{
				case TechStates.Core:
				case TechStates.Branch:
				case TechStates.HighFeasibility:
					return true;
				default:
					return false;
			}
		}

		private static bool AIHaveTech(TechStates state)
		{
			return state == TechStates.Researched;
		}

		private static PlayerTechInfo AIGetPlayerTechInfo(StrategicAI ai, Tech tech)
		{
			return ai.Game.GameDatabase.GetPlayerTechInfo(ai.Player.ID, ai.Game.GameDatabase.GetTechID(tech.Id));
		}

		private static List<Tech> AISelectAvailableTechs(
		  StrategicAI ai,
		  AIResearchModes? mode,
		  TechFamilies? family)
		{
			return ai.Game.GameDatabase.GetPlayerTechInfos(ai.Player.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => AIResearchFramework.AIIsTechAvailable(x.State))).Select<PlayerTechInfo, Tech>((Func<PlayerTechInfo, Tech>)(y => ai.Game.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(z => y.TechFileID == z.Id)))).Where<Tech>((Func<Tech, bool>)(x => AIResearchFramework.AIContainsTech(mode, family, x))).ToList<Tech>();
		}

		private static int AICalcTechCost(double s, double r, double n)
		{
			return (int)(s * r * n);
		}

		private static AIResearchModes AIGetResearchMode(
		  AIResearchFramework.AIResearchRhythm rhythm,
		  int age)
		{
			return rhythm.Beats[age % rhythm.Beats.Length];
		}

		private AIStance AIGetStance(StrategicAI ai)
		{
			return ai.Game.GameDatabase.GetAIInfo(ai.Player.ID).Stance;
		}

		private AIResearchModes AIGetResearchMode(StrategicAI ai)
		{
			return AIResearchFramework.AIGetResearchMode(this._rhythms[this.AIGetStance(ai)], this.AIGetPhase(ai));
		}

		private static bool AITechStyleContains(StrategicAI ai, AITechStyleInfo style, Tech tech)
		{
			TechFamilies techFamilyEnum = ai.Game.AssetDatabase.MasterTechTree.GetTechFamilyEnum(tech);
			return style.TechFamily == techFamilyEnum;
		}

		private static double AICalcTechStyleCost(StrategicAI ai, AITechStyleInfo style, Tech tech)
		{
			if (AIResearchFramework.AITechStyleContains(ai, style, tech))
				return (double)style.CostFactor;
			return 1.0;
		}

		private static double AICalcCombinedTechStyleCost(StrategicAI ai, Tech tech)
		{
			double num = 1.0;
			foreach (AITechStyleInfo techStyleInfo in ai.TechStyles.TechStyleInfos)
				num *= AIResearchFramework.AICalcTechStyleCost(ai, techStyleInfo, tech);
			return num;
		}

		private static int AIGetTurnsToComplete(StrategicAI ai, Tech tech)
		{
			PlayerTechInfo playerTechInfo = ai.Game.GameDatabase.GetPlayerTechInfo(ai.Player.ID, ai.Game.GameDatabase.GetTechID(tech.Id));
			ai.Game.GameDatabase.GetPlayerInfo(ai.Player.ID);
			int researchPointsPerTurn = ai.CachedAvailableResearchPointsPerTurn;
			int? completeResearch = GameSession.CalculateTurnsToCompleteResearch(playerTechInfo.ResearchCost, playerTechInfo.Progress, researchPointsPerTurn);
			if (!completeResearch.HasValue)
				return int.MaxValue;
			return completeResearch.Value;
		}

		private static int AICalcTechCost(StrategicAI ai, Tech tech)
		{
			return AIResearchFramework.AICalcTechCost(AIResearchFramework.AICalcCombinedTechStyleCost(ai, tech), (double)tech.AICostFactors.Faction(ai.Player.Faction.Name), (double)AIResearchFramework.AIGetTurnsToComplete(ai, tech));
		}

		private static Tech AISelectFavoriteTech(StrategicAI ai, IList<Tech> techs)
		{
			if (techs.Count == 0)
				return (Tech)null;
			List<Tech> techList = new List<Tech>();
			int num = AIResearchFramework.AICalcTechCost(ai, techs[0]);
			foreach (Tech tech in (IEnumerable<Tech>)techs)
			{
				if (num == AIResearchFramework.AICalcTechCost(ai, tech))
				{
					techList.Add(tech);
				}
				else
				{
					if (AIResearchFramework.AIGetTurnsToComplete(ai, tech) <= 15)
					{
						techList.Add(tech);
						break;
					}
					break;
				}
			}
			return techList[ai.Random.Next(techList.Count)];
		}

		private static char AIGetResearchModeSymbol(AIResearchModes mode)
		{
			switch (mode)
			{
				case AIResearchModes.Empire:
					return 'E';
				case AIResearchModes.Engine:
					return 'N';
				case AIResearchModes.Weapon:
					return 'W';
				case AIResearchModes.Expansion:
					return 'X';
				default:
					return ' ';
			}
		}

		private Tech AISelectDefaultTechPass(
		  StrategicAI ai,
		  AIResearchModes? mode,
		  TechFamilies? family)
		{
			List<Tech> list = AIResearchFramework.AISelectAvailableTechs(ai, mode, family).OrderBy<Tech, int>((Func<Tech, int>)(x => AIResearchFramework.AICalcTechCost(ai, x))).ToList<Tech>();
			if (list.Count > 0)
				list.Insert(0, list[0]);
			if (this._log != null)
			{
				this._log.Print(string.Format(" {0} prospects: ", mode.HasValue ? (object)string.Format("{0}/{1}", (object)AIResearchFramework.AIGetResearchModeSymbol(mode.Value), (object)ai.Game.GameDatabase.GetAIInfo(ai.Player.ID).Stance.ToString()) : (object)"ALL"));
				foreach (Tech tech in list)
				{
					int num = AIResearchFramework.AICalcTechCost(ai, tech);
					this._log.Print(string.Format("{0} ({1}); ", (object)tech.Id, (object)num));
				}
			}
			return AIResearchFramework.AISelectFavoriteTech(ai, (IList<Tech>)list);
		}

		private static Tech AIGetResearchingTech(StrategicAI ai)
		{
			int researchingTechId = ai.Game.GameDatabase.GetPlayerResearchingTechID(ai.Player.ID);
			if (researchingTechId == 0)
				return (Tech)null;
			string fileId = ai.Game.GameDatabase.GetTechFileID(researchingTechId);
			return ai.Game.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(x => x.Id == fileId));
		}

		private IEnumerable<AIResearchModes> AIGetRhythm(AIStance stance)
		{
			return (IEnumerable<AIResearchModes>)this._rhythms[stance].Beats;
		}

		private List<AIResearchModes> AIGetPhasedRhythm(
		  IEnumerable<AIResearchModes> rhythm,
		  int phase)
		{
			int count = phase % rhythm.Count<AIResearchModes>();
			return rhythm.Skip<AIResearchModes>(count).Take<AIResearchModes>(rhythm.Count<AIResearchModes>() - count).Concat<AIResearchModes>(rhythm.Take<AIResearchModes>(count)).ToList<AIResearchModes>();
		}

		private IEnumerable<Tech> AIGetAvailableTechs(StrategicAI ai)
		{
			return ai.Game.GameDatabase.GetPlayerTechInfos(ai.Player.ID).Where<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => AIResearchFramework.AIIsTechAvailable(x.State))).Select<PlayerTechInfo, Tech>((Func<PlayerTechInfo, Tech>)(y => ai.Game.AssetDatabase.MasterTechTree.Technologies.First<Tech>((Func<Tech, bool>)(z => y.TechFileID == z.Id))));
		}

		private IEnumerable<AIResearchFramework.TechBeat> AIGetTechBeats(
		  IEnumerable<Tech> techs,
		  List<AIResearchModes> rhythm,
		  List<PlayerTechInfo> desiredTech = null)
		{
			foreach (Tech tech1 in techs)
			{
				Tech tech = tech1;
				for (int beat = 0; beat < rhythm.Count; ++beat)
				{
					if (AIResearchFramework.AIContainsTech(new AIResearchModes?(rhythm[beat]), new TechFamilies?(), tech))
					{
						if (desiredTech != null)
							desiredTech.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(x => x.TechFileID == tech.Id));
						yield return new AIResearchFramework.TechBeat()
						{
							Tech = tech,
							Beat = beat
						};
					}
				}
			}
		}

		private bool AIIsShortTermTech(StrategicAI ai, Tech tech)
		{
			return AIResearchFramework.AIGetTurnsToComplete(ai, tech) < 15;
		}

		private IEnumerable<AIResearchFramework.TechBeat> AIGetProspects(
		  StrategicAI ai,
		  IEnumerable<AIResearchFramework.TechBeat> techs)
		{
			return (IEnumerable<AIResearchFramework.TechBeat>)techs.OrderBy<AIResearchFramework.TechBeat, int>((Func<AIResearchFramework.TechBeat, int>)(x => !this.AIIsShortTermTech(ai, x.Tech) ? 1 : 0)).ThenBy<AIResearchFramework.TechBeat, int>((Func<AIResearchFramework.TechBeat, int>)(y => y.Beat)).ThenBy<AIResearchFramework.TechBeat, int>((Func<AIResearchFramework.TechBeat, int>)(z => AIResearchFramework.AICalcTechCost(ai, z.Tech)));
		}

		private List<AIResearchFramework.TechBeat> AIGetCulledProspects(
		  StrategicAI ai,
		  IEnumerable<AIResearchFramework.TechBeat> sorted)
		{
			int num1 = 0;
			double num2 = 0.0;
			List<AIResearchFramework.TechBeat> techBeatList = new List<AIResearchFramework.TechBeat>();
			foreach (AIResearchFramework.TechBeat techBeat in sorted)
			{
				techBeatList.Add(techBeat);
				if (num1 == 0)
				{
					techBeatList.Add(techBeat);
					techBeatList.Add(techBeat);
					num2 = (double)AIResearchFramework.AICalcTechCost(ai, techBeat.Tech);
				}
				if ((double)AIResearchFramework.AICalcTechCost(ai, techBeat.Tech) == num2)
					++num1;
				else
					break;
			}
			return techBeatList;
		}

		private void LogProspects(
		  StrategicAI ai,
		  IList<AIResearchModes> phasedRhythm,
		  IEnumerable<AIResearchFramework.TechBeat> prospects)
		{
			if (this._log == null)
				return;
			foreach (AIResearchFramework.TechBeat prospect in prospects)
				this._log.Print(string.Format("{0}/{1} ({2}{3}); ", (object)AIResearchFramework.AIGetResearchModeSymbol(phasedRhythm[prospect.Beat]), (object)prospect.Tech.Id, (object)AIResearchFramework.AICalcTechCost(ai, prospect.Tech), this.AIIsShortTermTech(ai, prospect.Tech) ? (object)"" : (object)"*"));
		}

		private Tech AISelectDefaultTech(
		  StrategicAI ai,
		  List<PlayerTechInfo> desiredTech = null,
		  Dictionary<string, int> familyWeights = null)
		{
			if (this._log != null)
			{
				this._log.Print("{");
				bool flag = true;
				foreach (AITechStyleInfo techStyleInfo in ai.TechStyles.TechStyleInfos)
				{
					if (!flag)
						this._log.Print(",");
					else
						flag = false;
					this._log.Print(techStyleInfo.TechFamily.ToString());
				}
				this._log.Print("}");
			}
			Tech tech1 = (Tech)null;
			if (AIResearchFramework.AIGetResearchingTech(ai) == null && tech1 == null)
			{
				AIStance stance = this.AIGetStance(ai);
				int phase = this.AIGetPhase(ai);
				List<AIResearchModes> phasedRhythm = this.AIGetPhasedRhythm(this.AIGetRhythm(stance), phase);
				IEnumerable<AIResearchFramework.TechBeat> prospects = this.AIGetProspects(ai, this.AIGetTechBeats(this.AIGetAvailableTechs(ai), phasedRhythm, (List<PlayerTechInfo>)null));
				List<AIResearchFramework.TechBeat> culledProspects = this.AIGetCulledProspects(ai, prospects);
				if (this._log != null)
				{
					this._log.Print(string.Format(" (phase {0}) {1}/{2} prospects: ", (object)phase, (object)AIResearchFramework.AIGetResearchModeSymbol(phasedRhythm[0]), (object)stance));
					this.LogProspects(ai, (IList<AIResearchModes>)phasedRhythm, (IEnumerable<AIResearchFramework.TechBeat>)culledProspects);
					if (App.Log.Level >= Kerberos.Sots.Engine.LogLevel.Verbose)
					{
						this._log.Print(string.Format(" ... (phase {0}) {1}/{2} ALL prospects: ", (object)phase, (object)AIResearchFramework.AIGetResearchModeSymbol(phasedRhythm[0]), (object)stance));
						this.LogProspects(ai, (IList<AIResearchModes>)phasedRhythm, prospects);
					}
				}
				if (culledProspects.Any<AIResearchFramework.TechBeat>())
				{
					if (desiredTech != null && culledProspects.Any<AIResearchFramework.TechBeat>((Func<AIResearchFramework.TechBeat, bool>)(x => desiredTech.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(y => y.TechFileID == x.Tech.Id)))))
						culledProspects.RemoveAll((Predicate<AIResearchFramework.TechBeat>)(x => !desiredTech.Any<PlayerTechInfo>((Func<PlayerTechInfo, bool>)(y => y.TechFileID == x.Tech.Id))));
					if (familyWeights != null)
					{
						List<Weighted<Tech>> weightedList = new List<Weighted<Tech>>();
						foreach (Tech tech2 in culledProspects.Select<AIResearchFramework.TechBeat, Tech>((Func<AIResearchFramework.TechBeat, Tech>)(x => x.Tech)).ToList<Tech>())
						{
							int num;
							if (familyWeights.TryGetValue(tech2.Family, out num))
								weightedList.Add(new Weighted<Tech>()
								{
									Value = tech2,
									Weight = num
								});
						}
						if (weightedList.Count > 0)
							tech1 = WeightedChoices.Choose<Tech>(ai.Random, (IEnumerable<Weighted<Tech>>)weightedList);
					}
					if (tech1 == null)
						tech1 = ai.Random.Choose<AIResearchFramework.TechBeat>((IList<AIResearchFramework.TechBeat>)culledProspects).Tech;
				}
			}
			return tech1;
		}

		private static int AIGetTechProgress(StrategicAI ai, Tech tech)
		{
			PlayerTechInfo playerTechInfo = AIResearchFramework.AIGetPlayerTechInfo(ai, tech);
			if (playerTechInfo == null)
				return 0;
			return playerTechInfo.Progress;
		}

		private Tech AISelectPartialTech(StrategicAI ai)
		{
			Tech tech1 = (Tech)null;
			if (AIResearchFramework.AIGetResearchingTech(ai) == null)
			{
				List<Tech> techList = AIResearchFramework.AISelectAvailableTechs(ai, new AIResearchModes?(), new TechFamilies?());
				if (techList.Count > 0)
				{
					int num = 0;
					foreach (Tech tech2 in techList)
					{
						int techProgress = AIResearchFramework.AIGetTechProgress(ai, tech2);
						if (techProgress > num)
						{
							num = techProgress;
							tech1 = tech2;
						}
					}
				}
			}
			if (this._log != null && tech1 != null)
				this._log.Print(string.Format("Resuming {0}", (object)tech1.Id));
			return tech1;
		}

		private static bool AICanSelectBinaryResponseTech(StrategicAI ai)
		{
			Tech researchingTech = AIResearchFramework.AIGetResearchingTech(ai);
			if (researchingTech != null)
				return AIResearchFramework.AIGetTurnsToComplete(ai, researchingTech) > 5;
			return true;
		}

		private static Tech AISelectBinaryResponseTech(StrategicAI ai)
		{
			Tech tech = (Tech)null;
			AIResearchFramework.AICanSelectBinaryResponseTech(ai);
			return tech;
		}

		private Tech AIBaseSelectNextTech(
		  StrategicAI ai,
		  List<PlayerTechInfo> desiredTech = null,
		  Dictionary<string, int> familyWeights = null)
		{
			Tech tech = (((Tech)null ?? this.AISelectPartialTech(ai)) ?? AIResearchFramework.AISelectBinaryResponseTech(ai)) ?? this.AISelectDefaultTech(ai, desiredTech, familyWeights);
			Tech researchingTech = AIResearchFramework.AIGetResearchingTech(ai);
			if (tech != null && tech != researchingTech)
			{
				if (this._log != null)
				{
					if (researchingTech != null)
						this._log.Print(string.Format("\n          >>> {0} (replacing {1})\n", (object)tech.Id, (object)researchingTech.Id));
					else
						this._log.Print(string.Format("\n          >>> {0}\n", (object)tech.Id));
				}
				return tech;
			}
			if (this._log != null)
				this._log.ClearRecord();
			return (Tech)null;
		}

		private void PrepareLog()
		{
			if (this._log != null || !ScriptHost.AllowConsole)
				return;
			this._log = new AIResearchFramework.AITechLog();
		}

		public Tech AISelectNextTech(
		  StrategicAI ai,
		  List<PlayerTechInfo> desiredTech = null,
		  Dictionary<string, int> familyWeights = null)
		{
			this.PrepareLog();
			if (this._log != null)
				this._log.BeginTurn(ai);
			try
			{
				return this.AIBaseSelectNextTech(ai, desiredTech, familyWeights);
			}
			finally
			{
				if (this._log != null)
					this._log.EndTurn();
			}
		}

		internal void TestTechStyleSelection(string factionName, int iterations)
		{
			Random random = new Random();
			for (int index = 0; index < iterations; ++index)
			{
				App.Log.Trace("--- TestTechStyleSelection (" + factionName + ") " + (object)(index + 1) + "/" + (object)iterations, "ai");
				foreach (object obj in this.AISelectTechStylesCore(1, random, factionName))
					App.Log.Trace(obj.ToString(), "ai");
			}
		}

		private List<AITechStyleInfo> AISelectTechStylesCore(
		  int playerId,
		  Random random,
		  string factionName)
		{
			this.PrepareLog();
			List<AITechStyleInfo> aiTechStyleInfoList = new List<AITechStyleInfo>();
			foreach (AIResearchFramework.AITechStyleGroup styleGroup in this._styleGroups)
			{
				float num1 = 0.0f;
				List<Weighted<AIResearchFramework.AITechStyleRow>> weightedList = new List<Weighted<AIResearchFramework.AITechStyleRow>>();
				foreach (AIResearchFramework.AITechStyleRow style in styleGroup.Styles)
				{
					float num2 = style.SelectionChances.Faction(factionName);
					num1 += num2;
					weightedList.Add(new Weighted<AIResearchFramework.AITechStyleRow>(style, (int)((double)num2 * 1000.0)));
				}
				float num3 = 100f - num1;
				if ((double)num3 >= 1.0 / 1000.0)
					weightedList.Add(new Weighted<AIResearchFramework.AITechStyleRow>((AIResearchFramework.AITechStyleRow)null, (int)((double)num3 * 1000.0)));
				AIResearchFramework.AITechStyleRow aiTechStyleRow = WeightedChoices.Choose<AIResearchFramework.AITechStyleRow>(random, (IEnumerable<Weighted<AIResearchFramework.AITechStyleRow>>)weightedList);
				if (aiTechStyleRow != null)
					aiTechStyleInfoList.AddRange(aiTechStyleRow.ToTechStyleInfos(playerId, factionName));
			}
			return aiTechStyleInfoList;
		}

		public List<AITechStyleInfo> AISelectTechStyles(
		  StrategicAI ai,
		  Faction faction)
		{
			return this.AISelectTechStylesCore(ai.Player.ID, ai.Random, faction.Name);
		}

		private class AITechLog
		{
			private readonly StringBuilder _recordText = new StringBuilder();
			private string _prefix;
			private bool _writingRecord;

			public void ClearRecord()
			{
				this._recordText.Clear();
			}

			public void BeginTurn(StrategicAI ai)
			{
				this._writingRecord = true;
				this._prefix = string.Format("Turn {0} ({1}): ", (object)ai.Game.GameDatabase.GetTurnCount(), (object)ai.Player.ID);
			}

			public void EndTurn()
			{
				if (this._writingRecord && this._recordText.Length > 0)
					App.Log.Trace(this._prefix + this._recordText.ToString(), "ai");
				this._prefix = null;
				this._writingRecord = false;
				this.ClearRecord();
			}

			public void Print(string text)
			{
				if (!this._writingRecord)
					return;
				this._recordText.Append(text);
			}
		}

		private enum AITechReplacementContexts
		{
			Available,
			Researched,
		}

		private class AITechReplacementRow
		{
			public Tech New;
			public Tech Old;
			public AIResearchFramework.AITechReplacementContexts Contexts;
		}

		private class AITechStyleRow
		{
			public TechFamilies[] Families;
			public float CostFactor;
			public AICostFactors SelectionChances;

			public override string ToString()
			{
				string empty = string.Empty;
				bool flag = true;
				foreach (TechFamilies family in this.Families)
				{
					if (!flag)
						empty += ",";
					else
						flag = false;
					empty += family.ToString();
				}
				return empty;
			}

			public IEnumerable<AITechStyleInfo> ToTechStyleInfos(
			  int playerId,
			  string faction)
			{
				foreach (TechFamilies family in this.Families)
					yield return new AITechStyleInfo()
					{
						PlayerID = playerId,
						TechFamily = family,
						CostFactor = this.CostFactor
					};
			}

			private static AICostFactors ReadChances(BinaryReader r)
			{
				AICostFactors zero = AICostFactors.Zero;
				int num1 = r.ReadInt32();
				for (int index = 0; index < num1; ++index)
				{
					string faction = r.ReadString();
					float num2 = r.ReadSingle();
					zero.SetFaction(faction, num2);
				}
				return zero;
			}

			private static void WriteChances(AICostFactors value, BinaryWriter w)
			{
				w.Write(AICostFactors.Factions.Count);
				foreach (string faction in (IEnumerable<string>)AICostFactors.Factions)
				{
					w.Write(faction);
					w.Write(value.Faction(faction));
				}
			}

			public static AIResearchFramework.AITechStyleRow Read(BinaryReader r)
			{
				int length = r.ReadInt32();
				TechFamilies[] techFamiliesArray = new TechFamilies[length];
				for (int index = 0; index < length; ++index)
					techFamiliesArray[index] = (TechFamilies)Enum.Parse(typeof(TechFamilies), r.ReadString());
				float num = r.ReadSingle();
				AICostFactors aiCostFactors = AIResearchFramework.AITechStyleRow.ReadChances(r);
				return new AIResearchFramework.AITechStyleRow()
				{
					CostFactor = num,
					Families = techFamiliesArray,
					SelectionChances = aiCostFactors
				};
			}

			public static void Write(AIResearchFramework.AITechStyleRow value, BinaryWriter w)
			{
				w.Write(value.Families.Length);
				foreach (TechFamilies family in value.Families)
					w.Write(family.ToString());
				w.Write(value.CostFactor);
				AIResearchFramework.AITechStyleRow.WriteChances(value.SelectionChances, w);
			}

			private static TechFamilies[] ParseFamilies(XmlElement e)
			{
				if (e == null)
					return new TechFamilies[0];
				return e.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "family")).Select<XmlElement, TechFamilies>((Func<XmlElement, TechFamilies>)(y => (TechFamilies)Enum.Parse(typeof(TechFamilies), y.GetAttribute("value")))).ToArray<TechFamilies>();
			}

			private static AICostFactors ParseChances(XmlElement e)
			{
				if (e == null)
					return AICostFactors.Zero;
				AICostFactors zero = AICostFactors.Zero;
				foreach (XmlElement xmlElement in e.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "chance")))
					zero.SetFaction(xmlElement.GetAttribute("faction"), float.Parse(xmlElement.GetAttribute("value")));
				return zero;
			}

			public static AIResearchFramework.AITechStyleRow Parse(XmlElement e)
			{
				return new AIResearchFramework.AITechStyleRow()
				{
					CostFactor = float.Parse(e.GetAttribute("costmul")),
					Families = AIResearchFramework.AITechStyleRow.ParseFamilies(e["families"]),
					SelectionChances = AIResearchFramework.AITechStyleRow.ParseChances(e["chances"])
				};
			}
		}

		private class AITechStyleGroup
		{
			public List<AIResearchFramework.AITechStyleRow> Styles;

			public static AIResearchFramework.AITechStyleGroup Read(BinaryReader r)
			{
				List<AIResearchFramework.AITechStyleRow> aiTechStyleRowList = new List<AIResearchFramework.AITechStyleRow>();
				int num = r.ReadInt32();
				for (int index = 0; index < num; ++index)
					aiTechStyleRowList.Add(AIResearchFramework.AITechStyleRow.Read(r));
				return new AIResearchFramework.AITechStyleGroup()
				{
					Styles = aiTechStyleRowList
				};
			}

			public static void Write(AIResearchFramework.AITechStyleGroup value, BinaryWriter w)
			{
				w.Write(value.Styles.Count);
				foreach (AIResearchFramework.AITechStyleRow style in value.Styles)
					AIResearchFramework.AITechStyleRow.Write(style, w);
			}

			public static AIResearchFramework.AITechStyleGroup Parse(XmlElement e)
			{
				return new AIResearchFramework.AITechStyleGroup()
				{
					Styles = new List<AIResearchFramework.AITechStyleRow>(e.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "style")).Select<XmlElement, AIResearchFramework.AITechStyleRow>((Func<XmlElement, AIResearchFramework.AITechStyleRow>)(y => AIResearchFramework.AITechStyleRow.Parse(y))))
				};
			}
		}

		private class AIResearchRhythm
		{
			public AIResearchModes[] Beats;
		}

		private struct TechBeat
		{
			public Tech Tech;
			public int Beat;

			public override string ToString()
			{
				return this.Tech.ToString() + "," + this.Beat.ToString();
			}
		}
	}
}
