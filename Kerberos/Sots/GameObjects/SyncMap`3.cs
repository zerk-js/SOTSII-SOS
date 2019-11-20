// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.SyncMap`3
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;

namespace Kerberos.Sots.GameObjects
{
	internal class SyncMap<TObject, TInfo, TContext> : BidirMap<TObject, int>
	  where TObject : IGameObject
	  where TInfo : IIDProvider
	  where TContext : class
	{
		private readonly Func<GameObjectSet, TInfo, TContext, TObject> _create;
		private readonly Action<TObject, TInfo, TContext> _update;

		public SyncMap(
		  Func<GameObjectSet, TInfo, TContext, TObject> create,
		  Action<TObject, TInfo, TContext> update)
		{
			this._create = create;
			this._update = update;
		}

		public IEnumerable<TObject> Sync(
		  GameObjectSet gos,
		  IEnumerable<TInfo> all,
		  TContext context,
		  bool updateOnCreate = false)
		{
			List<TObject> objectList = new List<TObject>();
			HashSet<int> intSet = new HashSet<int>((IEnumerable<int>)this.Forward.Values);
			foreach (TInfo info in all)
			{
				intSet.Remove(info.ID);
				TObject f;
				if (this.Reverse.TryGetValue(info.ID, out f))
				{
					if (this._update != null)
						this._update(f, info, context);
				}
				else
				{
					f = this._create(gos, info, context);
					this.Insert(f, info.ID);
					objectList.Add(f);
					if (updateOnCreate && this._update != null)
						this._update(f, info, context);
				}
			}
			foreach (int r in intSet)
			{
				TObject f = this.Reverse[r];
				gos.Remove((IGameObject)f);
				gos.App.ReleaseObject((IGameObject)f);
				this.Remove(f, r);
			}
			return (IEnumerable<TObject>)objectList;
		}
	}
}
