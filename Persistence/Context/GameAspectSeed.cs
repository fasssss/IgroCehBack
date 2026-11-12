using Domain.Entities;

namespace Persistence.Context
{
    // the catalog of the aspects of games a player may like; ids are fixed, a new aspect gets the next one
    public static class GameAspectSeed
    {
        public static readonly GameAspect[] Aspects = new (string Name, string NameRu)[]
        {
            ("Story", "Сюжет"), ("Gameplay", "Геймплей"), ("Visuals", "Визуал"), ("Music", "Музыка"), ("Atmosphere", "Атмосфера"),
            ("Difficulty", "Сложность"), ("Achievements", "Ачивки"), ("Exploration", "Исследование"), ("Multiplayer", "Мультиплеер"), ("Co-op", "Кооператив"),
        }.Select((aspect, index) => new GameAspect { Id = index + 1, Name = aspect.Name, NameRu = aspect.NameRu }).ToArray();
    }
}
