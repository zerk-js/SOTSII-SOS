// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Combat.CombatConfig
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.ShipFramework;
using Kerberos.Sots.Data.Xml;
using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using Kerberos.Sots.GameObjects;
using Kerberos.Sots.GameStates;
using Kerberos.Sots.PlayerFramework;
using Kerberos.Sots.ShipFramework;
using Kerberos.Sots.Strategy.InhabitedPlanet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Kerberos.Sots.Combat
{
	internal static class CombatConfig
	{
		public static bool ParentBattleRiders = true;

		private static IGameObject CreateShipCompound(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			XmlElement xmlElement1 = node["ShipName"];
			XmlElement xmlElement2 = node["ShipDesign"];
			XmlElement source1 = xmlElement2["Weapons"];
			XmlElement source2 = xmlElement2["Modules"];
			XmlElement xmlElement3 = xmlElement2["WeaponAssignments"];
			XmlElement xmlElement4 = xmlElement2["ModuleAssignments"];
			XmlElement source3 = xmlElement2["Sections"];
			List<WeaponAssignment> weaponAssignmentList = new List<WeaponAssignment>();
			List<ModuleAssignment> moduleAssignmentList = new List<ModuleAssignment>();
			List<ShipSectionAsset> sectionAssets = new List<ShipSectionAsset>();
			List<LogicalModule> logicalModuleList = new List<LogicalModule>();
			if (source3 != null)
			{
				List<XmlElement> list = source3.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Section", StringComparison.InvariantCulture))).ToList<XmlElement>();
				list.Select<XmlElement, string>((Func<XmlElement, string>)(x => x["SectionFile"].InnerText));
				foreach (XmlElement source4 in list)
				{
					string sectionFile = source4["SectionFile"].InnerText;
					ShipSectionAsset shipSectionAsset = game.AssetDatabase.ShipSections.FirstOrDefault<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionFile));
					sectionAssets.Add(shipSectionAsset);
					List<LogicalModule> source5 = new List<LogicalModule>();
					foreach (XmlElement xmlElement5 in source4.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Module", StringComparison.InvariantCulture))))
					{
						string moduleNodeName = xmlElement5["Mount"].InnerText;
						string modulePath = xmlElement5["ModuleId"].InnerText;
						LogicalModule logicalModule = game.AssetDatabase.Modules.First<LogicalModule>((Func<LogicalModule, bool>)(x => x.ModulePath == modulePath));
						if (!source5.Contains(logicalModule))
							source5.Add(logicalModule);
						moduleAssignmentList.Add(new ModuleAssignment()
						{
							ModuleMount = ((IEnumerable<LogicalModuleMount>)shipSectionAsset.Modules).First<LogicalModuleMount>((Func<LogicalModuleMount, bool>)(x => x.NodeName == moduleNodeName)),
							Module = logicalModule,
							PsionicAbilities = (SectionEnumerations.PsionicAbility[])null
						});
					}
					IEnumerable<LogicalBank> source6 = ((IEnumerable<LogicalBank>)shipSectionAsset.Banks).Concat<LogicalBank>(source5.SelectMany<LogicalModule, LogicalBank>((Func<LogicalModule, IEnumerable<LogicalBank>>)(x => (IEnumerable<LogicalBank>)x.Banks)));
					foreach (XmlElement xmlElement5 in source4.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("Bank", StringComparison.InvariantCulture))))
					{
						Guid bankGuid = Guid.Parse(xmlElement5["Id"].InnerText);
						string weaponName = xmlElement5["Weapon"].InnerText;
						weaponAssignmentList.Add(new WeaponAssignment()
						{
							Bank = source6.First<LogicalBank>((Func<LogicalBank, bool>)(x => x.GUID == bankGuid)),
							Weapon = game.AssetDatabase.Weapons.First<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => x.WeaponName == weaponName))
						});
					}
				}
			}
			IEnumerable<string> weapons = source1 == null ? (IEnumerable<string>)new string[0] : source1.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("string", StringComparison.InvariantCulture))).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.InnerText));
			IEnumerable<string> modules = source2 == null ? (IEnumerable<string>)new string[0] : source2.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name.Equals("string", StringComparison.InvariantCulture))).Select<XmlElement, string>((Func<XmlElement, string>)(x => x.InnerText));
			string str = xmlElement1 == null ? "USS Placeholder" : xmlElement1.InnerText;
			int integerOrDefault = node["PlayerID"].ExtractIntegerOrDefault(0);
			Player player = game.GetPlayer(integerOrDefault);
			Ship ship = Ship.CreateShip(game, new CreateShipParams()
			{
				player = player,
				sections = (IEnumerable<ShipSectionAsset>)sectionAssets,
				turretHousings = game.AssetDatabase.TurretHousings,
				weapons = game.AssetDatabase.Weapons,
				preferredWeapons = game.AssetDatabase.Weapons.Where<LogicalWeapon>((Func<LogicalWeapon, bool>)(x => weapons.Contains<string>(x.Name))),
				assignedWeapons = (IEnumerable<WeaponAssignment>)weaponAssignmentList,
				modules = game.AssetDatabase.Modules,
				preferredModules = game.AssetDatabase.Modules.Where<LogicalModule>((Func<LogicalModule, bool>)(x => modules.Contains<string>(x.ModuleName))),
				assignedModules = (IEnumerable<ModuleAssignment>)moduleAssignmentList,
				psionics = game.AssetDatabase.Psionics,
				faction = game.AssetDatabase.Factions.First<Faction>((Func<Faction, bool>)(x => sectionAssets.First<ShipSectionAsset>().Faction == x.Name)),
				shipName = str,
				inputID = context.InputID
			});
			Vector3 vector3OrDefault1 = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			Vector3 vector3OrDefault2 = node["Rotation"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPositionAndRotation(ref vector3OrDefault1, ref vector3OrDefault2);
			ship.Position = vector3OrDefault1;
			ship.Rotation = vector3OrDefault2;
			return (IGameObject)ship;
		}

		private static IGameObject CreateProceduralStellarBody(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			float singleOrDefault1 = node["Radius"].ExtractSingleOrDefault(5000f);
			int integerOrDefault = node["RandomSeed"].ExtractIntegerOrDefault(0);
			string stringOrDefault1 = node["PlanetType"].ExtractStringOrDefault("normal");
			float singleOrDefault2 = node["HazardRating"].ExtractSingleOrDefault(0.0f);
			string stringOrDefault2 = node["Faction"].ExtractStringOrDefault("human");
			float singleOrDefault3 = node["Biosphere"].ExtractSingleOrDefault(0.0f);
			double doubleOrDefault = node["Population"].ExtractDoubleOrDefault(0.0);
			Vector3 vector3OrDefault = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref vector3OrDefault);
			int? typeVariant = new int?();
			if (node["Variant"] != null)
				typeVariant = new int?(node["Variant"].ExtractIntegerOrDefault(0));
			StellarBody.Params stellarBodyParams = game.AssetDatabase.PlanetGenerationRules.GetStellarBodyParams("sysmap_planet", vector3OrDefault, singleOrDefault1, integerOrDefault, 0, stringOrDefault1, singleOrDefault2, 750f, stringOrDefault2, singleOrDefault3, doubleOrDefault, typeVariant, ColonyStage.Open, SystemColonyType.Normal);
			stellarBodyParams.Civilians = new StellarBody.PlanetCivilianData[0];
			stellarBodyParams.ImperialPopulation = 0.0;
			stellarBodyParams.Suitability = 0.0f;
			stellarBodyParams.Infrastructure = 0.0f;
			return (IGameObject)StellarBody.Create(game, stellarBodyParams);
		}

		private static IGameObject CreateLegacyStellarBody(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			StellarBody.Params p = StellarBody.Params.Default;
			p.SurfaceMaterial = node["Asset"].ExtractStringOrDefault(string.Empty);
			p.Position = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref p.Position);
			p.Radius = node["Scale"].ExtractSingleOrDefault(0.0f);
			p.AtmoThickness = node["AtmosphereThickness"].ExtractSingleOrDefault(0.0f);
			p.AtmoScatterWaveLengths = node["AtmosphereScatteringWavelengths"].ExtractVector3OrDefault(Vector3.Zero);
			p.AtmoKm = node["AtmosphereMieConstant"].ExtractSingleOrDefault(0.0f);
			p.AtmoKr = node["AtmosphereRayleighConstant"].ExtractSingleOrDefault(0.0f);
			p.AtmoScaleDepth = node["AtmosphereScaleDepth"].ExtractSingleOrDefault(0.0f);
			p.Civilians = new StellarBody.PlanetCivilianData[0];
			p.ImperialPopulation = 0.0;
			p.Suitability = 0.0f;
			p.Infrastructure = 0.0f;
			return (IGameObject)StellarBody.Create(game, p);
		}

		private static IGameObject CreateProceduralStar(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			string stringOrDefault1 = node["StellarClass"].ExtractStringOrDefault("G2V");
			string stringOrDefault2 = node["Name"].ExtractStringOrDefault(string.Empty);
			Vector3 vector3OrDefault = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref vector3OrDefault);
			return (IGameObject)Kerberos.Sots.GameStates.StarSystem.CreateStar(game, vector3OrDefault, StellarClass.Parse(stringOrDefault1), stringOrDefault2, 1f, true);
		}

		private static IGameObject CreateLegacyStarModel(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			bool impostorEnabled = true;
			Vector3 vector3OrDefault = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref vector3OrDefault);
			return (IGameObject)new StarModel(game, node["Asset"].ExtractStringOrDefault(string.Empty), vector3OrDefault, node["Scale"].ExtractSingleOrDefault(0.0f), true, node["ImpostorMaterial"].ExtractStringOrDefault(string.Empty), node["ImpostorSpriteScale"].ExtractVector2OrDefault(Vector2.One), node["ImpostorRange"].ExtractVector2OrDefault(Vector2.Zero), node["ImpostorVertexColor"].ExtractVector3OrDefault(Vector3.One), impostorEnabled, string.Empty);
		}

		private static IGameObject CreateDefaultGameObject(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			XmlElement xmlElement = node["Asset"];
			IGameObject gameObject = game.AddObject((InteropGameObjectType)Enum.Parse(typeof(InteropGameObjectType), node["Type"].InnerText), (object[])new string[1]
			{
		xmlElement != null ? xmlElement.InnerText : string.Empty
			});
			Vector3 vector3OrDefault1 = node["Position"].ExtractVector3OrDefault(Vector3.Zero);
			Vector3 vector3OrDefault2 = node["Rotation"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPositionAndRotation(ref vector3OrDefault1, ref vector3OrDefault2);
			if (gameObject is IPosition)
				(gameObject as IPosition).Position = vector3OrDefault1;
			if (gameObject is IScalable)
				(gameObject as IScalable).Scale = node["Scale"].ExtractSingleOrDefault(1f);
			if (gameObject is IOrientatable)
				(gameObject as IOrientatable).Rotation = vector3OrDefault2;
			return gameObject;
		}

		private static IGameObject CreateAsteroidBelt(
		  App game,
		  CombatConfig.DataContext context,
		  XmlElement node)
		{
			int integerOrDefault1 = node["RandomSeed"].ExtractIntegerOrDefault(0);
			Vector3 vector3OrDefault = node["Center"].ExtractVector3OrDefault(Vector3.Zero);
			context.TransformPosition(ref vector3OrDefault);
			float singleOrDefault1 = node["InnerRadius"].ExtractSingleOrDefault(10000f);
			float singleOrDefault2 = node["OuterRadius"].ExtractSingleOrDefault(20000f);
			float singleOrDefault3 = node["MinimumHeight"].ExtractSingleOrDefault(0.0f);
			float singleOrDefault4 = node["MaximumHeight"].ExtractSingleOrDefault(0.0f);
			int integerOrDefault2 = node["InitialCount"].ExtractIntegerOrDefault(1000);
			return (IGameObject)new AsteroidBelt(game, integerOrDefault1, vector3OrDefault, singleOrDefault1, singleOrDefault2, singleOrDefault3, singleOrDefault4, integerOrDefault2);
		}

		private static void CreateGameObjectsCore(
		  Dictionary<IGameObject, XmlElement> map,
		  App game,
		  CombatConfig.DataContext context,
		  Vector3 shipStartPositionHack,
		  XmlElement root)
		{
			foreach (XmlElement xmlElement in root.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x =>
		   {
			   if (!(x.Name == "GameObject"))
				   return x.Name == "Group";
			   return true;
		   })))
			{
				if (xmlElement.Name == "Group")
				{
					Vector3 vector3OrDefault1 = xmlElement["Position"].ExtractVector3OrDefault(Vector3.Zero);
					Vector3 vector3OrDefault2 = xmlElement["Rotation"].ExtractVector3OrDefault(Vector3.Zero);
					Matrix rotationYpr = Matrix.CreateRotationYPR(MathHelper.DegreesToRadians(vector3OrDefault2.X), MathHelper.DegreesToRadians(vector3OrDefault2.Y), MathHelper.DegreesToRadians(vector3OrDefault2.Z));
					rotationYpr.Position = vector3OrDefault1;
					CombatConfig.DataContext context1 = context.Clone();
					context1.Origin = rotationYpr * context.Origin;
					CombatConfig.CreateGameObjectsCore(map, game, context1, shipStartPositionHack, xmlElement);
				}
				else
				{
					string innerText = xmlElement["Type"].InnerText;
					if (innerText == "AsteroidBelt")
						map[CombatConfig.CreateAsteroidBelt(game, context, xmlElement)] = xmlElement;
					else if (innerText == "Star")
						map[CombatConfig.CreateProceduralStar(game, context, xmlElement)] = xmlElement;
					else if (innerText == "StellarBody")
						map[CombatConfig.CreateProceduralStellarBody(game, context, xmlElement)] = xmlElement;
					else if (innerText == "Ship")
					{
						CombatConfig.DataContext context1 = context;
						if (shipStartPositionHack != Vector3.Zero)
						{
							context1 = context.Clone();
							context1.Origin.Position -= shipStartPositionHack;
						}
						map[CombatConfig.CreateShipCompound(game, context1, xmlElement)] = xmlElement;
					}
					else if (innerText == "LegacyStellarBody")
						map[CombatConfig.CreateLegacyStellarBody(game, context, xmlElement)] = xmlElement;
					else if (innerText == "LegacyStar")
						map[CombatConfig.CreateLegacyStarModel(game, context, xmlElement)] = xmlElement;
					else
						map[CombatConfig.CreateDefaultGameObject(game, context, xmlElement)] = xmlElement;
				}
			}
		}

		public static Dictionary<IGameObject, XmlElement> CreateGameObjects(
		  App game,
		  Vector3 origin,
		  XmlDocument doc,
		  int inputId)
		{
			Dictionary<IGameObject, XmlElement> map = new Dictionary<IGameObject, XmlElement>();
			XmlElement xmlElement1 = doc[nameof(CombatConfig)];
			if (xmlElement1 == null)
				return map;
			XmlElement xmlElement2 = xmlElement1["GameObjects"];
			if (xmlElement2 == null)
				return map;
			CombatConfig.DataContext context = new CombatConfig.DataContext();
			context.Origin = Matrix.Identity;
			context.InputID = inputId;
			XmlElement xmlElement3 = xmlElement2["StartPoint"];
			Vector3 shipStartPositionHack = Vector3.Zero;
			if (xmlElement3 != null)
				shipStartPositionHack = xmlElement3["Position"].ExtractVector3OrDefault(Vector3.Zero);
			foreach (XmlElement xmlElement4 in xmlElement2.OfType<XmlElement>().Where<XmlElement>((Func<XmlElement, bool>)(x => x.Name == "Player")))
			{
				int integerOrDefault1 = xmlElement4["ID"].ExtractIntegerOrDefault(1);
				Player player = game.GetPlayer(integerOrDefault1);
				if (xmlElement4["EmpireColorIndex"] != null)
				{
					int integerOrDefault2 = xmlElement4["EmpireColorIndex"].ExtractIntegerOrDefault(0);
					player.SetEmpireColor(integerOrDefault2);
				}
				if (xmlElement4["PlayerColor"] != null)
				{
					Vector3 vector3OrDefault = xmlElement4["PlayerColor"].ExtractVector3OrDefault(Vector3.Zero);
					player.SetPlayerColor(vector3OrDefault);
				}
				if (xmlElement4["Badge"] != null)
				{
					string stringOrDefault = xmlElement4["Badge"].ExtractStringOrDefault(string.Empty);
					player.SetBadgeTexture(stringOrDefault);
				}
			}
			CombatConfig.CreateGameObjectsCore(map, game, context, shipStartPositionHack, xmlElement2);
			return map;
		}

		public static void ChangeXmlElementPositionAndRotation(
		  XmlElement gameObjectElement,
		  Vector3 position,
		  Vector3 rotation)
		{
			XmlElement xmlElement1 = gameObjectElement["Position"];
			XmlElement xmlElement2 = gameObjectElement["Rotation"];
			xmlElement1.InnerText = position.ToString();
			xmlElement2.InnerText = rotation.ToString();
		}

		public static XmlElement ExportXmlElementFromShipParameters(
		  App game,
		  XmlDocument owner,
		  IEnumerable<string> sectionFileNames,
		  IEnumerable<WeaponAssignment> weaponAssignments,
		  IEnumerable<ModuleAssignment> moduleAssignments,
		  int playerID,
		  Vector3 position,
		  Vector3 rotation)
		{
			XmlElement element1 = owner.CreateElement("GameObject", null);
			XmlElement element2 = owner.CreateElement("Type", null);
			element2.InnerText = "Ship";
			element1.AppendChild((XmlNode)element2);
			XmlElement element3 = owner.CreateElement("ShipName", null);
			element3.InnerText = sectionFileNames.Any<string>() ? Path.GetFileNameWithoutExtension(sectionFileNames.First<string>()) : "USS Placeholder";
			element1.AppendChild((XmlNode)element3);
			XmlElement element4 = owner.CreateElement("PlayerID", null);
			element4.InnerText = playerID.ToString();
			element1.AppendChild((XmlNode)element4);
			XmlElement element5 = owner.CreateElement("Position", null);
			element5.InnerText = position.ToString();
			element1.AppendChild((XmlNode)element5);
			XmlElement element6 = owner.CreateElement("Rotation", null);
			element6.InnerText = rotation.ToString();
			element1.AppendChild((XmlNode)element6);
			XmlElement element7 = owner.CreateElement("ShipDesign", null);
			XmlElement element8 = owner.CreateElement("Sections", null);
			foreach (string sectionFileName1 in sectionFileNames)
			{
				string sectionFileName = sectionFileName1;
				ShipSectionAsset section = game.AssetDatabase.ShipSections.First<ShipSectionAsset>((Func<ShipSectionAsset, bool>)(x => x.FileName == sectionFileName));
				XmlElement element9 = owner.CreateElement("Section", null);
				XmlElement element10 = owner.CreateElement("SectionFile", null);
				element10.InnerText = sectionFileName;
				element9.AppendChild((XmlNode)element10);
				List<ModuleAssignment> list = moduleAssignments.Where<ModuleAssignment>((Func<ModuleAssignment, bool>)(x => ((IEnumerable<LogicalModuleMount>)section.Modules).Contains<LogicalModuleMount>(x.ModuleMount))).ToList<ModuleAssignment>();
				foreach (ModuleAssignment moduleAssignment in list)
				{
					XmlElement element11 = owner.CreateElement("Mount", null);
					element11.InnerText = moduleAssignment.ModuleMount.NodeName;
					XmlElement element12 = owner.CreateElement("ModuleId", null);
					element12.InnerText = moduleAssignment.Module.ModulePath;
					XmlElement element13 = owner.CreateElement("Module", null);
					element13.AppendChild((XmlNode)element11);
					element13.AppendChild((XmlNode)element12);
					element9.AppendChild((XmlNode)element13);
				}
				List<LogicalBank> sectionBanks = ((IEnumerable<LogicalBank>)section.Banks).Concat<LogicalBank>(list.SelectMany<ModuleAssignment, LogicalBank>((Func<ModuleAssignment, IEnumerable<LogicalBank>>)(x => (IEnumerable<LogicalBank>)x.Module.Banks))).ToList<LogicalBank>();
				foreach (WeaponAssignment weaponAssignment in weaponAssignments.Where<WeaponAssignment>((Func<WeaponAssignment, bool>)(x => sectionBanks.Contains(x.Bank))))
				{
					XmlElement element11 = owner.CreateElement("Id", null);
					element11.InnerText = weaponAssignment.Bank.GUID.ToString();
					XmlElement element12 = owner.CreateElement("Weapon", null);
					element12.InnerText = weaponAssignment.Weapon.WeaponName;
					XmlElement element13 = owner.CreateElement("Bank", null);
					element13.AppendChild((XmlNode)element11);
					element13.AppendChild((XmlNode)element12);
					element9.AppendChild((XmlNode)element13);
				}
				element8.AppendChild((XmlNode)element9);
			}
			element7.AppendChild((XmlNode)element8);
			element1.AppendChild((XmlNode)element7);
			return element1;
		}

		public static XmlDocument CreateEmptyCombatConfigXml()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<CombatConfig xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n<GameObjects>\n</GameObjects>\n</CombatConfig>\n");
			return xmlDocument;
		}

		public static XmlElement GetGameObjectsElement(XmlDocument d)
		{
			return d[nameof(CombatConfig)]["GameObjects"];
		}

		public static void AppendConfigXml(XmlDocument destination, XmlDocument source)
		{
			XmlElement gameObjectsElement = CombatConfig.GetGameObjectsElement(destination);
			foreach (XmlElement xmlElement in CombatConfig.GetGameObjectsElement(source).OfType<XmlElement>())
				gameObjectsElement.AppendChild(gameObjectsElement.OwnerDocument.ImportNode((XmlNode)xmlElement, true));
		}

		private class DataContext
		{
			public Matrix Origin;
			public int InputID;

			public CombatConfig.DataContext Clone()
			{
				return new CombatConfig.DataContext()
				{
					Origin = this.Origin,
					InputID = this.InputID
				};
			}

			public void TransformPositionAndRotation(ref Vector3 pos, ref Vector3 rot)
			{
				Matrix rotationYpr = Matrix.CreateRotationYPR(Vector3.DegreesToRadians(rot));
				rotationYpr.Position = pos;
				Matrix matrix = rotationYpr * this.Origin;
				pos = matrix.Position;
				rot = Vector3.RadiansToDegrees(matrix.EulerAngles);
			}

			public void TransformPosition(ref Vector3 pos)
			{
				Vector3 zero = Vector3.Zero;
				this.TransformPositionAndRotation(ref pos, ref zero);
			}
		}
	}
}
