// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.PanelBinding
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.UI
{
	internal abstract class PanelBinding
	{
		private readonly List<PanelBinding> _panels = new List<PanelBinding>();

		protected UICommChannel UI { get; private set; }

		public string ID { get; private set; }

		public string LocalID { get; private set; }

		protected void AddPanels(params PanelBinding[] range)
		{
			this._panels.AddRange((IEnumerable<PanelBinding>)range);
		}

		protected internal void SetID(string value)
		{
			this.ID = value;
			int num = this.ID.LastIndexOf('.');
			if (num == -1)
				this.LocalID = this.ID;
			else
				this.LocalID = this.ID.Substring(num + 1);
		}

		protected PanelBinding(UICommChannel ui, string id)
		{
			if (ui == null)
				throw new ArgumentNullException(nameof(ui));
			if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id));
			this.UI = ui;
			this.SetID(id);
		}

		protected virtual void OnPanelMessage(string panelId, string msgType, string[] msgParams)
		{
		}

		public static bool TryPanelMessage(
		  IEnumerable<PanelBinding> panels,
		  string panelId,
		  string msgType,
		  string[] msgParams,
		  PanelBinding.PanelMessageTargetFlags targetFlags = PanelBinding.PanelMessageTargetFlags.Self)
		{
			foreach (PanelBinding panel in panels)
			{
				if (panel.TryPanelMessage(panelId, msgType, msgParams, targetFlags))
					return true;
			}
			return false;
		}

		public bool TryPanelMessage(
		  string panelId,
		  string msgType,
		  string[] msgParams,
		  PanelBinding.PanelMessageTargetFlags targetFlags = PanelBinding.PanelMessageTargetFlags.Self)
		{
			if ((targetFlags & PanelBinding.PanelMessageTargetFlags.Self) != (PanelBinding.PanelMessageTargetFlags)0 && panelId == this.LocalID)
			{
				this.OnPanelMessage(panelId, msgType, msgParams);
				return true;
			}
			return (targetFlags & PanelBinding.PanelMessageTargetFlags.Recursive) != (PanelBinding.PanelMessageTargetFlags)0 && PanelBinding.TryPanelMessage((IEnumerable<PanelBinding>)this._panels, panelId, msgType, msgParams, targetFlags | PanelBinding.PanelMessageTargetFlags.Self);
		}

		protected virtual void OnGameEvent(string eventName, string[] eventParams)
		{
		}

		public void TryGameEvent(string eventName, string[] eventParams)
		{
			this.OnGameEvent(eventName, eventParams);
		}

		public virtual void SetEnabled(bool value)
		{
			this.UI.SetEnabled(this.ID, value);
		}

		public void SetVisible(bool value)
		{
			this.UI.SetVisible(this.ID, value);
		}

		public override string ToString()
		{
			return this.ID + " (" + this.GetType().ToString() + ")";
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		[Flags]
		public enum PanelMessageTargetFlags
		{
			Self = 1,
			Recursive = 2,
		}
	}
}
