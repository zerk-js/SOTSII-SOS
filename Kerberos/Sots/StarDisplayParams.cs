// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarDisplayParams
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;

namespace Kerberos.Sots
{
	internal class StarDisplayParams
	{
		public string AssetPath { get; set; }

		public Vector3 ImposterColor { get; set; }

		public StarDisplayParams()
		{
			this.AssetPath = string.Empty;
			this.ImposterColor = DefaultStarModelParameters.ImposterColor;
		}
	}
}
