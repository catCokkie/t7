using Godot;
using System.Collections.Generic;
using SilentTestimony.Data;

namespace SilentTestimony.Systems
{
        /// <summary>
        /// 负责管理玩家背包中的所有物品。
        /// 作为 Autoload 节点存在于场景树根部。
        /// </summary>
        public partial class InventoryManager : Node
        {
                [Signal] public delegate void InventoryChangedEventHandler();

                private readonly Dictionary<string, InventoryItemData> _itemsById = new();
                private readonly List<InventoryItemData> _items = new();

                /// <summary>
                /// 添加一个物品。如果已经存在相同 ID，则忽略。
                /// </summary>
                /// <param name="item">要加入背包的物品数据。</param>
                /// <returns>是否成功加入背包。</returns>
                public bool AddItem(InventoryItemData item)
                {
                        if (item == null)
                        {
                                GD.PushWarning("InventoryManager: 尝试添加的物品为空。");
                                return false;
                        }

                        if (string.IsNullOrEmpty(item.ItemID))
                        {
                                GD.PushWarning($"InventoryManager: 物品 {item.Name} 缺少有效的 ItemID。");
                                return false;
                        }

                        if (_itemsById.ContainsKey(item.ItemID))
                        {
                                GD.Print($"InventoryManager: 物品 {item.ItemID} 已存在，跳过添加。");
                                return false;
                        }

                        _items.Add(item);
                        _itemsById[item.ItemID] = item;

                        EmitSignal(SignalName.InventoryChanged);
                        return true;
                }

                /// <summary>
                /// 判断背包中是否包含指定物品。
                /// </summary>
                /// <param name="itemId">物品唯一 ID。</param>
                public bool HasItem(string itemId)
                {
                        if (string.IsNullOrEmpty(itemId))
                        {
                                return false;
                        }

                        return _itemsById.ContainsKey(itemId);
                }

                /// <summary>
                /// 获取当前所有物品的只读列表，可用于 UI 展示。
                /// </summary>
                public IReadOnlyList<InventoryItemData> GetAllItems()
                {
                        return _items.AsReadOnly();
                }
        }
}
