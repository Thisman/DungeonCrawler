// Declares battle scene events exchanged through the scene event bus.
namespace DungeonCrawler.Gameplay.Battle
{
    public record BattleStateChanged(BattleState FromState, BattleState ToState, BattleContext Context);

    public record RequestBattlePreparationFinish();

    public record RequestSkipTurnAction();

    public record RequestWaitAction();

    public record RequestFleeFromBattle();

    public record RequestFinishBattle();
}
