// Tracks battle outcomes, squad snapshots, and evaluates victory, defeat, or fleeing conditions.
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public enum BattleOutcome
    {
        None,
        Victory,
        Defeat,
        Flee
    }

    public class BattleResult
    {
        private readonly List<BattleSquadResult> _squadResults;

        public BattleResult(IEnumerable<SquadModel> squads)
        {
            if (squads == null)
            {
                throw new ArgumentNullException(nameof(squads));
            }

            _squadResults = squads
                .Select(squad => new BattleSquadResult(squad, squad?.UnitCount ?? 0))
                .ToList();
        }

        public BattleOutcome Outcome { get; private set; } = BattleOutcome.None;

        public IReadOnlyList<BattleSquadResult> SquadResults => _squadResults;

        public IEnumerable<BattleSquadResult> GetPlayerSquads()
        {
            return _squadResults.Where(result => result.IsFriendly || result.IsHero);
        }

        public IEnumerable<BattleSquadResult> GetEnemySquads()
        {
            return _squadResults.Where(result => result.IsEnemy);
        }

        public bool TryEvaluate(bool playerRequestedFlee = false)
        {
            if (Outcome != BattleOutcome.None)
            {
                return true;
            }

            if (playerRequestedFlee)
            {
                Outcome = BattleOutcome.Flee;
                CaptureFinalCounts();
                return true;
            }

            if (IsHeroDefeated())
            {
                Outcome = BattleOutcome.Defeat;
                CaptureFinalCounts();
                return true;
            }

            if (AreEnemiesDefeated())
            {
                Outcome = BattleOutcome.Victory;
                CaptureFinalCounts();
                return true;
            }

            return false;
        }

        private bool AreEnemiesDefeated()
        {
            var enemySquads = _squadResults.Where(result => result.IsEnemy).ToList();
            return enemySquads.Count > 0 && enemySquads.All(result => result.Squad?.IsDead == true);
        }

        private bool IsHeroDefeated()
        {
            return _squadResults.Any(result => result.IsHero && result.Squad?.IsDead == true);
        }

        private void CaptureFinalCounts()
        {
            foreach (var squadResult in _squadResults)
            {
                squadResult.SetFinalCount(squadResult.Squad?.UnitCount ?? 0);
            }
        }

        public class BattleSquadResult
        {
            public BattleSquadResult(SquadModel squad, int initialCount)
            {
                Squad = squad;
                InitialCount = initialCount;
                FinalCount = initialCount;
            }

            public SquadModel Squad { get; }

            public int InitialCount { get; }

            public int FinalCount { get; private set; }

            public int Delta => FinalCount - InitialCount;

            public bool IsHero => Squad?.Unit?.Definition?.IsHero() == true;

            public bool IsFriendly => Squad?.Unit?.Definition?.IsFriendly() == true;

            public bool IsEnemy => Squad?.Unit?.Definition?.IsEnemy() == true;

            public void SetFinalCount(int finalCount)
            {
                FinalCount = finalCount;
            }
        }
    }
}
