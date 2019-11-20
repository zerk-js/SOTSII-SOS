// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.ShipFramework.ShipSparksLibrary
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.ShipFramework
{
	internal static class ShipSparksLibrary
	{
		public static IEnumerable<LogicalShipSpark> Enumerate(XmlDocument doc)
		{
			XmlElement listnode = doc["CommonAssets"];
			XmlElement sparkNodes = listnode["ShipSparks"];
			foreach (XmlElement xmlElement in sparkNodes.OfType<XmlElement>())
				yield return new LogicalShipSpark()
				{
					Type = (LogicalShipSpark.ShipSparkType)Enum.Parse(typeof(LogicalShipSpark.ShipSparkType), xmlElement.GetAttribute("type")),
					SparkEffect = new LogicalEffect()
					{
						Name = xmlElement.GetAttribute("effect") ?? string.Empty
					}
				};
		}
	}
}
