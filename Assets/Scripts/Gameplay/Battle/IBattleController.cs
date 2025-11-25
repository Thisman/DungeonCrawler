// Describes a controller that selects actions for a squad during battle.
using System.Threading;
using System.Threading.Tasks;
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    public interface IBattleController
    {
        Task<PlannedUnitAction> DecideActionAsync(
            SquadModel actor,
            BattleContext context,
            CancellationToken cancellationToken);
    }
}
