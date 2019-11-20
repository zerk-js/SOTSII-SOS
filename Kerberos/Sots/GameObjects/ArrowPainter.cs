// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.ArrowPainter
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_ARROWPAINTER)]
	internal class ArrowPainter : GameObject, IActive, IDisposable
	{
		private bool _active;
		private APStyle _style;

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

		public APStyle Style
		{
			get
			{
				return this._style;
			}
			set
			{
				if (value == this._style)
					return;
				this._style = value;
				this.PostSetProp("SetStyle", (object)this._style);
			}
		}

		public void AddSection(List<Vector3> path, APStyle style, int special = 0, Vector3? Color = null)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)path.Count<Vector3>());
			foreach (Vector3 vector3 in path)
				objectList.Add((object)vector3);
			objectList.Add((object)(int)style);
			objectList.Add((object)special);
			Color = Color.HasValue ? Color : new Vector3?(Vector3.Zero);
			objectList.Add((object)Color);
			this.PostSetProp(nameof(AddSection), objectList.ToArray());
		}

		public void ClearSections()
		{
			this.PostSetProp(nameof(ClearSections));
		}

		public ArrowPainter(App game)
		{
			game.AddExistingObject((IGameObject)this);
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.ReleaseObject((IGameObject)this);
		}
	}
}
