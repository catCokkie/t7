using Godot;
using System;
using SilentTestimony.Global;

namespace SilentTestimony.AI
{
        /// <summary>
        /// 敌人 AI 控制器。
        /// 负责巡逻、对噪音做出反应，并在看到玩家时进入追逐状态。
        /// </summary>
        public partial class EnemyAIController : CharacterBody2D
        {
                public enum EnemyState { Patrolling, Alerted, Chasing }

                [ExportGroup("Movement")]
                [Export] private float _moveSpeed = 90.0f;
                [Export(PropertyHint.Range, "0,64,1")] private float _targetTolerance = 8.0f;

                [ExportGroup("Patrol")]
                [Export] private Godot.Collections.Array<Vector2> _patrolPoints = new();
                [Export] private bool _loopPatrol = true;

                [ExportGroup("Alert")]
                [Export] private float _alertDuration = 3.0f;

                private NavigationAgent2D _navigationAgent;
                private Area2D _visionArea;
                private RayCast2D _visionRay;
                private Timer _alertTimer;
                private GlobalEventBus _eventBus;

                private EnemyState _currentState = EnemyState.Patrolling;
                private int _currentPatrolIndex;
                private Vector2 _investigatePoint;
                private Vector2 _lastKnownPlayerPosition;
                private bool _hasLastKnownPlayerPosition;
                private CharacterBody2D _trackedPlayer;

                public override void _Ready()
                {
                        _navigationAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
                        _visionArea = GetNode<Area2D>("VisionArea");
                        _visionRay = GetNode<RayCast2D>("VisionRay");
                        _alertTimer = GetNode<Timer>("AlertTimer");

                        _visionArea.BodyEntered += OnVisionBodyEntered;
                        _visionArea.BodyExited += OnVisionBodyExited;
                        _alertTimer.Timeout += OnAlertTimerTimeout;

                        _eventBus = GetNode<GlobalEventBus>("/root/GlobalEventBus");
                        _eventBus.NoiseMade += OnNoiseMade;

                        _visionRay.Enabled = true;
                        _visionRay.ExcludeParent = true;

                        _hasLastKnownPlayerPosition = false;

                        if (_patrolPoints.Count > 0)
                        {
                                _currentPatrolIndex = 0;
                                UpdatePatrolTarget();
                        }
                        else
                        {
                                EnterAlertState(GlobalPosition);
                        }
                }

                public override void _ExitTree()
                {
                        if (_eventBus != null)
                        {
                                _eventBus.NoiseMade -= OnNoiseMade;
                        }

                        if (_visionArea != null)
                        {
                                _visionArea.BodyEntered -= OnVisionBodyEntered;
                                _visionArea.BodyExited -= OnVisionBodyExited;
                        }

                        if (_alertTimer != null)
                        {
                                _alertTimer.Timeout -= OnAlertTimerTimeout;
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
                                        ProcessAlerted();
                                        break;
                                case EnemyState.Chasing:
                                        ProcessChasing();
                                        break;
                        }

                        MoveAndSlide();
                }

                private void ProcessPatrol()
                {
                        if (_patrolPoints.Count == 0)
                        {
                                Velocity = Vector2.Zero;
                                return;
                        }

                        Vector2 target = _patrolPoints[_currentPatrolIndex];
                        ApplyMovementTowards(target);

                        if (GlobalPosition.DistanceTo(target) <= _targetTolerance)
                        {
                                AdvancePatrolIndex();
                                UpdatePatrolTarget();
                        }
                }

                private void ProcessAlerted()
                {
                        ApplyMovementTowards(_investigatePoint);

                        if (GlobalPosition.DistanceTo(_investigatePoint) <= _targetTolerance)
                        {
                                if (_alertTimer.IsStopped())
                                {
                                        _alertTimer.Start(_alertDuration);
                                }
                        }
                }

                private void ProcessChasing()
                {
                        if (_trackedPlayer == null)
                        {
                                StartAlertFromLastKnown();
                                return;
                        }

                        Vector2 playerPosition = _trackedPlayer.GlobalPosition;
                        _navigationAgent.TargetPosition = playerPosition;
                        _lastKnownPlayerPosition = playerPosition;
                        _hasLastKnownPlayerPosition = true;
                        ApplyMovementTowards(playerPosition);

                        if (!HasLineOfSight(_trackedPlayer))
                        {
                                StartAlertFromLastKnown();
                        }
                }

