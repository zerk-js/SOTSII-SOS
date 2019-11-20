// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.UICommand
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;

namespace Kerberos.Sots.Engine
{
	public class UICommand
	{
		private bool _isEnabled = true;
		private bool? _isChecked = new bool?(false);
		private readonly Action _trigger;
		private readonly Action<IUIPollCommandState> _poll;

		public string Name { get; private set; }

		public event Action<UICommand> IsEnabledChanged;

		public event Action<UICommand> IsCheckedChanged;

		internal void Poll()
		{
			this._poll((IUIPollCommandState)new UICommand.UIPollCommandState()
			{
				Command = this
			});
		}

		public bool IsEnabled
		{
			get
			{
				return this._isEnabled;
			}
			internal set
			{
				if (this._isEnabled == value)
					return;
				this._isEnabled = value;
				if (this.IsEnabledChanged == null)
					return;
				this.IsEnabledChanged(this);
			}
		}

		public bool? IsChecked
		{
			get
			{
				return this._isChecked;
			}
			internal set
			{
				bool? isChecked = this._isChecked;
				bool? nullable = value;
				if ((isChecked.GetValueOrDefault() != nullable.GetValueOrDefault() ? 0 : (isChecked.HasValue == nullable.HasValue ? 1 : 0)) != 0)
					return;
				this._isChecked = value;
				if (this.IsCheckedChanged == null)
					return;
				this.IsCheckedChanged(this);
			}
		}

		public void Trigger()
		{
			this.Poll();
			if (!this._isEnabled)
				return;
			this._trigger();
		}

		public UICommand(string name, Action trigger, Action<IUIPollCommandState> poll)
		{
			this.Name = name;
			this._trigger = trigger;
			this._poll = poll;
		}

		private class UIPollCommandState : IUIPollCommandState
		{
			internal UICommand Command { private get; set; }

			public bool? IsChecked
			{
				set
				{
					this.Command.IsChecked = value;
				}
			}

			public bool IsEnabled
			{
				set
				{
					this.Command.IsEnabled = value;
				}
			}
		}
	}
}
