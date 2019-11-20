// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.FleetManager
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_FLEETMANAGER)]
	internal class FleetManager : GameObject, IActive, IDisposable
	{
		private bool _active;

		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (value == this._active)
					return;
				this._active = value;
				this.PostSetActive(this._active);
			}
		}

		public FleetManager(App game)
		{
			game.AddExistingObject((IGameObject)this);
			game.UI.PanelMessage += new UIEventPanelMessage(this.UICommChannel_OnPanelMessage);
		}

		protected void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			if (!(msgType == "button_clicked") || !panelName.StartsWith("shipbutton"))
				return;
			string[] strArray = panelName.Split('|');
			if (((IEnumerable<string>)strArray).Count<string>() <= 1)
				return;
			this.PostSetProp("SelectShip", int.Parse(strArray[1]));
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
