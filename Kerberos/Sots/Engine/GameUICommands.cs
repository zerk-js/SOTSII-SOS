// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.GameUICommands
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kerberos.Sots.Engine
{
	internal class GameUICommands
	{
		private readonly List<UICommand> _commands = new List<UICommand>();
		public readonly UICommand EndTurn;
		public readonly UICommand Exit;

		public GameUICommands(App game)
		{
			GameUICommands gameUiCommands = this;
			this.EndTurn = new UICommand(nameof(EndTurn), (Action)(() => game.EndTurn()), (Action<IUIPollCommandState>)(cmd => cmd.IsEnabled = game.CanEndTurn()));
			this.Exit = new UICommand(nameof(Exit), (Action)(() => game.RequestExit()), (Action<IUIPollCommandState>)(cmd => cmd.IsEnabled = true));
			this._commands.AddRange(((IEnumerable<FieldInfo>)this.GetType().GetFields()).Select<FieldInfo, UICommand>((Func<FieldInfo, UICommand>)(x => x.GetValue((object)this) as UICommand)).Where<UICommand>((Func<UICommand, bool>)(x => x != null)));
		}

		public void Poll()
		{
			this._commands.Poll();
		}
	}
}
