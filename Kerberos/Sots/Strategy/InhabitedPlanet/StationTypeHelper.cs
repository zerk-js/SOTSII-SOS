// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.InhabitedPlanet.StationTypeHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

namespace Kerberos.Sots.Strategy.InhabitedPlanet
{
	internal static class StationTypeHelper
	{
		public static StationTypeFlags ToFlags(this StationType value)
		{
			return (StationTypeFlags)(1 << (int)(value & (StationType)31));
		}

		public static StationType ToType(this StationTypeFlags value)
		{
			for (int index = 0; index < 8; ++index)
			{
				StationType stationType = (StationType)index;
				StationTypeFlags flags = stationType.ToFlags();
				if ((flags & value) == flags)
					return stationType;
			}
			return StationType.INVALID_TYPE;
		}
	}
}
