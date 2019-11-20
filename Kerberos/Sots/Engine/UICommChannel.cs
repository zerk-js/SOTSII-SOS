// Decompiled with JetBrains decompiler
// Type: Kerberos.Sots.Engine.UICommChannel
// Assembly: sots2_managed, Version=2.0.25104.1, Culture=neutral, PublicKeyToken=null
// MVID: 7BEBB796-D765-47D7-AFD1-D31EAC2170CD
// Assembly location: D:\Games\Sword of the Stars II Enhanced Edition\bin\x86\sots2_managed.dll

using Kerberos.Sots.Framework;
using Kerberos.Sots.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kerberos.Sots.Engine
{
	internal class UICommChannel
	{
		private static int DrawLayer = 15000;
		private ScriptMessageWriter _scriptMessageWriter = new ScriptMessageWriter();
		private readonly UnicodeEncoding _stringcoder = new UnicodeEncoding();
		private List<Dialog> _dialogStack = new List<Dialog>();
		public const string GameEventObjectClicked = "ObjectClicked";
		public const string GameEventContextMenu = "ContextMenu";
		public const string GameEventListContextMenu = "ListContextMenu";
		public const string GameEventDragAndDrop = "DragAndDropEvent";
		public const string GameEventMouseOver = "MouseOver";
		public const string GameEventLocalizeText = "LocalizeText";
		public const string ScreenReadyID = "screen_ready";
		public const string ScreenLoadedID = "screen_loaded";
		public const string TextChangedMsg = "text_changed";
		public const string TextConfirmedMsg = "edit_confirmed";
		public const string ButtonClickedID = "button_clicked";
		public const string ButtonRightClickedID = "button_rclicked";
		public const string EndTurnActivated = "endturn_activated";
		public const string SliderValueChangedMsg = "slider_value";
		public const string SliderOnNotchedMsg = "slider_notched";
		public const string ListSelectionChangedID = "list_sel_changed";
		public const string ListItemDblClickedID = "list_item_dblclk";
		public const string ListItemRightClickedID = "list_item_rightclk";
		public const string CheckBoxClickedMsg = "checkbox_clicked";
		public const string ColorChangedMsg = "color_changed";
		public const string DialogClosedMsg = "dialog_closed";
		public const string DialogOpenedMsg = "dialog_opened";
		public const string MovieDoneMsg = "movie_done";
		public const string MouseEnter = "mouse_enter";
		public const string MouseLeave = "mouse_leave";
		public const string ChatMessageReceived = "ChatMessage";
		public const string SliderPropValueMin = "value_min";
		public const string SliderPropValueMax = "value_max";
		public const string SliderPropValue = "value";
		public const string TextBoxPropTextFile = "text_file";
		public const string CheckBoxPropChecked = "checked";
		public const string LabelValuePropLabel = "label";
		public const string LabelValuePropValue = "value";
		public const string PanelPropText = "text";
		public const string ImagePropColor = "color";
		public const string ButtonTimeout = "timeout";
		public const string SliderPropNotchTolerance = "notch_tolerance";
		public const string SliderPropAddNotch = "add_notch";
		public const string SliderPropRemoveNotch = "rem_notch";
		public const string SliderPropClearNotches = "clear_notchs";
		public const string SliderSnapToNearestNotch = "auto_notch_snap";
		public const string PlayerInfoRequestResponse = "PlayerInfoRequestResponse";
		public const string PlayerStatusChanged = "PlayerStatusChanged";
		public static bool LogEnable;
		private readonly IMessageQueue _messageQueue;
		private readonly byte[] _messageBuffer;
		private int _insertItemID;

		public event UIEventGameEvent GameEvent;

		public event UIEventPanelMessage PanelMessage;

		public event UIEventUpdate UpdateEvent;

		public UICommChannel(IMessageQueue messageQueue)
		{
			this._messageQueue = messageQueue;
			this._messageBuffer = new byte[this._messageQueue.IncomingCapacity];
		}

		public void Update()
		{
			List<string> stringList = new List<string>();
			this._messageQueue.Update();
			while (true)
			{
				int nextMessage = this._messageQueue.GetNextMessage(this._messageBuffer);
				if (nextMessage != 0)
					this.ProcessEngineMessage(this._stringcoder.GetString(this._messageBuffer, 0, nextMessage));
				else
					break;
			}
			if (this.UpdateEvent == null)
				return;
			this.UpdateEvent();
		}

		public string Path(params string[] panelNames)
		{
			string empty = string.Empty;
			for (int index = 0; index < panelNames.Length; ++index)
			{
				if (index != 0 && panelNames[index - 1].Length > 0)
					empty += ".";
				empty += panelNames[index];
			}
			return empty;
		}

		public Dialog GetTopDialog()
		{
			if (this._dialogStack.Count<Dialog>() == 0)
				return (Dialog)null;
			return this._dialogStack.Last<Dialog>();
		}

		public void HandleDialogMessage(ScriptMessageReader mr)
		{
			string str = mr.ReadString();
			foreach (Dialog dialog in this._dialogStack)
			{
				if (str == dialog.ID)
				{
					dialog.HandleScriptMessage(mr);
					break;
				}
			}
		}

		public string CreateDialog(Dialog dialog, string parentPanel = null)
		{
			this.CreatePanelFromTemplate(dialog.Template, dialog.ID);
			if (parentPanel != null)
				this.SetParent(dialog.ID, parentPanel);
			else
				this.ParentToMainPanel(dialog.ID);
			this.SetDrawLayer(dialog.ID, UICommChannel.DrawLayer++);
			this.Send((object)"PushFocus", (object)dialog.ID);
			this._dialogStack.Add(dialog);
			dialog.Initialize();
			return dialog.ID;
		}

		public void CloseDialog(Dialog dialog, bool dispose = true)
		{
			if (!this._dialogStack.Contains(dialog))
				return;
			this._dialogStack.Remove(dialog);
			this.Send((object)"PopFocus", (object)dialog.ID);
			this.PanelMessage(dialog.ID, "dialog_closed", dialog.CloseDialog());
			this.DestroyPanel(dialog.ID);
			if (!dispose)
				return;
			dialog.Dispose();
		}

		public string CreateOverlay(Dialog dialog, string parentPanel = null)
		{
			this.CreatePanelFromTemplate(dialog.Template, dialog.ID);
			if (parentPanel != null)
				this.SetParent(dialog.ID, parentPanel);
			else
				this.ParentToMainPanel(dialog.ID);
			this.SetDrawLayer(dialog.ID, UICommChannel.DrawLayer++);
			dialog.Initialize();
			return dialog.ID;
		}

		public void ShowOverlay(Dialog overlay)
		{
			this._dialogStack.Add(overlay);
		}

		public void HideOverlay(Dialog overlay)
		{
			this._dialogStack.Remove(overlay);
		}

		public string FormatColor(Vector3 color)
		{
			return string.Format("{0},{1},{2}", (object)(float)((double)color.X * (double)byte.MaxValue), (object)(float)((double)color.Y * (double)byte.MaxValue), (object)(float)((double)color.Z * (double)byte.MaxValue));
		}

		public bool ParseListItemId(string msgParam, out int id)
		{
			id = 0;
			if (string.IsNullOrEmpty(msgParam))
				return false;
			id = int.Parse(msgParam);
			return true;
		}

		public void SetPostMouseOverEvents(string panelId, bool value)
		{
			this.Send((object)nameof(SetPostMouseOverEvents), (object)panelId, (object)value);
		}

		public void ParentToMainPanel(string panelId)
		{
			this.Send((object)nameof(ParentToMainPanel), (object)panelId);
		}

		public void SetDrawLayer(string panelId, int value)
		{
			this.Send((object)nameof(SetDrawLayer), (object)panelId, (object)value);
		}

		public void MovePanelToMouse(
		  string panelId,
		  UICommChannel.AnchorPoint anchorPoint,
		  Vector2 positionOffset)
		{
			this.Send((object)"MoveToMouse", (object)panelId, (object)anchorPoint.ToString(), (object)positionOffset.X, (object)positionOffset.Y);
		}

		public void SetVisible(string panelId, bool value)
		{
			this.Send((object)nameof(SetVisible), (object)panelId, (object)value);
		}

		public void SetEnabled(string panelId, bool value)
		{
			this.Send((object)nameof(SetEnabled), (object)panelId, (object)value);
		}

		public void SetShape(string panelId, int left, int top, int width, int height)
		{
			this.Send((object)nameof(SetShape), (object)panelId, (object)left, (object)top, (object)width, (object)height);
		}

		public void SetShapeToPanel(string panelId, string shapePanel)
		{
			this.Send((object)nameof(SetShapeToPanel), (object)panelId, (object)shapePanel);
		}

		public void SetParent(string panelId, string parent)
		{
			this.Send((object)nameof(SetParent), (object)panelId, (object)parent);
		}

		public void SetPosition(string panelId, int x, int y)
		{
			this.Send((object)nameof(SetPosition), (object)panelId, (object)x, (object)y);
		}

		public void ForceLayout(string panelId)
		{
			this.Send((object)nameof(ForceLayout), (object)panelId);
		}

		public void ShakeViolently(string panelId)
		{
			this.Send((object)"ForceLayout", (object)panelId);
			this.Send((object)"ForceLayout", (object)panelId);
		}

		public void SetTooltip(string panelId, string text)
		{
			this.Send((object)nameof(SetTooltip), (object)panelId, (object)text);
		}

		public void SetPropertyString(string panelId, string propertyName, string propertyValue)
		{
			this.Send((object)"SetPropString", (object)panelId, (object)propertyName, (object)propertyValue);
		}

		public void SetPropertyInt(string panelId, string propertyName, int propertyValue)
		{
			this.Send((object)"SetPropInt", (object)panelId, (object)propertyName, (object)propertyValue);
		}

		public void SetPropertyFloat(string panelId, string propertyName, float propertyValue)
		{
			this.Send((object)"SetPropFloat", (object)panelId, (object)propertyName, (object)propertyValue);
		}

		public void SetPropertyBool(string panelId, string propertyName, bool propertyValue)
		{
			this.Send((object)"SetPropBool", (object)panelId, (object)propertyName, (object)propertyValue);
		}

		public void SetPropertyColor(string panelId, string propertyName, float r, float g, float b)
		{
			this.Send((object)"SetPropColor3", (object)panelId, (object)propertyName, (object)(float)((double)r / (double)byte.MaxValue), (object)(float)((double)g / (double)byte.MaxValue), (object)(float)((double)b / (double)byte.MaxValue));
		}

		public void SetPropertyColorNormalized(
		  string panelId,
		  string propertyName,
		  float r,
		  float g,
		  float b)
		{
			this.Send((object)"SetPropColor3", (object)panelId, (object)propertyName, (object)r, (object)g, (object)b);
		}

		public void SetPropertyColor(
		  string panelId,
		  string propertyName,
		  float r,
		  float g,
		  float b,
		  float a)
		{
			this.Send((object)"SetPropColor4", (object)panelId, (object)propertyName, (object)(float)((double)r / (double)byte.MaxValue), (object)(float)((double)g / (double)byte.MaxValue), (object)(float)((double)b / (double)byte.MaxValue), (object)(float)((double)a / (double)byte.MaxValue));
		}

		public void SetPropertyColorNormalized(
		  string panelId,
		  string propertyName,
		  float r,
		  float g,
		  float b,
		  float a)
		{
			this.Send((object)"SetPropColor4", (object)panelId, (object)propertyName, (object)r, (object)g, (object)b, (object)a);
		}

		public void SetPropertyColor(string panelId, string propertyName, Vector3 value)
		{
			this.SetPropertyColor(panelId, propertyName, value.X, value.Y, value.Z);
		}

		public void SetPropertyColorNormalized(string panelId, string propertyName, Vector3 value)
		{
			this.SetPropertyColorNormalized(panelId, propertyName, value.X, value.Y, value.Z);
		}

		public void SetPropertyColorNormalized(string panelId, string propertyName, Vector4 value)
		{
			this.SetPropertyColorNormalized(panelId, propertyName, value.X, value.Y, value.Z, value.W);
		}

		public void SetPropertyColor(string panelId, string propertyName, Vector4 value)
		{
			this.SetPropertyColor(panelId, propertyName, value.X, value.Y, value.Z, value.W);
		}

		public void SetChecked(string panelId, bool isChecked)
		{
			this.SetPropertyBool(panelId, "checked", isChecked);
		}

		public void SetSliderValue(string panelId, int value)
		{
			this.SetPropertyInt(panelId, nameof(value), value);
		}

		public void SetSliderRange(string panelId, int min, int max)
		{
			this.SetPropertyInt(panelId, "value_min", min);
			this.SetPropertyInt(panelId, "value_max", max);
		}

		public void InitializeSlider(string panelId, int minValue, int maxValue, int value)
		{
			this.SetPropertyInt(panelId, "value_min", minValue);
			this.SetPropertyInt(panelId, "value_max", maxValue);
			this.SetSliderValue(panelId, value);
		}

		public void AddSliderNotch(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "add_notch", value);
		}

		public void RemoveSliderNotch(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "rem_notch", value);
		}

		public void SetSliderTolerance(string panelId, int value)
		{
			this.SetPropertyInt(panelId, "notch_tolerance", value);
		}

		public void SetSliderAutoSnap(string panelId, bool value)
		{
			this.SetPropertyBool(panelId, "auto_notch_snap", value);
		}

		public void ClearSliderNotches(string panelId)
		{
			this.SetPropertyBool(panelId, "clear_notchs", true);
		}

		public void AutoSize(string panelId)
		{
			this.Send((object)nameof(AutoSize), (object)panelId);
		}

		public void SetListCleanClear(string listPanelId, bool value)
		{
			this.Send((object)nameof(SetListCleanClear), (object)listPanelId, (object)value);
		}

		public void SetExpanded(string panelId, bool expanded)
		{
			this.Send(new List<object>()
	  {
		(object) nameof (SetExpanded),
		(object) panelId,
		(object) expanded
	  }.ToArray());
		}

		public void Reshape(string panelId)
		{
			this.Send(new List<object>()
	  {
		(object) nameof (Reshape),
		(object) panelId
	  }.ToArray());
		}

		public void AutoSizeContents(string panelId)
		{
			this.Send((object)nameof(AutoSizeContents), (object)panelId);
		}

		public void ClearItems(string listId)
		{
			this.Send((object)nameof(ClearItems), (object)listId);
		}

		public void ClearItemsTopLayer(string listId)
		{
			this.Send((object)"ClearItemsTop", (object)listId);
		}

		public void RemoveItems(string listId, int userItemId)
		{
			this.Send((object)"DelItem", (object)listId, (object)1, (object)userItemId);
		}

		public void RemoveItems(string listId, IEnumerable<int> userItemIds)
		{
			if (!userItemIds.Any<int>())
				return;
			List<object> objectList = new List<object>();
			objectList.Add((object)"DelItem");
			objectList.Add((object)listId);
			objectList.Add((object)userItemIds.Count<int>());
			objectList.AddRange(userItemIds.Cast<object>());
			this.SendElements((IEnumerable)objectList);
		}

		public void AddItem(string listId, string fieldId, int userItemId, string text = "")
		{
			this.Send((object)nameof(AddItem), (object)listId, (object)fieldId, (object)userItemId, (object)text, (object)"");
		}

		public void AddItem(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string text,
		  string panelTemplate)
		{
			this.Send((object)nameof(AddItem), (object)listId, (object)fieldId, (object)userItemId, (object)text, (object)panelTemplate);
		}

		public void AddSpacer(string listId)
		{
			this.Send((object)"AddItem", (object)listId, (object)"", (object)-1, (object)"", (object)"use_spacer_template");
		}

		public string GetItemGlobalID(string listId, string fieldId, int userItemId, string text = "")
		{
			++this._insertItemID;
			if (this._insertItemID < 0)
				this._insertItemID = 1;
			this.SetItemPropertyInt(listId, fieldId, userItemId, text, "globalid", this._insertItemID);
			return "&" + this._insertItemID.ToString();
		}

		public string GetGlobalID(string panelId)
		{
			++this._insertItemID;
			if (this._insertItemID < 0)
				this._insertItemID = 1;
			this.SetPropertyInt(panelId, "globalid", this._insertItemID);
			return "&" + this._insertItemID.ToString();
		}

		public void SetItemText(string listId, string fieldId, int userItemId, string text)
		{
			this.SetItemPropertyString(listId, fieldId, userItemId, string.Empty, nameof(text), text);
		}

		public void SetItemPropertyString(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  string value)
		{
			this.Send((object)"SetItemPropString", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)value);
		}

		public void SetItemPropertyInt(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  int value)
		{
			this.Send((object)"SetItemPropInt", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)value);
		}

		public void SetItemPropertyFloat(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  float value)
		{
			this.Send((object)"SetItemPropFloat", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)value);
		}

		public void SetItemPropertyBool(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  bool value)
		{
			this.Send((object)"SetItemPropBool", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)value);
		}

		public void SetItemPropertyColor(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  float x,
		  float y,
		  float z)
		{
			this.Send((object)"SetItemPropColor3", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)(float)((double)x / (double)byte.MaxValue), (object)(float)((double)y / (double)byte.MaxValue), (object)(float)((double)z / (double)byte.MaxValue));
		}

		public void SetItemPropertyColor(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  float x,
		  float y,
		  float z,
		  float w)
		{
			this.Send((object)"SetItemPropColor4", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)(float)((double)x / (double)byte.MaxValue), (object)(float)((double)y / (double)byte.MaxValue), (object)(float)((double)z / (double)byte.MaxValue), (object)(float)((double)w / (double)byte.MaxValue));
		}

		public void SetItemPropertyColor(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  Vector3 color)
		{
			this.Send((object)"SetItemPropColor3", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)(float)((double)color.X / (double)byte.MaxValue), (object)(float)((double)color.Y / (double)byte.MaxValue), (object)(float)((double)color.Z / (double)byte.MaxValue));
		}

		public void SetItemPropertyColor(
		  string listId,
		  string fieldId,
		  int userItemId,
		  string subPanelId,
		  string propertyName,
		  Vector4 color)
		{
			this.Send((object)"SetItemPropColor4", (object)listId, (object)fieldId, (object)userItemId, (object)subPanelId, (object)propertyName, (object)(float)((double)color.X / (double)byte.MaxValue), (object)(float)((double)color.Y / (double)byte.MaxValue), (object)(float)((double)color.Z / (double)byte.MaxValue), (object)(float)((double)color.W / (double)byte.MaxValue));
		}

		public void SetSelection(string listId, int userItemId)
		{
			this.Send((object)"SetSel", (object)listId, (object)1, (object)userItemId);
		}

		public void ClearSelection(string listId)
		{
			this.Send((object)"ClearSel", (object)listId);
		}

		public void SetSelection(string listId, IEnumerable<int> userItemIds)
		{
			if (userItemIds.Any<int>())
			{
				List<object> objectList = new List<object>();
				objectList.Add((object)"SetSel");
				objectList.Add((object)listId);
				objectList.Add((object)userItemIds.Count<int>());
				objectList.AddRange(userItemIds.Cast<object>());
				this.SendElements((IEnumerable)objectList);
			}
			else
				this.ClearSelection(listId);
		}

		public void ClearDisabledItems(string listId)
		{
			this.Send((object)"ClearDisabled", (object)listId);
		}

		public void SetDisabledItems(string listId, IEnumerable<int> userItemIds)
		{
			if (userItemIds.Any<int>())
			{
				List<object> objectList = new List<object>();
				objectList.Add((object)"SetDisabled");
				objectList.Add((object)listId);
				objectList.Add((object)userItemIds.Count<int>());
				objectList.AddRange(userItemIds.Cast<object>());
				this.SendElements((IEnumerable)objectList);
			}
			else
				this.ClearDisabledItems(listId);
		}

		public void SetButtonText(string panelId, string text)
		{
			this.Send((object)"SetText", (object)(panelId + ".idle.menulabel"), (object)text);
			this.Send((object)"SetText", (object)(panelId + ".mouse_over.menulabel"), (object)text);
			this.Send((object)"SetText", (object)(panelId + ".pressed.menulabel"), (object)text);
			this.Send((object)"SetText", (object)(panelId + ".disabled.menulabel"), (object)text);
		}

		public void SetText(string panelId, string text)
		{
			this.Send((object)nameof(SetText), (object)panelId, (object)text);
		}

		public void LocalizeText(string panelId, string text)
		{
			this.Send((object)nameof(LocalizeText), (object)panelId, (object)text);
		}

		public void SetTextFile(string panelId, string file)
		{
			this.SetPropertyString(panelId, "text_file", file);
		}

		public void SetScreen(string screenId)
		{
			this.Send((object)nameof(SetScreen), (object)screenId);
		}

		public void LoadScreen(string screenId)
		{
			this.Send((object)nameof(LoadScreen), (object)screenId);
		}

		public void DeleteScreen(string screenId)
		{
			this.Send((object)nameof(DeleteScreen), (object)screenId);
		}

		public void PurgeFleetWidgetCache()
		{
			this.Send((object)nameof(PurgeFleetWidgetCache));
		}

		public void LockUI()
		{
			this.Send((object)nameof(LockUI));
		}

		public void UnlockUI()
		{
			this.Send((object)nameof(UnlockUI));
		}

		public string CreatePanelFromTemplate(string templateId, string id = null)
		{
			if (id == null)
				id = Guid.NewGuid().ToString();
			this.Send((object)nameof(CreatePanelFromTemplate), (object)id, (object)templateId);
			return id;
		}

		public void DestroyPanel(string id)
		{
			this.Send((object)nameof(DestroyPanel), (object)id);
		}

		public void ShowTooltip(string id, float x, float y)
		{
			this.Send((object)nameof(ShowTooltip), (object)id, (object)x, (object)y);
		}

		public void HideTooltip()
		{
			this.Send((object)nameof(HideTooltip));
		}

		private void ProcessEngineMessage(string engMsg)
		{
			string[] subStrings = engMsg.Split(',');
			if (subStrings.Length > 0 && (subStrings[0] == "GameEvent" && this.ProcessGameEventMessage(subStrings) || subStrings[0] == "Panel" && this.ProcessPanelMessage(subStrings)))
				return;
			this.Msg("UICC ProcessEngineMessages {" + engMsg + "} what is this?");
		}

		public void Send(params object[] elements)
		{
			this.SendElements((IEnumerable)elements);
		}

		public void SendElements(IEnumerable elements)
		{
			this._scriptMessageWriter.Clear();
			this._scriptMessageWriter.Write(elements);
			this._messageQueue.PutMessage(this._scriptMessageWriter.GetBuffer(), (int)this._scriptMessageWriter.GetSize());
		}

		private string[] SeparateParams(string[] subStrings, int firstParamIndex)
		{
			string[] strArray = new string[subStrings.Length - firstParamIndex];
			for (int index = firstParamIndex; index < subStrings.Length; ++index)
				strArray[index - firstParamIndex] = subStrings[index];
			return strArray;
		}

		private bool ProcessGameEventMessage(string[] subStrings)
		{
			string subString = subStrings[1];
			string[] eventParams = this.SeparateParams(subStrings, 2);
			if (this.GameEvent != null)
				this.GameEvent(subString, eventParams);
			return true;
		}

		private bool ProcessPanelMessage(string[] subStrings)
		{
			if (subStrings.Length < 3)
				return false;
			string subString1 = subStrings[1];
			string subString2 = subStrings[2];
			string[] msgParams = this.SeparateParams(subStrings, 3);
			if (this.PanelMessage != null)
				this.PanelMessage(subString1, subString2, msgParams);
			return true;
		}

		private void Msg(string message)
		{
			if (!UICommChannel.LogEnable)
				return;
			App.Log.Trace(message, "gui");
		}

		public enum AnchorPoint
		{
			TopLeft,
		}
	}
}
