// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameStates.StarMapStateMode
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Strategy;

namespace Kerberos.Sots.GameStates
{
	internal class StarMapStateMode
	{
		protected App App { get; private set; }

		protected GameSession Sim { get; private set; }

		protected StarMapStateMode(GameSession sim)
		{
			this.Sim = sim;
			this.App = sim.App;
		}

		public virtual void Initialize()
		{
		}

		public virtual void Terminate()
		{
		}

		public virtual bool OnGameObjectClicked(IGameObject obj)
		{
			return false;
		}

		public virtual bool OnGameObjectMouseOver(IGameObject obj)
		{
			return false;
		}

		public virtual bool OnUIButtonPressed(string panelName)
		{
			return false;
		}

		public virtual bool OnUIDialogClosed(string panelName, string[] msgParams)
		{
			return false;
		}
	}
}