                private void ApplyMovementTowards(Vector2 target)
                {
                        if (_navigationAgent == null)
                        {
                                Vector2 direct = target - GlobalPosition;
                                Velocity = direct.LengthSquared() > 1.0f ? direct.Normalized() * _moveSpeed : Vector2.Zero;
                                return;
                        }

                        if (_navigationAgent.TargetPosition != target)
                        {
                                _navigationAgent.TargetPosition = target;
                        }

                        Vector2 desired;

                        if (_navigationAgent.IsNavigationFinished())
                        {
                                Vector2 direct = target - GlobalPosition;
                                desired = direct.LengthSquared() > 1.0f ? direct.Normalized() * _moveSpeed : Vector2.Zero;
                        }
                        else
                        {
                                Vector2 nextPosition = _navigationAgent.GetNextPathPosition();
                                Vector2 toNext = nextPosition - GlobalPosition;
                                desired = toNext.LengthSquared() > 1.0f ? toNext.Normalized() * _moveSpeed : Vector2.Zero;
                        }

                        Velocity = desired;
                }

                private void AdvancePatrolIndex()
                {
                        if (_patrolPoints.Count == 0)
                                return;

                        if (_currentPatrolIndex + 1 < _patrolPoints.Count)
                        {
                                _currentPatrolIndex++;
                        }
                        else if (_loopPatrol)
                        {
                                _currentPatrolIndex = 0;
                        }
                        else
                        {
                                _currentPatrolIndex = Math.Max(_patrolPoints.Count - 1, 0);
                        }
                }

                private void UpdatePatrolTarget()
                {
                        if (_patrolPoints.Count == 0)
                                return;

                        _navigationAgent.TargetPosition = _patrolPoints[_currentPatrolIndex];
                }

                private void UpdateInvestigateTarget()
                {
                        _navigationAgent.TargetPosition = _investigatePoint;
                }

                private void StartAlertFromLastKnown()
                {
                        if (!_hasLastKnownPlayerPosition)
                        {
                                if (_patrolPoints.Count > 0)
                                {
                                        _currentState = EnemyState.Patrolling;
                                        UpdatePatrolTarget();
                                }
                                _trackedPlayer = null;
                                return;
                        }

                        EnterAlertState(_lastKnownPlayerPosition);
                        _hasLastKnownPlayerPosition = false;
                        _trackedPlayer = null;
                }

                private bool HasLineOfSight(Node2D target)
                {
                        if (_visionRay == null || target == null)
                        {
                                return false;
                        }

                        _visionRay.TargetPosition = ToLocal(target.GlobalPosition);
                        _visionRay.ForceRaycastUpdate();

                        if (!_visionRay.IsColliding())
                        {
                                return true;
                        }

                        return _visionRay.GetCollider() == target;
                }

                private void OnVisionBodyEntered(Node2D body)
                {
                        if (body is CharacterBody2D character && body.IsInGroup("Player"))
                        {
                                _trackedPlayer = character;

                                if (HasLineOfSight(character))
                                {
                                        StartChasing();
                                }
                                else
                                {
                                        _lastKnownPlayerPosition = character.GlobalPosition;
                                        _hasLastKnownPlayerPosition = true;
                                        EnterAlertState(_lastKnownPlayerPosition);
                                }
                        }
                }

                private void OnVisionBodyExited(Node2D body)
                {
                        if (_trackedPlayer != null && body == _trackedPlayer)
                        {
                                StartAlertFromLastKnown();
                        }
                }

                private void OnNoiseMade(Vector2 location, float radius, Node instigator)
                {
                        if (_currentState == EnemyState.Chasing)
                                return;

                        if (instigator == this)
                                return;

                        float distance = GlobalPosition.DistanceTo(location);
                        if (distance <= radius)
                        {
                                EnterAlertState(location);
                        }
                }

                private void OnAlertTimerTimeout()
                {
                        if (_patrolPoints.Count > 0)
                        {
                                _currentState = EnemyState.Patrolling;
                                UpdatePatrolTarget();
                        }
                        else
                        {
                                _currentState = EnemyState.Alerted;
                        }
                }

                private void EnterAlertState(Vector2 position)
                {
                        _investigatePoint = position;
                        _currentState = EnemyState.Alerted;
                        UpdateInvestigateTarget();
                        StopAlertTimer();
                }

                private void StartChasing()
                {
                        if (_trackedPlayer == null)
                                return;

                        _lastKnownPlayerPosition = _trackedPlayer.GlobalPosition;
                        _hasLastKnownPlayerPosition = true;
                        _currentState = EnemyState.Chasing;
                        StopAlertTimer();
                }

                private void StopAlertTimer()
                {
                        if (_alertTimer != null)
                        {
                                _alertTimer.Stop();
                        }
                }
        }
}
