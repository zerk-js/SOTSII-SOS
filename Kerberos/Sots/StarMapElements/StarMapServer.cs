// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StarMapServer
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.StarMapElements
{
	[GameObjectType(InteropGameObjectType.IGOT_STARMAPSERVER)]
	internal class StarMapServer : StarMapObject
	{
		public StarMapServer(
		  App game,
		  Vector3 position,
		  string name,
		  string map,
		  string version,
		  int players,
		  int maxPlayers,
		  int ping,
		  bool passworded)
		{
			game.AddExistingObject((IGameObject)this, (object)maxPlayers, (object)passworded);
			this.PostSetPosition(position);
			string str1 = "Players: " + (object)players + "/" + (object)maxPlayers;
			string str2 = "Map: " + map;
			string str3 = "Ping: " + (object)ping;
			string str4 = "Version: " + version;
			this.PostSetProp("MultiLabel", (object)name, (object)4, (object)str2, (object)str4, (object)str1, (object)str3);
		}
	}
}
