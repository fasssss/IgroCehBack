using Domain.Entities;

namespace Persistence.Context
{
    // the catalog of preferences: genres, release periods and the scale of games;
    // ids are fixed, a new tag gets the next one
    public static class PreferenceTagSeed
    {
        public static readonly PreferenceTag[] Tags = new (string Name, string NameRu)[]
        {
            ("Action", "Экшен"), ("Adventure", "Приключения"), ("RPG", "RPG"), ("A-RPG", "A-RPG"), ("JRPG", "JRPG"),
            ("MMORPG", "MMORPG"), ("Shooter", "Шутер"), ("Platformer", "Платформер"), ("Metroidvania", "Метроидвания"), ("Soulslike", "Соулслайк"),
            ("Roguelike", "Рогалик"), ("Strategy", "Стратегия"), ("RTS", "RTS"), ("Turn-based strategy", "Пошаговая стратегия"), ("Tactics", "Тактика"),
            ("Simulation", "Симулятор"), ("Racing", "Гонки"), ("Sports", "Спорт"), ("Fighting", "Файтинг"), ("Hack and slash", "Слэшер"),
            ("Beat 'em up", "Битемап"), ("Stealth", "Стелс"), ("Survival", "Выживание"), ("Horror", "Хоррор"), ("Puzzle", "Головоломка"),
            ("Point-and-click", "Квест"), ("Visual novel", "Визуальная новелла"), ("Rhythm", "Ритм-игра"), ("Sandbox", "Песочница"), ("City builder", "Градостроитель"),
            ("Card game", "Карточная игра"), ("MOBA", "MOBA"), ("Battle royale", "Королевская битва"),
            ("...-1999", "...-1999"), ("2000-2009", "2000-2009"), ("2010-2019", "2010-2019"), ("2020-...", "2020-..."),
            ("AAA", "AAA"), ("Indie", "Инди"),
        }.Select((tag, index) => new PreferenceTag { Id = index + 1, Name = tag.Name, NameRu = tag.NameRu }).ToArray();
    }
}
