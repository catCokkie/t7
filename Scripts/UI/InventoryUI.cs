using Godot;
using SilentTestimony.Data;
using System.Collections.Generic;

namespace SilentTestimony.UI
{
    public partial class InventoryUI : Control
    {
        private ItemList _list;
        private Button _close;

        public override void _Ready()
        {
            _list = GetNodeOrNull<ItemList>("CenterContainer/Panel/VBox/Items");
            _close = GetNodeOrNull<Button>("CenterContainer/Panel/VBox/CloseButton");
            if (_close != null) _close.Pressed += () => Visible = false;
            Visible = false;
        }

        public void ShowWithItems(IReadOnlyList<InventoryItemData> items)
        {
            UpdateItems(items);
            Visible = true;
        }

        public void UpdateItems(IReadOnlyList<InventoryItemData> items)
        {
            if (_list == null) return;
            _list.Clear();
            if (items == null) return;
            foreach (var it in items)
            {
                string name = it?.ItemID ?? "(null)";
                if (it != null)
                {
                    if (!string.IsNullOrEmpty(it.NameKey))
                    {
                        var t = TranslationServer.Translate(it.NameKey);
                        if (!string.IsNullOrEmpty(t)) name = t;
                        else if (!string.IsNullOrEmpty(it.Name)) name = it.Name;
                    }
                    else if (!string.IsNullOrEmpty(it.Name))
                    {
                        name = it.Name;
                    }
                }
                _list.AddItem(name);
            }
        }
    }
}
