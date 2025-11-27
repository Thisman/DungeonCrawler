using DungeonCrawler.Gameplay.Squad;
using System.Collections.Generic;

namespace DungeonCrawler.Gameplay.Dungeon
{
    public record RequestEnterBattle(List<SquadModel> Enemies);

    public record BattleEnded();
}