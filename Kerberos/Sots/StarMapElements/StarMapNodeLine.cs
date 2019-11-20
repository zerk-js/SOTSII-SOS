// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapNodeLine
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPNODELINE)]
	internal class StarMapNodeLine : StarMapObject
	{
		public StarMapNodeLine(App game, Vector3 from, Vector3 to)
		{
			game.AddExistingObject((IGameObject)this);
			this.PostSetProp("Points", (object)from, (object)to);
		}
	}
}
