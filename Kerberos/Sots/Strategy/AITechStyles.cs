// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Strategy.AITechStyles
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Data.TechnologyFramework;
using System.Collections.Generic;

namespace Kerberos.Sots.Strategy
{
	internal class AITechStyles
	{
		public IEnumerable<AITechStyleInfo> TechStyleInfos { get; private set; }

		public IEnumerable<Tech> TechUnion { get; private set; }

		public AITechStyles(AssetDatabase assetdb, IEnumerable<AITechStyleInfo> styles)
		{
			this.TechStyleInfos = styles;
			List<Tech> techList = new List<Tech>();
			foreach (Tech technology in assetdb.MasterTechTree.Technologies)
			{
				foreach (AITechStyleInfo style in styles)
				{
					if (assetdb.MasterTechTree.GetTechFamilyEnum(technology) == style.TechFamily)
					{
						techList.Add(technology);
						break;
					}
				}
			}
			this.TechUnion = (IEnumerable<Tech>)techList;
		}
	}
}
