# Yawordle — A Modern Word Puzzle Game for Mobile 🧩📱

Yet Another Wordle, crafted with **clean architecture**, a smooth **UI Toolkit** user experience, and optional backend integration via **Unity Gaming Services**. This project serves as a portfolio piece demonstrating best practices in modern Unity development.

- **Engine**: Unity 6000.2
- **Platform**: Android
- **UI**: UI Toolkit + PrimeTween animations
- **Architecture**: Clean Architecture with MVVM
- **DI Container**: VContainer
- **Async**: UniTask

![Gameplay Screenshot](docs/screenshots/gameplay.png)

---

## Table of Contents
- [Features](#features-)
- [Tech Stack & Architecture](#tech-stack--architecture-️)
- [Project Structure](#project-structure-)
- [Getting Started](#getting-started-)
- [Backend Deep Dive](#backend-deep-dive--cloud-code-)
- [How It Works](#how-it-works-)
- [Roadmap](#roadmap-)
- [License](#license-)
- [Acknowledgements](#acknowledgements-)

---

## Features 🎯
- **Two Game Modes**:
  - **Daily**: Server-driven “Word of the Day” powered by Unity Cloud Code, with robust offline fallback.
  - **Unlimited**: Endless practice with local dictionaries.
- **Multi-language Support**: Fully localized for English and Polish, with an architecture ready for more languages.
- **Modern & Responsive UI**: Built entirely with UI Toolkit, featuring a Safe Area component and a scalable design system (`tokens.uss`).
- **"Juicy" Animations**: Smooth tile flips, invalid-guess shakes, and non-intrusive toast notifications, all powered by PrimeTween.
- **Configurable Gameplay**: In-game settings panel to change language and game mode.

---

## Tech Stack & Architecture 🏗️

This project is a practical implementation of **Clean Architecture** principles, adapted for a Unity environment and combined with the **MVVM** pattern for the UI.

```
+-------------------------------------------------+
|               Presentation (MVVM)               |  <- UI Layer (UI Toolkit, Views, ViewModels)
|-------------------------------------------------|
|               Infrastructure                    |  <- "Dirty" Details (UGS, File I/O, Unity APIs)
|-------------------------------------------------|
|                     Core                        |  <- Pure Game Logic (Unity-agnostic)
+-------------------------------------------------+
|               DI (VContainer)                   |  <- Composition Root
+-------------------------------------------------+
```

- **Core**: Contains pure, platform-agnostic game logic and interfaces (`IGameManager`, `IWordProvider`). It has no dependencies on Unity, making it highly testable and portable.
- **Infrastructure**: Implements the Core interfaces. This is where all platform-specific code lives, such as saving settings to JSON, loading dictionaries from `Resources`, and communicating with Unity Gaming Services.
- **Presentation (MVVM)**: The UI layer. "Dumb" Views (UXML/USS) are controlled by ViewModels, which adapt data from the Core layer for presentation. This separation ensures that game logic knows nothing about the UI.
- **VContainer**: Acts as the Composition Root, wiring up all dependencies with clearly defined lifetimes (`Singleton`, `Transient`), ensuring a loosely coupled and maintainable codebase.

---

## Project Structure 📦
```
Assets/_Yawordle
  Scripts/
    Core/            # Game logic and interfaces (UI-agnostic)
    Infrastructure/  # UGS, JSON services, ResourceWordProvider
    DI/              # VContainer LifetimeScope and GameInitializer
    Presentation/
      ViewModels/    # MVVM layer connecting Core to Views
      Views/         # C# controllers for UI Toolkit Views
  Resources/
    Dictionaries/    # Local word lists for Unlimited mode
  UI/
    Screens/         # GameScreen (UXML/USS)
    Panels/          # Reusable modals: Settings, Instructions, EndGame
    Components/      # Shared components like modal styles
    Themes/          # tokens.uss: The single source of truth for design
```

---

## Getting Started 🚀

- **Prerequisites**: Unity 6000.2 or newer.
- **Clone**: `git clone https://github.com/hanejkopawel/yawordle.git`
- **Open**: Open the project via Unity Hub, load `Assets/Scenes/MainScene.unity`, and press Play.

The UI is configured for portrait mode using `Scale With Screen Size` (reference 1080x1920, Match Width).

---

## Backend Deep Dive (Cloud Code) ☁️

The "Daily" mode is powered by a robust **JavaScript Cloud Code function** that demonstrates several advanced concepts.

- **Function Name**: `getWordOfTheDay`
- **Client Sends**: `{ language, wordLength }`
- **Server Responds**: `{ word, date, dictVersion }`

### Key Features of the Cloud Script:

1.  **Remote Dictionaries**: The script fetches word lists from `answers.json` files hosted on GitHub Pages, making it easy to update dictionaries without a new app build.
2.  **Server-Side Caching**: It uses an in-memory cache with a 1-hour TTL to minimize HTTP requests to the dictionary source, ensuring scalability and low latency.
3.  **Deterministic Selection**: The word of the day is chosen based on the day of the year, guaranteeing all players get the same word.
4.  **Robust Fallback**: If the remote dictionary is unavailable, the script gracefully falls back to a hardcoded word list, ensuring the game always works.

**To deploy the script:**
1.  Link your project to Unity Gaming Services.
2.  In the UGS Dashboard, create a new Cloud Code script named `getWordOfTheDay`.
3.  Copy the contents of `tools/getWordOfTheDay.js` (the more advanced version) into the script and deploy.


---

## How It Works 🔧

- **Startup**: `GameLifetimeScope` (VContainer) registers all services. `GameInitializer` is then started, which fetches the target word (from Cloud Code for "Daily" or a local provider for "Unlimited") and starts the `GameManager`.
- **Gameplay**: Player input is passed to the `GameBoardViewModel`, which delegates to the `IGameManager`. The `GameManager` validates the guess and evaluates the letters.
- **UI Updates**: `GameBoardViewModel` listens to events from the `GameManager` and updates the state of `TileViewModel` and `KeyViewModel`. The `GameScreenView` observes these changes and plays the appropriate animations.
- **Data Persistence**: Player settings are serialized to JSON and stored in `Application.persistentDataPath`.

---

## Roadmap 🧭

### 1.0 (Play Store-ready)
- [x] Full UI localization (EN/PL)
- [x] Dynamic backend for daily word (Cloud Code, remote dictionaries)
- [ ] **Player Statistics**: Track wins, streaks, and guess distribution (local storage).
- [ ] **Sound Effects & Haptics**: Add audio-visual feedback for key interactions.
- [ ] **Light/Dark Themes**: Leverage `tokens.uss` to easily add theme switching.
- [ ] **Shareable Results**: Implement a "Share" button to copy emoji results to the clipboard.
- [ ] **App Icon & Store Graphics**.

### 1.1+ (Future Enhancements)
- [ ] **Hard Mode**: Enforce the use of revealed hints in subsequent guesses.
- [ ] **Addressables Integration**: Move dictionaries from `Resources` to Addressables for dynamic updates.
- [ ] **Interactive Tutorial**: A guided first-time user experience.
- [ ] **Achievements & Leaderboards**: Integrate with Google Play Games Services.
- [ ] **Monetization**: Investigate ads or other models.


---

## License 📝

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## Acknowledgements 💐

Special thanks to the creators of these fantastic open-source packages:

- [UniTask](https://github.com/Cysharp/UniTask) (Cysharp)
- [VContainer](https://github.com/hadashiA/VContainer) (hadashiA)
- [PrimeTween](https://github.com/KyryloKuzyk/PrimeTween) (Kyrylo Kuzyk)
- [UI Toolkit Safe Area](https://github.com/Bitbebop/Unity-UI-Toolkit-SafeArea) (Bitbebop)
