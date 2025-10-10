# Yawordle - A Modern Word Puzzle Game

![Yawordle Gameplay Screenshot](https://via.placeholder.com/800x450.png?text=Gameplay+Screenshot+Here)
<!-- TODO: Zastąp powyższy link zrzutem ekranu z Twojej gry! -->

**Yawordle** (Yet Another Wordle) is a polished and feature-rich word puzzle game built with Unity. This project serves as a portfolio piece to demonstrate modern development practices for mobile games, including clean architecture, responsive UI, and backend integration. The game is designed to be highly flexible, supporting multiple languages and variable word lengths.

## 🌟 Key Features

*   **Dynamic Word Length:** Unlike the original, Yawordle allows players to choose the word length (from 4 to 7 letters), which also serves as a difficulty setting.
*   **Multi-language Support:** The game is fully localized (UI and dictionaries) for English and Polish, with an architecture ready to support more languages.
*   **Two Game Modes:**
    *   **Daily Challenge:** A single, shared puzzle for everyone in the world each day, powered by a secure server-side logic.
    *   **Unlimited Mode:** Play as many puzzles as you want for endless practice.
*   **Responsive UI:** Built with Unity's UI Toolkit, the interface gracefully adapts to various screen sizes and aspect ratios, from phones to tablets.
*   **Online Leaderboards:** Compete with other players worldwide! Separate leaderboards are maintained for each game mode, language, and word length combination.

## 🛠️ Tech Stack & Architecture

This project was built with a strong focus on clean, scalable, and testable code, utilizing modern tools and patterns.

### Core Technologies
*   **Engine:** Unity 6.2+ 
*   **UI:** UI Toolkit (for a modern, data-driven UI approach)
*   **Asynchronous Code:** UniTask (for high-performance, allocation-free async/await)
*   **Animations:** PrimeTween (for efficient and powerful UI animations)
*   **Dependency Injection:** VContainer (for robust decoupling and improved testability)

### Architecture
The project follows a **Model-View-ViewModel (MVVM)** pattern to ensure a clear separation of concerns:

*   **Model:** The core game logic. Contains services like `GameManager`, `WordProvider`, and `SettingsService`. It is pure C# and has no knowledge of Unity's UI.
*   **ViewModel:** The presentation logic. It prepares data from the Model for the View and handles user interactions via commands. Examples include `GameBoardViewModel` and `SettingsViewModel`.
*   **View:** The UI layer, built with UI Toolkit. It is responsible only for displaying what the ViewModel tells it to. The Views are "dumb" and simply bind to ViewModel properties.

### Backend Services (Unity Gaming Services)
*   **Authentication:** Anonymous player authentication to identify users for leaderboards.
*   **Cloud Code:** Server-side logic to securely provide the "Word of the Day," preventing client-side cheating by changing the device's date.
*   **Leaderboards:** Secure submission and retrieval of player scores.

## 🚀 How to Run the Project

1.  **Clone the repository:**

    ```bash
        # Make sure you have Git LFS installed to pull large files correctly.
    git clone https://github.com/hanejkopawel/yawordle.git
    ```
    
2.  **Open in Unity:**
    *   Open the project using Unity Hub (version 6.2 or newer recommended).
    *   The editor will automatically resolve all the required packages (VContainer, UniTask, etc.).
3.  **Configure Unity Gaming Services (Optional):**
    *   To run with backend features, you will need to link the project to your own UGS organization in `Edit -> Project Settings -> Services`.
    *   You will need to deploy the Cloud Code script provided in the project.
4.  **Run the Game:**
    *   Open the `MainScene` from the `Scenes` folder.
    *   Press the Play button in the editor.

## 📜 Future Development Ideas

This project is a solid foundation that can be extended with more features:
*   [ ] Additional languages and dictionaries.
*   [ ] Hard mode (requiring revealed hints to be used in subsequent guesses).
*   [ ] Player statistics and achievements.
*   [ ] Friends system and social leaderboards.
*   [ ] Theming options (e.g., light/dark mode).

---

*This project was created by [Your Name/Handle] as a portfolio piece. Feel free to browse the code to see the architecture and implementation details.*
