using Godot;
using SilentTestimony.Systems;
using System;

namespace SilentTestimony.UI
{
    /// <summary>
    /// Autoload: 管理 InventoryUI 的实例与显示逻辑
    /// </summary>
    public partial class InventoryUIManager : Node
    {
        private const string ScenePath = "res://Scenes/UI/InventoryUI.tscn";
        private CanvasLayer _layer;
        private InventoryUI _ui;
        private InventoryManager _inventory;

        public override void _Ready()
        {
            _layer = new CanvasLayer();
            AddChild(_layer);

            var packed = GD.Load<PackedScene>(ScenePath);
            if (packed != null)
            {
                _ui = packed.Instantiate<InventoryUI>();
                _layer.AddChild(_ui);
            }
            else
            {
                GD.PushError($"InventoryUIManager: 无法加载 {ScenePath}");
            }

            _inventory = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
            if (_inventory != null)
            {
                _inventory.InventoryChanged += OnInventoryChanged;
            }
        }

        public override void _ExitTree()
        {
            if (_inventory != null)
            {
                _inventory.InventoryChanged -= OnInventoryChanged;
            }
        }

        private void OnInventoryChanged()
        {
            if (_ui != null && _ui.Visible && _inventory != null)
            {
                _ui.UpdateItems(_inventory.GetAllItems());
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("inventory"))
            {
                Toggle();
                GetViewport()?.SetInputAsHandled();
            }
        }

        private void Toggle()
        {
            if (_ui == null || _inventory == null) return;
            if (_ui.Visible)
            {
                _ui.Visible = false;
                GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard")?.Release();
            }
            else
            {
                _ui.ShowWithItems(_inventory.GetAllItems());
                GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard")?.Acquire();
            }
        }
    }
}
