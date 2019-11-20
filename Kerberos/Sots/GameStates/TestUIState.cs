// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestUIState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System.IO;

namespace Kerberos.Sots.GameStates
{
	internal class TestUIState : GameState
	{
		private GameObjectSet _crits;

		public override bool IsScreenState
		{
			get
			{
				return false;
			}
		}

		protected override void OnPrepare(GameState prev, object[] parms)
		{
			this._crits = new GameObjectSet(this.App);
		}

		protected override void UICommChannel_OnPanelMessage(
		  string panelName,
		  string msgType,
		  string[] msgParams)
		{
			base.UICommChannel_OnPanelMessage(panelName, msgType, msgParams);
			if (!(msgType == "button_clicked"))
				return;
			string strB = "gameReloadScreen_";
			if (string.Compare(panelName, 0, strB, 0, strB.Length) != 0)
				return;
			AssetDatabase.CommonStrings.Reload();
			string str = panelName.Substring(strB.Length);
			this.App.UI.Send((object)"ReloadScreen", (object)str);
			if (!(str == "TestDegrassi"))
				return;
			this.App.UI.AddItem("eventList", "", 0, "");
			string itemGlobalId = this.App.UI.GetItemGlobalID("eventList", "", 0, "");
			this.App.UI.AddItem(this.App.UI.Path(itemGlobalId, "eventItemList"), "", 0, "");
			this.App.UI.SetPropertyString(this.App.UI.Path(this.App.UI.GetItemGlobalID(this.App.UI.Path(itemGlobalId, "eventItemList"), "", 0, ""), "eventInfo"), "text", "Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.");
			this.App.UI.AddItem(this.App.UI.Path(itemGlobalId, "eventItemList"), "", 1, "");
			this.App.UI.SetPropertyString(this.App.UI.Path(this.App.UI.GetItemGlobalID(this.App.UI.Path(itemGlobalId, "eventItemList"), "", 1, ""), "eventInfo"), "text", "holder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous. Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.Probably should have just copied some generic placeholder text, this is getting a little bit ridiculous.");
		}

		protected override void OnEnter()
		{
			this._crits.Add<PanelReference>((object)Path.Combine(this.App.GameRoot, "ui\\screens\\TestUI.xml"), (object)"gameDebugUIControls");
		}

		protected override void OnExit(GameState next, ExitReason reason)
		{
			if (this._crits == null)
				return;
			this._crits.Dispose();
		}

		protected override void OnUpdate()
		{
		}

		public override bool IsReady()
		{
			if (this._crits != null)
				return this._crits.IsReady();
			return false;
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}

		public TestUIState(App game)
		  : base(game)
		{
		}
	}
}
