// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.StarMapElements.StellarProp
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.StarMapFramework;
using Kerberos.Sots.Framework;

namespace Kerberos.Sots.StarMapElements
{
	internal sealed class StellarProp : ILegacyStarMapObject
	{
		Feature ILegacyStarMapObject.Params
		{
			get
			{
				return (Feature)this.Params;
			}
		}

		public StellarBody Params { get; set; }

		public Matrix Transform { get; set; }
	}
}
