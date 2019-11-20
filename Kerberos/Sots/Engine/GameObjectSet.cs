// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.GameObjectSet
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Engine
{
	internal sealed class GameObjectSet : IDisposable, IEnumerable<IGameObject>, IEnumerable
	{
		private List<IGameObject> _pending = new List<IGameObject>();
		private bool _disposed;

		public App App { get; set; }

		public T Add<T>(params object[] initParams) where T : IGameObject
		{
			T obj = this.App.AddObject<T>(initParams);
			this._pending.Add((IGameObject)obj);
			return obj;
		}

		public void Add(IGameObject value)
		{
			if (value == null)
				throw new ArgumentNullException(nameof(value));
			if (this._pending.Contains(value))
				return;
			this._pending.Add(value);
		}

		public void Add(IEnumerable<IGameObject> range)
		{
			foreach (IGameObject gameObject in range)
				this.Add(gameObject);
		}

		public void Remove(IGameObject value)
		{
			this._pending.Remove(value);
		}

		public IEnumerable<IGameObject> Objects
		{
			get
			{
				return (IEnumerable<IGameObject>)this._pending;
			}
		}

		public bool IsReady()
		{
			return this._pending.All<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectStatus != GameObjectStatus.Pending));
		}

		public bool AnyFailed()
		{
			return this._pending.Any<IGameObject>((Func<IGameObject, bool>)(x => x.ObjectStatus == GameObjectStatus.Failed));
		}

		public GameObjectStatus CheckStatus()
		{
			foreach (IGameObject gameObject in this._pending)
			{
				GameObjectStatus objectStatus = gameObject.ObjectStatus;
				if (objectStatus != GameObjectStatus.Ready)
					return objectStatus;
			}
			return GameObjectStatus.Ready;
		}

		public void Clear(bool releaseObjects)
		{
			if (releaseObjects)
			{
				foreach (IGameObject gameObject in this._pending)
				{
					if (gameObject is IDisposable)
						(gameObject as IDisposable).Dispose();
					else
						this.App.ReleaseObject(gameObject);
				}
			}
			this._pending.Clear();
		}

		public void Dispose()
		{
			if (this._disposed)
				return;
			this.Clear(true);
			this._disposed = true;
		}

		public GameObjectSet(App game)
		{
			this.App = game;
		}

		public void Activate()
		{
			foreach (IGameObject gameObject in this._pending)
			{
				if (gameObject is IActive)
					(gameObject as IActive).Active = true;
			}
		}

		public void Deactivate()
		{
			foreach (IGameObject gameObject in this._pending)
			{
				if (gameObject is IActive)
					(gameObject as IActive).Active = false;
			}
		}

		public IGameObject GetObjectRef(int objectID)
		{
			foreach (IGameObject gameObject in this._pending)
			{
				if (objectID == gameObject.ObjectID)
					return gameObject;
			}
			return (IGameObject)null;
		}

		public IEnumerator<IGameObject> GetEnumerator()
		{
			return (IEnumerator<IGameObject>)this._pending.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)this._pending.GetEnumerator();
		}
	}
}
