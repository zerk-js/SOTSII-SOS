// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.IntelMissionDescMap
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal sealed class IntelMissionDescMap : IEnumerable<IntelMissionDesc>, IEnumerable
	{
		private static readonly IntelMissionDesc[] NoItems = new IntelMissionDesc[0];
		private readonly IntelMissionDesc[] _items;
		private readonly Dictionary<IntelMission, IntelMissionDesc> _byID;
		private readonly Dictionary<TurnEventType, HashSet<IntelMissionDesc>> _byTurnEvent;

		public IntelMissionDesc Choose(Random random)
		{
			return this.ByID(random.Choose<IntelMission>((IEnumerable<IntelMission>)this._byID.Keys));
		}

		public IEnumerable<IntelMissionDesc> ByTurnEventType(
		  TurnEventType value)
		{
			HashSet<IntelMissionDesc> intelMissionDescSet;
			if (this._byTurnEvent.TryGetValue(value, out intelMissionDescSet))
				return (IEnumerable<IntelMissionDesc>)intelMissionDescSet;
			return (IEnumerable<IntelMissionDesc>)IntelMissionDescMap.NoItems;
		}

		private IntelMissionDesc ByID(IntelMission value)
		{
			IntelMissionDesc intelMissionDesc = (IntelMissionDesc)null;
			this._byID.TryGetValue(value, out intelMissionDesc);
			return intelMissionDesc;
		}

		public IntelMissionDescMap()
		{
			this._items = new IntelMissionDesc[4]
			{
		(IntelMissionDesc) new IntelMissionDesc_RandomSystem(),
		(IntelMissionDesc) new IntelMissionDesc_HighestTradeSystem(),
		(IntelMissionDesc) new IntelMissionDesc_NewestColonySystem(),
		(IntelMissionDesc) new IntelMissionDesc_CurrentResearch()
			};
			this._byID = new Dictionary<IntelMission, IntelMissionDesc>();
			this._byTurnEvent = new Dictionary<TurnEventType, HashSet<IntelMissionDesc>>();
			foreach (IntelMissionDesc intelMissionDesc in this._items)
			{
				this._byID.Add(intelMissionDesc.ID, intelMissionDesc);
				foreach (TurnEventType turnEventType in intelMissionDesc.TurnEventTypes)
				{
					HashSet<IntelMissionDesc> intelMissionDescSet;
					if (!this._byTurnEvent.TryGetValue(turnEventType, out intelMissionDescSet))
					{
						intelMissionDescSet = new HashSet<IntelMissionDesc>();
						this._byTurnEvent.Add(turnEventType, intelMissionDescSet);
					}
					intelMissionDescSet.Add(intelMissionDesc);
				}
			}
		}

		public IEnumerator<IntelMissionDesc> GetEnumerator()
		{
			return ((IEnumerable<IntelMissionDesc>)this._items).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._items.GetEnumerator();
		}
	}
}
