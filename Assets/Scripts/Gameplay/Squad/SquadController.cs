// Displays squad visuals by binding a model to icon and count text renderers and triggers squad-level animations.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DungeonCrawler.Gameplay.Unit;
using DungeonCrawler.Gameplay.Battle;

namespace DungeonCrawler.Gameplay.Squad
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
        }

        public Task Wait()
        {
            if (AnimationController == null)
            {
                return Task.CompletedTask;
            }

            return AnimationController.PlayWaitAnimation();
        }

        public Task SkipTurn()
        {
            if (AnimationController == null)
            {
                return Task.CompletedTask;
            }

            return AnimationController.PlaySkipTurnAnimation();
        }

        public void SetAsTarget(bool isTarget)
        {
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

            var movementDirection = GetDirectionToScreenCenter();

            var animations = new List<Task>();
            if (AnimationController != null)
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
                var animations = new List<Task>();

                if (AnimationController != null)
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
            _info.text = $"{unitDefinition.Name} x {_model.UnitCount}";
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
    }
}
