// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.StarMapFramework.GasGiantSmallOrbit
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.StarMapFramework
{
	public class GasGiantSmallOrbit : Orbit
	{
		public string MaterialName = "";
		public const string XmlGasGiantSmallName = "GasGiantSmall";
		public const string XmlSizeName = "Size";
		public const string XmlMaterialName = "MaterialName";
		private float? _size;

		public float? Size
		{
			get
			{
				return this._size;
			}
			set
			{
				if (value.HasValue && (double)value.Value > 0.0)
					this._size = value;
				else
					this._size = new float?();
			}
		}

		public override string XmlName
		{
			get
			{
				return "GasGiantSmall";
			}
		}

		public override void AttachToXmlNode(ref XmlElement node)
		{
			base.AttachToXmlNode(ref node);
			XmlHelper.AddNode((object)this.Size, "Size", ref node);
			XmlHelper.AddNode((object)this.MaterialName, "MaterialName", ref node);
		}

		public override void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			base.LoadFromXmlNode(node);
			this.Size = XmlHelper.GetData<float?>(node, "Size");
			this.MaterialName = XmlHelper.GetData<string>(node, "MaterialName");
		}
	}
}
