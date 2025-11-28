// Simulates bonfire flickering by smoothly randomizing a Light2D component's intensity and radius.
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DungeonCrawler.Gameplay.Dungeon
{
    [RequireComponent(typeof(Light2D))]
    public class BonfireLightController : MonoBehaviour
    {
        [SerializeField] private Light2D _light;
        [SerializeField] private float _baseIntensity = 1.2f;
        [SerializeField] private float _intensityJitter = 0.3f;
        [SerializeField] private float _baseOuterRadius = 2.5f;
        [SerializeField] private float _radiusJitter = 0.35f;
        [SerializeField] private float _changeInterval = 0.2f;
        [SerializeField] private float _smoothFactor = 6f;

        private float _targetIntensity;
        private float _targetOuterRadius;
        private float _elapsed;

        private void Awake()
        {
            if (_light == null)
            {
                _light = GetComponent<Light2D>();
            }

            CacheNewTargets(true);
        }

        private void Update()
        {
            if (_light == null)
            {
                return;
            }

            _elapsed += Time.deltaTime;
            if (_elapsed >= _changeInterval)
            {
                _elapsed = 0f;
                CacheNewTargets(false);
            }

            var t = 1f - Mathf.Exp(-_smoothFactor * Time.deltaTime);
            _light.intensity = Mathf.Lerp(_light.intensity, _targetIntensity, t);
            _light.pointLightOuterRadius = Mathf.Lerp(_light.pointLightOuterRadius, _targetOuterRadius, t);
        }

        private void CacheNewTargets(bool force)
        {
            var intensityMin = _baseIntensity - _intensityJitter;
            var intensityMax = _baseIntensity + _intensityJitter;
            var radiusMin = Mathf.Max(0f, _baseOuterRadius - _radiusJitter);
            var radiusMax = _baseOuterRadius + _radiusJitter;

            _targetIntensity = Random.Range(intensityMin, intensityMax);
            _targetOuterRadius = Random.Range(radiusMin, radiusMax);

            if (force && _light != null)
            {
                _light.intensity = _targetIntensity;
                _light.pointLightOuterRadius = _targetOuterRadius;
            }
        }

        private void OnValidate()
        {
            if (_light == null)
            {
                _light = GetComponent<Light2D>();
            }
        }
    }
}
