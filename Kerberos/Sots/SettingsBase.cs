// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.SettingsBase
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Kerberos.Sots
{
	internal abstract class SettingsBase
	{
		protected XmlDocument _doc = new XmlDocument();
		protected string _path;
		protected XmlElement _settingsElement;
		protected PropertyInfo[] _props;

		private XmlElement GetPropertyElement(PropertyInfo propInfo)
		{
			XmlElement element = this._settingsElement[propInfo.Name];
			if (element == null)
			{
				element = this._doc.CreateElement(propInfo.Name);
				element.InnerText = Convert.ChangeType(propInfo.GetValue((object)this, (object[])null), typeof(string)) as string;
				this._settingsElement.AppendChild((XmlNode)element);
			}
			return element;
		}

		protected SettingsBase(string settingsDirectory)
		{
			this.RegisterProperties();
			this._path = Path.Combine(settingsDirectory, "settings.xml");
		}

		private void RegisterProperties()
		{
			PropertyInfo[] properties = this.GetType().GetProperties();
			List<PropertyInfo> propertyInfoList = new List<PropertyInfo>();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.CanRead && propertyInfo.CanWrite && (propertyInfo.GetGetMethod().IsPublic && propertyInfo.GetSetMethod().IsPublic))
					propertyInfoList.Add(propertyInfo);
			}
			this._props = propertyInfoList.ToArray();
		}

		private void New()
		{
			this._doc = new XmlDocument();
			this._doc.AppendChild((XmlNode)this._doc.CreateElement("Settings"));
		}

		public void Load()
		{
			if (File.Exists(this._path))
			{
				this._doc = new XmlDocument();
				if (App.GetStreamForFile(this._path) == null)
					this._doc.Load(this._path);
				else
					this._doc.Load(App.GetStreamForFile(this._path));
			}
			else
				this.New();
			this._settingsElement = this._doc["Settings"];
			foreach (PropertyInfo prop in this._props)
			{
				XmlElement propertyElement = this.GetPropertyElement(prop);
				try
				{
					prop.SetValue((object)this, Convert.ChangeType((object)propertyElement.InnerText, prop.PropertyType), (object[])null);
				}
				catch (FormatException ex)
				{
					App.Log.Warn(string.Format("Could not convert {0} value '{1}' to type {2}. Keeping existing value '{3}'.", (object)prop.Name, (object)propertyElement.InnerText, (object)prop.PropertyType.Name, (object)prop.GetValue((object)this, (object[])null).ToString()), "config");
				}
			}
		}

		public void Save()
		{
			foreach (PropertyInfo prop in this._props)
				this.GetPropertyElement(prop).InnerText = Convert.ChangeType(prop.GetValue((object)this, (object[])null), typeof(string)) as string;
			if (App.GetStreamForFile(this._path) == null)
			{
				this._doc.Save(this._path);
			}
			else
			{
				Stream streamForFile = App.GetStreamForFile(this._path);
				streamForFile.SetLength(0L);
				this._doc.Save(streamForFile);
			}
		}
	}
}
