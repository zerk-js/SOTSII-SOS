// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestShipsState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using System.Xml;

namespace Kerberos.Sots.GameStates
{
	internal class TestShipsState : CommonCombatState
	{
		private XmlDocument _config;

		public TestShipsState(App game)
		  : base(game)
		{
		}

		protected override void OnCombatEnding()
		{
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			this._config = new XmlDocument();
			this._config.Load(ScriptHost.FileSystem, "data\\TestShipsConfig.xml");
			int num = 0;
			base.OnPrepare(prev, new object[3]
			{
		(object) num,
		(object) this._config,
		(object) true
			});
		}

		protected override void OnEnter()
		{
			base.OnEnter();
		}

		protected override void OnExit(GameState prev, ExitReason reason)
		{
			base.OnExit(prev, reason);
			this._config = (XmlDocument)null;
		}

		protected override void SyncPlayerList()
		{
		}

		public override void OnEngineMessage(InteropMessageID messageID, ScriptMessageReader mr)
		{
		}
	}
}
