// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Dialog
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal abstract class Dialog : PanelBinding, IDisposable
	{
		protected App _app;

		public string Template { get; private set; }

		public Dialog(App game, string template)
		  : base(game.UI, Guid.NewGuid().ToString())
		{
			this.Template = template;
			this._app = game;
			this._app.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this._app.UI.UpdateEvent += new UIEventUpdate(this.UICommChannel_Update);
		}

		public abstract string[] CloseDialog();

		public virtual void Initialize()
		{
		}

		private void UICommChannel_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (this._app.UI.GetTopDialog() != this)
				return;
			this.OnPanelMessage(panelName, msgType, msgParams);
		}

		protected virtual void OnUpdate()
		{
		}

		private void UICommChannel_Update()
		{
			this.OnUpdate();
		}

		public virtual void Dispose()
		{
			this._app.UI.PanelMessage -= new UIEventPanelMessage(this.UICommChannel_PanelMessage);
			this._app.UI.UpdateEvent -= new UIEventUpdate(this.UICommChannel_Update);
		}

		public virtual void HandleScriptMessage(ScriptMessageReader mr)
		{
		}
	}
}
