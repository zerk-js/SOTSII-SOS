// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Framework.Sphere
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Framework
{
	internal struct Sphere
	{
		public int player_id;
		public Vector3 center;
		public float radius;

		public Sphere(int p, Vector3 c, float r)
		{
			this.player_id = p;
			this.center = c;
			this.radius = r;
		}
	}
}
