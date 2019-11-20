// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapProvince
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.PlayerFramework;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPPROVINCE)]
	internal class StarMapProvince : StarMapObject
	{
		public StarMapProvince(App game)
		{
			game.AddExistingObject((IGameObject)this);
		}

		public void SetLabel(string value)
		{
			this.PostSetProp("Label", value);
		}

		public void SetCapital(StarMapSystem value)
		{
			this.PostSetProp("Capital", (IGameObject)value);
		}

		public void SetPlayer(Player value)
		{
			this.PostSetProp("Player", (IGameObject)value);
		}
	}
}
