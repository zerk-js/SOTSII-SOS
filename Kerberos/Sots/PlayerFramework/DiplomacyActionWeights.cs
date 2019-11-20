// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.PlayerFramework.DiplomacyActionWeights
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.PlayerFramework
{
	public class DiplomacyActionWeights
	{
		private List<DiplomacyActionWeights.DiplomacyActionWeight> Weights = new List<DiplomacyActionWeights.DiplomacyActionWeight>();

		public DiplomacyActionWeights()
		{
		}

		public DiplomacyActionWeights(XmlDocument doc)
		{
			foreach (XmlElement source in doc[nameof(DiplomacyActionWeights)].OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "DiplomacyReaction")))
			{
				DiplomaticMood diplomaticMood = (DiplomaticMood)Enum.Parse(typeof(DiplomaticMood), source.GetAttribute("value"));
				foreach (XmlElement element in source.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Action")))
				{
					DiplomacyActionWeights.DiplomacyActionWeight weight = new DiplomacyActionWeights.DiplomacyActionWeight();
					weight.Mood = diplomaticMood;
					weight.DiplomacyAction = (DiplomacyAction)Enum.Parse(typeof(DiplomacyAction), element.GetAttribute("id"));
					weight.Value = float.Parse(element.GetAttribute("value"));
					if (DiplomacyActionWeights.RequiresType(weight.DiplomacyAction) && !element.HasAttribute("type"))
						throw new Exception(string.Format("XML node for diplomatic action type: {0} requires a type.", (object)weight.DiplomacyAction.ToString()));
					switch (weight.DiplomacyAction)
					{
						case DiplomacyAction.REQUEST:
							this.ProcessRequest(element, weight);
							break;
						case DiplomacyAction.DEMAND:
							this.ProcessDemand(element, weight);
							break;
						case DiplomacyAction.TREATY:
							this.ProcessTreaty(element, weight);
							break;
						case DiplomacyAction.LOBBY:
							this.ProcessLobby(element, weight);
							break;
					}
					this.Weights.Add(weight);
				}
			}
		}

		public Dictionary<DiplomacyAction, float> GetCumulativeWeights(
		  DiplomaticMood mood)
		{
			Dictionary<DiplomacyAction, float> dictionary = new Dictionary<DiplomacyAction, float>();
			foreach (object obj in Enum.GetValues(typeof(DiplomacyAction)))
			{
				float cumulativeWeight = this.GetCumulativeWeight((DiplomacyAction)obj, mood);
				dictionary.Add((DiplomacyAction)obj, cumulativeWeight);
			}
			return dictionary;
		}

		public float GetCumulativeWeight(DiplomacyAction action, DiplomaticMood mood)
		{
			float num1 = 0.0f;
			float num2;
			switch (action)
			{
				case DiplomacyAction.REQUEST:
					Array values1 = Enum.GetValues(typeof(RequestType));
					foreach (object type in values1)
						num1 += this.GetWeight(action, mood, type);
					num2 = num1 / (float)values1.Length;
					break;
				case DiplomacyAction.DEMAND:
					Array values2 = Enum.GetValues(typeof(DemandType));
					foreach (object type in values2)
						num1 += this.GetWeight(action, mood, type);
					num2 = num1 / (float)values2.Length;
					break;
				case DiplomacyAction.TREATY:
					Array values3 = Enum.GetValues(typeof(TreatyType));
					Array values4 = Enum.GetValues(typeof(LimitationTreatyType));
					foreach (object type in values3)
					{
						if ((TreatyType)type != TreatyType.Limitation)
							num1 += this.GetWeight(action, mood, type);
					}
					foreach (object type in values4)
						num1 += this.GetWeight(action, mood, type);
					num2 = num1 / (float)(values3.Length + values4.Length);
					break;
				case DiplomacyAction.LOBBY:
					Array values5 = Enum.GetValues(typeof(LobbyType));
					foreach (object type in values5)
						num1 += this.GetWeight(action, mood, type);
					num2 = num1 / (float)values5.Length;
					break;
				default:
					num2 = num1 + this.GetWeight(action, mood, (object)null);
					break;
			}
			return num2;
		}

		public Dictionary<object, float> GetWeights(
		  DiplomacyAction action,
		  DiplomaticMood mood)
		{
			Dictionary<object, float> dictionary = new Dictionary<object, float>();
			List<Type> types = DiplomacyActionWeights.GetTypes(action);
			if (types != null)
			{
				foreach (Type enumType in types)
				{
					foreach (object obj in Enum.GetValues(enumType))
						dictionary.Add(obj, this.GetWeight(action, mood, obj));
				}
			}
			else
			{
				float weight = this.GetWeight(action, mood, (object)null);
				dictionary.Add((object)action, weight);
			}
			return dictionary;
		}

		public float GetWeight(DiplomacyAction action, DiplomaticMood mood, object type = null)
		{
			if (DiplomacyActionWeights.RequiresType(action) && type == null)
				throw new Exception("Action requires a type.");
			DiplomacyActionWeights.DiplomacyActionWeight diplomacyActionWeight = type != null ? this.Weights.FirstOrDefault<DiplomacyActionWeights.DiplomacyActionWeight>((Func<DiplomacyActionWeights.DiplomacyActionWeight, bool>)(x =>
		   {
			   if (x.DiplomacyAction == action && x.Mood == mood)
				   return x.Type.Equals(type);
			   return false;
		   })) : this.Weights.FirstOrDefault<DiplomacyActionWeights.DiplomacyActionWeight>((Func<DiplomacyActionWeights.DiplomacyActionWeight, bool>)(x =>
	 {
			   if (x.DiplomacyAction == action)
				   return x.Mood == mood;
			   return false;
		   }));
			if (diplomacyActionWeight == null)
				return 1f;
			return diplomacyActionWeight.Value;
		}

		private static List<Type> GetTypes(DiplomacyAction action)
		{
			List<Type> typeList = new List<Type>();
			switch (action)
			{
				case DiplomacyAction.REQUEST:
					typeList.Add(typeof(RequestType));
					break;
				case DiplomacyAction.DEMAND:
					typeList.Add(typeof(DemandType));
					break;
				case DiplomacyAction.TREATY:
					typeList.Add(typeof(TreatyType));
					typeList.Add(typeof(LimitationTreatyType));
					break;
				case DiplomacyAction.LOBBY:
					typeList.Add(typeof(LobbyType));
					break;
				default:
					return (List<Type>)null;
			}
			return typeList;
		}

		public static DiplomacyAction GetActionFromType(Type type)
		{
			if (type == typeof(LobbyType))
				return DiplomacyAction.LOBBY;
			if (type == typeof(DemandType))
				return DiplomacyAction.DEMAND;
			if (type == typeof(RequestType))
				return DiplomacyAction.REQUEST;
			if (type == typeof(TreatyType) || type == typeof(LimitationTreatyType))
				return DiplomacyAction.TREATY;
			throw new Exception("Unable to determine action for type.");
		}

		public static bool RequiresType(DiplomacyAction action)
		{
			if (action != DiplomacyAction.DEMAND && action != DiplomacyAction.LOBBY && action != DiplomacyAction.REQUEST)
				return action == DiplomacyAction.TREATY;
			return true;
		}

		private void ProcessDemand(
		  XmlElement element,
		  DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			DemandType demandType = (DemandType)Enum.Parse(typeof(DemandType), element.GetAttribute("type"));
			weight.Type = (object)demandType;
		}

		private void ProcessLobby(
		  XmlElement element,
		  DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			LobbyType lobbyType = (LobbyType)Enum.Parse(typeof(LobbyType), element.GetAttribute("type"));
			weight.Type = (object)lobbyType;
		}

		private void ProcessRequest(
		  XmlElement element,
		  DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			RequestType requestType = (RequestType)Enum.Parse(typeof(RequestType), element.GetAttribute("type"));
			weight.Type = (object)requestType;
		}

		private void ProcessTreaty(
		  XmlElement element,
		  DiplomacyActionWeights.DiplomacyActionWeight weight)
		{
			string attribute = element.GetAttribute("type");
			if (attribute.Contains("Limitation"))
			{
				LimitationTreatyType limitationTreatyType = (LimitationTreatyType)Enum.Parse(typeof(LimitationTreatyType), attribute.Replace("Limitation", ""));
				weight.Type = (object)limitationTreatyType;
			}
			else
			{
				int num = (int)Enum.Parse(typeof(TreatyType), attribute);
			}
		}

		public class DiplomacyActionWeight
		{
			public DiplomaticMood Mood;
			public DiplomacyAction DiplomacyAction;
			public float Value;
			public object Type;
		}
	}
}
