# Yawordle — A Modern Word Puzzle Game for Mobile 🧩📱

Yet Another Wordle, crafted with clean architecture, smooth UI Toolkit UX, and optional backend integration via Unity Gaming Services.

- Engine: Unity 6000.2.4f1
- Platform: Android (portrait-only)
- UI: UI Toolkit + PrimeTween animations
- DI: VContainer
- Async: UniTask

![Gameplay Screenshot](docs/screenshots/gameplay.png)
<!-- Tip: Replace the path above with your actual screenshot or GIF -->

---

## Table of Contents
- [Features](#features-)
- [Tech Stack & Architecture](#tech-stack--architecture-️)
- [Project Structure](#project-structure-)
- [Getting Started](#getting-started-)
- [Backend (optional)](#backend-optional-)
- [How It Works](#how-it-works-)
- [Roadmap](#roadmap-)
- [Contributing](#contributing-)
- [License](#license-)
- [Acknowledgements](#acknowledgements-)
- [Known Issues](#known-issues-️)

---

## Features 🎯
- Two Game Modes
  - Daily: Server-driven “Word of the Day” (Unity Cloud Code) with offline fallback
  - Unlimited: Endless practice with local dictionaries
- Multi-language dictionaries: English and Polish
- Responsive, touch-first UI (UI Toolkit + Safe Area)
- Smooth animations: tile flips, invalid-guess shake, toasts (PrimeTween)
- Settings: language and game mode
- Note: word length is currently fixed at 5 (difficulty by length is not used)

---

## Tech Stack & Architecture 🏗️

- Core Technologies
  - UI Toolkit: modern, responsive UI for mobile
  - PrimeTween: performant UI animations and sequences
  - VContainer: dependency injection with clear lifetimes
  - UniTask: allocation-friendly async/await for Unity
  - Unity Gaming Services: Authentication, Cloud Code (optional), Analytics (planned minimal set)

- Architecture (MVVM-style)
  - Model (Core)
    - Game rules and contracts (IGameManager, IWordProvider, ISettingsService)
    - GameManager validates guesses and evaluates letters
  - ViewModel (Presentation)
    - GameBoardViewModel, TileViewModel, KeyViewModel, SettingsViewModel
    - Prepares UI data and reacts to GameManager events
  - View (UI Toolkit)
    - “Dumb” Views: build UI, bind to ViewModels, play animations

---

## Project Structure 📦
```
Assets/_Yawordle
  Scripts/
    Core/            # game logic and interfaces (UI-agnostic)
    Infrastructure/  # UGS, JSON save, Resources word provider, keyboard layouts
    DI/              # VContainer LifetimeScope and startup
    Presentation/
      ViewModels/    # MVVM layer for UI
      Views/         # UI Toolkit views, binders, animations
  Resources/
    Dictionaries/
      solutions_en_5.txt
      guesses_en_5.txt
      solutions_pl_5.txt
      guesses_pl_5.txt
  UI/
    Screens/         # GameScreen (UXML/USS)
    Panels/          # Settings, Instructions, EndGame (UXML/USS)
    Components/      # Modal helpers
    Themes/          # tokens.uss (colors, sizes)
Scenes/
  MainScene.unity
```

---

## Getting Started 🚀

- Prerequisites
  - Unity 6000.2.4f1 (or newer 6.x)
  - Git LFS recommended for large assets

- Clone
  ```bash
  git clone https://github.com/hanejkopawel/yawordle.git
  ```

- Open in Unity
  - Open via Unity Hub
  - Load Assets/Scenes/MainScene.unity
  - Press Play

- Platform & UI
  - Android, portrait-only
  - Panel Settings: Scale With Screen Size, reference 1080x1920, Match = Width (0)

---

## Backend (optional) ☁️

- Unity Gaming Services
  - Link your project in Edit → Project Settings → Services
  - Enable Authentication and Cloud Code

- Cloud Code (JavaScript)
  - Create a function named `getWordOfTheDay`
  - The client sends: `{ language, wordLength }`
  - The function returns: `{ word, date }`

  Example script:
  ```js
  const _ = require('lodash-4.17');

  module.exports = async ({ params, context, logger }) => {
    const language = params.language || 'en';
    const wordLength = params.wordLength || 5;

    const dictionaries = {
      'en_5': ['UNITY','CLOUD','PROXY','GRIDS','ALPHA','BRAVO','HOTEL'],
      'pl_5': ['CHMURA','KODER','SKRYPT','PANEL','ALFABET','GRACZ'],
      'en_4': ['CODE','GAME','TEST','VIEW'],
      'pl_4': ['KODU','GRAJ','TEST'],
      'en_6': ['ACTIVE','BUTTON','RENDER','SCRIPT'],
      'pl_6': ['SKRYPT','WIDOKU','PANELU','GRACZA'],
    };

    const key = `${language}_${wordLength}`;
    const wordList = dictionaries[key] || dictionaries['en_5'];
    if (!wordList?.length) {
      logger.error(`Word list not found or empty for key: ${key}`);
      throw new Error('Word list not found.');
    }

    const now = new Date();
    const serverDateUTC = now.toISOString().slice(0, 10);
    const startOfYear = new Date(now.getUTCFullYear(), 0, 0);
    const oneDay = 1000 * 60 * 60 * 24;
    const dayOfYear = Math.floor((now - startOfYear) / oneDay);
    const word = wordList[dayOfYear % wordList.length];

    logger.info(`Selected word: ${word} for ${serverDateUTC}`);
    return { word, date: serverDateUTC };
  };
  ```

- Client behavior
  - Daily Mode: fetches word from Cloud Code; falls back to a local random solution if unavailable
  - Unlimited Mode: local random solution

---

## How It Works 🔧

- Startup
  - VContainer wires Core/Infrastructure/Presentation
  - GameInitializer selects the target word (Daily via Cloud Code or Unlimited via local lists)

- Gameplay
  - Player inputs letters (UI keyboard or hardware keyboard)
  - GameManager validates guesses and evaluates letters in two passes (Correct/Present/Absent)
  - ViewModels update Tile/Key states; Views play flip/shake/toast animations
  - EndGame modal appears on win/lose

- Data
  - Settings stored as JSON (persistentDataPath)
  - Dictionaries loaded from Resources (planned: Addressables + CCD)

---

## Roadmap 🧭

- 1.0 (Play Store-ready)
  - [x] Full UI localization (EN/PL)
  - [ ] Dynamic backend for daily word (Cloud Code, remote dictionaries)
  - [ ] Player statistics (local)
  - [ ] Sound effects and basic haptics
  - [ ] Light/Dark themes
  - [ ] Shareable results
  - [ ] Minimal analytics (UGS)
  - [ ] App icon and store graphics

- 1.1+
  - [ ] Hard Mode (use revealed hints in subsequent guesses)
  - [ ] Addressables/CCD word packs (easy language updates post-release)
  - [ ] Interactive tutorial
  - [ ] Global leaderboards (UGS)
  - [ ] Achievements (Google Play Games)
  - [ ] Ads (TBD)
  - [ ] Performance and build size optimization

---

## Contributing 🤝

- Code Style
  - MVVM-style separation
  - Prefer async with UniTask in Unity layers
  - Keep Core minimal and UI-agnostic

---

## License 📝

- MIT 

---

## Acknowledgements 💐

- UniTask (Cysharp)
- VContainer (hadashiA)
- PrimeTween (Kyrylo Kuzyk)
- UI Toolkit Safe Area (Bitbebop)

---

## Known Issues ⚠️

- Word length is currently fixed at 5
- Dictionaries are loaded from Resources (planned: Addressables/CCD)
- Initial Cloud Code sample uses a static list (to be replaced by remote dictionaries)
