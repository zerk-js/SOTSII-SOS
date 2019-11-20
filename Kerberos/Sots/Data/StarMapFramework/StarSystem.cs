// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.StarSystem
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Collections.Generic;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class StarSystem : Feature
	{
		public string Type = "";
		public string SubType = "";
		public string Size = "";
		public List<Orbit> Orbits = new List<Orbit>();
		internal const string XmlSystemName = "System";
		private const string XmlProvinceIdName = "ProvinceId";
		private const string XmlGuidName = "Guid";
		private const string XmlTypeName = "Type";
		private const string XmlSubTypeName = "SubType";
		private const string XmlSizeName = "Size";
		private const string XmlPlayerSlotName = "PlayerSlot";
		private const string XmlRandomizeName = "Randomize";
		private const string XmlIsStartLocationName = "IsStartLocation";
		private const string XmlOrbitsName = "Orbits";
		public int? ProvinceId;
		public bool isRandom;
		public bool isStartLocation;
		public int? Guid;
		public int? PlayerSlot;

		public override string XmlName
		{
			get
			{
				return "System";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode((object)this.ProvinceId, "ProvinceId", ref node);
			XmlHelper.AddNode((object)this.Guid, "Guid", ref node);
			XmlHelper.AddNode((object)this.isRandom, "Randomize", ref node);
			XmlHelper.AddNode((object)this.isStartLocation, "IsStartLocation", ref node);
			XmlHelper.AddNode((object)this.PlayerSlot, "PlayerSlot", ref node);
			XmlHelper.AddNode((object)this.Type, "Type", ref node);
			XmlHelper.AddNode((object)this.SubType, "SubType", ref node);
			XmlHelper.AddNode((object)this.Size, "Size", ref node);
			XmlHelper.AddObjectCollectionNode((IEnumerable<IXmlLoadSave>)this.Orbits, "Orbits", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
			this.ProvinceId = XmlHelper.GetData<int?>(node, "ProvinceId");
			int? provinceId = this.ProvinceId;
			if ((provinceId.GetValueOrDefault() != -1 ? 0 : (provinceId.HasValue ? 1 : 0)) != 0)
				this.ProvinceId = new int?();
			this.Guid = XmlHelper.GetData<int?>(node, "Guid");
			int num = this.Guid.HasValue ? 1 : 0;
			this.isRandom = XmlHelper.GetData<bool>(node, "Randomize");
			this.isStartLocation = XmlHelper.GetData<bool>(node, "IsStartLocation");
			this.PlayerSlot = XmlHelper.GetData<int?>(node, "PlayerSlot");
			this.Type = XmlHelper.GetData<string>(node, "Type");
			this.SubType = XmlHelper.GetData<string>(node, "SubType");
			this.Size = XmlHelper.GetData<string>(node, "Size");
			this.Orbits = XmlHelper.GetDataObjectCollection<Orbit>(node, "Orbits", Orbit.TypeMap);
		}
	}
}
