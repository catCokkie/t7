using Godot;
using SilentTestimony.Data;
using System.Collections.Generic;

namespace SilentTestimony.UI
{
    public partial class EvidenceBoard : Control
    {
        private ItemList _list;
        private Label _title;
        private RichTextLabel _content;
        private Button _close;
        private IReadOnlyList<EvidenceData> _data = new List<EvidenceData>();

        public override void _Ready()
        {
            _list = GetNodeOrNull<ItemList>("CenterContainer/Panel/HBox/List");
            _title = GetNodeOrNull<Label>("CenterContainer/Panel/HBox/Detail/Title");
            _content = GetNodeOrNull<RichTextLabel>("CenterContainer/Panel/HBox/Detail/Content");
            _close = GetNodeOrNull<Button>("CenterContainer/Panel/Bottom/CloseButton");
            if (_close != null)
            {
                _close.Pressed += () => Visible = false;
                var t = TranslationServer.Translate("ui.close");
                if (!string.IsNullOrEmpty(t)) _close.Text = t;
            }
            if (_list != null) _list.ItemSelected += OnItemSelected;
            Visible = false;
        }

        public void ShowWith(IReadOnlyList<EvidenceData> evidences)
        {
            _data = evidences ?? new List<EvidenceData>();
            RefreshList();
            Visible = true;
        }

        public void UpdateList(IReadOnlyList<EvidenceData> evidences)
        {
            _data = evidences ?? new List<EvidenceData>();
            RefreshList();
        }

        private void RefreshList()
        {
            _list?.Clear();
            if (_data == null || _list == null) return;
            for (int i = 0; i < _data.Count; i++)
            {
                var d = _data[i];
                string title = d?.EvidenceID ?? "(null)";
                if (d != null)
                {
                    if (!string.IsNullOrEmpty(d.TitleKey))
                    {
                        var t = TranslationServer.Translate(d.TitleKey);
                        if (!string.IsNullOrEmpty(t)) title = t;
                        else if (!string.IsNullOrEmpty(d.Title)) title = d.Title;
                    }
                    else if (!string.IsNullOrEmpty(d.Title))
                    {
                        title = d.Title;
                    }
                }
                _list.AddItem(title);
            }
            if (_data.Count > 0)
            {
                _list.Select(0);
                ShowDetail(0);
            }
            else
            {
                if (_title != null) _title.Text = "";
                if (_content != null) _content.Text = "";
            }
        }

        private void OnItemSelected(long index)
        {
            ShowDetail((int)index);
        }

        private void ShowDetail(int index)
        {
            if (_data == null || index < 0 || index >= _data.Count) return;
            var d = _data[index];
            if (_title != null)
            {
                var title = d?.Title ?? string.Empty;
                if (!string.IsNullOrEmpty(d?.TitleKey))
                {
                    var t = TranslationServer.Translate(d.TitleKey);
                    if (!string.IsNullOrEmpty(t)) title = t;
                }
                _title.Text = title;
            }
            if (_content != null)
            {
                var content = d?.Content ?? string.Empty;
                if (!string.IsNullOrEmpty(d?.ContentKey))
                {
                    var t = TranslationServer.Translate(d.ContentKey);
                    if (!string.IsNullOrEmpty(t)) content = t;
                }
                _content.Text = content;
                _content.ScrollToLine(0);
            }
        }
    }
}
