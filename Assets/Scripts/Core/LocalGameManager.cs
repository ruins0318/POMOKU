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
            CreateManagers();
            CreateAndShowBoard();
            CreateDeckAndDealHands();
            ShowCurrentPlayerHand();
            LogLocalGameStartState();
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

            boardView.ShowChipAtCell(cellIndex, currentPlayerTeam);

            Debug.Log("Placed TeamA chip on " + cellData.Card.GetDisplayName() + " at cell index " + cellIndex);

            hasSelectedCard = false;
            selectedCardData = default(CardData);
            boardView.ClearHighlightedCells();
            handView.ClearSelectedCard();
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

        private void LogLocalGameStartState()
        {
            for (int playerIndex = 0; playerIndex < handManager.PlayerCount; playerIndex++)
            {
                Debug.Log("Player " + (playerIndex + 1) + " Hand: " + FormatHandForLog(handManager.GetPlayerHand(playerIndex)));
            }

            Debug.Log("Remaining Deck Count: " + deckManager.RemainingCardCount);
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
