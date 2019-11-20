// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Label
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class Label : PanelBinding
	{
		public void SetText(string text)
		{
			this.UI.SetText(this.ID, text);
		}

		public Label(UICommChannel ui, string id)
		  : base(ui, id)
		{
		}
	}
}
