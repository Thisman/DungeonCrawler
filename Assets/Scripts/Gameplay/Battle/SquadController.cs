// Displays squad visuals by binding a model to icon and count text renderers, handling damage application, and toggling visuals for dead squads.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;

namespace Assets.Scripts.Gameplay.Battle
{
    [DisallowMultipleComponent]
    public class SquadController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _iconRenderer;

        [SerializeField]
        private TextMeshProUGUI _info;

        [SerializeField]
        private SquadAnimationController _animationController;

        private SquadModel _model;

        public SquadModel Model => _model;

        public SquadAnimationController AnimationController
        {
            get
            {
                if (_animationController == null)
                {
                    _animationController = GetComponent<SquadAnimationController>();
                }

                return _animationController;
            }
        }

        public void Initalize(SquadModel squad)
        {
            _model = squad ?? throw new ArgumentNullException(nameof(squad));

            _model.Changed -= HandleSquadChanged;
            _model.Changed += HandleSquadChanged;

            RefreshIcon();
            RefreshInfo();
            RefreshState();
        }

        public Task Wait()
        {
            if (_model?.IsDead == true)
            {
                return Task.CompletedTask;
            }

            if (AnimationController == null)
            {
                return Task.CompletedTask;
            }

            return AnimationController.PlayWaitAnimation();
        }

        public Task SkipTurn()
        {
            if (_model?.IsDead == true)
            {
                return Task.CompletedTask;
            }

            if (AnimationController == null)
            {
                return Task.CompletedTask;
            }

            return AnimationController.PlaySkipTurnAnimation();
        }

        public void SetAsTarget(bool isTarget)
        {
            if (_model?.IsDead == true)
            {
                return;
            }

            if (AnimationController == null)
            {
                return;
            }

            if (isTarget)
            {
                AnimationController.HighlightAsTarget();
            }
            else
            {
                AnimationController.ResetColor();
            }
        }

        public async Task ResolveAttack(DamageInstance damage)
        {
            if (damage == null)
            {
                return;
            }

            if (_model?.IsDead == true)
            {
                return;
            }

            var movementDirection = GetDirectionToScreenCenter();

            var animations = new List<Task>();
            if (AnimationController != null && AnimationController.enabled)
            {
                animations.Add(AnimationController.PlayAttackAnimation());
                animations.Add(AnimationController.PlayAttackMovementAsync(movementDirection));
            }

            if (animations.Count > 0)
            {
                await Task.WhenAll(animations);
            }
        }

        public async Task TakeDamage(DamageInstance damage)
        {
            if (damage == null)
            {
                return;
            }

            var direction = GetDirectionToScreenCenter();

            if (damage.IsHit)
            {
                _model?.ApplyDamage(damage.Amount);
                RefreshInfo();
                RefreshState();

                if (_model?.IsDead == true)
                {
                    return;
                }

                var animations = new List<Task>();

                if (AnimationController != null && AnimationController.enabled)
                {
                    animations.Add(AnimationController.PlayDamageAnimation());
                    animations.Add(AnimationController.PlayDamageShakeAsync());
                }

                if (animations.Count > 0)
                {
                    await Task.WhenAll(animations);
                }
            }
            else if (AnimationController != null)
            {
                await AnimationController.PlayDodgeAsync(direction);
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.Changed -= HandleSquadChanged;
            }
        }

        private void HandleSquadChanged(SquadModel squad, int newCount, int oldCount)
        {
            RefreshInfo();
            RefreshState();
        }

        private void RefreshIcon()
        {
            if (_iconRenderer == null || _model == null)
            {
                return;
            }

            _iconRenderer.sprite = _model.Unit.Definition.Icon;
        }

        private void RefreshInfo()
        {
            if (_info == null || _model == null)
            {
                return;
            }

            UnitDefinition unitDefinition = _model.Unit.Definition;
            _info.text = _model.IsDead
                ? "Dead"
                : $"{unitDefinition.Name} x {_model.UnitCount}";
        }

        private Vector3 GetDirectionToScreenCenter()
        {
            var direction = -transform.position;

            if (direction.sqrMagnitude < Mathf.Epsilon)
            {
                direction = transform.right;
            }

            return direction.normalized;
        }

        private void RefreshState()
        {
            if (_model == null)
            {
                return;
            }

            var isAlive = !_model.IsDead;

            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = isAlive;
            }

            if (AnimationController != null)
            {
                AnimationController.enabled = isAlive;
            }
        }
    }
}
