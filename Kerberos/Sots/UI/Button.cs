// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Button
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal class Button : Panel
	{
		public event EventHandler Clicked;

		protected virtual void OnClicked()
		{
		}

		public Button(UICommChannel ui, string id, string createFromTemplateID = null)
		  : base(ui, id, createFromTemplateID)
		{
		}

		protected override void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
			base.OnPanelMessage(panelId, msgType, msgParams);
			if (!(msgType == "button_clicked"))
				return;
			this.OnClicked();
			if (this.Clicked == null)
				return;
			this.Clicked((object)this, EventArgs.Empty);
		}
	}
}
