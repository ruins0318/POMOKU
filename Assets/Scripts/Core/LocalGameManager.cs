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

        private BoardManager boardManager;
        private DeckManager deckManager;
        private HandManager handManager;
        private ScoreManager scoreManager;
        private BoardView boardView;
        private HandView handView;
        private CardData selectedCardData;
        private bool hasSelectedCard;
        private int currentPlayerIndex;
        private readonly List<CardData> usedCards = new List<CardData>();
        private readonly List<CompletedPomokuLine> completedPomokuLines = new List<CompletedPomokuLine>();
        private readonly HashSet<string> completedPomokuLineKeys = new HashSet<string>();

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
            scoreManager.ResetScores();
            CreateAndShowBoard();
            CreateDeckAndDealHands();
            LogLocalGameStartState();
            ValidateMvpCardCounts("initial deal");
            StartCurrentPlayerTurn();
        }

        private void ResetLocalGameState()
        {
            usedCards.Clear();
            completedPomokuLines.Clear();
            completedPomokuLineKeys.Clear();
            currentPlayerIndex = 0;
            hasSelectedCard = false;
            selectedCardData = default(CardData);
        }

        private void CreateManagers()
        {
            boardManager = gameObject.AddComponent<BoardManager>();
            deckManager = gameObject.AddComponent<DeckManager>();
            handManager = gameObject.AddComponent<HandManager>();
            scoreManager = gameObject.AddComponent<ScoreManager>();
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
            IReadOnlyList<CardData> currentPlayerHand = handManager.GetPlayerHand(currentPlayerIndex);
            handView.ShowCurrentPlayerHand(currentPlayerIndex, currentPlayerHand, SelectCurrentPlayerCard);
        }

        private void SelectCurrentPlayerCard(CardData cardData)
        {
            selectedCardData = cardData;
            hasSelectedCard = true;

            Debug.Log(GetPlayerDisplayName(currentPlayerIndex) + " selected card: " + selectedCardData.GetDisplayName());
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
            TeamId currentTeam = GetTeamForPlayer(currentPlayerIndex);
            string currentPlayerName = GetPlayerDisplayName(currentPlayerIndex);
            string currentTeamName = GetTeamDisplayName(currentTeam);

            if (!handManager.RemoveFirstMatchingCardFromHand(currentPlayerIndex, usedCard))
            {
                Debug.LogWarning("Could not remove used card from " + currentPlayerName + " hand: " + usedCard.GetDisplayName());
                return;
            }

            boardView.ShowChipAtCell(cellIndex, currentTeam);
            usedCards.Add(usedCard);

            Debug.Log("Placed " + currentTeamName + " chip on " + cellData.Card.GetDisplayName() + " at cell index " + cellIndex);
            Debug.Log(currentPlayerName + " used card: " + usedCard.GetDisplayName());

            CheckPomokuAfterChipPlacement(cellIndex, currentTeam);
            DrawReplacementCardForCurrentPlayer();

            hasSelectedCard = false;
            selectedCardData = default(CardData);
            boardView.ClearHighlightedCells();
            handView.ClearSelectedCard();
            ValidateMvpCardCounts("after " + currentPlayerName + " card use");
            EndCurrentPlayerTurn(currentPlayerName);
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
                handManager.AddCardToHand(currentPlayerIndex, drawnCard);
                Debug.Log("Drew replacement card for " + GetPlayerDisplayName(currentPlayerIndex) + ": " + drawnCard.GetDisplayName());
            }
            else
            {
                Debug.LogWarning("Deck is empty. " + GetPlayerDisplayName(currentPlayerIndex) + " did not draw a replacement card.");
            }

            Debug.Log("Remaining Deck Count: " + deckManager.RemainingCardCount);
        }

        private void CheckPomokuAfterChipPlacement(int placedCellIndex, TeamId teamId)
        {
            PomokuLineResult pomokuResult = PomokuChecker.CheckForPomoku(
                boardManager.BoardCells,
                BoardManager.BoardSize,
                placedCellIndex,
                teamId);

            if (!pomokuResult.IsCompleted)
            {
                Debug.Log("No Pomoku completed");
                return;
            }

            RegisterCompletedPomokuLine(pomokuResult);
        }

        private void RegisterCompletedPomokuLine(PomokuLineResult pomokuResult)
        {
            string pomokuLineKey = CreatePomokuLineKey(pomokuResult.CellIndices);

            if (completedPomokuLineKeys.Contains(pomokuLineKey))
            {
                Debug.Log("Pomoku already registered: " + pomokuLineKey);
                return;
            }

            CompletedPomokuLine completedPomokuLine = new CompletedPomokuLine(
                pomokuResult.TeamId,
                new List<int>(pomokuResult.CellIndices),
                pomokuResult.DirectionName,
                pomokuResult.ContainsAnchorJari);

            completedPomokuLines.Add(completedPomokuLine);
            completedPomokuLineKeys.Add(pomokuLineKey);

            boardView.LockPomokuLine(completedPomokuLine.CellIndices, completedPomokuLine.TeamId);

            Debug.Log("Registered Pomoku Line #" + completedPomokuLines.Count);
            Debug.Log("Team: " + GetTeamDisplayName(completedPomokuLine.TeamId));
            Debug.Log("Direction: " + completedPomokuLine.DirectionName);
            Debug.Log("Cells: " + FormatCellIndicesForLog(completedPomokuLine.CellIndices));

            if (completedPomokuLine.ContainsAnchorJari)
            {
                Debug.Log("Contains AnchorJari: true");
            }

            AddScoreForCompletedPomoku(completedPomokuLine);
        }

        private void AddScoreForCompletedPomoku(CompletedPomokuLine completedPomokuLine)
        {
            const int mvpPomokuScore = 1;

            scoreManager.AddScore(completedPomokuLine.TeamId, mvpPomokuScore);
            Debug.Log(GetTeamDisplayName(completedPomokuLine.TeamId) + " scored " + mvpPomokuScore + " point");
            LogCurrentScore();
            UpdateScoreHud();
        }

        private void LogCurrentScore()
        {
            Debug.Log("Current Score - TeamA: " + scoreManager.GetScore(TeamId.TeamA) + " / TeamB: " + scoreManager.GetScore(TeamId.TeamB));
        }

        private void RefreshCurrentPlayerHandView()
        {
            ShowCurrentPlayerHand();
        }

        private void StartCurrentPlayerTurn()
        {
            hasSelectedCard = false;
            selectedCardData = default(CardData);

            if (boardView != null)
            {
                boardView.ClearHighlightedCells();
            }

            if (handView != null)
            {
                handView.ClearSelectedCard();
                RefreshCurrentPlayerHandView();
                handView.ShowCurrentTurn(currentPlayerIndex, GetTeamForPlayer(currentPlayerIndex));
                UpdateScoreHud();
            }

            Debug.Log("Turn started: " + GetPlayerDisplayName(currentPlayerIndex) + " / " + GetTeamDisplayName(GetTeamForPlayer(currentPlayerIndex)));
        }

        private void UpdateScoreHud()
        {
            if (handView == null || scoreManager == null)
            {
                return;
            }

            handView.ShowScores(scoreManager.GetScore(TeamId.TeamA), scoreManager.GetScore(TeamId.TeamB));
        }

        private void EndCurrentPlayerTurn(string previousPlayerName)
        {
            AdvanceToNextPlayer();
            Debug.Log("Turn ended: " + previousPlayerName + ". Next player: " + GetPlayerDisplayName(currentPlayerIndex) + " / " + GetTeamDisplayName(GetTeamForPlayer(currentPlayerIndex)));
            StartCurrentPlayerTurn();
        }

        private void AdvanceToNextPlayer()
        {
            currentPlayerIndex++;

            if (currentPlayerIndex >= LocalPlayerCount)
            {
                currentPlayerIndex = 0;
            }
        }

        private static TeamId GetTeamForPlayer(int playerIndex)
        {
            if (playerIndex == 0 || playerIndex == 2)
            {
                return TeamId.TeamA;
            }

            return TeamId.TeamB;
        }

        private static string GetPlayerDisplayName(int playerIndex)
        {
            return "Player " + (playerIndex + 1);
        }

        private static string GetTeamDisplayName(TeamId teamId)
        {
            switch (teamId)
            {
                case TeamId.TeamA:
                    return "TeamA";
                case TeamId.TeamB:
                    return "TeamB";
                case TeamId.TeamC:
                    return "TeamC";
                default:
                    return "None";
            }
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

        private static string FormatCellIndicesForLog(IReadOnlyList<int> cellIndices)
        {
            List<string> cellIndexTexts = new List<string>();

            for (int i = 0; i < cellIndices.Count; i++)
            {
                cellIndexTexts.Add(cellIndices[i].ToString());
            }

            return string.Join(", ", cellIndexTexts);
        }

        private static string CreatePomokuLineKey(IReadOnlyList<int> cellIndices)
        {
            List<int> sortedCellIndices = new List<int>();

            for (int i = 0; i < cellIndices.Count; i++)
            {
                sortedCellIndices.Add(cellIndices[i]);
            }

            sortedCellIndices.Sort();

            List<string> cellIndexTexts = new List<string>();

            for (int i = 0; i < sortedCellIndices.Count; i++)
            {
                cellIndexTexts.Add(sortedCellIndices[i].ToString());
            }

            return string.Join("-", cellIndexTexts);
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
