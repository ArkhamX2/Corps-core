using MegaCorps.Core.Model.Cards;
using MegaCorps.Core.Model.Enums;

namespace MegaCorps.Core.Model
{
    /// <summary>
    /// Класс игрока
    /// </summary>
    public class Player
    {

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// Количество очков
        /// </summary>
        public int Score { get; set; } = 1;
        /// <summary>
        /// "Рука" игрока
        /// </summary>
        public PlayerHand Hand { get; set; } = new(new());
        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Флаг готовности игрока сделать ход
        /// </summary>
        public bool IsReady { get; set; } = false;

        public Player(int id) => Id = id;

        public Player(int id, string username) : this(id) => Name = username;

        /// <summary>
        /// Сыграть руку
        /// </summary>
        public void PlayHand()
        {
            int scoreDelta = Hand.Play();
            Score += scoreDelta;
            Score = Score <= 1 ? 1 : Score;
        }

    }
    /// <summary>
    /// Класс "руки" игрока
    /// </summary>
    public class PlayerHand
    {

        /// <summary>
        /// Содержание руки игрока
        /// </summary>
        public List<GameCard> Cards { get; set; } = new();

        /// <summary>
        /// Направленные на игрока атаки
        /// </summary>
        public List<AttackCard> Targeted { get; set; } = new();
        /// <summary>
        /// Очередь выбранных карт
        /// </summary>
        public Queue<int> SelectedCardQueue { get; private set; } = new();

        public PlayerHand(List<GameCard> cards) => Cards = cards;


        /// <summary>
        /// Метод для реализации текущей руки. Отбиваем направленные атаки, используем выбранные карты
        /// </summary>
        public int Play() => PlayDevelopers() - PlayTargeted(ExtractDefenceTypes());


        private int PlayDevelopers() => Cards
                .Where((card) => card is DeveloperCard && card.State == CardState.Used)
                .Sum(card => (card as DeveloperCard)!.DevelopmentPoint);


        private int PlayTargeted(List<AttackType> defenceTypes) => Targeted
                .Where(x => !defenceTypes.Contains(x.AttackType))
                .ToList()
                .Sum(x => x.Damage);


        private List<AttackType> ExtractDefenceTypes() => Cards
                .Where((card) => card is DefenceCard && card.State == CardState.Used)
                .Select(x => (x as DefenceCard)!.AttackTypes)
                .SelectMany(y => y).ToList();


        /// <summary>
        /// Метод выбора карты с учётом выбранных ранее карт.
        /// Если выбранных карт становится
        /// </summary>
        /// <param name="selectedCardId"></param>
        /// <returns></returns>
        public int PushCardToSelectedQueue(int selectedCardId)
        {
            GameCard selectcard = Cards.FirstOrDefault(card => card.Id == selectedCardId)!;
            int unSelectId = -1;
            SelectedCardQueue.Enqueue(selectcard.Id);
            if (SelectedCardQueue.Count > 3)
            {
                unSelectId = SelectedCardQueue.Peek();
                SelectedCardQueue.Dequeue();
            }
            return unSelectId;
        }
    }
}
