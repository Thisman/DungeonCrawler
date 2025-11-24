// Displays squad visuals by binding a model to icon and count text renderers.
using System;
using TMPro;
using UnityEngine;
using DungeonCrawler.Gameplay.Unit;

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
    }
}
