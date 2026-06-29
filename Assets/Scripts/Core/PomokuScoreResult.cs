namespace Pomoku.Core
{
    public struct PomokuScoreResult
    {
        public PomokuHandType HandType;
        public int Score;
        public bool ContainsAnchorJari;
        public string Description;
        public string CardsDescription;

        public PomokuScoreResult(PomokuHandType handType, int score, bool containsAnchorJari, string description, string cardsDescription)
        {
            HandType = handType;
            Score = score;
            ContainsAnchorJari = containsAnchorJari;
            Description = description;
            CardsDescription = cardsDescription;
        }
    }
}
