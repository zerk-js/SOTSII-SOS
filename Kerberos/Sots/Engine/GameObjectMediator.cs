// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.GameObjectMediator
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kerberos.Sots.Engine
{
	internal class GameObjectMediator
	{
		private readonly Dictionary<int, IGameObject> _pending = new Dictionary<int, IGameObject>();
		private readonly Dictionary<int, IGameObject> _objs = new Dictionary<int, IGameObject>();
		private readonly Dictionary<Type, InteropGameObjectType> _forwardTypeMap = new Dictionary<Type, InteropGameObjectType>();
		private readonly Dictionary<InteropGameObjectType, Type> _reverseTypeMap = new Dictionary<InteropGameObjectType, Type>();
		private readonly Dictionary<IGameObject, object> _objectTags = new Dictionary<IGameObject, object>();
		private App _game;
		private int _prevGameObjectID;

		private int GetNextGameObjectID()
		{
			if (this._prevGameObjectID == int.MaxValue)
				this._prevGameObjectID = 0;
			++this._prevGameObjectID;
			return this._prevGameObjectID;
		}

		private static InteropGameObjectType? GetAssociatedGameObjectType(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(GameObjectTypeAttribute), true);
			if (customAttributes.Length > 0)
				return new InteropGameObjectType?((customAttributes[0] as GameObjectTypeAttribute).Value);
			return new InteropGameObjectType?();
		}

		public IGameObject GetObject(int id)
		{
			IGameObject gameObject;
			if (this._objs.TryGetValue(id, out gameObject))
				return gameObject;
			return (IGameObject)null;
		}

		public void SetObjectTag(IGameObject state, object value)
		{
			this._objectTags[state] = value;
		}

		public void RemoveObjectTag(IGameObject state)
		{
			if (!this._objectTags.Keys.Contains<IGameObject>(state))
				return;
			this._objectTags[state] = (object)null;
			this._objectTags.Remove(state);
		}

		public object GetObjectTag(IGameObject state)
		{
			object obj = (object)null;
			this._objectTags.TryGetValue(state, out obj);
			return obj;
		}

		private void RegisterType(Type type)
		{
			InteropGameObjectType? associatedGameObjectType = GameObjectMediator.GetAssociatedGameObjectType(type);
			if (!associatedGameObjectType.HasValue)
				throw new InvalidOperationException("No associated game object type for " + (object)type + ".");
			this._forwardTypeMap.Add(type, associatedGameObjectType.Value);
			this._reverseTypeMap.Add(associatedGameObjectType.Value, type);
		}

		private void RegisterTypes(params Type[] types)
		{
			foreach (Type type in types)
				this.RegisterType(type);
		}

		public GameObjectMediator(App game)
		{
			this._game = game;
			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (GameObjectMediator.GetAssociatedGameObjectType(type).HasValue)
					this.RegisterType(type);
			}
		}

		private void AddExistingObjectCore(
		  IGameObject o,
		  InteropGameObjectType gameObjectType,
		  params object[] initParams)
		{
			GameObject gameObject = (GameObject)o;
			gameObject.ObjectID = this.GetNextGameObjectID();
			gameObject.App = this._game;
			List<object> objectList = new List<object>(3);
			objectList.Add((object)InteropMessageID.IMID_ENGINE_OBJECT_ADD);
			objectList.Add((object)gameObject.ObjectID);
			objectList.Add((object)gameObjectType);
			if (initParams != null)
				objectList.AddRange((IEnumerable<object>)initParams);
			this._game.PostEngineMessage((IEnumerable)objectList);
			this._pending.Add(gameObject.ObjectID, (IGameObject)gameObject);
			this._objs.Add(gameObject.ObjectID, (IGameObject)gameObject);
		}

		private IGameObject AddObjectCore(
		  Type type,
		  InteropGameObjectType gameObjectType,
		  params object[] initParams)
		{
			IGameObject instance = (IGameObject)Activator.CreateInstance(type);
			this.AddExistingObjectCore(instance, gameObjectType, initParams);
			return instance;
		}

		public IGameObject AddObject(Type type, params object[] initParams)
		{
			return this.AddObjectCore(type, this._forwardTypeMap[type], initParams);
		}

		public void AddExistingObject(IGameObject o, params object[] initParams)
		{
			if (o.App != null)
				throw new InvalidOperationException("Game object (" + (object)o.GetType() + ", " + (object)o.ObjectID + ") is already in use.");
			this.AddExistingObjectCore(o, this._forwardTypeMap[o.GetType()], initParams);
		}

		public IGameObject AddObject(
		  InteropGameObjectType gameObjectType,
		  params object[] initParams)
		{
			return this.AddObjectCore(this._reverseTypeMap[gameObjectType], gameObjectType, initParams);
		}

		public void ReleaseObject(IGameObject obj)
		{
			this._pending.Remove(obj.ObjectID);
			this._objs.Remove(obj.ObjectID);
			this._game.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_RELEASE, (object)obj.ObjectID);
		}

		public void ReleaseObjects(IEnumerable<IGameObject> range)
		{
			foreach (IGameObject gameObject in range)
			{
				this._pending.Remove(gameObject.ObjectID);
				this._objs.Remove(gameObject.ObjectID);
			}
			this._game.PostEngineMessage((IEnumerable)((IEnumerable<object>)new object[2]
			{
		(object) InteropMessageID.IMID_ENGINE_OBJECT_RELEASEMULTI,
		(object) range.Count<IGameObject>()
			}).Concat<object>(range.Select<IGameObject, int>((Func<IGameObject, int>)(x => x.ObjectID)).Cast<object>()));
		}

		public void OnObjectStatus(int objectID, GameObjectStatus objectStatus)
		{
			IGameObject gameObject;
			if (!this._pending.TryGetValue(objectID, out gameObject))
				return;
			(gameObject as GameObject).PromoteEngineObjectStatus(objectStatus);
			if (objectStatus == GameObjectStatus.Pending)
				return;
			this._pending.Remove(objectID);
		}

		public void OnObjectScriptMessage(
		  InteropMessageID messageId,
		  int objectId,
		  ScriptMessageReader mr)
		{
			IGameObject gameObject;
			if (!this._objs.TryGetValue(objectId, out gameObject))
				App.Log.Warn("Received message " + messageId.ToString() + " for nonexistant object ID = " + (object)objectId, "engine");
			else
				this._objs[objectId].OnEngineMessage(messageId, mr);
		}
	}
}
