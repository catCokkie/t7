// Scripts/Player/PlayerController.cs
using Godot;
using System;
using SilentTestimony.Global; // 引用 EventBus 的命名空间
using SilentTestimony.Systems;

namespace SilentTestimony.Player
{
    /// <summary>
    /// 玩家控制器
    /// 负责处理玩家输入、移动物理、状态切换(潜行/行走/奔跑)
    /// 以及根据状态发出“噪音”信号
    /// </summary>
    public partial class PlayerController : CharacterBody2D
    {
        partial void InitializeInteraction();
        partial void UpdateInteraction(double delta);

        // --- 玩家状态 ---
        public enum PlayerState { Sneaking, Walking, Running }
        public PlayerState CurrentState { get; private set; } = PlayerState.Walking;

        // --- 导出属性(可在Godot编辑器中调整) ---
        [ExportGroup("Movement")]
        [Export] private float _sneakSpeed = 70.0f;
        [Export] private float _walkSpeed = 130.0f;
        [Export] private float _runSpeed = 220.0f;

		[ExportGroup("Noise Properties")]
		[Export(PropertyHint.Range, "0,500,1")] private float _walkNoiseRadius = 100.0f;
		[Export(PropertyHint.Range, "0,500,1")] private float _runNoiseRadius = 250.0f;
		[Export] private double _walkNoiseInterval = 0.6; // 行走时，0.6 秒发一次噪音
		[Export] private double _runNoiseInterval = 0.25; // 奔跑时，0.25 秒发一次噪音

		[ExportGroup("Audio")]
		[Export] private AudioStream _walkFootstep;
		[Export] private AudioStream _runFootstep;

        // --- 节点引用 ---
		private GlobalEventBus _eventBus;
		private Timer _noiseTimer;
		private AudioManager _audio;

        public override void _Ready()
        {
            // 获取全局单例
            _eventBus = GetNode<GlobalEventBus>("/root/GlobalEventBus");

			// 获取子节点
			_noiseTimer = GetNode<Timer>("NoiseTimer");
			_audio = GetNodeOrNull<AudioManager>("/root/AudioManager");

            // 初始化交互检测器与交互提示
            InitializeInteractor();
            InitializeInteraction();
        }

        public override void _PhysicsProcess(double delta)
        {
            var guard = GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard");
            if (guard != null && guard.Blocked)
            {
                Velocity = Vector2.Zero;
                return;
            }
            // 1. 处理输入并更新速度
            HandleInputAndState();

            // 2. 执行物理移动
            MoveAndSlide();

            // 3. 管理噪音计时器
            UpdateNoiseTimer();

            // 4. 刷新交互（提示/目标）
            UpdateInteraction(delta);
        }

        /// <summary>
        /// 1. 检查输入
        /// 2. 设置当前状态(Sneaking/Walking/Running)
        /// 3. 设置 Velocity 属性
        /// </summary>
        private void HandleInputAndState()
        {
            // 获取归一化的输入向量 (Godot 4 的便捷方法)
            Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

            float targetSpeed;
            PlayerState newState;

            // 根据按键设置状态和速度
            if (Input.IsActionPressed("sneak"))
            {
                newState = PlayerState.Sneaking;
                targetSpeed = _sneakSpeed;
            }
            else if (Input.IsActionPressed("run"))
            {
                newState = PlayerState.Running;
                targetSpeed = _runSpeed;
            }
            else
            {
                newState = PlayerState.Walking;
                targetSpeed = _walkSpeed;
            }

            CurrentState = newState;
            Velocity = inputDirection * targetSpeed;
        }

        /// <summary>
        /// 根据当前状态和是否在移动，更新噪音计时器
        /// </summary>
        private void UpdateNoiseTimer()
        {
            // 如果在潜行，或者没有移动，则停止计时器并返回
            if (CurrentState == PlayerState.Sneaking || Velocity.LengthSquared() == 0)
            {
                _noiseTimer.Stop();
                return;
            }

            // 确定当前状态的噪音间隔
            double interval = (CurrentState == PlayerState.Running) ? _runNoiseInterval : _walkNoiseInterval;

            // 如果计时器已停止（说明刚开始移动），则设置新间隔并启动
            if (_noiseTimer.IsStopped())
            {
                _noiseTimer.WaitTime = interval;
                _noiseTimer.Start();
            }
            else
            {
                // 如果计时器正在运行，但状态切换了（例如从走到跑），也更新其间隔
                _noiseTimer.WaitTime = interval;
            }
        }

        /// <summary>
        /// 当 NoiseTimer 倒计时结束时调用（由场景信号连接）
        /// </summary>
		private void OnNoiseTimerTimeout()
		{
            float noiseRadius = 0;

            if (CurrentState == PlayerState.Running)
            {
                noiseRadius = _runNoiseRadius;
            }
            else if (CurrentState == PlayerState.Walking)
            {
                noiseRadius = _walkNoiseRadius;
            }
            // (注意：潜行状态永远不会触发这个计时器)

			// 向全局事件总线发出“噪音”信号
			_eventBus.EmitSignal(
				GlobalEventBus.SignalName.NoiseMade,
				this.GlobalPosition, // 噪音发出的位置
				noiseRadius,          // 噪音的半径
				this                  // 噪音的制造者(玩家自己)
			);

			// 播放脚步声（基于状态）
			if (_audio != null)
			{
				AudioStream sfx = CurrentState == PlayerState.Running ? _runFootstep : _walkFootstep;
				if (sfx != null)
				{
					// 轻微音调随机，避免机械感
					float pitch = 1.0f + (float)GD.RandRange(-0.06, 0.06);
					_audio.PlaySFX(sfx, GlobalPosition, -6.0f, pitch);
				}
			}
		}
    }
}
