\# AGENTS.md



\## Project Overview



This repository contains \*\*POMOKU\*\*, a 2D strategic card board game made with Unity.



POMOKU combines playing cards with gomoku-style board placement.

Players use cards from their hand to place chips on matching card cells on a 10×10 board.

A team scores by connecting 5 chips horizontally, vertically, or diagonally.



The long-term goal of this project is to complete the game for \*\*PC / Steam release\*\*.



\## Current Engine and Target



\* Engine: Unity 6.4

\* Template: Universal 2D

\* Language: C#

\* Target Platform: PC / Steam

\* Version Control: Git



Important: Some older planning documents may mention Unreal Engine 5.6 or Unreal-specific class names.

Those documents should be treated as game design references only.

All implementation must be done in \*\*Unity C#\*\*.



\## Current Development Phase



The current phase is:



\*\*Phase 1: Local MVP\*\*



Do not implement online multiplayer, Steamworks, matchmaking, ranking, advanced VFX, sound, or full release features unless explicitly requested.



The current priority is to build a stable local rule core first.



\## Main Development Order



Follow this order unless the user explicitly asks otherwise:



1\. Project folder structure

2\. Card data structure

3\. Board cell data structure

4\. 10×10 board generation

5\. Board UI display

6\. Deck generation and shuffle

7\. Player hand distribution

8\. Card selection

9\. Valid board cell highlighting

10\. Chip placement

11\. Pomoku line check

12\. Basic scoring

13\. Turn progression

14\. Rule expansion

15\. UI / UX polish

16\. Online multiplayer

17\. Steam integration



Do not skip directly to multiplayer or Steam integration.



\## Important Game Rules



\### Board



\* The board is 10×10.

\* There are 100 cells total.

\* Normal cells contain card information.

\* Anchor Jari cells are special helper cells.

\* MVP may start with a simple random board.



\### Card Use



\* A player selects one card from their hand.

\* The player can place a chip only on a board cell matching the selected card.

\* A chip cannot be placed on an occupied cell.



\### Turn Flow



The basic turn flow is:



```text

Select card

→ Select board cell

→ Place chip

→ Check Pomoku

→ Calculate score

→ Draw card

→ End turn

```



\### Pomoku



A Pomoku is completed when 5 chips from the same team are connected in one of these directions:



\* Horizontal

\* Vertical

\* Diagonal top-left to bottom-right

\* Diagonal top-right to bottom-left



\## Repository Layout



Use this structure as the default organization.



```text

Assets/

├─ Scenes/

│  ├─ LobbyScene.unity

│  └─ GameScene.unity

│

├─ Scripts/

│  ├─ Core/

│  ├─ Cards/

│  ├─ Board/

│  ├─ Players/

│  └─ UI/

│

├─ Prefabs/

│  ├─ Board/

│  ├─ Cards/

│  ├─ Chips/

│  └─ UI/

│

├─ Sprites/

│  ├─ Cards/

│  ├─ Chips/

│  └─ UI/

│

└─ ScriptableObjects/

```



If the actual project structure differs, adapt to the existing structure instead of blindly replacing it.



\## Coding Style



Write code for a beginner Unity developer to understand.



\* Prefer clear names over short names.

\* Keep each class focused on one responsibility.

\* Avoid large “god classes.”

\* Add comments only when they explain why the code exists.

\* Do not add unnecessary design patterns.

\* Do not add complex architecture before the MVP needs it.

\* Do not use external packages unless the user approves them first.

\* Prefer Unity UI and basic C# before adding advanced frameworks.



\## Suggested Script Responsibilities



\### GameManager



Controls the overall local game flow.



\### BoardManager



Creates and stores the 10×10 board data.



\### BoardCellView



Displays one board cell and handles board cell click interaction.



\### Card



Stores card suit, rank, and joker information.



\### DeckManager



Creates, shuffles, and draws cards.



\### HandManager



Stores and manages player hands.



\### TurnManager



Tracks the current player and advances turns.



\### PomokuChecker



Checks whether a newly placed chip completes a Pomoku.



\### ScoreManager



Calculates and stores team scores.



\### UIManager



Coordinates HUD updates.



\## Unity Rules



\* Preserve `.meta` files.

\* Do not edit files inside `Library/`, `Temp/`, `Obj/`, `Logs/`, or build folders.

\* Do not commit generated Unity cache files.

\* Do not rename assets or folders unnecessarily.

\* When creating prefabs or scenes, explain what the user must connect in the Unity Editor.

\* If a scene or prefab cannot be fully created through code, provide clear manual setup steps.



\## Git Rules



\* Do not run `git add .` unless the user explicitly asks.

\* Do not commit unless the user explicitly asks.

\* Before finishing, summarize modified files.

\* Keep changes small and phase-based.



Good commit message examples:



```text

Add card data model

Add basic board generation

Add board cell UI

Add deck shuffle logic

Add local turn manager

```



\## Testing and Verification



There may not be automated tests yet.



When changing code:



1\. Check for C# compile errors.

2\. Explain how to test the feature in Unity Editor.

3\. If no automated tests exist, say so clearly.

4\. Do not claim a feature was tested unless it was actually tested.



For MVP features, verification should be practical.



Example:



```text

Open GameScene

Press Play

Confirm that a 10×10 board appears

Confirm that each cell displays card information

```



\## Do Not Do Yet



Do not implement these unless the user explicitly asks:



\* Online multiplayer

\* Steamworks integration

\* Steam lobby

\* Friend invite

\* Ranking

\* Matchmaking

\* Full tutorial

\* Advanced board generation

\* Full VFX polish

\* Sound system

\* Monetization systems



These are future phases, not abandoned features.



\## Response Requirements



After each task, respond with:



1\. What was changed

2\. Files created

3\. Files modified

4\. How to test in Unity

5\. What should be done next



If something is uncertain, say it clearly instead of guessing.



\## Current First Goal



The first implementation goal is:



```text

Create and display a 10×10 board in GameScene.

Each cell should contain basic card data.

Do not implement card selection, chip placement, scoring, turns, multiplayer, or Steam yet.

```



