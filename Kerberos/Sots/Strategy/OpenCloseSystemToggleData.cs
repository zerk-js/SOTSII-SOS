// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.OpenCloseSystemToggleData
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Strategy
{
	public class OpenCloseSystemToggleData
	{
		private List<OpenCloseSystemInfo> Systems;

		public List<OpenCloseSystemInfo> ToggledSystems
		{
			get
			{
				return this.Systems;
			}
		}

		public OpenCloseSystemToggleData()
		{
			this.Systems = new List<OpenCloseSystemInfo>();
		}

		public void ClearData()
		{
			this.Systems.Clear();
		}

		public void SystemToggled(int playerId, int systemId, bool isOpen)
		{
			OpenCloseSystemInfo openCloseSystemInfo = this.Systems.FirstOrDefault<OpenCloseSystemInfo>((Func<OpenCloseSystemInfo, bool>)(x => x.SystemID == systemId));
			if (openCloseSystemInfo != null)
				this.Systems.Remove(openCloseSystemInfo);
			else
				this.Systems.Add(new OpenCloseSystemInfo()
				{
					PlayerID = playerId,
					SystemID = systemId,
					IsOpen = isOpen
				});
		}
	}
}
