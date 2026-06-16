using System.Collections.Generic;
using Pomoku.Board;
using Pomoku.Cards;
using Pomoku.Players;
using Pomoku.UI;
using UnityEngine;

namespace Pomoku.Core
{
    public class LocalGameManager : MonoBehaviour
    {
        private const int LocalPlayerCount = 4;
        private const int InitialHandSize = 6;
        private const int CurrentPlayerIndex = 0;

        private BoardManager boardManager;
        private DeckManager deckManager;
        private HandManager handManager;
        private BoardView boardView;
        private HandView handView;
        private CardData selectedCardData;
        private bool hasSelectedCard;
        private readonly TeamId currentPlayerTeam = TeamId.TeamA;
        private readonly List<CardData> usedCards = new List<CardData>();

        public bool HasSelectedCard
        {
            get { return hasSelectedCard; }
        }

        public CardData SelectedCardData
        {
            get { return selectedCardData; }
        }

        public void StartLocalGame()
        {
            ResetLocalGameState();
            CreateManagers();
            CreateAndShowBoard();
            CreateDeckAndDealHands();
            ShowCurrentPlayerHand();
            LogLocalGameStartState();
            ValidateMvpCardCounts("initial deal");
        }

        private void ResetLocalGameState()
        {
            usedCards.Clear();
            hasSelectedCard = false;
            selectedCardData = default(CardData);
        }

        private void CreateManagers()
        {
            boardManager = gameObject.AddComponent<BoardManager>();
            deckManager = gameObject.AddComponent<DeckManager>();
            handManager = gameObject.AddComponent<HandManager>();
            boardView = gameObject.AddComponent<BoardView>();
            handView = gameObject.AddComponent<HandView>();
        }

        private void CreateAndShowBoard()
        {
            boardManager.CreateBoard();
            boardView.ShowBoard(boardManager.BoardCells, BoardManager.BoardSize, TryPlaceChipOnBoardCell);
        }

        private void CreateDeckAndDealHands()
        {
            deckManager.CreateMvpDeck();
            deckManager.ShuffleDeck();

            handManager.CreatePlayerHands(LocalPlayerCount);
            handManager.DealInitialHands(deckManager, InitialHandSize);
        }

        private void ShowCurrentPlayerHand()
        {
            IReadOnlyList<CardData> currentPlayerHand = handManager.GetPlayerHand(CurrentPlayerIndex);
            handView.ShowCurrentPlayerHand(CurrentPlayerIndex, currentPlayerHand, SelectCurrentPlayerCard);
        }

        private void SelectCurrentPlayerCard(CardData cardData)
        {
            selectedCardData = cardData;
            hasSelectedCard = true;

            Debug.Log("Selected Card: " + selectedCardData.GetDisplayName());
            int highlightedCellCount = boardView.HighlightCellsMatchingCard(selectedCardData);
            Debug.Log("Highlighted valid cells for: " + selectedCardData.GetDisplayName() + " (" + highlightedCellCount + ")");
        }

        private void TryPlaceChipOnBoardCell(int cellIndex, BoardCellData cellData)
        {
            if (!CanPlaceChipOnCell(cellIndex, cellData))
            {
                return;
            }

            CardData usedCard = selectedCardData;

            if (!handManager.RemoveFirstMatchingCardFromHand(CurrentPlayerIndex, usedCard))
            {
                Debug.LogWarning("Could not remove used card from Player 1 hand: " + usedCard.GetDisplayName());
                return;
            }

            boardView.ShowChipAtCell(cellIndex, currentPlayerTeam);
            usedCards.Add(usedCard);

            Debug.Log("Placed TeamA chip on " + cellData.Card.GetDisplayName() + " at cell index " + cellIndex);
            Debug.Log("Used card: " + usedCard.GetDisplayName());

            DrawReplacementCardForCurrentPlayer();

            hasSelectedCard = false;
            selectedCardData = default(CardData);
            boardView.ClearHighlightedCells();
            handView.ClearSelectedCard();
            RefreshCurrentPlayerHandView();
            ValidateMvpCardCounts("after card use");
        }

