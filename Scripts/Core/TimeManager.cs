// Scripts/Core/TimeManager.cs
using Godot;
using System;
// 确保命名空间与你在 Phase 0 中设置的一致
namespace SilentTestimony.Core
{
	/// <summary>
	/// 全局时间管理器 (Autoload Singleton)。
	/// 负责驱动游戏世界的时间流逝，并广播小时和天的变化。
	/// </summary>
	public partial class TimeManager : Node
	{
		// --- 信号定义 ---

		/// <summary>
		/// 当游戏内的小时发生变化时发出。
		/// AI日程表、光照系统等都将监听此信号。
		/// </summary>
		/// <param name="newHour">新的小时 (0-23)</param>
		[Signal] public delegate void HourChangedEventHandler(int newHour);

		/// <summary>
		/// 当新的一天开始时（午夜）发出。
		/// </summary>
		/// <param name="newDay">新一天的天数 (Day 1, Day 2...)</param>
		[Signal] public delegate void DayChangedEventHandler(int newDay);

		// --- 导出属性 (可在Godot编辑器中调整) ---

		/// <summary>
		/// 现实世界中的多少“秒”等于游戏中的一“分钟”。
		/// 示例：
		/// 1.0 = 1秒/分钟 (60倍速, 游戏一天24分钟)
		/// 0.5 = 0.5秒/分钟 (120倍速, 游戏一天12分钟) (推荐用于快速调试)
		/// 2.0 = 2秒/分钟 (30倍速, 游戏一天48分钟) (推荐用于正常游戏)
		/// </summary>
		[Export] private double _secondsPerGameMinute = 1.0;

		/// <summary>
		/// 游戏开始时的天数。
		/// </summary>
		[Export] private int _startDay = 1;

		/// <summary>
		/// 游戏开始时的小时 (例如 6.0 = 早上6:00)。
		/// </summary>
		[Export(PropertyHint.Range, "0,23.99,0.01")]
		private float _startTimeInHours = 6.0f;

		// --- 公共属性 (只读) ---
		public int CurrentDay { get; private set; }
		public float CurrentTimeInHours { get; private set; }

		// --- 私有变量 ---
		private int _lastHourFired;

		/// <summary>
		/// 初始化
		/// </summary>
		public override void _Ready()
		{
			CurrentDay = _startDay;
			CurrentTimeInHours = _startTimeInHours;
			_lastHourFired = (int)Mathf.Floor(CurrentTimeInHours);
		}

		/// <summary>
		/// 每一帧调用，用于推进时间
		/// </summary>
		public override void _Process(double delta)
		{
			// 1. 计算这一帧过去了多少游戏小时
			double minutesPassed = delta / _secondsPerGameMinute;
			double hoursPassed = minutesPassed / 60.0;

			float newTime = CurrentTimeInHours + (float)hoursPassed;

			// 2. 检查小时是否变更
			int newHour = (int)Mathf.Floor(newTime);
			
			if (newHour != _lastHourFired)
			{
				// 2a. 检查是否跨过午夜 (24:00 -> 00:00)
				if (newHour >= 24)
				{
					newHour = 0; // 午夜是 0 点
					
					// --- 触发“天”变更 ---
					CurrentDay++;
					EmitSignal(SignalName.DayChanged, CurrentDay);
					// GD.Print($"--- DAY {CurrentDay} ---"); // 调试用
				}

				_lastHourFired = newHour;
				
				// --- 触发“小时”变更 ---
				EmitSignal(SignalName.HourChanged, newHour);
				// GD.Print($"New Hour: {newHour}:00"); // 调试用
			}

			// 3. 更新当前时间，并处理24小时循环
			if (newTime >= 24.0f)
			{
				CurrentTimeInHours = newTime - 24.0f;
			}
			else
			{
				CurrentTimeInHours = newTime;
			}
		}

		// --- 公共辅助方法 ---

		/// <summary>
		/// 获取格式化的时间字符串 (例如 "08:30")。
		/// </summary>
		public string GetTimeAsString()
		{
			int hour = (int)Mathf.Floor(CurrentTimeInHours);
			int minute = (int)Mathf.Floor((CurrentTimeInHours * 60) % 60);
			return $"{hour:D2}:{minute:D2}";
		}

		/// <summary>
		/// 获取当前天数的字符串 (例如 "Day 1")。
		/// </summary>
		public string GetDayAsString()
		{
			return $"Day {CurrentDay}";
		}

		/// <summary>
		/// 设置游戏时间（用于读档或跳时）。可选择是否发出更改信号。
		/// </summary>
		/// <param name="day">>= 1</param>
		/// <param name="hour">0.0 - 23.99</param>
		/// <param name="emitSignals">是否广播 DayChanged/HourChanged</param>
		public void SetClock(int day, float hour, bool emitSignals = true)
		{
			if (day < 1) day = 1;
			// 规范化小时到 0..24 范围
			if (hour < 0f) hour = 0f;
			if (hour >= 24f) hour = hour % 24f;

			int oldDay = CurrentDay;
			int oldHour = (int)Mathf.Floor(CurrentTimeInHours);

			CurrentDay = day;
			CurrentTimeInHours = hour;
			int newHour = (int)Mathf.Floor(CurrentTimeInHours);
			_lastHourFired = newHour; // 避免下一帧重复触发

			if (emitSignals)
			{
				if (CurrentDay != oldDay)
				{
					EmitSignal(SignalName.DayChanged, CurrentDay);
				}
				if (newHour != oldHour)
				{
					EmitSignal(SignalName.HourChanged, newHour);
				}
			}
		}
	}
}
