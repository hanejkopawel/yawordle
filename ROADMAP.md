# Yawordle Project Roadmap

This document outlines the development roadmap for Yawordle, from the current state to a polished, store-ready product. Tasks are organized into logical phases.

## Phase 1: Core Experience Completion
*Goal: To make the game fully playable, understandable, and satisfying in a single-player mode.*

- [x] **Core Game Mechanic:** Implemented grid, typing, and evaluation logic.
- [x] **Flexible Settings:** Support for variable word lengths and multiple languages (PL/EN).
- [x] **Settings Panel:** Functional UI for changing language, word length, and game mode.
- [x] **"Daily Word" Mode:** Secure, server-authoritative daily word using UGS.
- [x] **"How to Play" Panel:** Instructions for new players, shown on first launch.
- [x] **End Game Panels:** Win/Loss panels with a "Play Again" option.
- [x] **Error Feedback:** Shake animation and toast notifications for invalid guesses.
- [x] **Keyboard Feedback:** On-screen keyboard keys update their color based on letter status.

- [ ] **Integrate with Complete Dictionaries:**
    - [ ] Find and prepare full word lists (solutions and valid guesses) for PL and EN.
    - [ ] Modify the `WordProvider` to handle two separate lists for better gameplay.

- [ ] **Implement Player Statistics:**
    - [ ] Create a `PlayerStats` model and a service for persistence (games played, win %, streaks, guess distribution).
    - [ ] Design and implement a UI panel to display player statistics.

- [ ] **Full UI Localization (i18n):**
    - [ ] Integrate the Unity Localization package.
    - [ ] Create String Tables for all UI text and connect them to the UI elements.

---

## Phase 2: Polish & Juiciness
*Goal: To make the game feel responsive, satisfying, and enjoyable to interact with.*

- [ ] **Sound Effects and Haptics:**
    - [ ] Implement an `AudioService` for key game events.
    - [ ] Add options in the settings panel to toggle sound and vibrations.

- [ ] **Visual Themes (Light/Dark Mode):**
    - [ ] Create alternative color themes (e.g., Light Mode, High Contrast).
    - [ ] Implement a theme switcher in the settings panel.

- [ ] **Shareable Results:**
    - [ ] Implement the "Share" button functionality on the end-game panel to generate an emoji grid.

- [ ] **Advanced Animations:**
    - [ ] Add minor animations for a "juicier" feel (e.g., popping letters on key press).
    - [ ] Refine all existing UI transitions.

---

## Phase 3: Gameplay Extension & Retention
*Goal: To increase replayability and offer new challenges to keep players engaged.*

- [ ] **Implement 'Hard Mode':**
    - [ ] Add a "Hard Mode" toggle in settings.
    - [ ] Implement validation logic in `GameManager` to enforce hard mode rules.

- [ ] **Achievement System:**
    - [ ] Design a list of achievements.
    - [ ] Implement a system for tracking and awarding achievements.
    - [ ] (Optional) Integrate with Game Center / Google Play Games.

- [ ] **Hint System:**
    - [ ] Design and implement a hint mechanic (e.g., "reveal a letter").
    - [ ] (Optional) Link hints to a rewarded ad monetization strategy.

---

## Phase 4: Production & LiveOps
*Goal: To prepare the project for a store release and future maintenance.*

- [ ] **Refactor to Addressables:**
    - [ ] Migrate all dictionary files from the `Resources` folder to the Addressables system.

- [ ] **Global Leaderboards:**
    - [ ] Integrate with UGS Leaderboards for the "Daily" mode.
    - [ ] Create a UI panel to display rankings.

- [ ] **Analytics:**
    - [ ] Integrate Unity Analytics to track key metrics (e.g., game completion, mode popularity).

- [ ] **Monetization:**
    - [ ] Integrate an ad network (e.g., Unity Ads).
    - [ ] Design and implement ad placements.

- [ ] **Optimization and Testing:**
    - [ ] Profile performance on target mobile devices.
    - [ ] Test on a variety of screen sizes and aspect ratios.

- [ ] **Store Presence:**
    - [ ] Create the application icon.
    - [ ] Prepare screenshots and promotional materials.
    - [ ] Write the store page descriptions and privacy policy.
