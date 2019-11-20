// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Data.Xml.XmlHelper
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;
using Kerberos.Sots.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Kerberos.Sots.Data.Xml
{
	public static class XmlHelper
	{
		public static Exception NoXmlNameException = new Exception("The item you tried to save does not have a default XML name - Either provide one with the XmlName.get() function or use the save method that provides a node name.");

		public static void Load(this XmlDocument document, IFileSystem fileSystem, string path)
		{
			using (Stream stream = fileSystem.CreateStream(path))
				document.Load(stream);
		}

		public static int ExtractIntegerOrDefault(this XmlElement e, int defaultValue)
		{
			if (e == null)
				return defaultValue;
			return int.Parse(e.InnerText);
		}

		public static string ExtractStringOrDefault(this XmlElement e, string defaultValue)
		{
			if (e == null)
				return defaultValue;
			return e.InnerText;
		}

		public static float ExtractSingleOrDefault(this XmlElement e, float defaultValue)
		{
			if (e == null)
				return defaultValue;
			return float.Parse(e.InnerText);
		}

		public static double ExtractDoubleOrDefault(this XmlElement e, double defaultValue)
		{
			if (e == null)
				return defaultValue;
			return double.Parse(e.InnerText);
		}

		public static Vector2 ExtractVector2OrDefault(this XmlElement e, Vector2 defaultValue)
		{
			if (e == null)
				return defaultValue;
			return Vector2.Parse(e.InnerText);
		}

		public static Vector3 ExtractVector3OrDefault(this XmlElement e, Vector3 defaultValue)
		{
			if (e == null)
				return defaultValue;
			return Vector3.Parse(e.InnerText);
		}

		public static T GetDataOrDefault<T>(XmlElement node, T defaultValue)
		{
			if (node == null)
				return defaultValue;
			T obj;
			try
			{
				obj = !typeof(T).IsGenericType || !typeof(T).GetGenericTypeDefinition().Equals(typeof(Nullable<>)) ? (T)Convert.ChangeType((object)node.InnerText, typeof(T)) : (!string.IsNullOrEmpty(node.InnerText) ? (T)Convert.ChangeType((object)node.InnerText, Nullable.GetUnderlyingType(typeof(T))) : default(T));
			}
			catch
			{
				if (App.Log != null)
					App.Log.Trace(string.Format("The node {0} could not be parsed in {1}.  The default '{2}' will be used for the variable.", (object)node.Name, node.ParentNode != null ? (object)node.ParentNode.Name : (object)"<root>", (object)defaultValue), "data");
				obj = defaultValue;
			}
			return obj;
		}

		public static T GetData<T>(XmlElement node)
		{
			return XmlHelper.GetDataOrDefault<T>(node, default(T));
		}

		public static T GetData<T>(XmlElement rootNode, string nodeName)
		{
			return XmlHelper.GetDataOrDefault<T>(rootNode[nodeName], default(T));
		}

		public static T GetData<T>(XmlElement rootNode, Dictionary<string, Type> TypeMap)
		{
			if (!rootNode.HasChildNodes)
				return default(T);
			XmlElement firstChild = (XmlElement)rootNode.FirstChild;
			if (!TypeMap.ContainsKey(firstChild.Name))
				return default(T);
			T instance = (T)Activator.CreateInstance(TypeMap[firstChild.Name]);
			((object)instance as IXmlLoadSave).LoadFromXmlNode(firstChild);
			return instance;
		}

		public static List<T> GetDataObjectCollection<T>(
		  XmlElement rootNode,
		  string nodeName,
		  string memberName)
		  where T : new()
		{
			List<T> objList = new List<T>();
			if (rootNode[nodeName] != null)
			{
				foreach (XmlElement childNode in rootNode[nodeName].ChildNodes)
				{
					if (childNode.Name == memberName)
					{
						T obj = new T();
						((object)obj as IXmlLoadSave).LoadFromXmlNode(childNode);
						objList.Add(obj);
					}
				}
			}
			return objList;
		}

		public static T GetDataObject<T>(
		  XmlElement rootNode,
		  string nodeName,
		  Dictionary<string, Type> TypeMap)
		  where T : new()
		{
			if (rootNode[nodeName] != null)
			{
				XmlElement firstChild = (XmlElement)rootNode[nodeName].FirstChild;
				if (TypeMap.ContainsKey(firstChild.Name))
				{
					T instance = (T)Activator.CreateInstance(TypeMap[firstChild.Name]);
					((object)instance as IXmlLoadSave).LoadFromXmlNode(firstChild);
					return instance;
				}
			}
			return default(T);
		}

		public static List<T> GetDataObjectCollection<T>(
		  XmlElement rootNode,
		  string nodeName,
		  Dictionary<string, Type> TypeMap)
		  where T : new()
		{
			List<T> objList = new List<T>();
			if (rootNode[nodeName] != null)
			{
				foreach (XmlElement childNode in rootNode[nodeName].ChildNodes)
				{
					if (TypeMap.ContainsKey(childNode.Name))
					{
						T instance = (T)Activator.CreateInstance(TypeMap[childNode.Name]);
						((object)instance as IXmlLoadSave).LoadFromXmlNode(childNode);
						objList.Add(instance);
					}
				}
			}
			return objList;
		}

		public static List<T> GetDataCollection<T>(
		  XmlElement rootNode,
		  string nodeName,
		  string memberName)
		{
			List<T> objList = new List<T>();
			if (rootNode[nodeName] != null)
			{
				foreach (XmlElement childNode in rootNode[nodeName].ChildNodes)
				{
					if (childNode.Name == memberName)
						objList.Add(XmlHelper.GetData<T>(childNode));
				}
			}
			return objList;
		}

		public static void AddObjectNode(IXmlLoadSave item, string nodeName, ref XmlElement Root)
		{
			XmlElement element1 = Root.OwnerDocument.CreateElement(nodeName);
			XmlElement element2 = Root.OwnerDocument.CreateElement(item.XmlName);
			item.AttachToXmlNode(ref element2);
			element1.AppendChild((XmlNode)element2);
			Root.AppendChild((XmlNode)element1);
		}

		public static void AddObjectCollectionNode(
		  IEnumerable<IXmlLoadSave> items,
		  string nodeName,
		  ref XmlElement Root)
		{
			XmlElement element1 = Root.OwnerDocument.CreateElement(nodeName);
			foreach (IXmlLoadSave xmlLoadSave in items)
			{
				XmlElement element2 = Root.OwnerDocument.CreateElement(xmlLoadSave.XmlName);
				xmlLoadSave.AttachToXmlNode(ref element2);
				element1.AppendChild((XmlNode)element2);
			}
			Root.AppendChild((XmlNode)element1);
		}

		public static void AddObjectCollectionNode(
		  IEnumerable<IXmlLoadSave> items,
		  string nodeName,
		  string memberName,
		  ref XmlElement Root)
		{
			XmlElement element1 = Root.OwnerDocument.CreateElement(nodeName);
			foreach (IXmlLoadSave xmlLoadSave in items)
			{
				XmlElement element2 = Root.OwnerDocument.CreateElement(memberName);
				xmlLoadSave.AttachToXmlNode(ref element2);
				element1.AppendChild((XmlNode)element2);
			}
			Root.AppendChild((XmlNode)element1);
		}

		public static void AddCollectionNode<T>(
		  IEnumerable<T> items,
		  string nodeName,
		  string memberName,
		  ref XmlElement Root)
		{
			XmlElement element = Root.OwnerDocument.CreateElement(nodeName);
			foreach (T obj in items)
				XmlHelper.AddNode((object)obj, memberName, ref element);
			Root.AppendChild((XmlNode)element);
		}

		public static void AddNode(IXmlLoadSave Data, string Name, ref XmlElement Root)
		{
			if (Data == null)
				return;
			XmlElement element = Root.OwnerDocument.CreateElement(Name);
			Data.AttachToXmlNode(ref element);
			Root.AppendChild((XmlNode)element);
		}

		public static void AddNode(object Data, string Name, ref XmlElement Root)
		{
			XmlElement element = Root.OwnerDocument.CreateElement(Name);
			if (Data != null)
				element.InnerText = Data.ToString();
			Root.AppendChild((XmlNode)element);
		}
	}
}
