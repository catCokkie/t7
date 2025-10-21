// Scripts/Core/PlayerStats.cs
using Godot;
using System;

// 确保命名空间与你在 Phase 0 中设置的一致
namespace SilentTestimony.Core
{
	/// <summary>
	/// 全局玩家状态管理器 (Autoload Singleton)。
	/// 负责跟踪玩家的健康、理智等核心资源。
	/// </summary>
	public partial class PlayerStats : Node
	{
		// --- 信号定义 ---

		/// <summary>
		/// 当玩家健康值改变时发出。
		/// </summary>
		/// <param name="newHealth">新的健康值</param>
		/// <param name="changeAmount">变化的量 (例如 -10 或 +5)</param>
		[Signal] public delegate void HealthChangedEventHandler(int newHealth, int changeAmount);

		/// <summary>
		/// 当玩家理智值改变时发出。
		/// </summary>
		/// <param name="newSanity">新的理智值</param>
		/// <param name="changeAmount">变化的量</param>
		[Signal] public delegate void SanityChangedEventHandler(int newSanity, int changeAmount);

		/// <summary>
		/// 当玩家健康值降至0或以下时发出。
		/// </summary>
		[Signal] public delegate void PlayerDiedEventHandler();

		// --- 导出属性 (可在Godot编辑器中调整) ---

		[Export] private int _maxHealth = 100;
		[Export] private int _startHealth = 100;
		[Export] private int _maxSanity = 100;
		[Export] private int _startSanity = 100;

		// --- 公共属性 (只读) ---
		public int Health { get; private set; }
		public int Sanity { get; private set; }
		public int MaxHealth => _maxHealth;
		public int MaxSanity => _maxSanity;

		private bool _isDead = false;

		/// <summary>
		/// 初始化
		/// </summary>
		public override void _Ready()
		{
			Health = _startHealth;
			Sanity = _startSanity;
		}

		// --- 公共方法 ---

		/// <summary>
		/// 改变玩家的健康值。
		/// </summary>
		/// <param name="amount">要改变的量 (负数为伤害，正数为治疗)</param>
		public void ChangeHealth(int amount)
		{
			if (_isDead) return; // 死亡后不再改变

			// 计算新值并限制在 [0, MaxHealth] 范围内
			int newHealth = Mathf.Clamp(Health + amount, 0, _maxHealth);

			if (newHealth == Health) return; // 值没有变化

			Health = newHealth;
			EmitSignal(SignalName.HealthChanged, Health, amount);
			// GD.Print($"Health changed: {Health}"); // 调试用

			// 检查死亡
			if (Health <= 0)
			{
				_isDead = true;
				EmitSignal(SignalName.PlayerDied);
				// GD.Print("Player has died."); // 调试用
			}
		}

		/// <summary>
		/// 改变玩家的理智值。
		/// </summary>
		/// <param name="amount">要改变的量 (负数为降低，正数为恢复)</param>
		public void ChangeSanity(int amount)
		{
			if (_isDead) return;

			// 计算新值并限制在 [0, MaxSanity] 范围内
			int newSanity = Mathf.Clamp(Sanity + amount, 0, _maxSanity);

			if (newSanity == Sanity) return; // 值没有变化

			Sanity = newSanity;
			EmitSignal(SignalName.SanityChanged, Sanity, amount);
			// GD.Print($"Sanity changed: {Sanity}"); // 调试用
		}
	}
}
