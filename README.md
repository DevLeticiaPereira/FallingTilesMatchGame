# Falling Tiles Match Game - README

Welcome to the Falling Tiles Match Game repository! This README provides an overview of the project, its components, and features. The game is designed and tested for Android devices, specifically the Samsung Galaxy A52s 5G with Android 13. Unfortunately, there is no iOS testing.

## Overview

The Falling Tiles Match Game is a puzzle game where players match falling tiles to clear them from the grid and score points. The project demonstrates a well-structured architecture and various components that contribute to the game's functionality.

## Game Components

### Game Manager

The Game Manager oversees the overall game flow and manages different game states, including Main Menu, Game Start, Game Paused, Game Running, and Game End. This separation of concerns ensures a maintainable and scalable codebase, making it easier to add and improve features.

### Event Manager

The Event Manager centralizes in-game event handling, promoting consistency and reuse across the game. This organized approach simplifies event management and contributes to a more robust and error-resistant codebase.

### UI Manager

The UI Manager loads and unloads various UI panels, maintaining a clear structure for panels such as Main Menu, HUD, Pause Menu, and more. Efficient instantiation and destruction of panels, along with focus management, enhance user experience and ensure consistent UI behavior.

### Input Manager

The Input Manager handles player input and translates it into game actions. Centralized input handling simplifies expansion and maintenance of input-related features.

### Grid Manager

The Grid Manager class is responsible for managing grid-based gameplay mechanics, including tile spawning, matching, and interactions. It handles tile movement, connections, and states, contributing to a dynamic gameplay experience.

### Grid Utilities

Grid Utilities provide a library of static functions for managing grid-based gameplay elements, enabling reusability and efficient access from various components.

### Tile States

Tile behavior is organized using a state machine approach, with distinct states for different tile behaviors. This separation of states improves code maintainability and readability.

### Matches Calculation

Efficient match-checking is implemented by managing tile connections within the grid. This approach optimizes match detection and eliminates the need for redundant flood-fill algorithms.

## Missing Features

While the Falling Tiles Match Game showcases a robust architecture and several core features, there are a couple of features that were not fully implemented due to time constraints:

1.  **Faster Falling Pairs**

2.  **Two Players Game Mode**

## Possible future Features

1. Optimize Tile Management: Employ object pooling to efficiently manage tiles and mitigate the drawbacks of frequent spawning and destruction. This technique minimizes the strain on garbage collection, leading to smoother and more resource-efficient gameplay.
2. Save game high score
3. Sounds

## Conclusion

The Falling Tiles Match Game repository demonstrates a well-organized architecture and key gameplay components. It offers an engaging puzzle experience where players match falling tiles to clear the grid and score points. While a couple of features are yet to be implemented, the existing foundation provides a solid base for further development and enhancements.

Feel free to explore the repository, review the code, and contribute to the project's growth! If you have any questions or feedback, please don't hesitate to reach out to the project owner, Leticia Pereira de Souza.
