using Godot;
using System.Collections.Generic;
using SilentTestimony.Global;
using SilentTestimony.Player;

namespace SilentTestimony.AI
{
    /// <summary>
    /// 基础敌人 AI 控制器
    /// 支持简单巡逻、噪音响应与视野追逐逻辑。
    /// </summary>
    public partial class EnemyAIController : CharacterBody2D
    {
        private enum EnemyState
        {
            Patrolling,
            Alerted,
            Chasing
        }

        [ExportGroup("Movement")]
        [Export(PropertyHint.Range, "16,480,1")] private float _patrolSpeed = 80.0f;
        [Export(PropertyHint.Range, "16,600,1")] private float _chaseSpeed = 160.0f;

        [ExportGroup("Perception")]
        [Export(PropertyHint.Range, "0,180,1")] private float _visionLoseAngle = 75.0f;
        [Export(PropertyHint.Range, "0,10,0.1")] private float _alertDuration = 3.0f;
        [Export(PropertyHint.NodePath)] private NodePath _patrolContainerPath;

        private EnemyState _currentState = EnemyState.Patrolling;
        private readonly List<Node2D> _patrolPoints = new();
        private int _currentPatrolIndex = 0;
        private double _alertTimer = 0.0f;

        private PlayerController _player;
        private NavigationAgent2D _navigationAgent;
        private Area2D _visionArea;
        private RayCast2D _lineOfSight;
        private GlobalEventBus _eventBus;

        public override void _Ready()
        {
            _navigationAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
            _visionArea = GetNodeOrNull<Area2D>("VisionArea");
            _lineOfSight = GetNodeOrNull<RayCast2D>("LineOfSight");
            _eventBus = GetNodeOrNull<GlobalEventBus>("/root/GlobalEventBus");

            if (_visionArea != null)
            {
                _visionArea.BodyEntered += OnVisionBodyEntered;
                _visionArea.BodyExited += OnVisionBodyExited;
            }

            if (_eventBus != null)
            {
                _eventBus.NoiseMade += OnNoiseMade;
            }

            // 收集巡逻点
            if (_patrolContainerPath != null && !_patrolContainerPath.IsEmpty)
            {
                if (GetNodeOrNull<Node>(_patrolContainerPath) is Node container)
                {
                    foreach (Node child in container.GetChildren())
                    {
                        if (child is Node2D node2D)
                        {
                            _patrolPoints.Add(node2D);
                        }
                    }
                }
            }

            if (_patrolPoints.Count == 0)
            {
                GD.PrintErr($"[EnemyAI] {Name} 没有巡逻点，将停留在原地。");
            }
            else
            {
                SetNextPatrolTarget();
            }
        }

        public override void _ExitTree()
        {
            if (_visionArea != null)
            {
                _visionArea.BodyEntered -= OnVisionBodyEntered;
                _visionArea.BodyExited -= OnVisionBodyExited;
            }

            if (_eventBus != null)
            {
                _eventBus.NoiseMade -= OnNoiseMade;
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            switch (_currentState)
            {
                case EnemyState.Patrolling:
                    ProcessPatrol();
                    break;
                case EnemyState.Alerted:
                    ProcessAlerted(delta);
                    break;
                case EnemyState.Chasing:
                    ProcessChasing();
                    break;
            }
        }

        private void ProcessPatrol()
        {
            if (_navigationAgent == null || _patrolPoints.Count == 0)
            {
                Velocity = Vector2.Zero;
                return;
            }

            if (_navigationAgent.IsNavigationFinished())
            {
                SetNextPatrolTarget();
            }

            MoveAlongNavigation(_patrolSpeed);
        }

        private void ProcessAlerted(double delta)
        {
            _alertTimer -= delta;
            if (_alertTimer <= 0.0)
            {
                ChangeState(EnemyState.Patrolling);
                return;
            }

            if (_navigationAgent != null && !_navigationAgent.IsNavigationFinished())
            {
                MoveAlongNavigation(_patrolSpeed);
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }

        private void ProcessChasing()
        {
            if (_player == null)
            {
                ChangeState(EnemyState.Alerted);
                return;
            }

            if (!HasLineOfSightTo(_player))
            {
                if (_navigationAgent != null)
                {
                    _navigationAgent.TargetPosition = _player.GlobalPosition;
                }
                // 失去视线，进入警戒状态并在当前位置搜索
                ChangeState(EnemyState.Alerted);
                return;
            }

            if (_navigationAgent != null)
            {
                _navigationAgent.TargetPosition = _player.GlobalPosition;
                MoveAlongNavigation(_chaseSpeed);
            }
            else
            {
                Vector2 direction = (_player.GlobalPosition - GlobalPosition);
                if (direction.LengthSquared() > 0.01f)
                {
                    direction = direction.Normalized();
                    Rotation = direction.Angle();
                }
                Velocity = direction * _chaseSpeed;
                MoveAndSlide();
            }
        }

        private void MoveAlongNavigation(float speed)
        {
            if (_navigationAgent == null)
            {
                Velocity = Vector2.Zero;
                return;
            }

            Vector2 nextPosition = _navigationAgent.GetNextPathPosition();
            Vector2 direction = (nextPosition - GlobalPosition);
            if (direction.LengthSquared() > 0.01f)
            {
                direction = direction.Normalized();
                Rotation = direction.Angle();
            }

            Velocity = direction * speed;
            _navigationAgent.Velocity = Velocity;
            MoveAndSlide();
        }

        private void SetNextPatrolTarget()
        {
            if (_navigationAgent == null || _patrolPoints.Count == 0)
            {
                return;
            }

            Node2D targetPoint = _patrolPoints[_currentPatrolIndex];
            _navigationAgent.TargetPosition = targetPoint.GlobalPosition;
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
        }

        private void ChangeState(EnemyState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            _currentState = newState;
            switch (_currentState)
            {
                case EnemyState.Patrolling:
                    if (_patrolPoints.Count > 0)
                    {
                        SetNextPatrolTarget();
                    }
                    break;
                case EnemyState.Alerted:
                    _alertTimer = _alertDuration;
                    break;
                case EnemyState.Chasing:
                    break;
            }
        }

        private void OnVisionBodyEntered(Node body)
        {
            if (body is PlayerController player)
            {
                _player = player;
                LookAt(player.GlobalPosition);
                if (HasLineOfSightTo(player))
                {
                    ChangeState(EnemyState.Chasing);
                }
            }
        }

        private void OnVisionBodyExited(Node body)
        {
            if (body == _player)
            {
                _player = null;
                ChangeState(EnemyState.Alerted);
            }
        }

        private bool HasLineOfSightTo(PlayerController player)
        {
            if (_lineOfSight == null || player == null)
            {
                return false;
            }

            Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
            Vector2 forward = Vector2.Right.Rotated(Rotation);
            float angle = forward.AngleTo(toPlayer);

            // RayCast 配置
            _lineOfSight.TargetPosition = ToLocal(player.GlobalPosition);
            _lineOfSight.ForceRaycastUpdate();

            if (_lineOfSight.IsColliding())
            {
                return _lineOfSight.GetCollider() == player;
            }

            return Mathf.RadToDeg(Mathf.Abs(angle)) <= _visionLoseAngle;
        }

        private void OnNoiseMade(Vector2 location, float radius, Node instigator)
        {
            float distance = GlobalPosition.DistanceTo(location);
            if (distance > radius)
            {
                return;
            }

            if (_navigationAgent != null)
            {
                _navigationAgent.TargetPosition = location;
            }

            ChangeState(EnemyState.Alerted);
            _alertTimer = _alertDuration;
        }
    }
}
