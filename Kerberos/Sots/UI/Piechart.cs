// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Piechart
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal class Piechart : PanelBinding
	{
		public void SetSlices(params PiechartSlice[] values)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)nameof(SetSlices));
			objectList.Add((object)this.ID);
			objectList.Add((object)values.Length);
			foreach (PiechartSlice piechartSlice in values)
			{
				objectList.Add((object)piechartSlice.Color);
				objectList.Add((object)(float)(piechartSlice.Fraction * 6.28318548202515));
			}
			this.UI.Send(objectList.ToArray());
		}

		public Piechart(UICommChannel ui, string id)
		  : base(ui, id)
		{
		}
	}
}
