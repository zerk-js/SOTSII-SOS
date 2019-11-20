// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.TestScratchCombatState
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System.IO;
using System.Xml;

namespace Kerberos.Sots.GameStates
{
	internal class TestScratchCombatState : CombatState
	{
		public TestScratchCombatState(App game)
		  : base(game)
		{
		}

		protected override void OnPrepare(GameState prev, object[] stateParams)
		{
			if (this.App.GameDatabase == null)
				this.App.NewGame();
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(this.App.GameRoot, "data/scratch_combat.xml"));
			base.OnPrepare(prev, new object[3]
			{
		(object) 0,
		(object) xmlDocument,
		(object) true
			});
		}
	}
}
