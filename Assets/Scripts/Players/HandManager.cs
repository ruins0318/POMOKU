using System.Collections.Generic;
using Pomoku.Cards;
using UnityEngine;

namespace Pomoku.Players
{
    public class HandManager : MonoBehaviour
    {
        private readonly List<List<CardData>> playerHands = new List<List<CardData>>();

        public int PlayerCount
        {
            get { return playerHands.Count; }
        }

        public void CreatePlayerHands(int playerCount)
        {
            playerHands.Clear();

            for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
            {
                playerHands.Add(new List<CardData>());
            }
        }

        public void DealInitialHands(DeckManager deckManager, int cardsPerPlayer)
        {
            if (deckManager == null)
            {
                Debug.LogError("HandManager cannot deal cards because DeckManager is missing.");
                return;
            }

            for (int cardIndex = 0; cardIndex < cardsPerPlayer; cardIndex++)
            {
                for (int playerIndex = 0; playerIndex < playerHands.Count; playerIndex++)
                {
                    if (deckManager.TryDrawCard(out CardData drawnCard))
                    {
                        playerHands[playerIndex].Add(drawnCard);
                    }
                    else
                    {
                        Debug.LogError("The deck ran out of cards while dealing initial hands.");
                        return;
                    }
                }
            }
        }

        public IReadOnlyList<CardData> GetPlayerHand(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= playerHands.Count)
            {
                Debug.LogError("Invalid player index: " + playerIndex);
                return new List<CardData>();
            }

            return playerHands[playerIndex];
        }

        public bool RemoveFirstMatchingCardFromHand(int playerIndex, CardData cardToRemove)
        {
            if (!IsValidPlayerIndex(playerIndex))
            {
                return false;
            }

            List<CardData> playerHand = playerHands[playerIndex];

            for (int i = 0; i < playerHand.Count; i++)
            {
                CardData handCard = playerHand[i];

                if (handCard.Suit == cardToRemove.Suit && handCard.Rank == cardToRemove.Rank)
                {
                    playerHand.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void AddCardToHand(int playerIndex, CardData cardToAdd)
        {
            if (!IsValidPlayerIndex(playerIndex))
            {
                return;
            }

            playerHands[playerIndex].Add(cardToAdd);
        }

        private bool IsValidPlayerIndex(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < playerHands.Count)
            {
                return true;
            }

            Debug.LogError("Invalid player index: " + playerIndex);
            return false;
        }
    }
}
