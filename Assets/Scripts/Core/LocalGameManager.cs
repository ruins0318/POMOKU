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
        private DebugPanelView debugPanelView;
        private CardData selectedCardData;
        private bool hasSelectedCard;
        private int currentPlayerIndex;
        private LocalPlayMode currentPlayMode = LocalPlayMode.NormalFourPlayer;
        private BoardGenerationMode currentBoardGenerationMode = BoardGenerationMode.RandomMvp;
        private HandGenerationMode currentHandGenerationMode = HandGenerationMode.RandomHands;
        private readonly List<CardData> usedCards = new List<CardData>();
        private readonly List<CardData> debugScoringReservedDrawCards = new List<CardData>();
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
            EnsureManagers();
            ShowDebugPanel();
            StartNewLocalGame(LocalPlayMode.NormalFourPlayer, BoardGenerationMode.RandomMvp, HandGenerationMode.RandomHands);
        }

        public void StartNewLocalGame(LocalPlayMode localPlayMode, BoardGenerationMode boardGenerationMode, HandGenerationMode handGenerationMode)
        {
            EnsureManagers();

            currentPlayMode = localPlayMode;
            currentBoardGenerationMode = boardGenerationMode;
            currentHandGenerationMode = handGenerationMode;

            Debug.Log("Starting local game mode: " + currentPlayMode + " / " + currentBoardGenerationMode + " / " + currentHandGenerationMode);

            ResetLocalGameState();
            scoreManager.ResetScores();
            CreateAndShowBoard();
            CreateDeckAndDealHands();
            LogLocalGameStartState();
            ValidateMvpCardCounts("initial deal");
            StartCurrentPlayerTurn();
        }

        public void RestartCurrentMode()
        {
            StartNewLocalGame(currentPlayMode, currentBoardGenerationMode, currentHandGenerationMode);
        }

        private void ResetLocalGameState()
        {
            usedCards.Clear();
            debugScoringReservedDrawCards.Clear();
            completedPomokuLines.Clear();
            completedPomokuLineKeys.Clear();
            currentPlayerIndex = 0;
            hasSelectedCard = false;
            selectedCardData = default(CardData);
        }

        private void EnsureManagers()
        {
            if (boardManager == null)
            {
                boardManager = gameObject.AddComponent<BoardManager>();
            }

            if (deckManager == null)
            {
                deckManager = gameObject.AddComponent<DeckManager>();
            }

            if (handManager == null)
            {
                handManager = gameObject.AddComponent<HandManager>();
            }

            if (scoreManager == null)
            {
                scoreManager = gameObject.AddComponent<ScoreManager>();
            }

            if (boardView == null)
            {
                boardView = gameObject.AddComponent<BoardView>();
            }

            if (handView == null)
            {
                handView = gameObject.AddComponent<HandView>();
            }

            if (debugPanelView == null)
            {
                debugPanelView = gameObject.AddComponent<DebugPanelView>();
            }
        }

        private void ShowDebugPanel()
        {
            debugPanelView.Show(
                StartNormalFourPlayerRandomGame,
                StartDebugScoringTestGame,
                RestartCurrentMode);
        }

        private void StartNormalFourPlayerRandomGame()
        {
            StartNewLocalGame(LocalPlayMode.NormalFourPlayer, BoardGenerationMode.RandomMvp, HandGenerationMode.RandomHands);
        }

        private void StartDebugScoringTestGame()
        {
            StartNewLocalGame(LocalPlayMode.DebugPlayer1Only, BoardGenerationMode.TestScoringPreset, HandGenerationMode.TestScoringHands);
        }

        private void CreateAndShowBoard()
        {
            boardManager.CreateBoard(currentBoardGenerationMode);
            boardView.ShowBoard(boardManager.BoardCells, BoardManager.BoardSize, TryPlaceChipOnBoardCell);
        }

        private void CreateDeckAndDealHands()
        {
            deckManager.CreateMvpDeck();
            handManager.CreatePlayerHands(LocalPlayerCount);

            if (currentHandGenerationMode == HandGenerationMode.TestScoringHands)
            {
                CreateTestScoringHands();
                return;
            }

            deckManager.ShuffleDeck();
            handManager.DealInitialHands(deckManager, InitialHandSize);
        }

        private void CreateTestScoringHands()
        {
            AddTestCardToPlayerHand(0, Suit.Spade, Rank.Seven);
            AddTestCardToPlayerHand(0, Suit.Spade, Rank.Eight);
            AddTestCardToPlayerHand(0, Suit.Spade, Rank.Nine);
            AddTestCardToPlayerHand(0, Suit.Spade, Rank.Ten);
            AddTestCardToPlayerHand(0, Suit.Spade, Rank.Queen);
            AddTestCardToPlayerHand(0, Suit.Heart, Rank.Two);

            List<CardData> testDrawCards = new List<CardData>
            {
                CreateTestCard(Suit.Heart, Rank.Five),
                CreateTestCard(Suit.Heart, Rank.Eight),
                CreateTestCard(Suit.Heart, Rank.Ten),
                CreateTestCard(Suit.Heart, Rank.Ace),
                CreateTestCard(Suit.Club, Rank.Three),
                CreateTestCard(Suit.Diamond, Rank.Three),
                CreateTestCard(Suit.Spade, Rank.Three),
                CreateTestCard(Suit.Heart, Rank.Seven),
                CreateTestCard(Suit.Diamond, Rank.Nine),
                CreateTestCard(Suit.Club, Rank.Four),
                CreateTestCard(Suit.Diamond, Rank.Four),
                CreateTestCard(Suit.Spade, Rank.Eight),
                CreateTestCard(Suit.Heart, Rank.Eight),
                CreateTestCard(Suit.Club, Rank.Ace),
                CreateTestCard(Suit.Club, Rank.Five),
                CreateTestCard(Suit.Club, Rank.Six),
                CreateTestCard(Suit.Club, Rank.Seven),
                CreateTestCard(Suit.Club, Rank.Eight)
            };

            ReserveDebugScoringDrawCards(testDrawCards);

            Debug.Log("Debug Scoring Test Mode Started");
            Debug.Log("Straight test target: cells 10, 11, 12, 13, 14");
            Debug.Log("Suggested play order: S7, S8, S9, S10, SQ");
            Debug.Log("TestScoringHands Player 1 initial hand: S7, S8, S9, S10, SQ, H2");
            Debug.Log("TestScoringHands reserved draw order: " + FormatHandForLog(debugScoringReservedDrawCards));
        }

        private void AddTestCardToPlayerHand(int playerIndex, Suit suit, Rank rank)
        {
            CardData requestedCard = CreateTestCard(suit, rank);

            if (deckManager.TryRemoveFirstMatchingCard(requestedCard, out CardData removedCard))
            {
                handManager.AddCardToHand(playerIndex, removedCard);
            }
            else
            {
                Debug.LogError("TestScoringHands failed to add card to hand: " + requestedCard.GetDisplayName());
            }
        }

        private void ReserveDebugScoringDrawCards(List<CardData> testDrawCards)
        {
            for (int i = 0; i < testDrawCards.Count; i++)
            {
                CardData requestedCard = testDrawCards[i];

                if (deckManager.TryRemoveFirstMatchingCard(requestedCard, out CardData removedCard))
                {
                    debugScoringReservedDrawCards.Add(removedCard);
                }
                else
                {
                    Debug.LogError("TestScoringHands failed to prepare draw card: " + requestedCard.GetDisplayName());
                }
            }
        }

        private static CardData CreateTestCard(Suit suit, Rank rank)
        {
            return new CardData(suit, rank, false);
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
                Debug.Log("Cannot place chip: no card selected");
                return false;
            }

            if (cellData == null)
            {
                Debug.Log("Cannot place chip: cell data is missing at cell index " + cellIndex);
                return false;
            }

            if (cellData.CellType == BoardCellType.AnchorJari)
            {
                Debug.Log("Cannot place chip: AnchorJari is not directly placeable at cell index " + cellIndex);
                return false;
            }

            if (cellData.CellType != BoardCellType.Normal)
            {
                Debug.Log("Cannot place chip: cell is not a normal card cell at cell index " + cellIndex);
                return false;
            }

            if (cellData.ChipOwnerTeam != TeamId.None)
            {
                Debug.Log("Cannot place chip: cell already occupied at cell index " + cellIndex);
                return false;
            }

            if (cellData.Card.Suit != selectedCardData.Suit || cellData.Card.Rank != selectedCardData.Rank)
            {
                Debug.Log("Cannot place chip: selected card " + selectedCardData.GetDisplayName() + " does not match cell " + cellData.Card.GetDisplayName() + " at cell index " + cellIndex);
                return false;
            }

            if (!boardView.IsCellHighlighted(cellIndex))
            {
                Debug.Log("Cannot place chip: matching cell is not highlighted at cell index " + cellIndex);
                return false;
            }

            return true;
        }

        private void DrawReplacementCardForCurrentPlayer()
        {
            if (TryDrawDebugScoringReservedCard(out CardData debugDrawnCard))
            {
                handManager.AddCardToHand(currentPlayerIndex, debugDrawnCard);
                Debug.Log("Drew debug scoring replacement card for " + GetPlayerDisplayName(currentPlayerIndex) + ": " + debugDrawnCard.GetDisplayName());
                Debug.Log("Debug Scoring Reserved Draw Count: " + debugScoringReservedDrawCards.Count);
                Debug.Log("Remaining Deck Count: " + deckManager.RemainingCardCount);
                return;
            }

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

        private bool TryDrawDebugScoringReservedCard(out CardData drawnCard)
        {
            if (currentHandGenerationMode != HandGenerationMode.TestScoringHands || debugScoringReservedDrawCards.Count == 0)
            {
                drawnCard = default(CardData);
                return false;
            }

            drawnCard = debugScoringReservedDrawCards[0];
            debugScoringReservedDrawCards.RemoveAt(0);
            return true;
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
            if (currentPlayMode == LocalPlayMode.DebugPlayer1Only)
            {
                currentPlayerIndex = 0;
                Debug.Log("DebugPlayer1Only mode: keeping turn on Player 1.");
                return;
            }

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
            totalTrackedCards += CountCards(debugScoringReservedDrawCards, cardCounts);

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
