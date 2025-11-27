// Declares battle scene events exchanged through the scene event bus.
using DungeonCrawler.Gameplay.Squad;

namespace DungeonCrawler.Gameplay.Battle
{
    // Event published when the battle state changes.
    public record BattleStateChanged(BattleState FromState, BattleState ToState, BattleContext Context);

    // Event published when preparing for battle requires finishing
    public record RequestBattlePreparationFinish();

    // Event published when a unit requests to perform skipping its turn.
    public record RequestSkipTurnAction();

    // Event published when a unit requests to perform waiting.
    public record RequestWaitAction();

    // Event published when a unit requests to flee from battle.
    public record RequestFleeFromBattle();

    // Event published when a unit requests to finish the battle.
    public record RequestFinishBattle(BattleResult Result);

    // Event published when user select action.
    public record RequestSelectAction(UnitAction Action);

    // Event publisehd whe user select action target.
    public record RequestSelectTarget(SquadModel Target);

    // Event published when a unit plan action.
    public record UnitPlanSelected(PlannedUnitAction Plan);

}
