// Drives squad icon highlight and blink animations for battle feedback.
using System.Collections;
using System.Threading.Tasks;
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
        private float _blinkDurationSeconds = 0.5f;

        private Coroutine _currentAnimation;
        private Color _originalColor;
        private Color _pendingResetColor;

        private void Awake()
        {
            if (_iconRenderer != null)
            {
                _originalColor = _iconRenderer.color;
            }
        }

        private void OnDisable()
        {
            ResetColor();
        }

        public void HighlightAsTarget()
        {
            if (_iconRenderer == null)
            {
                return;
            }

            StopCurrentAnimation();
            _iconRenderer.color = _targetHighlightColor;
        }

        public Task PlaySkipTurnAnimation()
        {
            return PlayBlinkAsync(_skipTurnColor);
        }

        public Task PlayWaitAnimation()
        {
            return PlayBlinkAsync(_waitColor);
        }

        public void ResetColor()
        {
            if (_iconRenderer == null)
            {
                return;
            }

            StopCurrentAnimation();
            _iconRenderer.color = _originalColor;
        }

        private Task PlayBlinkAsync(Color blinkColor)
        {
            if (_iconRenderer == null)
            {
                return Task.CompletedTask;
            }

            StopCurrentAnimation();
            _pendingResetColor = _iconRenderer.color;

            var completionSource = new TaskCompletionSource<bool>();
            _currentAnimation = StartCoroutine(BlinkRoutine(blinkColor, completionSource));
            return completionSource.Task;
        }

        private IEnumerator BlinkRoutine(Color blinkColor, TaskCompletionSource<bool> completionSource)
        {
            _iconRenderer.color = blinkColor;

            yield return new WaitForSeconds(_blinkDurationSeconds);

            _iconRenderer.color = _pendingResetColor;
            _currentAnimation = null;
            completionSource.TrySetResult(true);
        }

        private void StopCurrentAnimation()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _iconRenderer.color = _pendingResetColor;
                _currentAnimation = null;
            }
        }
    }
}
