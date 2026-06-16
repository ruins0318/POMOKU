using System.Collections.Generic;
using UnityEngine;

namespace Pomoku.Cards
{
    public class DeckManager : MonoBehaviour
    {
        private readonly List<CardData> deckCards = new List<CardData>();
        private readonly System.Random random = new System.Random();

        public int RemainingCardCount
        {
            get { return deckCards.Count; }
        }

        public void CreateMvpDeck()
        {
            deckCards.Clear();
            deckCards.AddRange(MvpCardRules.CreateMvpRegularCardPool(MvpCardRules.CopiesPerRegularCard));
        }

        public void ShuffleDeck()
        {
            for (int i = deckCards.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                CardData temporaryCard = deckCards[i];
                deckCards[i] = deckCards[randomIndex];
                deckCards[randomIndex] = temporaryCard;
            }
        }

        public bool TryDrawCard(out CardData drawnCard)
        {
            if (deckCards.Count == 0)
            {
                drawnCard = default(CardData);
                return false;
            }

            int lastIndex = deckCards.Count - 1;
            drawnCard = deckCards[lastIndex];
            deckCards.RemoveAt(lastIndex);
            return true;
        }

        // J and Joker cards will be added later when their special effects are implemented.
    }
}
