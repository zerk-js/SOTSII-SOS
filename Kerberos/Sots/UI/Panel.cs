// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Panel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using System;

namespace Kerberos.Sots.UI
{
	internal abstract class Panel : PanelBinding, IDisposable
	{
		private bool _disposed;
		private bool _needsDispose;

		protected Panel(UICommChannel ui, string id)
		  : this(ui, id, null)
		{
		}

		protected Panel(UICommChannel ui, string id, string createFromTemplateID)
		  : base(ui, id)
		{
			if (createFromTemplateID != null)
			{
				if (createFromTemplateID == string.Empty)
					throw new ArgumentNullException(nameof(createFromTemplateID));
				this.SetID(ui.CreatePanelFromTemplate(createFromTemplateID, id));
				this._needsDispose = true;
			}
			else if (string.IsNullOrEmpty(id))
				throw new ArgumentNullException(nameof(id), "If no panel template is specified, a valid panel ID must be provided");
		}

		protected virtual void OnDisposing()
		{
		}

		public void Dispose()
		{
			if (!this._needsDispose)
				App.Log.Warn("Tried to dispose panel '" + this.ToString() + "' that doesn't have ownership of the underlying object.", "gui");
			else if (this._disposed)
			{
				App.Log.Warn("Panel '" + this.ToString() + "' has already been disposed.", "gui");
			}
			else
			{
				try
				{
					this.OnDisposing();
				}
				finally
				{
					this.UI.DestroyPanel(this.ID);
					this._disposed = true;
				}
			}
		}
	}
}
