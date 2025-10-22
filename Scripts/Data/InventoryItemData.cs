using Godot;

namespace SilentTestimony.Data
{
        /// <summary>
        /// 基础物品数据，用于描述背包中的一个条目。
        /// </summary>
        [GlobalClass]
        public partial class InventoryItemData : Resource
        {
                [Export] public string ItemID { get; set; } = string.Empty;
                [Export] public string Name { get; set; } = string.Empty;
                [Export(PropertyHint.MultilineText)] public string Description { get; set; } = string.Empty;
                [Export] public Texture2D Icon { get; set; }
                [Export] public bool IsKeyItem { get; set; }
        }
}
