using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pomoku.Cards
{
    public class DeckManager : MonoBehaviour
    {
        private const int MvpDeckSetCount = 2;

        private readonly List<CardData> deckCards = new List<CardData>();
        private readonly System.Random random = new System.Random();

        public int RemainingCardCount
        {
            get { return deckCards.Count; }
        }

        public void CreateMvpDeck()
        {
            deckCards.Clear();

            for (int setIndex = 0; setIndex < MvpDeckSetCount; setIndex++)
            {
                AddOneRegularCardSet();
            }
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

        private void AddOneRegularCardSet()
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int rankValue = 0; rankValue < 13; rankValue++)
                {
                    Rank rank = (Rank)rankValue;
                    deckCards.Add(new CardData(suit, rank, false));
                }
            }
        }
    }
}
