// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.UI.ImageLabel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Engine;

namespace Kerberos.Sots.UI
{
	internal class ImageLabel : PanelBinding
	{
		private readonly Image _image;
		private readonly Label _label;

		public Image Image
		{
			get
			{
				return this._image;
			}
		}

		public Label Label
		{
			get
			{
				return this._label;
			}
		}

		public ImageLabel(UICommChannel ui, string id)
		  : base(ui, id)
		{
			this._image = new Image(ui, this.UI.Path(this.ID, "icon"));
			this._label = new Label(ui, this.UI.Path(this.ID, "label"));
		}
	}
}
