using Godot;
using SilentTestimony.Interfaces;
using SilentTestimony.Systems;

namespace SilentTestimony.World
{
	/// <summary>
	/// 需要钥匙才能开启的门。实现 IInteractable 接口
	/// </summary>
	public partial class LockedDoor : StaticBody2D, IInteractable
	{
		[Export] public string RequiredKeyItemID { get; set; } = string.Empty;
		[Export] public NodePath AnimationPlayerPath { get; set; } = default;

		private bool _isOpened;

		public string GetInteractPrompt()
		{
			if (_isOpened)
				return "通过";

			return string.IsNullOrEmpty(RequiredKeyItemID)
				? "打开"
				: "使用钥匙";
		}

		public void Interact(Node2D interactor)
		{
			if (_isOpened)
				return;

			var inventory = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
			if (inventory == null)
			{
				GD.PushWarning("LockedDoor: 未找到 InventoryManager，全局背包系统可能未初始化");
				return;
			}

			if (string.IsNullOrEmpty(RequiredKeyItemID) || inventory.HasItem(RequiredKeyItemID))
			{
				OpenDoor();
			}
			else
			{
				GD.Print($"{Name}: 门被锁住了，需要物品 {RequiredKeyItemID}");
			}
		}

		private void OpenDoor()
		{
			_isOpened = true;
			GD.Print($"{Name}: 门已打开");

			AnimationPlayer animationPlayer = null;
			if (!AnimationPlayerPath.IsEmpty)
			{
				animationPlayer = GetNodeOrNull<AnimationPlayer>(AnimationPlayerPath);
			}
			else
			{
				animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
			}

			animationPlayer?.Play("Open");

			// 简单处理：禁用碰撞并隐藏精灵
			CollisionLayer = 0;
			CollisionMask = 0;
			var sprite = GetNodeOrNull<Node2D>("Sprite2D");
			sprite?.SetDeferred("visible", false);
		}
	}
}
