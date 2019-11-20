// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ImageButton
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class ImageButton : Button
	{
		public Image IdleImage { get; private set; }

		public Image MouseOverImage { get; private set; }

		public Image PressedImage { get; private set; }

		public Image DisabledImage { get; private set; }

		public void SetTexture(string textureAssetPath)
		{
			this.IdleImage.SetTexture(textureAssetPath);
			this.MouseOverImage.SetTexture(textureAssetPath);
			this.PressedImage.SetTexture(textureAssetPath);
			this.DisabledImage.SetTexture(textureAssetPath);
		}

		public void SetSprite(string spriteAssetPath)
		{
			this.IdleImage.SetSprite(spriteAssetPath);
			this.MouseOverImage.SetSprite(spriteAssetPath);
			this.PressedImage.SetSprite(spriteAssetPath);
			this.DisabledImage.SetSprite(spriteAssetPath);
		}

		public ImageButton(UICommChannel ui, string id, string createFromTemplateID = null)
		  : base(ui, id, createFromTemplateID)
		{
			this.IdleImage = new Image(this.UI, this.UI.Path(this.ID, "idle"));
			this.MouseOverImage = new Image(this.UI, this.UI.Path(this.ID, "mouse_over"));
			this.PressedImage = new Image(this.UI, this.UI.Path(this.ID, "pressed"));
			this.DisabledImage = new Image(this.UI, this.UI.Path(this.ID, "disabled"));
		}
	}
}
