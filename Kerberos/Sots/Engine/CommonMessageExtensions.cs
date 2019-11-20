// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.CommonMessageExtensions
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kerberos.Sots.Engine
{
	internal static class CommonMessageExtensions
	{
		public static void PostReport(this App state, string text)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_GAME_REPORT_EVENT, (object)text);
		}

		public static void PostNewGame(this App state, int playerId)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_NEW_GAME, (object)playerId);
		}

		public static void PostSetLocalPlayer(this App state, int playerId)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_SET_LOCAL_PLAYER, (object)playerId);
		}

		public static void PostRequestSpeech(
		  this App state,
		  string cueName,
		  int priority,
		  int duration,
		  float timeout)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_REQUEST_SPEECH, (object)cueName, (object)priority, (object)duration, (object)timeout);
		}

		public static void PostRequestEffectSound(this App state, string cueName)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_REQUEST_EFFECT_SOUND, (object)cueName);
		}

		public static void PostRequestGuiSound(this App state, string cueName)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_REQUEST_GUI_SOUND, (object)cueName);
		}

		public static void PostEnableEffectsSounds(this App state, bool enabled)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_EFFECTS, (object)enabled);
		}

		public static void PostEnableGuiSounds(this App state, bool enabled)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_GUI, (object)enabled);
		}

		public static void PostEnableSpeechSounds(this App state, bool enabled)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_SPEECH, (object)enabled);
		}

		public static void PostEnableMusicSounds(this App state, bool enabled)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_MUSIC, (object)enabled);
		}

		public static void PostDisableAllSounds(this App state)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_EFFECTS, (object)false);
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_GUI, (object)false);
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_SPEECH, (object)false);
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_MUSIC, (object)false);
		}

		public static void PostEnableAllSounds(this App state)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_EFFECTS, (object)true);
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_GUI, (object)true);
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_SPEECH, (object)true);
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE_MUSIC, (object)true);
		}

		public static void TurnOffSound(this App state)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE, (object)false);
		}

		public static void TurnOnSound(this App state)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_ENABLE, (object)true);
		}

		public static void PostSpeechSubtitles(this App state, bool value)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_SPEECH_SUBTITLES, (object)value);
		}

		public static void PostSetVolumeMusic(this App state, int volume)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_VOLUME_MUSIC, (object)volume);
		}

		public static void PostSetVolumeEffects(this App state, int volume)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_VOLUME_EFFECTS, (object)volume);
		}

		public static void PostSetVolumeSpeech(this App state, int volume)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_SET_VOLUME_SPEECH, (object)volume);
		}

		public static void PostRequestStopSounds(this App state)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_REQUEST_STOP_SOUNDS);
		}

		public static void PostRequestStopSound(this App state, string cueName)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_REQUEST_STOP_SOUND, (object)cueName);
		}

		public static void PostPlayMusic(this App state, string cueName)
		{
			state.PostEngineMessage((object)InteropMessageID.IMID_SOUND_PLAY_MUSIC, (object)cueName);
		}

		public static void PostAddGoal(this IGameObject state, Vector3 targetPos, Vector3 look)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_ADD_GOAL, (object)state.ObjectID, (object)targetPos.X, (object)targetPos.Y, (object)targetPos.Z, (object)look.X, (object)look.Y, (object)look.Z);
		}

		public static void PostSetLook(this IGameObject state, Vector3 look)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETLOOK, (object)state.ObjectID, (object)look.X, (object)look.Y, (object)look.Z);
		}

		public static void PostSetAggregate(this IGameObject state, IGameObject target)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETAGGREGATE, (object)state.ObjectID, (object)target.ObjectID);
		}

		public static void PostObjectAddObjects(this IGameObject state, params IGameObject[] objects)
		{
			if (objects == null || objects.Length == 0)
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)InteropMessageID.IMID_ENGINE_OBJECT_ADDOBJECTS);
			objectList.Add((object)state.ObjectID);
			objectList.Add((object)objects.Length);
			objectList.AddRange(((IEnumerable<IGameObject>)objects).Select<IGameObject, int>((Func<IGameObject, int>)(x => x.ObjectID)).Cast<object>());
			state.App.PostEngineMessage((IEnumerable)objectList);
		}

		public static void PostAttach(this IGameObject state, IGameObject target)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_ATTACH, (object)state.ObjectID, (object)1, (object)target.ObjectID);
		}

		public static void PostAttach(
		  this IGameObject state,
		  IGameObject paired,
		  IGameObject target,
		  IGameObject socket1,
		  string socket1NodeName,
		  IGameObject socket2,
		  string socket2NodeName)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_ATTACH, (object)state.ObjectID, (object)6, (object)target.ObjectID, (object)paired.ObjectID, (object)socket1.ObjectID, (object)(socket1NodeName ?? string.Empty), (object)socket2.ObjectID, (object)(socket2NodeName ?? string.Empty));
		}

		public static void PostDetach(this IGameObject state, IGameObject target)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_DETACH, (object)state.ObjectID, (object)target.ObjectID);
		}

		public static void PostSetParent(this IGameObject state, IGameObject parent)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPARENT, (object)state.ObjectID, (object)1, (object)parent.ObjectID);
		}

		public static void PostSetParent(
		  this IGameObject state,
		  IGameObject parent,
		  string parentNodeName)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPARENT, (object)state.ObjectID, (object)2, (object)parent.ObjectID, (object)parentNodeName);
		}

		public static void PostSetParent(
		  this IGameObject state,
		  IGameObject parent,
		  string parentNodeName,
		  string offsetNodeName)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPARENT, (object)state.ObjectID, (object)3, (object)parent.ObjectID, (object)parentNodeName, (object)offsetNodeName);
		}

		public static void PostSetBattleRiderParent(this IGameObject state, int parentID)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_BATTLERIDER_SETPARENT, (object)state.ObjectID, (object)parentID);
		}

		public static void PostSetActive(this IGameObject state, bool value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETACTIVE, (object)state.ObjectID, (object)(value ? 1 : 0));
		}

		public static void PostSetPosition(this IGameObject state, Vector3 value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPOS, (object)state.ObjectID, (object)value.X, (object)value.Y, (object)value.Z);
		}

		public static void PostSetRotation(this IGameObject state, Vector3 value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETROT, (object)state.ObjectID, (object)value.X, (object)value.Y, (object)value.Z);
		}

		public static void PostSetScale(this IGameObject state, float value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETSCALE, (object)state.ObjectID, (object)value);
		}

		public static void PostSetPlayer(this IGameObject state, int playerId)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPLAYER, (object)state.ObjectID, (object)playerId);
		}

		public static void PostNotifyObjectHasBeenAdded(this IGameObject state, int objectID)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_ADDED, (object)state.ObjectID, (object)objectID);
		}

		public static void PostNotifyObjectsHaveBeenAdded(this IGameObject state, int[] objectIDs)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)InteropMessageID.IMID_ENGINE_OBJECTS_ADDED);
			objectList.Add((object)state.ObjectID);
			objectList.Add((object)objectIDs.Length);
			foreach (int objectId in objectIDs)
				objectList.Add((object)objectId);
			state.App.PostEngineMessage(objectList.ToArray());
		}

		public static void PostSetProp(this IGameObject state, string propertyName, Vector2 value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value.X, (object)value.Y);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, Vector3 value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value.X, (object)value.Y, (object)value.Z);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, Matrix value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value.M11, (object)value.M12, (object)value.M13, (object)value.M14, (object)value.M21, (object)value.M22, (object)value.M23, (object)value.M24, (object)value.M31, (object)value.M32, (object)value.M33, (object)value.M34, (object)value.M41, (object)value.M42, (object)value.M43, (object)value.M44);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, float value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, string value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, int value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, bool value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)value);
		}

		public static void PostSetProp(this IGameObject state, string propertyName, IGameObject value)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP, (object)state.ObjectID, (object)propertyName, (object)(value != null ? value.ObjectID : 0));
		}

		public static void PostSetProp(
		  this IGameObject state,
		  string propertyName,
		  IGameObject[] value)
		{
			List<object> objectList = new List<object>();
			objectList.Add((object)InteropMessageID.IMID_ENGINE_OBJECT_SETPROP);
			objectList.Add((object)state.ObjectID);
			objectList.Add((object)propertyName);
			if (value != null)
			{
				objectList.Add((object)value.Length);
				for (int index = 0; index < value.Length; ++index)
					objectList.Add((object)(value[index] != null ? value[index].ObjectID : 0));
			}
			state.App.PostEngineMessage((IEnumerable)objectList);
		}

		public static void PostSetProp(
		  this IGameObject state,
		  string propertyName,
		  params object[] values)
		{
			object[] array = ((IEnumerable<object>)new object[3]
			{
		(object) InteropMessageID.IMID_ENGINE_OBJECT_SETPROP,
		(object) state.ObjectID,
		(object) propertyName
			}).Concat<object>((IEnumerable<object>)values).ToArray<object>();
			state.App.PostEngineMessage(array);
		}

		public static void PostSetInt(this IGameObject state, int property, params object[] values)
		{
			object[] array = ((IEnumerable<object>)new object[3]
			{
		(object) InteropMessageID.IMID_ENGINE_OBJECT_SETINT,
		(object) state.ObjectID,
		(object) property
			}).Concat<object>((IEnumerable<object>)values).ToArray<object>();
			state.App.PostEngineMessage(array);
		}

		public static void PostFormationDefinition(
		  this IGameObject state,
		  Vector3 position,
		  Vector3 facing,
		  Vector3 dimensions)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_FORMATION_DEF, (object)state.ObjectID, (object)position.X, (object)position.Y, (object)position.Z, (object)facing.X, (object)facing.Y, (object)facing.Z, (object)dimensions.X, (object)dimensions.Y, (object)dimensions.Z);
		}

		public static void PostCreateFormationFromShips(this App game, params object[] msgParams)
		{
			object[] array = ((IEnumerable<object>)new object[1]
			{
		(object) InteropMessageID.IMID_ENGINE_FORMATION_FROM_SHIPS
			}).Concat<object>((IEnumerable<object>)msgParams).ToArray<object>();
			game.PostEngineMessage(array);
		}

		public static void PostApplyFormationPattern(this App game, params object[] msgParams)
		{
			object[] array = ((IEnumerable<object>)new object[1]
			{
		(object) InteropMessageID.IMID_ENGINE_FORMATION_APPLY_PATTERN
			}).Concat<object>((IEnumerable<object>)msgParams).ToArray<object>();
			game.PostEngineMessage(array);
		}

		public static void PostRemoveShipsFromFormation(this App game, params object[] msgParams)
		{
			object[] array = ((IEnumerable<object>)new object[1]
			{
		(object) InteropMessageID.IMID_ENGINE_FORMATION_REMOVE_SHIPS
			}).Concat<object>((IEnumerable<object>)msgParams).ToArray<object>();
			game.PostEngineMessage(array);
		}

		public static void PostShipFormationPosition(
		  this IGameObject state,
		  IGameObject ship,
		  Vector3 position)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_SHIP_FORMATION_POS, (object)state.ObjectID, (object)ship.ObjectID, (object)position.X, (object)position.Y, (object)position.Z);
		}

		public static void PostFormationBattleRider(
		  this IGameObject state,
		  IGameObject ship,
		  int parentID,
		  Vector3 position)
		{
			state.App.PostEngineMessage((object)InteropMessageID.IMID_ENGINE_FORMATION_BATTLE_RIDER, (object)state.ObjectID, (object)ship.ObjectID, (object)parentID, (object)position.X, (object)position.Y, (object)position.Z);
		}

		public static void PostNetworkMessage(this App game, string msgType, params object[] msgParams)
		{
			object[] array = ((IEnumerable<object>)new object[2]
			{
		(object) InteropMessageID.IMID_ENGINE_NETWORK,
		(object) msgType
			}).Concat<object>((IEnumerable<object>)msgParams).ToArray<object>();
			game.PostEngineMessage(array);
		}

		public static object GetTag(this IGameObject state)
		{
			return state.App.GetObjectTag(state);
		}

		public static T GetTag<T>(this IGameObject state) where T : class
		{
			return state.App.GetObjectTag(state) as T;
		}

		public static void SetTag(this IGameObject state, object value)
		{
			state.App.SetObjectTag(state, value);
		}

		public static void ClearTag(this IGameObject state)
		{
			state.App.RemoveObjectTag(state);
		}
	}
}
