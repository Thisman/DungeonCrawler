// Defines dungeon scene-level events for transitioning into and out of battles.
using DungeonCrawler.Gameplay.Battle;
using DungeonCrawler.Gameplay.Squad;
using System.Collections.Generic;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public record RequestEnterBattle(List<SquadModel> Enemies);

    public record BattleEnded(BattleResult Result);
}