        private bool CanPlaceChipOnCell(int cellIndex, BoardCellData cellData)
        {
            if (!hasSelectedCard)
            {
                return false;
            }

            if (cellData == null)
            {
                return false;
            }

            if (!boardView.IsCellHighlighted(cellIndex))
            {
                return false;
            }

            if (cellData.CellType != BoardCellType.Normal)
            {
                return false;
            }

            if (cellData.ChipOwnerTeam != TeamId.None)
            {
                return false;
            }

            return cellData.Card.Suit == selectedCardData.Suit && cellData.Card.Rank == selectedCardData.Rank;
        }

        private void DrawReplacementCardForCurrentPlayer()
        {
            if (deckManager.TryDrawCard(out CardData drawnCard))
            {
                handManager.AddCardToHand(CurrentPlayerIndex, drawnCard);
                Debug.Log("Drew replacement card for Player 1: " + drawnCard.GetDisplayName());
            }
            else
            {
                Debug.LogWarning("Deck is empty. Player 1 did not draw a replacement card.");
            }

            Debug.Log("Remaining Deck Count: " + deckManager.RemainingCardCount);
        }

        private void RefreshCurrentPlayerHandView()
        {
            IReadOnlyList<CardData> currentPlayerHand = handManager.GetPlayerHand(CurrentPlayerIndex);
            handView.ShowCurrentPlayerHand(CurrentPlayerIndex, currentPlayerHand, SelectCurrentPlayerCard);
        }

        private void LogLocalGameStartState()
        {
            for (int playerIndex = 0; playerIndex < handManager.PlayerCount; playerIndex++)
            {
                Debug.Log("Player " + (playerIndex + 1) + " Hand: " + FormatHandForLog(handManager.GetPlayerHand(playerIndex)));
            }

            Debug.Log("Remaining Deck Count: " + deckManager.RemainingCardCount);
        }

        private void ValidateMvpCardCounts(string context)
        {
            Dictionary<string, int> cardCounts = new Dictionary<string, int>();
            int totalTrackedCards = 0;

            totalTrackedCards += CountCards(deckManager.RemainingCards, cardCounts);

            for (int playerIndex = 0; playerIndex < handManager.PlayerCount; playerIndex++)
            {
                totalTrackedCards += CountCards(handManager.GetPlayerHand(playerIndex), cardCounts);
            }

            totalTrackedCards += CountCards(usedCards, cardCounts);

            bool isValid = true;

            foreach (KeyValuePair<string, int> cardCount in cardCounts)
            {
                if (cardCount.Value > MvpCardRules.CopiesPerRegularCard)
                {
                    Debug.LogError("Card count validation failed (" + context + "): " + cardCount.Key + " count is " + cardCount.Value + ", expected at most " + MvpCardRules.CopiesPerRegularCard);
                    isValid = false;
                }
            }

            int expectedTotalCards = MvpCardRules.RegularCardTypeCount * MvpCardRules.CopiesPerRegularCard;

            if (totalTrackedCards != expectedTotalCards)
            {
                Debug.LogError("Card count validation failed (" + context + "): tracked card total is " + totalTrackedCards + ", expected " + expectedTotalCards);
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log("Card count validation passed (" + context + "): tracked " + totalTrackedCards + " cards, no card type exceeds " + MvpCardRules.CopiesPerRegularCard + " copies.");
            }
        }

        private static int CountCards(IReadOnlyList<CardData> cards, Dictionary<string, int> cardCounts)
        {
            int countedCards = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                CardData card = cards[i];

                if (!MvpCardRules.IsMvpRegularCard(card))
                {
                    continue;
                }

                string cardKey = MvpCardRules.GetCardKey(card);

                if (!cardCounts.ContainsKey(cardKey))
                {
                    cardCounts.Add(cardKey, 0);
                }

                cardCounts[cardKey]++;
                countedCards++;
            }

            return countedCards;
        }

        private static string FormatHandForLog(IReadOnlyList<CardData> handCards)
        {
            List<string> cardNames = new List<string>();

            for (int i = 0; i < handCards.Count; i++)
            {
                cardNames.Add(handCards[i].GetDisplayName());
            }

            return string.Join(", ", cardNames);
        }
    }
}
