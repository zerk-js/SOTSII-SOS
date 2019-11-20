// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.HotKeyManager
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Kerberos.Sots.Engine
{
	[GameObjectType(InteropGameObjectType.IGOT_HOTKEYMANAGER)]
	internal class HotKeyManager : GameObject
	{
		private const string _nodeHotkey = "hotkey";
		private const string _nodeEvent = "event";
		private const string _nodeStates = "states";
		private const string _nodeShift = "shift";
		private const string _nodeCtrl = "ctrl";
		private const string _nodeAlt = "alt";
		private const string _nodeKey = "key";
		private static string _profileRootDirectory;
		private string _profileName;
		private bool _loaded;
		private Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo> HotKeys;
		private Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo> DefaultHotKeys;
		private List<IKeyBindListener> _listeners;
		private List<IHotkeyVKListener> _vkListeners;
		private string _assetdir;
		private bool _enabled;

		public static string HotkeyProfileRootDirectory
		{
			get
			{
				return HotKeyManager._profileRootDirectory;
			}
		}

		public string HotkeyProfileName
		{
			get
			{
				return this._profileName;
			}
		}

		public bool Loaded
		{
			get
			{
				return this._loaded;
			}
		}

		public HotKeyManager(App game, string assetdir)
		{
			game.AddExistingObject((IGameObject)this);
			this.HotKeys = new Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo>();
			this.DefaultHotKeys = new Dictionary<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo>();
			this._listeners = new List<IKeyBindListener>();
			this._vkListeners = new List<IHotkeyVKListener>();
			this._assetdir = assetdir;
			this._enabled = true;
		}

		public void SetEnabled(bool value)
		{
			this._enabled = value;
			this.PostSetProp("SetLocked", !this._enabled);
		}

		public bool GetEnabled()
		{
			return this._enabled;
		}

		public void AddListener(IKeyBindListener listener)
		{
			if (this._listeners.Contains(listener))
				return;
			this._listeners.Add(listener);
		}

		public void AddVKListener(IHotkeyVKListener listener)
		{
			if (this._vkListeners.Contains(listener))
				return;
			this._vkListeners.Add(listener);
		}

		public void RemoveVKListener(IHotkeyVKListener listener)
		{
			this._vkListeners.Remove(listener);
		}

		public void SetVkReportMode(bool value)
		{
			this.PostSetProp("SetVKReportMode", value);
		}

		public HotKeyManager.KeyCombo GetHotKeyCombo(HotKeyManager.HotKeyActions action)
		{
			if (this.HotKeys.ContainsKey(action))
				return this.HotKeys[action];
			return (HotKeyManager.KeyCombo)null;
		}

		public List<HotKeyManager.HotKeyActions> SetHotKeyCombo(
		  HotKeyManager.HotKeyActions action,
		  HotKeyManager.KeyCombo combo)
		{
			List<HotKeyManager.HotKeyActions> hotKeyActionsList = new List<HotKeyManager.HotKeyActions>();
			string[] strArray = combo.states.Split('|');
			foreach (HotKeyManager.HotKeyActions key in this.HotKeys.Keys)
			{
				if (key != action && this.HotKeys[key].key == combo.key && (this.HotKeys[key].alt == combo.alt && this.HotKeys[key].control == combo.control) && this.HotKeys[key].shift == combo.shift)
				{
					string states = this.HotKeys[key].states;
					char[] chArray = new char[1] { '|' };
					foreach (string str in states.Split(chArray))
					{
						if (((IEnumerable<string>)strArray).Contains<string>(str))
						{
							this.HotKeys[key].key = Keys.None;
							hotKeyActionsList.Add(key);
						}
					}
				}
			}
			this.HotKeys[action] = combo;
			hotKeyActionsList.Add(action);
			return hotKeyActionsList;
		}

		public void RemoveListener(IKeyBindListener listener)
		{
			this._listeners.Remove(listener);
		}

		public void ClearListeners()
		{
			this._listeners.Clear();
		}

		public void SyncKeyProfile(string state = "")
		{
			if (!this._loaded)
				return;
			if (this.App.CurrentState != null && state == "")
				this.SyncKeyProfileState(this.App.CurrentState.Name);
			else
				this.SyncKeyProfileState(state);
		}

		private void SyncKeyProfileState(string state)
		{
			if (!this._loaded)
				return;
			this.PostSetProp("ClearCombos");
			foreach (HotKeyManager.HotKeyActions index in this.HotKeys.Keys.Where<HotKeyManager.HotKeyActions>((Func<HotKeyManager.HotKeyActions, bool>)(x => this.HotKeys[x].states.Contains(state))))
			{
				if (this.HotKeys[index].key != Keys.None || index == HotKeyManager.HotKeyActions.NoAction)
					this.PostSetProp("KeyCombos", (object)(int)index, (object)this.HotKeys[index].shift, (object)this.HotKeys[index].control, (object)this.HotKeys[index].alt, (object)this.HotKeys[index].key);
			}
		}

		public override bool OnEngineMessage(InteropMessageID messageId, ScriptMessageReader message)
		{
			switch (messageId)
			{
				case InteropMessageID.IMID_SCRIPT_KEYCOMBO:
					if (!this._enabled)
						return true;
					HotKeyManager.HotKeyActions index = (HotKeyManager.HotKeyActions)message.ReadInteger();
					if (this.HotKeys.ContainsKey(index))
					{
						foreach (IKeyBindListener listener in this._listeners)
						{
							if (listener.OnKeyBindPressed(index, this.HotKeys[index].states))
								break;
						}
					}
					return true;
				case InteropMessageID.IMID_SCRIPT_VKREPORT:
					if (!this._enabled)
						return true;
					Keys key = (Keys)message.ReadInteger();
					bool shift = message.ReadBool();
					bool ctrl = message.ReadBool();
					bool alt = message.ReadBool();
					foreach (IHotkeyVKListener vkListener in this._vkListeners)
					{
						if (vkListener.OnVKReported(key, shift, ctrl, alt))
							break;
					}
					return true;
				default:
					return false;
			}
		}

		public static void SetHotkeyProfileDirectory(string directory)
		{
			HotKeyManager._profileRootDirectory = directory;
		}

		public string GetStringforKey(Keys key)
		{
			if (key >= Keys.D0 && key <= Keys.D9)
				return key.ToString().Substring(1);
			string str;
			switch (key)
			{
				case Keys.Prior:
					str = "PageUp";
					break;
				case Keys.Next:
					str = "PageDown";
					break;
				case Keys.OemSemicolon:
					str = ";";
					break;
				case Keys.Oemplus:
					str = "+";
					break;
				case Keys.Oemcomma:
					str = ",";
					break;
				case Keys.OemMinus:
					str = "-";
					break;
				case Keys.OemPeriod:
					str = ".";
					break;
				case Keys.OemQuestion:
					str = "/";
					break;
				case Keys.Oemtilde:
					str = "`";
					break;
				case Keys.OemOpenBrackets:
					str = "[";
					break;
				case Keys.OemPipe:
					str = "\\";
					break;
				case Keys.OemCloseBrackets:
					str = "]";
					break;
				case Keys.OemQuotes:
					str = "'";
					break;
				default:
					str = key.ToString();
					break;
			}
			return str;
		}

		public bool LoadProfile(string profileName, bool absolutepath = false)
		{
			string str = !absolutepath ? HotKeyManager._profileRootDirectory + "\\" + profileName + ".keys" : profileName;
			if (profileName == "~Default")
				str = this._assetdir + "\\" + profileName + ".keys";
			if (!File.Exists(str))
				return false;
			this.HotKeys.Clear();
			if (profileName == "~Default")
				this.DefaultHotKeys.Clear();
			this._profileName = profileName;
			XPathNavigator navigator = new XPathDocument(App.GetStreamForFile(str) != null ? App.GetStreamForFile(str) : (Stream)new FileStream(str, FileMode.Open)).CreateNavigator();
			navigator.MoveToFirstChild();
			do
			{
				if (navigator.HasChildren)
				{
					navigator.MoveToFirstChild();
					do
					{
						if (navigator.HasChildren)
						{
							HotKeyManager.KeyCombo keyCombo = new HotKeyManager.KeyCombo();
							HotKeyManager.HotKeyActions key = HotKeyManager.HotKeyActions.NoAction;
							navigator.MoveToFirstChild();
							do
							{
								switch (navigator.Name)
								{
									case "event":
										if (Enum.IsDefined(typeof(HotKeyManager.HotKeyActions), (object)navigator.Value))
										{
											key = (HotKeyManager.HotKeyActions)Enum.Parse(typeof(HotKeyManager.HotKeyActions), navigator.Value, true);
											break;
										}
										break;
									case "states":
										keyCombo.states = navigator.Value;
										break;
									case "shift":
										keyCombo.shift = bool.Parse(navigator.Value);
										break;
									case "ctrl":
										keyCombo.control = bool.Parse(navigator.Value);
										break;
									case "alt":
										keyCombo.alt = bool.Parse(navigator.Value);
										break;
									case "key":
										keyCombo.key = !Enum.IsDefined(typeof(Keys), (object)navigator.Value) ? Keys.None : (Keys)Enum.Parse(typeof(Keys), navigator.Value, true);
										break;
								}
							}
							while (navigator.MoveToNext());
							if (key != HotKeyManager.HotKeyActions.NoAction && !this.HotKeys.ContainsKey(key))
							{
								this.HotKeys.Add(key, keyCombo);
								if (profileName == "~Default")
									this.DefaultHotKeys.Add(key, keyCombo);
							}
							navigator.MoveToParent();
						}
					}
					while (navigator.MoveToNext());
				}
			}
			while (navigator.MoveToNext());
			this._loaded = true;
			foreach (HotKeyManager.HotKeyActions action in Enum.GetValues(typeof(HotKeyManager.HotKeyActions)))
			{
				if (!this.HotKeys.Keys.Contains<HotKeyManager.HotKeyActions>(action) && this.DefaultHotKeys.Keys.Contains<HotKeyManager.HotKeyActions>(action))
					this.SetHotKeyCombo(action, this.DefaultHotKeys[action]);
			}
			this.SyncKeyProfile("");
			this.SaveProfile();
			return true;
		}

		public bool CreateProfile(string profileName)
		{
			if (!this.HotKeys.Any<KeyValuePair<HotKeyManager.HotKeyActions, HotKeyManager.KeyCombo>>() && !this.LoadProfile("~Default", false))
				return false;
			this._profileName = profileName;
			if (File.Exists(HotKeyManager._profileRootDirectory + "\\" + profileName + ".keys"))
				return false;
			this.SaveProfile();
			this._loaded = true;
			return true;
		}

		public bool SaveProfile()
		{
			if (this._profileName == "~Default")
				return false;
			string str = HotKeyManager._profileRootDirectory + "\\" + this._profileName + ".keys";
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<hotkeys></hotkeys>");
			XmlElement Root = xmlDocument["hotkeys"];
			foreach (HotKeyManager.HotKeyActions key in this.HotKeys.Keys)
			{
				XmlHelper.AddNode((object)"", "hotkey", ref Root);
				XmlElement lastChild = (XmlElement)Root.LastChild;
				XmlHelper.AddNode((object)key.ToString(), "event", ref lastChild);
				XmlHelper.AddNode((object)this.HotKeys[key].states, "states", ref lastChild);
				XmlHelper.AddNode((object)this.HotKeys[key].shift, "shift", ref lastChild);
				XmlHelper.AddNode((object)this.HotKeys[key].control, "ctrl", ref lastChild);
				XmlHelper.AddNode((object)this.HotKeys[key].alt, "alt", ref lastChild);
				XmlHelper.AddNode((object)this.HotKeys[key].key.ToString(), "key", ref lastChild);
			}
			if (App.GetStreamForFile(str) == null)
			{
				xmlDocument.Save(str);
				App.LockFileStream(str);
			}
			else
			{
				Stream streamForFile = App.GetStreamForFile(str);
				streamForFile.SetLength(0L);
				xmlDocument.Save(streamForFile);
			}
			return true;
		}

		public static List<string> GetAvailableProfiles()
		{
			string[] files = Directory.GetFiles(HotKeyManager._profileRootDirectory);
			List<string> stringList = new List<string>();
			foreach (string path in files)
			{
				if (!path.Contains("~Default") && path.EndsWith(".keys"))
				{
					if (File.Exists(path))
						stringList.Add(path);
				}
			}
			return stringList;
		}

		public void DeleteProfile()
		{
			this.HotKeys.Clear();
			string str = HotKeyManager._profileRootDirectory + "\\" + this._profileName + ".keys";
			App.UnLockFileStream(str);
			if (File.Exists(str))
				File.Delete(str);
			this._profileName = string.Empty;
			this._loaded = false;
		}

		public enum HotKeyActions
		{
			NoAction = -1, // 0xFFFFFFFF
			State_Starmap = 0,
			State_BuildScreen = 1,
			State_DesignScreen = 2,
			State_ResearchScreen = 3,
			State_ComparativeAnalysysScreen = 4,
			State_EmpireSummaryScreen = 5,
			State_SotspediaScreen = 6,
			State_StarSystemScreen = 7,
			State_FleetManagerScreen = 8,
			State_DefenseManagerScreen = 9,
			State_BattleRiderScreen = 10, // 0x0000000A
			State_DiplomacyScreen = 11, // 0x0000000B
			Starmap_EndTurn = 12, // 0x0000000C
			Starmap_NextFleet = 13, // 0x0000000D
			Starmap_LastFleet = 14, // 0x0000000E
			Starmap_NextIdleFleet = 15, // 0x0000000F
			Starmap_LastIdleFleet = 16, // 0x00000010
			Starmap_NextSystem = 17, // 0x00000011
			Starmap_LastSystem = 18, // 0x00000012
			Starmap_NextIncomingFleet = 19, // 0x00000013
			Starmap_OpenFleetManager = 20, // 0x00000014
			Starmap_OpenPlanetManager = 21, // 0x00000015
			Starmap_OpenStationManager = 22, // 0x00000016
			Starmap_OpenRepairScreen = 23, // 0x00000017
			Starmap_OpenPopulationManager = 24, // 0x00000018
			Starmap_NormalViewFilter = 25, // 0x00000019
			Starmap_SurveyViewFilter = 26, // 0x0000001A
			Starmap_ProvinceFilter = 27, // 0x0000001B
			Starmap_SupportRangeFilter = 28, // 0x0000001C
			Starmap_SensorRangeFilter = 29, // 0x0000001D
			Starmap_TerrainFilter = 30, // 0x0000001E
			Starmap_TradeViewFilter = 31, // 0x0000001F
			Starmap_NextNewsEvent = 32, // 0x00000020
			Starmap_LastNewsEvent = 33, // 0x00000021
			Starmap_OpenMenu = 34, // 0x00000022
			Research_NextTree = 35, // 0x00000023
			Research_LastTree = 36, // 0x00000024
			Combat_FocusCNC = 37, // 0x00000025
			Combat_SelectHome = 38, // 0x00000026
			Combat_SelectNext = 39, // 0x00000027
			Combat_FocusOnMouse = 40, // 0x00000028
			Combat_ToggleSensorView = 41, // 0x00000029
			Combat_StopSelectedShips = 42, // 0x0000002A
			Combat_Pause = 43, // 0x0000002B
			Combat_AccelTime = 44, // 0x0000002C
			Combat_DecelTime = 45, // 0x0000002D
			Combat_ExitAccelTime = 46, // 0x0000002E
			Combat_FreeCamera = 47, // 0x0000002F
			Combat_TrackCamera = 48, // 0x00000030
		}

		public class KeyCombo
		{
			public string states = "";
			public bool shift;
			public bool control;
			public bool alt;
			public Keys key;
		}
	}
}
