// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.ResearchState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.GameStates
{
	[GameObjectType(InteropGameObjectType.IGOT_RESEARCHSTATE)]
	internal class ResearchState : GameObject, IActive
	{
		public bool Active
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				this.PostSetActive(true);
			}
		}

		public void NextTechFamily()
		{
			this.PostSetProp("SelectNextFamily");
		}

		public void PrevTechFamily()
		{
			this.PostSetProp("SelectPrevFamily");
		}

		public void RebindModels()
		{
			this.PostSetProp(nameof(RebindModels));
		}

		public void Clear()
		{
			this.PostSetProp(nameof(Clear));
		}
	}
}
