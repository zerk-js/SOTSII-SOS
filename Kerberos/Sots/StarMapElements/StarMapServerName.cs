// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapServerName
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPSERVERNAME)]
	internal class StarMapServerName : StarMapObject
	{
		public StarMapServerName(App game, Vector3 origin, string label)
		{
			game.AddExistingObject((IGameObject)this);
			this.SetPosition(origin);
			this.PostSetProp("Label", label);
		}
	}
}
