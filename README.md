# Sanctum-Core
## Overview

Sanctum-Core is a card game implemented in C# that handles various game aspects such as card insertion, container management, player management, game state, and card zones. The game is designed with a robust networking capability to manage game attributes across different players and devices, allowing any server to connect using various network attribute type systems.
Features

    Card Management: Insert and manage cards within the game.
    Container Management: Handle different containers for cards.
    Player Management: Add and manage players within the game.
    Game State Management: Maintain and update the state of the game.
    Card Zones: Manage different zones where cards can be placed.
    Networking: Utilize the NetworkAttributeFactory for synchronizing game attributes across any server.
    Unit Tests: Comprehensive unit tests for card data to ensure reliability and correctness.

## Classes
### Playtable

The Playtable class is the central hub of the game, responsible for:

    Managing players
    Maintaining the game state
    Handling card zones

## NetworkAttributeFactory

The NetworkAttributeFactory is a key component for enabling flexible networking capabilities in the game. It handles the creation, synchronization, and management of network attributes, ensuring that game attributes are consistent across different servers and clients.
Key Responsibilities

    Creation of Network Attributes:
        The factory creates instances of network attributes that can be shared across the network.
        These attributes represent various game state elements such as player information, card states, and game settings.

    Synchronization:
        The factory ensures that changes to network attributes are propagated to all connected servers and clients.
        It uses appropriate networking protocols to maintain real-time synchronization of game states.

    Management:
        The factory manages the lifecycle of network attributes, including initialization, updates, and disposal.
        It handles the underlying communication mechanisms to ensure efficient and reliable data transfer.

Interoperability with Playtable Implementation

The NetworkAttributeFactory is designed to be flexible and interoperable with any server that supports network attribute type systems. This allows Sanctum-Core to be integrated into various server environments seamlessly.