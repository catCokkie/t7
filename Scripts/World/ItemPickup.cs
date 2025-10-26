using Godot;
using SilentTestimony.Interfaces;
using SilentTestimony.Systems;
using SilentTestimony.Data;

namespace SilentTestimony.World
{
	/// <summary>
	/// 可交互的物品拾取点，拾取后加入 Inventory
	/// </summary>
        public partial class ItemPickup : StaticBody2D, IInteractable
        {
                [ExportGroup("Tile Placement")]
                [Export] public bool UseTileCoordinates { get; set; } = false;
                [Export] public Vector2I TileCoords { get; set; } = Vector2I.Zero;
                [Export] public Vector2 TileOffset { get; set; } = Vector2.Zero;
                [Export(PropertyHint.Range, "4,256,1")] public float TileCellSize { get; set; } = 16f;

                [Export] public InventoryItemData Item;
                [Export] public bool DestroyOnPickup = true;

                public override void _Ready()
                {
                        base._Ready();
                        ApplyTilePlacement();
                }

                private void ApplyTilePlacement()
                {
                        if (!UseTileCoordinates)
                                return;

                        GlobalPosition = RuntimeTilemapBuilderLayers.TileToWorldPosition(TileCoords, TileCellSize, TileOffset);
                }

		public string GetInteractPrompt()
		{
			var name = Item != null && !string.IsNullOrEmpty(Item.Name) ? Item.Name : "物品";
			return $"拾取：{name}";
		}

		public void Interact(Node2D interactor)
		{
			var inventory = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
			if (inventory == null)
			{
				GD.PushWarning("ItemPickup: 未找到 InventoryManager，无法添加物品。");
				return;
			}

			if (Item == null)
			{
				GD.PushWarning("ItemPickup: 未配置 InventoryItemData。");
				return;
			}

			if (inventory.AddItem(Item))
			{
				GD.Print($"拾取物品: {Item.ItemID} - {Item.Name}");
				if (DestroyOnPickup)
				{
					QueueFree();
        }
}
		}
	}
}
