// Drives squad icon highlight and movement animations for battle feedback.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace DungeonCrawler.Gameplay.Squad
{
    [DisallowMultipleComponent]
    public class SquadAnimationController : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _iconRenderer;

        [SerializeField]
        private Color _targetHighlightColor = Color.green;

        [SerializeField]
        private Color _skipTurnColor = Color.yellow;

        [SerializeField]
        private Color _waitColor = new Color(1f, 0.85f, 0f);

        [SerializeField]
        private Color _attackColor = Color.cyan;

        [SerializeField]
        private Color _damageColor = Color.red;

        [SerializeField]
        private float _blinkDurationSeconds = 0.5f;

        [SerializeField]
        private float _blinkFrequency = 4f;

        [SerializeField]
        private float _attackMoveDistance = 0.35f;

        [SerializeField]
        private float _attackMoveDuration = 0.12f;

        [SerializeField]
        private float _shakeDuration = 0.2f;

        [SerializeField]
        private float _shakeStrength = 0.1f;

        [SerializeField, Range(0.1f, 1f)]
        private float _dodgeFadeAlpha = 0.35f;

        [SerializeField]
        private float _dodgeFadeDuration = 0.12f;

        private Sequence _blinkSequence;
        private readonly List<Tween> _runningTweens = new();
        private Color _originalColor;
        private Color _pendingResetColor;
        private bool _isTargetHighlighted;

        private void Awake()
        {
            if (_iconRenderer != null)
            {
                _originalColor = _iconRenderer.color;
                _pendingResetColor = _originalColor;
                _iconRenderer.flipX = transform.position.x < 0f;
            }
        }

        private void OnDisable()
        {
            StopAllAnimations();
            ResetColor();
        }

        public void HighlightAsTarget()
        {
            if (_iconRenderer == null)
            {
                return;
            }

            _isTargetHighlighted = true;
            StopBlink();
            ApplyBaseColor();
        }

        public Task PlaySkipTurnAnimation() => PlayBlinkAsync(_skipTurnColor);

        public Task PlayWaitAnimation() => PlayBlinkAsync(_waitColor);

        public Task PlayAttackAnimation() => PlayBlinkAsync(_attackColor);

        public Task PlayDamageAnimation() => PlayBlinkAsync(_damageColor);

        public Task PlayAttackMovementAsync(Vector3 direction)
        {
            if (direction == Vector3.zero)
            {
                direction = transform.right;
            }

            return PlayLungeAsync(direction.normalized);
        }

        public Task PlayDamageShakeAsync()
        {
            var startPosition = transform.localPosition;
            var tween = DOVirtual.Float(0f, 1f, _shakeDuration, _ => ApplyShake(transform, startPosition))
                .SetEase(Ease.Linear)
                .OnKill(() => transform.localPosition = startPosition)
                .OnComplete(() => transform.localPosition = startPosition);

            RegisterTween(tween);
            return tween.AsyncWaitForCompletion();
        }

        public Task PlayDodgeAsync(Vector3 direction)
        {
            if (direction == Vector3.zero)
            {
                direction = transform.right;
            }

            var movement = PlayLungeAsync(-direction.normalized);

            if (_iconRenderer == null)
            {
                return movement;
            }

            var fadeTween = _iconRenderer.DOFade(_dodgeFadeAlpha, _dodgeFadeDuration)
                .SetLoops(2, LoopType.Yoyo);
            RegisterTween(fadeTween);

            return Task.WhenAll(movement, fadeTween.AsyncWaitForCompletion());
        }

        public void ResetColor()
        {
            if (_iconRenderer == null)
            {
                return;
            }

            _isTargetHighlighted = false;
            StopBlink();
            ApplyBaseColor();
        }

        private Task PlayBlinkAsync(Color blinkColor)
        {
            if (_iconRenderer == null)
            {
                return Task.CompletedTask;
            }

            StopBlink();

            _pendingResetColor = GetBaseColor();
            var completionSource = new TaskCompletionSource<bool>();

            var period = GetBlinkPeriod();
            var loops = Mathf.Max(1, Mathf.RoundToInt(_blinkDurationSeconds / period));

            _blinkSequence = DOTween.Sequence();
            _blinkSequence.Append(_iconRenderer.DOColor(blinkColor, period * 0.5f));
            _blinkSequence.Append(_iconRenderer.DOColor(_pendingResetColor, period * 0.5f));
            _blinkSequence.SetLoops(loops);
            _blinkSequence.OnComplete(() =>
            {
                _iconRenderer.color = _pendingResetColor;
                _blinkSequence = null;
                completionSource.TrySetResult(true);
            });
            _blinkSequence.OnKill(() =>
            {
                _iconRenderer.color = _pendingResetColor;
                completionSource.TrySetResult(true);
            });

            return completionSource.Task;
        }

        private Task PlayLungeAsync(Vector3 direction)
        {
            var startPosition = transform.localPosition;
            var lungeTarget = startPosition + direction * _attackMoveDistance;

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOLocalMove(lungeTarget, _attackMoveDuration).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOLocalMove(startPosition, _attackMoveDuration).SetEase(Ease.InQuad));
            sequence.OnKill(() => transform.localPosition = startPosition);
            sequence.OnComplete(() => transform.localPosition = startPosition);

            RegisterTween(sequence);
            return sequence.AsyncWaitForCompletion();
        }

        private float GetBlinkPeriod()
        {
            return 1f / Mathf.Max(0.01f, _blinkFrequency);
        }

        private void StopBlink()
        {
            _blinkSequence?.Kill();
            _blinkSequence = null;
        }

        private void StopAllAnimations()
        {
            StopBlink();

            foreach (var tween in _runningTweens.ToArray())
            {
                tween?.Kill();
            }

            _runningTweens.Clear();

            ApplyBaseColor();
        }

        private void RegisterTween(Tween tween)
        {
            if (tween == null)
            {
                return;
            }

            _runningTweens.Add(tween);
            tween.OnKill(() => _runningTweens.Remove(tween));
        }

        private void ApplyShake(Transform target, Vector3 basePosition)
        {
            if (target == null)
            {
                return;
            }

            float amplitude = Mathf.Max(0f, _shakeStrength);
            if (amplitude <= 0f)
            {
                return;
            }

            Vector2 offset = Random.insideUnitCircle * amplitude;
            target.localPosition = basePosition + new Vector3(offset.x, offset.y, 0f);
        }

        private Color GetBaseColor()
        {
            return _isTargetHighlighted ? _targetHighlightColor : _originalColor;
        }

        private void ApplyBaseColor()
        {
            if (_iconRenderer != null)
            {
                _iconRenderer.color = GetBaseColor();
            }

            _pendingResetColor = GetBaseColor();
        }
    }
}
