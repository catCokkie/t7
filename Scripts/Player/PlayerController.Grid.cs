using Godot;

namespace SilentTestimony.Player
{
    public partial class PlayerController
    {
        [ExportGroup("Grid Movement")]
        [Export] private bool _useGridMovement = false;
        [Export(PropertyHint.Range, "4,128,1")] private int _cellSize = 16;
        [Export(PropertyHint.Range, "1,30,0.5")] private float _tilesPerSecond = 6f;

        private bool _isGridMoving;
        private Vector2 _gridTargetWorld;
        private Tween _gridTween;

        private void HandleGridMovement(double delta)
        {
            if (!_useGridMovement)
                return;

            // 若正在移动，等待 Tween 完成
            if (_isGridMoving)
                return;

            // 读取输入并转换为 4 方向
            Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (input == Vector2.Zero)
            {
                Velocity = Vector2.Zero;
                return;
            }

            Vector2 dir;
            if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
                dir = new Vector2(Mathf.Sign(input.X), 0);
            else
                dir = new Vector2(0, Mathf.Sign(input.Y));

            // 依据按键设置当前状态（影响脚步声与噪音节奏）
            if (Input.IsActionPressed("sneak"))
                CurrentState = PlayerState.Sneaking;
            else if (Input.IsActionPressed("run"))
                CurrentState = PlayerState.Running;
            else
                CurrentState = PlayerState.Walking;

            Vector2 motion = dir * _cellSize;

            // 物理探测：目标格若被阻挡则不移动
            // 使用 PhysicsBody2D.TestMove 检查从当前变换到目标位移是否会撞到碰撞体
            if (TestMove(GlobalTransform, motion))
            {
                Velocity = Vector2.Zero;
                return;
            }

            // 启动到目标格的补间
            _isGridMoving = true;
            _gridTargetWorld = GlobalPosition + motion;

            // 让 Velocity 在移动期间为非零，维持噪音/脚步逻辑（不参与 MoveAndSlide）
            float speed = _walkSpeed;
            if (CurrentState == PlayerState.Sneaking) speed = _sneakSpeed;
            else if (CurrentState == PlayerState.Running) speed = _runSpeed;
            Velocity = dir * speed;

            // 每格用固定时长（tiles/second）
            float duration = 1.0f / Mathf.Max(0.01f, _tilesPerSecond);

            _gridTween?.Kill();
            _gridTween = CreateTween();
            _gridTween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            _gridTween.TweenProperty(this, "global_position", _gridTargetWorld, duration);
            _gridTween.Finished += () =>
            {
                GlobalPosition = _gridTargetWorld;
                Velocity = Vector2.Zero;
                _isGridMoving = false;
            };
        }
    }
}

