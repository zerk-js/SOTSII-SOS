// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.ServerInfo
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.Strategy;
using System.Collections.Generic;

namespace Kerberos.Sots.Data
{
	internal class ServerInfo : IIDProvider
	{
		public ulong serverID;
		public string name;
		public string map;
		public string version;
		public int players;
		public int maxPlayers;
		public int ping;
		public bool passworded;
		public List<PlayerSetup> playerInfo;
		public Vector3 Origin;

		public int ID { get; set; }
	}
}
