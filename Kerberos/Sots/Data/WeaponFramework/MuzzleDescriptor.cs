// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.MuzzleDescriptor
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class MuzzleDescriptor : IXmlLoadSave
	{
		public string MuzzleType = "";
		protected const string XmlMuzzleDescriptorName = "MuzzleDescriptor";
		private const string XmlMuzzleTypeName = "MuzzleType";
		private const string XmlWidthName = "Width";
		private const string XmlHeightName = "Height";
		public float Width;
		public float Height;

		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)this.MuzzleType, "MuzzleType", ref node);
			XmlHelper.AddNode((object)this.Width, "Width", ref node);
			XmlHelper.AddNode((object)this.Height, "Height", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			this.MuzzleType = XmlHelper.GetData<string>(node, "MuzzleType");
			this.Width = (float)XmlHelper.GetData<int>(node, "Width");
			this.Height = (float)XmlHelper.GetData<int>(node, "Height");
		}
	}
}
