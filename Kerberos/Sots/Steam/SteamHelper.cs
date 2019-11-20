// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Steam.SteamHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;

namespace Kerberos.Sots.Steam
{
	public class SteamHelper
	{
		private readonly List<UserAchievementStoredData> _achievements = new List<UserAchievementStoredData>();
		private ulong _appID;
		private ISteam _steam;
		private bool _initialized;

		public bool Initialized
		{
			get
			{
				return this._initialized;
			}
		}

		private void GlobalStatsReceivedEventHandler(GlobalStatsReceivedData data)
		{
			App.Log.Warn("Global stats received.", "steam");
		}

		private void UserStatsReceivedEventHandler(UserStatsReceivedData data)
		{
			App.Log.Warn("Received user stats data.", "steam");
			if (this._steam.IsAvailable)
				this._steam.SetStat("UNIQUE_PROFILES", 1);
			App.Log.Warn("Steam profile status: " + this._steam.GetGlobalStat("UNIQUE_PROFILES").ToString(), "steam");
		}

		private void UserStatsStoredEventHandler(UserStatsStoredData data)
		{
			if ((long)data.GameID != (long)this._appID || data.Result != EResult.k_EResultOK)
				return;
			App.Log.Warn("Received user stats stored.", "steam");
			this._initialized = true;
		}

		private void UserAchievementStoredEventHandler(UserAchievementStoredData data)
		{
			if ((long)data.GameID == (long)this._appID || this._achievements.Contains(data))
				return;
			this._achievements.Add(data);
		}

		public SteamHelper(ISteam steam)
		{
			this._initialized = steam.IsAvailable;
			this._steam = steam;
			this._steam.UserStatsStored += new UserStatsStoredEventHandler(this.UserStatsStoredEventHandler);
			this._steam.UserStatsReceived += new UserStatsReceivedEventHandler(this.UserStatsReceivedEventHandler);
			this._steam.UserAchievementStored += new UserAchievementStoredEventHandler(this.UserAchievementStoredEventHandler);
			this._steam.GlobalStatsReceived += new GlobalStatsReceivedEventHandler(this.GlobalStatsReceivedEventHandler);
			if (!this._initialized)
				return;
			this._appID = this._steam.GetGameID();
			this.RequestStats();
		}

		public void RequestStats()
		{
			if (this._steam.IsAvailable && this._steam.BLoggedOn())
			{
				App.Log.Warn("Requesting steam stats.", "steam");
				this._steam.RequestCurrentStats();
				this._steam.RequestGlobalStats(60);
			}
			else
				App.Log.Warn("Steam not available.", "steam");
		}

		public void DoAchievement(AchievementType cheevo)
		{
			if (!this._initialized || this.HasAchievement(cheevo))
				return;
			this.AddAchievement(cheevo);
		}

		private void AddAchievement(AchievementType cheevo)
		{
			try
			{
				this._steam.SetAchievement(cheevo.ToString());
			}
			catch (Exception ex)
			{
				App.Log.Warn("SetAchievement: " + cheevo.ToString() + " failed.", "steam");
			}
		}

		private bool HasAchievement(AchievementType cheevo)
		{
			bool flag = false;
			try
			{
				flag = this._steam.GetAchievement(cheevo.ToString());
			}
			catch (Exception ex)
			{
				App.Log.Warn("GetAchievement " + cheevo.ToString() + " failed.", "steam");
			}
			return flag;
		}
	}
}
