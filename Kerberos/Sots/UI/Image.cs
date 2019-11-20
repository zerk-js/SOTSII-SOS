// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.Image
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class Image : PanelBinding
	{
		public string TextureAssetPath { get; private set; }

		public string SpriteName { get; private set; }

		public void SetTexture(string textureAssetPath)
		{
			if (textureAssetPath == null)
				textureAssetPath = string.Empty;
			this.TextureAssetPath = textureAssetPath;
			this.SpriteName = null;
			this.UI.SetPropertyString(this.ID, "texture", textureAssetPath);
		}

		public void SetSprite(string spriteName)
		{
			if (spriteName == null)
				spriteName = string.Empty;
			this.TextureAssetPath = null;
			this.SpriteName = spriteName;
			this.UI.SetPropertyString(this.ID, "sprite", spriteName);
		}

		public Image(UICommChannel ui, string id)
		  : base(ui, id)
		{
		}
	}
}
