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
                var name = string.IsNullOrEmpty(it?.Name) ? it?.ItemID ?? "(null)" : it.Name;
                _list.AddItem(name);
            }
        }
    }
}

