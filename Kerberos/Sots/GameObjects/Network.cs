// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.GameObjects.Network
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.StarFleet;
using Kerberos.Sots.Strategy;
using Kerberos.Sots.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Kerberos.Sots.GameObjects
{
	[GameObjectType(InteropGameObjectType.IGOT_NETWORK)]
	internal class Network : GameObject, IDisposable
	{
		private Dictionary<int, List<string>> _dbBuffer = new Dictionary<int, List<string>>();
		private string _gameName = "";
		private StarMapLobbyState _starMapLobby;
		private bool _gameLoaded;
		private bool _isHosting;
		private bool _isJoined;
		private bool _isOffline;
		private bool _isLoggedIn;
		private bool _dbLoaded;
		private int _lastDBRow;
		private bool _gameInfoUpdated;
		private List<ReactionInfo> _reactionList;
		private float _turnSeconds;
		private NetConnectionDialog _connectionDialog;
		private string _username;

		public string Username
		{
			get
			{
				return this._username;
			}
		}

		public float TurnSeconds
		{
			get
			{
				return this._turnSeconds;
			}
			set
			{
				this._turnSeconds = value;
			}
		}

		public void PostLogPlayerInfo()
		{
			this.PostSetInt(33);
		}

		public void PostIRCChatMessage(string name, string message)
		{
			this.PostSetInt(34, (object)name, (object)message);
		}

		public void PostIRCNick(string name)
		{
			this.PostSetInt(35, (object)name);
		}

		public StarMapLobbyState StarMapLobby
		{
			get
			{
				return this._starMapLobby;
			}
			set
			{
				this._starMapLobby = value;
			}
		}

		public bool IsHosting
		{
			get
			{
				return this._isHosting;
			}
			set
			{
				this._isHosting = value;
			}
		}

		public bool IsJoined
		{
			get
			{
				return this._isJoined;
			}
			set
			{
				this._isJoined = value;
			}
		}

		public bool IsOffline
		{
			get
			{
				return this._isOffline;
			}
			set
			{
				this._isOffline = value;
			}
		}

		public bool IsLoggedIn
		{
			get
			{
				return this._isLoggedIn;
			}
			set
			{
				this._isLoggedIn = value;
			}
		}

		public bool LoginRequired
		{
			get
			{
				if (!this._isOffline)
					return !this._isLoggedIn;
				return false;
			}
		}

		public string GameName
		{
			get
			{
				return this._gameName;
			}
			set
			{
				this._gameName = value;
			}
		}

		private void Reset()
		{
			this._gameLoaded = false;
			this._isHosting = false;
			this._isJoined = false;
			this._dbLoaded = false;
		}

		private void Trace(string message)
		{
			App.Log.Trace(message, "net");
		}

		public void SetChatWidgetVisibility(bool? visible = null)
		{
			if (visible.HasValue)
				this.PostSetInt(31, (object)true, (object)visible.Value);
			else
				this.PostSetInt(31, (object)false);
		}

		public void EnableChatWidgetPlayerList(bool enabled)
		{
			this.PostSetInt(30, (object)enabled);
		}

		public void EnableChatWidget(bool enabled)
		{
			this.PostSetInt(29, (object)enabled);
		}

		public bool GameLoaded
		{
			get
			{
				return this._gameLoaded;
			}
			set
			{
				this._gameLoaded = value;
			}
		}

		public void Initialize()
		{
			this.App.UI.PanelMessage += new UIEventPanelMessage(this.UI_PanelMessage);
		}

		public void Dispose()
		{
			if (this.App == null)
				return;
			this.App.UI.PanelMessage -= new UIEventPanelMessage(this.UI_PanelMessage);
		}

		private void UI_PanelMessage(string panelName, string msgType, string[] msgParams)
		{
			if (!(msgType == "dialog_closed") || this._connectionDialog == null || !(panelName == this._connectionDialog.ID))
				return;
			if (msgParams[0] == "True")
			{
				if (this._starMapLobby != null)
					this._starMapLobby.OnJoined();
			}
			else
			{
				this.Disconnect();
				if (this._starMapLobby != null)
					this._starMapLobby.OnNetworkError();
			}
			this._connectionDialog = (NetConnectionDialog)null;
		}

		private void NM_START_GAME_NewGame()
		{
			if (!this._gameLoaded)
				this.App.NewGame();
			else
				this.App.GameSetup.IsMultiplayer = true;
			this.App.ConfirmAI();
			this.App.GameDatabase.GetDatabaseHistory(out this._lastDBRow);
			if (this.App.Network != null && this.App.Network.IsHosting)
				this.App.GameDatabase.SaveMultiplayerSyncPoint(this.App.CacheDir);
			this.PostSetInt(17, (object)(int)this.App.GameDatabase.GetDbPointer());
		}

		private bool ClientStartLoad()
		{
			return this._dbLoaded;
		}

		public void ProcessEngineMessage(ScriptMessageReader mr)
		{
			switch (mr.ReadInteger())
			{
				case 0:
					{
						uint num = (uint)mr.ReadInteger();
						uint num2 = (uint)mr.ReadInteger();
						ulong id = (ulong)num + ((ulong)num2 << 32);
						string name = mr.ReadString();
						string version = mr.ReadString();
						string map = mr.ReadString();
						int players = mr.ReadInteger();
						int maxPlayers = mr.ReadInteger();
						int ping = mr.ReadInteger();
						bool passworded = mr.ReadBool();
						List<PlayerSetup> list = new List<PlayerSetup>();
						int num3 = mr.ReadInteger();
						for (int i2 = 0; i2 < num3; i2++)
						{
							list.Add(new PlayerSetup
							{
								slot = i2,
								Name = mr.ReadString(),
								Avatar = mr.ReadString(),
								Badge = mr.ReadString(),
								Faction = mr.ReadString(),
								EmpireColor = new int?(mr.ReadInteger())
							});
						}
						if (this._starMapLobby != null)
						{
							this._starMapLobby.AddServer(id, name, map, version, players, maxPlayers, ping, passworded, list);
							return;
						}
						break;
					}
				case 1:
					if (this._isHosting)
					{
						base.App.StartGame(new Action(this.NM_START_GAME_NewGame), null, new object[0]);
						return;
					}
					break;
				case 2:
					if (base.App.CurrentState != base.App.GetGameState<LoadingScreenState>())
					{
						base.App.SwitchGameStateViaLoadingScreen(null, new LoadingFinishedDelegate(this.ClientStartLoad), base.App.GetGameState<StarMapState>(), new object[0]);
						return;
					}
					break;
				case 3:
					break;
				case 4:
					{
						this.Trace("Combat start message received.");
						int id2 = mr.ReadInteger();
						mr.ReadInteger();
						bool authority = mr.ReadBool();
						bool sim = mr.ReadBool();
						int num4 = mr.ReadInteger();
						PendingCombat pendingCombatByUniqueID = base.App.Game.GetPendingCombatByUniqueID(id2);
						if (pendingCombatByUniqueID != null)
						{
							for (int j = 0; j < num4; j++)
							{
								int key = mr.ReadInteger();
								int value = mr.ReadInteger();
								int value2 = mr.ReadInteger();
								int value3 = mr.ReadInteger();
								pendingCombatByUniqueID.SelectedPlayerFleets[key] = value;
								pendingCombatByUniqueID.CombatResolutionSelections[key] = (ResolutionType)value2;
								pendingCombatByUniqueID.CombatStanceSelections[key] = (AutoResolveStance)value3;
							}
							base.App.Game.LaunchCombat(pendingCombatByUniqueID, false, sim, authority);
							return;
						}
						break;
					}
				case 5:
					{
						GameSetup gameSetup = base.App.GameSetup;
						gameSetup.CombatTurnLength = mr.ReadSingle();
						gameSetup.StrategicTurnLength = mr.ReadSingle();
						int playerCount = mr.ReadInteger();
						int[] values = new int[]
						{
					mr.ReadInteger()
						};
						gameSetup._averageResources = mr.ReadInteger();
						gameSetup._economicEfficiency = mr.ReadInteger();
						gameSetup._grandMenaceCount = mr.ReadInteger();
						gameSetup._initialSystems = mr.ReadInteger();
						gameSetup._initialTechs = mr.ReadInteger();
						gameSetup._initialTreasury = mr.ReadInteger();
						gameSetup._randomEncounterFrequency = mr.ReadInteger();
						gameSetup._researchEfficiency = mr.ReadInteger();
						gameSetup._starCount = mr.ReadInteger();
						gameSetup._starSize = mr.ReadInteger();
						mr.ReadString();
						mr.ReadString();
						string gameName = mr.ReadString();
						this._gameName = gameName;
						int gameInProgressTurn = mr.ReadInteger();
						gameSetup._inProgress = mr.ReadBool();
						if (this._starMapLobby != null)
						{
							this._starMapLobby.EnablePlayerSetup(!gameSetup._inProgress);
							this._starMapLobby.GameInProgressTurn = gameInProgressTurn;
							this._starMapLobby.UpdateGameInProgress();
						}
						BitArray bitArray = new BitArray(values);
						gameSetup.AvailablePlayerFeatures.ReplaceFactions(from x in base.App.AssetDatabase.Factions
																		  where bitArray[x.ID]
																		  select x);
						gameSetup.SetPlayerCount(playerCount);
						base.App.GameSetup.IsMultiplayer = true;
						this._gameInfoUpdated = true;
						return;
					}
				case 6:
					this._gameName = mr.ReadString();
					return;
				case 7:
					{
						bool flag = mr.ReadBool();
						if (flag)
						{
							this.Reset();
							if (this._starMapLobby != null)
							{
								this._starMapLobby.OnNetworkError();
							}
							if (base.App.CurrentState != base.App.GetGameState<StarMapLobbyState>())
							{
								while (base.App.UI.GetTopDialog() != null)
								{
									base.App.UI.CloseDialog(base.App.UI.GetTopDialog(), true);
								}
								StarMapState gameState = base.App.GetGameState<StarMapState>();
								if (base.App.CurrentState == gameState)
								{
									gameState.Reset();
								}
								base.App.SwitchGameState<MainMenuState>(new object[0]);
							}
						}
						if (this._connectionDialog == null)
						{
							base.App.UI.CreateDialog(new GenericTextDialog(base.App, "Error!", mr.ReadString() + ".", "dialogGenericMessage"), null);
							return;
						}
						break;
					}
				case 8:
					base.App.GameDatabase.GetDatabaseHistory(out this._lastDBRow);
					if (base.App.Network != null && base.App.Network.IsHosting)
					{
						base.App.GameDatabase.SaveMultiplayerSyncPoint(base.App.CacheDir);
					}
					this.PostSetInt(17, new object[]
					{
					(int)base.App.GameDatabase.GetDbPointer()
					});
					return;
				case 9:
					{
						int num5 = mr.ReadInteger();
						IntPtr source = (IntPtr)mr.ReadInteger();
						byte[] array = new byte[num5];
						Marshal.Copy(source, array, 0, num5);
						BinaryWriter binaryWriter = new BinaryWriter(File.Open(base.App.CacheDir + "\\client.db", FileMode.Create));
						binaryWriter.Write(array);
						binaryWriter.Close();
						base.App.LoadGame(base.App.CacheDir + "\\client.db", base.App.GameSetup);
						base.App.GameDatabase.GetDatabaseHistory(out this._lastDBRow);
						this._dbLoaded = true;
						if (base.App.GameSetup._inProgress)
						{
							base.App.Game.State = SimState.SS_COMBAT;
							return;
						}
						break;
					}
				case 10:
					{
						int num6 = mr.ReadInteger();
						int? num7 = null;
						if (base.App.GameSetup.LocalPlayer != null)
						{
							num7 = new int?(base.App.GameSetup.LocalPlayer.slot);
						}
						if (num6 >= 0 && num6 < base.App.GameSetup.Players.Count)
						{
							if (base.App.GameSetup.Players[num6] == null)
							{
								base.App.GameSetup.Players[num6] = new PlayerSetup();
							}
							PlayerSetup playerSetup = base.App.GameSetup.Players[num6];
							string avatar = playerSetup.Avatar;
							string badge = playerSetup.Badge;
							string faction = playerSetup.Faction;
							playerSetup.localPlayer = mr.ReadBool();
							int value4 = mr.ReadInteger();
							playerSetup.Name = mr.ReadString();
							playerSetup.EmpireName = mr.ReadString();
							playerSetup.Faction = mr.ReadString();
							playerSetup.SubfactionIndex = mr.ReadInteger();
							string avatar2 = mr.ReadString();
							string badge2 = mr.ReadString();
							playerSetup.slot = num6;
							playerSetup.InitialColonies = mr.ReadInteger();
							playerSetup.InitialTechs = mr.ReadInteger();
							playerSetup.InitialTreasury = mr.ReadInteger();
							playerSetup.ShipColor = new Vector3(mr.ReadSingle(), mr.ReadSingle(), mr.ReadSingle());
							playerSetup.AI = mr.ReadBool();
							playerSetup.Status = (NPlayerStatus)mr.ReadInteger();
							playerSetup.Fixed = mr.ReadBool();
							playerSetup.Locked = mr.ReadBool();
							playerSetup.Ready = (playerSetup.Status == NPlayerStatus.PS_READY);
							playerSetup.Team = mr.ReadInteger();
							if (faction != playerSetup.Faction)
							{
								base.App.GameSetup.ReplaceAvatar(faction, avatar);
								base.App.GameSetup.ReplaceBadge(faction, badge);
							}
							base.App.GameSetup.SetEmpireColor(num6, new int?(value4));
							base.App.GameSetup.SetAvatar(num6, playerSetup.Faction, avatar2);
							base.App.GameSetup.SetBadge(num6, playerSetup.Faction, badge2);
							GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, true);
							if (base.App.Game != null)
							{
								Player player = base.App.GetPlayer(base.App.GameSetup.Players[num6].databaseId);
								if (player != null && player.IsAI() != playerSetup.AI)
								{
									player.SetAI(playerSetup.AI);
									this.Trace(string.Concat(new object[]
									{
								"Setting Player AI on player ",
								num6,
								" to ",
								playerSetup.AI
									}));
								}
							}
							if (playerSetup.localPlayer)
							{
								if (base.App.Game != null && base.App.Game.LocalPlayer.ID != playerSetup.databaseId)
								{
									Player player2 = base.App.GetPlayer(base.App.GameSetup.Players[num6].databaseId);
									if (player2 != null)
									{
										base.App.Game.SetLocalPlayer(player2);
									}
								}
								if (this._starMapLobby != null)
								{
									if (playerSetup.Fixed)
									{
										this._starMapLobby.EnablePlayerSetup(false);
									}
									else
									{
										this._starMapLobby.EnablePlayerSetup(true);
									}
									if (num7 != null && num7.Value != num6)
									{
										this._starMapLobby.ShowPlayerSetup(num6);
									}
									this._starMapLobby.IsLocalPlayerReady = playerSetup.Ready;
									if (this._gameInfoUpdated)
									{
										this._starMapLobby.EnableGameInProgress(base.App.GameSetup._inProgress);
									}
									this._starMapLobby.UpdateGameInProgress();
								}
							}
							if (this._starMapLobby != null && (playerSetup.localPlayer || (this.IsHosting && this._starMapLobby.SelectedSlot == num6)))
							{
								this._starMapLobby.ShowPlayerSetup(num6);
								this._gameInfoUpdated = false;
								return;
							}
						}
						break;
					}
				case 11:
					this._turnSeconds = 0f;
					if (base.App.Game == null)
					{
						return;
					}
					base.App.Game.GetPendingCombats().Clear();
					if (this._isHosting)
					{
						base.App.Game.NextTurn();
						this.Trace("Third/First sync sent!");
						base.App.GameDatabase.LogComment("SYNC 3/1 (NM_NEXT_TURN)");
						this.SendHistory(base.App.GameDatabase.GetTurnCount() - 1);
						return;
					}
					break;
				case 12:
					this.SendHistory(base.App.GameDatabase.GetTurnCount());
					return;
				case 13:
					{
						int key2 = mr.ReadInteger();
						int num8 = mr.ReadInteger();
						if (base.App.GameDatabase != null)
						{
							int turnCount = base.App.GameDatabase.GetTurnCount();
							List<string> list2 = new List<string>();
							for (int k = 0; k < num8; k++)
							{
								list2.Add(mr.ReadString());
							}
							Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
							dictionary[key2] = list2;
							base.App.GameDatabase.ExecuteDatabaseHistory(dictionary);
							base.App.GameDatabase.SetClientId(base.App.LocalPlayer.ID);
							int turnCount2 = base.App.GameDatabase.GetTurnCount();
							base.App.GameDatabase.GetDatabaseHistoryForTurn(turnCount2, out this._lastDBRow, new int?(this._lastDBRow));
							if (turnCount != turnCount2)
							{
								base.App.Game.NextTurn();
							}
							this.Trace("Database history applied.");
							return;
						}
						if (!this._dbBuffer.ContainsKey(key2))
						{
							this._dbBuffer[key2] = new List<string>();
							return;
						}
						for (int l = 0; l < num8; l++)
						{
							this._dbBuffer[key2].Add(mr.ReadString());
						}
						return;
					}
				case 14:
					{
						base.App.Game.ProcessMidTurn();
						base.App.GameDatabase.LogComment("SYNC 2 (NM_REACTION_PHASE)");
						this.SendHistory(base.App.GameDatabase.GetTurnCount());
						this.Trace("Second sync sent!");
						List<ReactionInfo> source2 = base.App.Game.GetPendingReactions();
						source2 = (from x in source2
								   where !base.App.GetPlayer(x.fleet.PlayerID).IsAI()
								   select x).ToList<ReactionInfo>();
						List<object> list3 = new List<object>();
						list3.Add(source2.Count<ReactionInfo>());
						int i;
						for (i = 1; i <= 8; i++)
						{
							IEnumerable<ReactionInfo> enumerable = from x in source2
																   where x.fleet.PlayerID == i
																   select x;
							if (enumerable.Count<ReactionInfo>() > 0)
							{
								list3.Add(i);
								list3.Add(enumerable.Count<ReactionInfo>());
								foreach (ReactionInfo reactionInfo in enumerable)
								{
									list3.Add(reactionInfo.fleet.ID);
									list3.Add(reactionInfo.fleetsInRange.Count<FleetInfo>());
									foreach (FleetInfo fleetInfo in reactionInfo.fleetsInRange)
									{
										list3.Add(fleetInfo.ID);
									}
								}
							}
						}
						this.PostSetInt(20, list3.ToArray());
						return;
					}
				case 15:
					{
						base.App.Game.Phase4_Combat();
						base.App.GameDatabase.LogComment("SYNC 3 (NM_COMBAT_PHASE)");
						this.SendHistory(base.App.GameDatabase.GetTurnCount());
						this.Trace("Third sync sent!");
						List<PendingCombat> pendingCombats = base.App.Game.GetPendingCombats();
						List<object> list4 = new List<object>();
						list4.Add(pendingCombats.Count);
						foreach (PendingCombat pendingCombat in pendingCombats)
						{
							list4.Add(pendingCombat.ConflictID);
							list4.Add(pendingCombat.SystemID);
							list4.Add((int)pendingCombat.Type);
							List<int> list5 = new List<int>();
							foreach (int num9 in pendingCombat.PlayersInCombat)
							{
								PlayerInfo playerInfo = base.App.GameDatabase.GetPlayerInfo(num9);
								if (playerInfo != null && playerInfo.isStandardPlayer)
								{
									list5.Add(num9);
								}
							}
							list4.Add(list5.Count<int>());
							list4.AddRange(list5.Cast<object>());
						}
						this.PostSetInt(22, list4.ToArray());
						return;
					}
				case 16:
					{
						string title = mr.ReadString();
						string text = mr.ReadString();
						base.App.UI.CreateDialog(new GenericTextDialog(base.App, title, text, "dialogGenericMessage"), null);
						return;
					}
				case 17:
					{
						int index = mr.ReadInteger();
						base.App.GameSetup.Players[index].Name = "";
						base.App.GameSetup.Players[index].AI = true;
						if (base.App.CurrentState == base.App.GetGameState<StarMapLobbyState>())
						{
							GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, true);
							return;
						}
						Player player3 = base.App.GetPlayer(base.App.GameSetup.Players[index].databaseId);
						if (player3 != null)
						{
							player3.ReplaceWithAI();
							base.App.GameSetup.Players[index].Name = "";
							this.Trace("PLAYER DISCONNECTED - Setting AI, ID: " + player3.ID);
							return;
						}
						break;
					}
				case 18:
					{
						int num10 = mr.ReadInteger();
						Player player4 = base.App.GetPlayer(base.App.GameSetup.Players[num10].databaseId);
						if (this._gameLoaded || base.App.CurrentState != base.App.GetGameState<StarMapLobbyState>())
						{
							int? num11 = base.App.GameDatabase.GetLastClientPlayerID(base.App.GameSetup.Players[num10].Name) - 1;
							if (num11 != null && base.App.GameSetup.Players[num11.Value].AI && num10 != num11 && !base.App.GameSetup.Players[num11.Value].Locked)
							{
								this.ChangeSlot(num10, num11.Value);
							}
						}
						if (player4 != null && player4.IsAI())
						{
							player4.SetAI(false);
							base.App.GameSetup.Players[num10].AI = false;
							this.Trace("PLAYER CONNECTED - Disabling AI, ID: " + player4.ID);
							return;
						}
						break;
					}
				case 19:
					this.EndTurn();
					return;
				case 20:
					if (base.App.UserProfile != null && base.App.UserProfile.Loaded)
					{
						base.App.UserProfile.Username = mr.ReadString();
						base.App.UserProfile.Password = mr.ReadString();
						base.App.UserProfile.SaveProfile();
						return;
					}
					break;
				case 21:
					{
						if (this._reactionList == null)
						{
							this._reactionList = new List<ReactionInfo>();
						}
						else
						{
							this._reactionList.Clear();
						}
						int num12 = mr.ReadInteger();
						for (int m = 0; m < num12; m++)
						{
							ReactionInfo reactionInfo2 = new ReactionInfo();
							reactionInfo2.fleetsInRange = new List<FleetInfo>();
							int fleetID = mr.ReadInteger();
							reactionInfo2.fleet = base.App.GameDatabase.GetFleetInfo(fleetID);
							int num13 = mr.ReadInteger();
							for (int n = 0; n < num13; n++)
							{
								int fleetID2 = mr.ReadInteger();
								reactionInfo2.fleetsInRange.Add(base.App.GameDatabase.GetFleetInfo(fleetID2));
							}
							this._reactionList.Add(reactionInfo2);
						}
						base.App.Game.SetPendingReactions(this._reactionList);
						base.App.UI.CreateDialog(new ReactionDialog(base.App, this._reactionList.First<ReactionInfo>()), null);
						return;
					}
				case 22:
					{
						List<PendingCombat> list6 = new List<PendingCombat>();
						Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
						int num14 = mr.ReadInteger();
						for (int num15 = 0; num15 < num14; num15++)
						{
							int num16 = mr.ReadInteger();
							int conflictID = mr.ReadInteger();
							int type = mr.ReadInteger();
							int cardID = 1;
							if (dictionary2.ContainsKey(num16))
							{
								cardID = dictionary2[num16] + 1;
								Dictionary<int, int> dictionary3;
								int key3;
								(dictionary3 = dictionary2)[key3 = num16] = dictionary3[key3] + 1;
							}
							else
							{
								dictionary2.Add(num16, 1);
							}
							List<PendingCombat> list7 = list6;
							PendingCombat pendingCombat2 = new PendingCombat();
							pendingCombat2.CardID = cardID;
							pendingCombat2.SystemID = num16;
							pendingCombat2.ConflictID = conflictID;
							pendingCombat2.Type = (CombatType)type;
							pendingCombat2.PlayersInCombat = (from x in GameSession.GetPlayersWithCombatAssets(base.App, num16)
															  select x.ID).ToList<int>();
							pendingCombat2.FleetIDs = (from x in base.App.GameDatabase.GetFleetInfoBySystemID(num16, FleetType.FL_NORMAL)
													   select x.ID).ToList<int>();
							pendingCombat2.NPCPlayersInCombat = base.App.GameDatabase.GetNPCPlayersBySystem(num16);
							list7.Add(pendingCombat2);
						}
						using (List<PendingCombat>.Enumerator enumerator3 = list6.ToList<PendingCombat>().GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								PendingCombat cmb = enumerator3.Current;
								if (cmb.Type == CombatType.CT_Piracy)
								{
									List<int> PlayersToExclude = new List<int>();
									List<int> list8 = new List<int>();
									foreach (int num17 in cmb.FleetIDs.ToList<int>())
									{
										MissionInfo missionByFleetID = base.App.GameDatabase.GetMissionByFleetID(num17);
										if (missionByFleetID != null && missionByFleetID.Type == MissionType.PIRACY)
										{
											if (!base.App.GameDatabase.GetPiracyFleetDetectionInfoForFleet(num17).Any((PiracyFleetDetectionInfo x) => cmb.PlayersInCombat.Any((int y) => y == x.PlayerID)))
											{
												FleetInfo fleetInfo2 = base.App.GameDatabase.GetFleetInfo(num17);
												PlayersToExclude.Add(fleetInfo2.PlayerID);
												list8.Add(num17);
											}
										}
									}
									cmb.FleetIDs = (from x in base.App.GameDatabase.GetFleetInfoBySystemID(cmb.SystemID, FleetType.FL_NORMAL)
													where !PlayersToExclude.Any((int h) => h == x.PlayerID)
													select x.ID).ToList<int>();
									cmb.FleetIDs.AddRange(list8);
								}
								if (cmb.CardID > 1)
								{
									foreach (int fleetID3 in cmb.FleetIDs)
									{
										FleetInfo fleetInfo3 = base.App.GameDatabase.GetFleetInfo(fleetID3);
										PlayerInfo playerInfo2 = base.App.GameDatabase.GetPlayerInfo(fleetInfo3.PlayerID);
										if (!playerInfo2.isStandardPlayer)
										{
											list6.Remove(cmb);
										}
									}
								}
							}
						}
						base.App.Game.GetPendingCombats().Clear();
						base.App.Game.GetPendingCombats().AddRange(list6);
						if (list6.Any((PendingCombat x) => x.PlayersInCombat.Contains(base.App.Game.LocalPlayer.ID)))
						{
							base.App.UI.CreateDialog(new EncounterDialog(base.App, list6), null);
							return;
						}
						base.App.Game.ShowCombatDialog(true, null);
						return;
					}
				case 23:
					{
						int key4 = mr.ReadInteger();
						int num18 = mr.ReadInteger();
						for (int num19 = 0; num19 < num18; num19++)
						{
							int id3 = mr.ReadInteger();
							mr.ReadInteger();
							int value5 = mr.ReadInteger();
							int value6 = mr.ReadInteger();
							int value7 = mr.ReadInteger();
							PendingCombat pendingCombatByUniqueID2 = base.App.Game.GetPendingCombatByUniqueID(id3);
							if (pendingCombatByUniqueID2 != null)
							{
								pendingCombatByUniqueID2.SelectedPlayerFleets[key4] = value5;
								pendingCombatByUniqueID2.CombatStanceSelections[key4] = (AutoResolveStance)value7;
								pendingCombatByUniqueID2.CombatResolutionSelections[key4] = (ResolutionType)value6;
							}
						}
						return;
					}
				case 24:
					{
						if (base.App.Game == null)
						{
							App.Log.Warn("Ignoring NM_COMBAT_DATA because we're not in the game yet.", "net");
							return;
						}
						base.App.Game.CombatData.AddCombat(mr, 1);
						CombatData lastCombat = base.App.Game.CombatData.GetLastCombat();
						if (base.App.Network.IsHosting)
						{
							base.App.GameDatabase.InsertCombatData(lastCombat.SystemID, lastCombat.CombatID, lastCombat.Turn, lastCombat.ToByteArray());
							return;
						}
						break;
					}
				case 25:
					{
						if (base.App.Game == null)
						{
							App.Log.Warn("Ignoring NM_SYNC_MULTI_COMBAT_DATA because we're not in the game yet.", "net");
							return;
						}
						int systemId = mr.ReadInteger();
						int num20 = mr.ReadInteger();
						List<int> list9 = new List<int>();
						for (int num21 = 0; num21 < num20; num21++)
						{
							list9.Add(mr.ReadInteger());
						}
						base.App.Game.MCCarryOverData.AddCarryOverCombatZoneInfo(systemId, list9);
						int num22 = mr.ReadInteger();
						for (int num23 = 0; num23 < num22; num23++)
						{
							int num24 = mr.ReadInteger();
							int retreatFleetId = mr.ReadInteger();
							base.App.Game.MCCarryOverData.SetRetreatFleetID(systemId, num24, retreatFleetId);
							int num25 = mr.ReadInteger();
							for (int num26 = 0; num26 < num25; num26++)
							{
								int shipId = mr.ReadInteger();
								float x2 = mr.ReadSingle();
								float y = mr.ReadSingle();
								float z = mr.ReadSingle();
								float yawRadians = mr.ReadSingle();
								float pitchRadians = mr.ReadSingle();
								float rollRadians = mr.ReadSingle();
								Matrix endShipTransform = Matrix.CreateRotationYPR(yawRadians, pitchRadians, rollRadians);
								endShipTransform.Position = new Vector3(x2, y, z);
								base.App.Game.MCCarryOverData.AddCarryOverInfo(systemId, num24, shipId, endShipTransform);
							}
						}
						return;
					}
				case 26:
					{
						int num27 = mr.ReadInteger();
						NPlayerStatus nplayerStatus = (NPlayerStatus)mr.ReadInteger();
						if (num27 >= 0 && num27 < base.App.GameSetup.Players.Count && base.App.GameSetup.Players[num27].Status != NPlayerStatus.PS_DEFEATED)
						{
							this.Trace(string.Concat(new object[]
							{
						"Setting status on player ",
						num27,
						" to ",
						nplayerStatus
							}));
							base.App.GameSetup.Players[num27].Status = nplayerStatus;
							base.App.GameSetup.Players[num27].Ready = (nplayerStatus == NPlayerStatus.PS_READY);
							if (base.App.CurrentState == base.App.GetGameState<StarMapLobbyState>())
							{
								GameSetupUI.SyncPlayerListWidget(base.App, "lstPlayers", base.App.GameSetup.Players, true);
								return;
							}
						}
						break;
					}
				case 27:
					{
						this._turnSeconds = mr.ReadSingle();
						float strategicTurnLength = mr.ReadSingle();
						if (base.App.Game != null)
						{
							base.App.Game.TurnTimer.StrategicTurnLength = strategicTurnLength;
							return;
						}
						break;
					}
				case 28:
					{
						StarMapLobbyState gameState2 = base.App.GetGameState<StarMapLobbyState>();
						if (base.App.CurrentState == gameState2)
						{
							gameState2.OnRefreshComplete();
							return;
						}
						break;
					}
				case 29:
					{
						string msg = mr.ReadString();
						base.App.IRC.SendChatMessage(msg);
						return;
					}
				case 30:
					if (base.App.IRC.irc.IsConnected)
					{
						base.App.IRC.Disconnect();
						this.PostIRCChatMessage(string.Empty, "*** Disconnected.");
						return;
					}
					base.App.IRC.SetupIRCClient(base.App.UserProfile.ProfileName);
					break;
				default:
					return;
			}
		}

		public void RefreshServers(bool stop)
		{
			this.PostSetInt(0, (object)stop);
		}

		public void RequestPlayerInformation()
		{
			this.PostSetInt(32);
		}

		public void RefreshBuddies()
		{
			this.PostSetInt(1);
		}

		public void Host()
		{
			this._isHosting = true;
			this.PostSetInt(5);
		}

		public void Login(string username)
		{
			this._username = username;
			this.PostSetInt(9, (object)username);
		}

		public void NewUser(string guid, string email, string user, string password)
		{
			this.PostSetInt(28, (object)guid, (object)email, (object)user, (object)password);
		}

		public void InLobby(bool val)
		{
			this.PostSetInt(8, (object)val);
		}

		public void UpdateGameInfo(string gameName, string gamePass)
		{
			this.PostSetInt(10, (object)gameName, (object)gamePass);
		}

		public void SetGameInfo(GameSetup setup)
		{
			string path1 = setup.HasStarMapFile() ? setup.StarMapFile : string.Empty;
			if (path1 != string.Empty)
				path1 = Path.GetFileNameWithoutExtension(path1);
			string path2 = setup.HasScenarioFile() ? setup.ScenarioFile : string.Empty;
			if (path2 != string.Empty)
				path2 = Path.GetFileNameWithoutExtension(path2);
			int starCount = setup.StarCount;
			int count = setup.Players.Count;
			List<Faction> list = setup.AvailablePlayerFeatures.Factions.Keys.ToList<Faction>();
			BitArray bitArray = new BitArray(32);
			foreach (Faction faction in list)
			{
				int? factionId = this.App.GetFactionID(faction.Name);
				if (factionId.HasValue && factionId.Value < 32)
					bitArray[factionId.Value] = true;
			}
			int[] numArray = new int[1];
			bitArray.CopyTo((Array)numArray, 0);
			int[] array = Enumerable.Repeat<int>(-4, 8).ToArray<int>();
			int index = 0;
			foreach (PlayerSetup player in setup.Players)
			{
				array[index] = !player.Locked ? (!player.AI ? -2 : -1) : -3;
				++index;
			}
			this.PostSetInt(13, (object)setup.CombatTurnLength, (object)setup.StrategicTurnLength, (object)setup.Players.Count, (object)numArray[0], (object)setup._averageResources, (object)setup._economicEfficiency, (object)setup._grandMenaceCount, (object)setup._initialSystems, (object)setup._initialTechs, (object)setup._initialTreasury, (object)setup._randomEncounterFrequency, (object)setup._researchEfficiency, (object)setup._starCount, (object)setup._starSize, (object)setup._inProgress, (object)path1, (object)path2, (object)array[0], (object)array[1], (object)array[2], (object)array[3], (object)array[4], (object)array[5], (object)array[6], (object)array[7]);
		}

		public void SetPlayerInfo(PlayerSetup ps, int slot)
		{
			if (ps == null)
				return;
			if (this._gameLoaded)
				ps.Fixed = true;
			if (ps.Faction == "" || ps.Faction == null)
				throw new Exception("Faction set as null or empty.");
			this.PostSetInt(11, (object)slot, (object)(ps.Faction ?? ""), (object)ps.SubfactionIndex, (object)(ps.Avatar ?? ""), (object)(ps.Badge ?? ""), (object)(ps.EmpireColor ?? -1), (object)ps.InitialColonies, (object)ps.InitialTechs, (object)ps.InitialTreasury, (object)ps.AI, (object)ps.localPlayer, (object)ps.ShipColor.X, (object)ps.ShipColor.Y, (object)ps.ShipColor.Z, (object)ps.EmpireName, (object)ps.Fixed, (object)ps.Locked, (object)ps.Team);
		}

		public void JoinGame(ulong serverID, string password = "")
		{
			this._connectionDialog = new NetConnectionDialog(this.App, "Connecting", "", "dialogNetConnection");
			this.App.UI.CreateDialog((Dialog)this._connectionDialog, null);
			uint num1 = (uint)serverID;
			uint num2 = (uint)(serverID >> 32);
			this._isJoined = true;
			this.PostSetInt(3, (object)(int)num1, (object)(int)num2, (object)password, (object)this._connectionDialog.ID);
		}

		public void SelectServer(int index)
		{
			this.PostSetInt(2, (object)index);
		}

		public bool DirectConnect(string address, string password)
		{
			this._connectionDialog = new NetConnectionDialog(this.App, "Connecting", "", "dialogNetConnection");
			this.App.UI.CreateDialog((Dialog)this._connectionDialog, null);
			try
			{
				string[] strArray = address.Split(':');
				IPAddress ipAddress = ((IEnumerable<IPAddress>)Dns.GetHostEntry(strArray[0]).AddressList).FirstOrDefault<IPAddress>((Func<IPAddress, bool>)(x => x.AddressFamily == AddressFamily.InterNetwork));
				if (ipAddress == null)
					return false;
				string str = ipAddress.ToString();
				if (((IEnumerable<string>)strArray).Count<string>() > 1)
					str = str + (object)':' + strArray[1];
				this._isJoined = true;
				this.PostSetInt(4, (object)str, (object)password, (object)this._connectionDialog.ID);
				return true;
			}
			catch (SocketException ex)
			{
				this._connectionDialog.AddString("Unable connect to Host: Address Invalid.");
				return false;
			}
		}

		public void LoadGame()
		{
			this.PostSetInt(12);
		}

		public void Ready()
		{
			this.PostSetInt(14);
		}

		public void SetSlot(int index)
		{
			this.PostSetInt(16, (object)index);
		}

		public void ChangeSlot(int firstSlot, int secondSlot)
		{
			this.PostSetInt(15, (object)firstSlot, (object)secondSlot);
		}

		public void Disconnect()
		{
			this.Reset();
			this.PostSetInt(6);
		}

		public void Kick(int index)
		{
			this.PostSetInt(7, (object)index);
		}

		public void DatabaseLoaded()
		{
			foreach (KeyValuePair<int, List<string>> keyValuePair in this._dbBuffer)
			{
				this.App.GameDatabase.ExecuteDatabaseHistory((IDictionary<int, List<string>>)this._dbBuffer);
				this.App.GameDatabase.SetClientId(this.App.LocalPlayer.ID);
			}
			this._dbBuffer.Clear();
		}

		public void EndTurn()
		{
			if (this.App.GameDatabase == null)
				return;
			this.SendHistoryCore(this.App.GameDatabase.GetTurnCount(), true);
		}

		public void SendHistory(int turn)
		{
			this.SendHistoryCore(turn, false);
		}

		private void SendHistoryCore(int turn, bool endTurn)
		{
			if (this.App.GameDatabase == null)
				return;
			string[] array = this.App.GameDatabase.GetDatabaseHistoryForTurn(turn, out this._lastDBRow, new int?(this._lastDBRow)).ToArray();
			List<object> objectList = new List<object>();
			objectList.Add((object)turn);
			objectList.Add((object)this.App.LocalPlayer.ID);
			objectList.Add((object)array.Length);
			objectList.AddRange((IEnumerable<object>)array);
			Network.NetworkInteropMessage networkInteropMessage = endTurn ? Network.NetworkInteropMessage.NIM_STARMAP_END_TURN : Network.NetworkInteropMessage.NIM_STARMAP_DB_HISTORY;
			this.Trace(string.Format("{0} Submitting {1} lines of DB history as {2}.", (object)networkInteropMessage, (object)array.Length, (object)this.App.GameDatabase.GetClientId()));
			this.PostSetInt((int)networkInteropMessage, objectList.ToArray());
		}

		public void SendCombatResponses(IEnumerable<PendingCombat> responses, int playerId)
		{
			if (responses.Count<PendingCombat>() == 0)
				return;
			List<object> objectList = new List<object>();
			IEnumerable<PendingCombat> source = responses.Where<PendingCombat>((Func<PendingCombat, bool>)(x => x.PlayersInCombat.Contains(playerId)));
			if (source.Count<PendingCombat>() == 0)
				return;
			objectList.Add((object)playerId);
			objectList.Add((object)source.Count<PendingCombat>());
			foreach (PendingCombat pendingCombat in source)
			{
				objectList.Add((object)pendingCombat.ConflictID);
				objectList.Add((object)pendingCombat.SystemID);
				objectList.Add((object)pendingCombat.SelectedPlayerFleets[playerId]);
				objectList.Add((object)(int)pendingCombat.CombatResolutionSelections[playerId]);
				objectList.Add((object)(int)pendingCombat.CombatStanceSelections[playerId]);
			}
			this.PostSetInt(23, objectList.ToArray());
		}

		public void SetTime(float current, float max)
		{
			if (!this.IsHosting || this.App.CurrentState == this.App.GetGameState<MainMenuState>() || this.App.CurrentState == this.App.GetGameState<StarMapLobbyState>())
				return;
			this.PostSetInt(24, (object)current, (object)max);
		}

		public void ReactionComplete()
		{
			this.PostSetInt(21);
		}

		public void SendCarryOverData(List<object> parms)
		{
			this.PostSetInt(25, parms.ToArray());
		}

		public void SendCombatData(CombatData cd)
		{
			this.PostSetInt(26, cd.ToList().ToArray());
		}

		public void CombatComplete(int systemId)
		{
			this.PostSetInt(27, (object)systemId);
		}

		private enum NetworkMessage
		{
			NM_LOBBY_GAME_FOUND,
			NM_START_GAME,
			NM_LOAD_GAME,
			NM_TRANSFER_PROGRESS,
			NM_START_COMBAT,
			NM_GAME_INFO,
			NM_GAME_NAME,
			NM_ERROR,
			NM_DB_GET,
			NM_DB_SET,
			NM_PLAYER_SET,
			NM_NEXT_TURN,
			NM_DB_HISTORY,
			NM_DB_APPLY_HISTORY,
			NM_REACTION_PHASE,
			NM_COMBAT_PHASE,
			NM_DIALOG,
			NM_PLAYER_DISCONNECTED,
			NM_PLAYER_CONNECTED,
			NM_POST_COMBAT,
			NM_SET_PROFILE_DATA,
			NM_REACTION_LIST,
			NM_COMBAT_LIST,
			NM_COMBAT_RESPONSE,
			NM_COMBAT_DATA,
			NM_SYNC_MULTI_COMBAT_DATA,
			NM_PLAYER_STATUS_CHANGED,
			NM_STARMAP_TIME,
			NM_REFRESH_COMPLETE,
			NM_SEND_IRC_MESSAGE,
			NM_IRC_RECONNECT,
		}

		public enum DialogAction
		{
			DA_FINALIZE,
			DA_RAW_STRING,
			DA_LOGIN_CONNECTING,
			DA_LOGIN_CONNECTING_FAIL_GP,
			DA_LOGIN_CONNECTING_BAD_PASSWORD,
			DA_LOGIN_CONNECTING_BAD_USERNAME,
			DA_LOGIN_CONNECTING_LOGIN_IN_USE,
			DA_LOGIN_CONNECTING_SUCCESS_GP,
			DA_LOGIN_CONNECTING_CHAT,
			DA_LOGIN_CONNECTING_CHAT_FAILED,
			DA_LOGIN_CONNECTED,
			DA_NEWUSER_CREATING,
			DA_NEWUSER_PASSWORD_MISMATCH,
			DA_NEWUSER_INVALID_USERNAME,
			DA_NEWUSER_NICK_IN_USE,
			DA_NEWUSER_OFFLINE,
			DA_NEWUSER_INVALID_PASSWORD,
			DA_NEWUSER_FAILED,
			DA_NEWUSER_SUCCESS,
			DA_CONNECT_CONNECTING,
			DA_CONNECT_FAILED,
			DA_CONNECT_SUCCESS,
			DA_CONNECT_TIMED_OUT,
			DA_CONNECT_INVALID_PASSWORD,
			DA_CONNECT_NAT_FAILURE,
		}

		private enum NetworkInteropMessage
		{
			NIM_LOBBY_REFRESH,
			NIM_LOBBY_REFRESH_BUDDIES,
			NIM_LOBBY_SERVER_SELECT,
			NIM_LOBBY_JOIN,
			NIM_LOBBY_DIRECT,
			NIM_LOBBY_HOST,
			NIM_LOBBY_DISCONNECT,
			NIM_LOBBY_KICK,
			NIM_LOBBY_STATE,
			NIM_LOBBY_LOGIN,
			NIM_LOBBY_UPDATE_GAME_INFO,
			NIM_LOBBY_PLAYER_INFO,
			NIM_LOBBY_GAME_START,
			NIM_LOBBY_GAME_INFO,
			NIM_LOBBY_READY,
			NIM_LOBBY_SLOT_CHANGE,
			NIM_LOBBY_SLOT_SWAP,
			NIM_LOBBY_SET_DATABASE,
			NIM_STARMAP_END_TURN,
			NIM_STARMAP_DB_HISTORY,
			NIM_STARMAP_REACTION_INFO,
			NIM_STARMAP_REACTION_COMPLETE,
			NIM_STARMAP_COMBAT_INFO,
			NIM_STARMAP_COMBAT_RESPONSES,
			NIM_STARMAP_TIME,
			NIM_COMBAT_CARRY_OVER_DATA,
			NIM_COMBAT_DATA,
			NIM_COMBAT_COMPLETE,
			NIM_NEWUSER,
			NIM_CHATWIDGET_ENABLED,
			NIM_CHATWIDGET_ENABLE_PLAYERS,
			NIM_CHATWIDGET_VISIBILITY,
			NIM_REQUEST_PLAYER_INFOS,
			NIM_LOG_PLAYER_INFO,
			NIM_HANDLE_IRC_MESSAGE,
			NIM_UPDATE_IRC_NICK,
		}

		private enum SlotInfo
		{
			SLOT_NONE = -4, // 0xFFFFFFFC
			SLOT_CLOSED = -3, // 0xFFFFFFFD
			SLOT_OPEN = -2, // 0xFFFFFFFE
			SLOT_AI = -1, // 0xFFFFFFFF
		}
	}
}
