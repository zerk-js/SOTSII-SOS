// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Profile
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Kerberos.Sots
{
	public class Profile
	{
		private const string _nodeProfile = "profile";
		private const string _nodeName = "name";
		private const string _nodeTechs = "techs";
		private const string _nodeUsername = "username";
		private const string _nodePassword = "password";
		private const string _nodeLastGamePlayed = "last_game_played";
		private const string _nodeAutoPlaceDefenses = "auto_place_defenses";
		private const string _nodeAutoRepairFleets = "auto_repair_fleets";
		private const string _nodeAutoUseGoop = "auto_goop";
		private const string _nodeAutoUseJoker = "auto_joker";
		private const string _nodeAutoAOE = "auto_aoe";
		private const string _nodeAutoPatrol = "auto_patrol";
		private static string _profileRootDirectory;
		private string _profileName;
		private bool _loaded;
		private List<string> _researchedTechs;

		public static string ProfileRootDirectory
		{
			get
			{
				return Profile._profileRootDirectory;
			}
		}

		public string ProfileName
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

		public List<string> ResearchedTechs
		{
			get
			{
				return this._researchedTechs;
			}
		}

		public string Username { get; set; }

		public string Password { get; set; }

		public string LastGamePlayed { get; set; }

		public bool AutoPlaceDefenseAssets { get; set; }

		public bool AutoRepairFleets { get; set; }

		public bool AutoUseGoop { get; set; }

		public bool AutoUseJoker { get; set; }

		public bool AutoAOE { get; set; }

		public bool AutoPatrol { get; set; }

		public Profile(string dir)
		{
			Profile._profileRootDirectory = dir;
			this._researchedTechs = new List<string>();
			this.Username = string.Empty;
			this.Password = string.Empty;
			this.LastGamePlayed = string.Empty;
			this.AutoPlaceDefenseAssets = false;
			this.AutoRepairFleets = false;
			this.AutoUseGoop = false;
			this.AutoUseJoker = false;
			this.AutoAOE = false;
			this.AutoPatrol = false;
		}

		public Profile()
		{
			this._researchedTechs = new List<string>();
			this.AutoPlaceDefenseAssets = false;
			this.AutoRepairFleets = false;
			this.AutoUseGoop = false;
			this.AutoUseJoker = false;
			this.AutoAOE = false;
			this.AutoPatrol = false;
		}

		public static void SetProfileDirectory(string directory)
		{
			Profile._profileRootDirectory = directory;
		}

		public bool LoadProfile(string profileName, bool absolutepath = false)
		{
			string str1 = !absolutepath ? Profile._profileRootDirectory + "\\" + profileName + ".xml" : profileName;
			if (!File.Exists(str1))
				return false;
			XPathNavigator navigator = new XPathDocument(App.GetStreamForFile(str1) ?? (Stream)new FileStream(str1, FileMode.Open)).CreateNavigator();
			navigator.MoveToFirstChild();
			do
			{
				if (navigator.HasChildren)
				{
					navigator.MoveToFirstChild();
					do
					{
						switch (navigator.Name)
						{
							case "name":
								this._profileName = navigator.Value;
								break;
							case "techs":
								string str2 = navigator.Value;
								this._researchedTechs.Clear();
								string[] strArray = str2.Split('!');
								for (int index = 0; index < strArray.Length; ++index)
								{
									if (strArray[index].Length > 0)
										this._researchedTechs.Add(strArray[index]);
								}
								break;
							case "username":
								this.Username = navigator.Value ?? string.Empty;
								break;
							case "password":
								this.Password = navigator.Value ?? string.Empty;
								break;
							case "last_game_played":
								this.LastGamePlayed = navigator.Value ?? string.Empty;
								break;
							case "auto_place_defenses":
								this.AutoPlaceDefenseAssets = bool.Parse(navigator.Value);
								break;
							case "auto_repair_fleets":
								this.AutoRepairFleets = bool.Parse(navigator.Value);
								break;
							case "auto_goop":
								this.AutoUseGoop = bool.Parse(navigator.Value);
								break;
							case "auto_joker":
								this.AutoUseJoker = bool.Parse(navigator.Value);
								break;
							case "auto_aoe":
								this.AutoAOE = bool.Parse(navigator.Value);
								break;
							case "auto_patrol":
								this.AutoPatrol = bool.Parse(navigator.Value);
								break;
						}
					}
					while (navigator.MoveToNext());
				}
			}
			while (navigator.MoveToNext());
			this._loaded = true;
			return true;
		}

		public bool SaveProfile()
		{
			string str1 = Profile._profileRootDirectory + "\\" + this._profileName + ".xml";
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<profile></profile>");
			XmlElement Root = xmlDocument["profile"];
			XmlHelper.AddNode((object)this._profileName, "name", ref Root);
			XmlHelper.AddNode((object)this.Username, "username", ref Root);
			XmlHelper.AddNode((object)this.Password, "password", ref Root);
			string str2 = "";
			for (int index = 0; index < this._researchedTechs.Count; ++index)
				str2 = str2 + this._researchedTechs[index].ToString() + "!";
			XmlHelper.AddNode((object)str2, "techs", ref Root);
			XmlHelper.AddNode((object)this.LastGamePlayed, "last_game_played", ref Root);
			XmlHelper.AddNode((object)this.AutoPlaceDefenseAssets.ToString(), "auto_place_defenses", ref Root);
			XmlHelper.AddNode((object)this.AutoRepairFleets.ToString(), "auto_repair_fleets", ref Root);
			XmlHelper.AddNode((object)this.AutoUseGoop.ToString(), "auto_goop", ref Root);
			XmlHelper.AddNode((object)this.AutoUseJoker.ToString(), "auto_joker", ref Root);
			XmlHelper.AddNode((object)this.AutoAOE.ToString(), "auto_aoe", ref Root);
			XmlHelper.AddNode((object)this.AutoPatrol.ToString(), "auto_patrol", ref Root);
			if (App.GetStreamForFile(str1) == null)
			{
				xmlDocument.Save(str1);
			}
			else
			{
				Stream streamForFile = App.GetStreamForFile(str1);
				streamForFile.SetLength(0L);
				xmlDocument.Save(streamForFile);
			}
			return true;
		}

		public bool CreateProfile(string profileName)
		{
			this._profileName = profileName;
			string str = Profile._profileRootDirectory + "\\" + profileName + ".xml";
			if (File.Exists(str))
				return false;
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<profile><name>" + profileName + "</name></profile>");
			xmlDocument.Save(str);
			App.LockFileStream(str);
			this._loaded = true;
			return true;
		}

		public static List<Profile> GetAvailableProfiles()
		{
			string[] files = Directory.GetFiles(Profile._profileRootDirectory);
			List<Profile> profileList = new List<Profile>();
			foreach (string profileName in files)
			{
				if (!profileName.EndsWith(".keys"))
				{
					Profile profile = new Profile();
					profile.LoadProfile(profileName, true);
					profileList.Add(profile);
				}
			}
			return profileList;
		}

		public void DeleteProfile()
		{
			string str = Profile._profileRootDirectory + "\\" + this._profileName + ".xml";
			App.UnLockFileStream(str);
			if (File.Exists(str))
				File.Delete(str);
			this._profileName = string.Empty;
			this._loaded = false;
		}
	}
}
