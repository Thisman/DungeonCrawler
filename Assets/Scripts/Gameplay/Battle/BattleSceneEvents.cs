// Declares battle scene events exchanged through the scene event bus.
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
    public record RequestFinishBattle();

    // Event published when an action is selected for a unit.
    public record RequestActionSelect(UnitAction Action);

    // Event published when a unit plan action.
    public record UnitActionSelected(PlannedUnitAction Plan);
}
