// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.WeaponFramework.GridDescriptor
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Data.Xml;
using System.Xml;

namespace Kerberos.Sots.Data.WeaponFramework
{
	public class GridDescriptor : IXmlLoadSave
	{
		public string Data = "";
		private const int DEFAULT_GRID_HEIGHT = 10;
		private const int DEFAULT_GRID_WIDTH = 10;
		private const string XmlDimensionsName = "Dimensions";
		private const string XmlDataName = "Data";
		private const string XmlCollisionPointName = "CollisionPoint";
		public int Width;
		public int Height;
		public int CollisionX;
		public int CollisionY;

		public GridDescriptor()
		{
			this.Height = 10;
			this.Width = 10;
		}

		public string XmlName
		{
			get
			{
				throw XmlHelper.NoXmlNameException;
			}
		}

		public void AttachToXmlNode(ref XmlElement node)
		{
			XmlHelper.AddNode((object)string.Format("{0},{1}", (object)this.Width, (object)this.Height), "Dimensions", ref node);
			XmlHelper.AddNode((object)string.Format("{0},{1}", (object)this.CollisionX, (object)this.CollisionY), "CollisionPoint", ref node);
			XmlHelper.AddNode((object)this.Data, "Data", ref node);
		}

		public void LoadFromXmlNode(XmlElement node)
		{
			if (node == null)
				return;
			string data1 = XmlHelper.GetData<string>(node["Dimensions"]);
			this.Width = int.Parse(data1.Split(',')[0]);
			this.Height = int.Parse(data1.Split(',')[1]);
			string data2 = XmlHelper.GetData<string>(node["CollisionPoint"]);
			this.CollisionX = int.Parse(data2.Split(',')[0]);
			this.CollisionY = int.Parse(data2.Split(',')[1]);
			this.Data = XmlHelper.GetData<string>(node["Data"]);
		}
	}
}
