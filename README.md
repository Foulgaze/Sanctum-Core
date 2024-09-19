
# Sanctum-Core

## Overview

Sanctum-Core is a Trading Card Game (TCG) playtable simulator structured across four key projects within this repository:

-   **Sanctum Core**
-   **Sanctum Server**
-   **Sanctum Runner**
-   **Sanctum Testing**

### Sanctum Core

Sanctum Core is the heart of the project, managing the core gameplay of the TCG. It enables the functionality to play cards, move them across different zones, and perform other essential game actions.

### Sanctum Server

Sanctum Server oversees the operation of multiple Sanctum Core instances. It features a lobby system where players can connect to various lobbies, each hosting its own instance of Sanctum Core. Lobbies run on separate threads and are closed when players disconnect or after a specified period of inactivity.

### Sanctum Runner

Sanctum Runner is responsible for executing the server with configurations that define lobby idle times and the frequency of idle checks.

### Sanctum Testing

Sanctum Testing encompasses unit and integration tests for both Sanctum Core and Sanctum Server, ensuring the reliability and functionality of the entire system.

# Sanctum Core Architecture
Sanctum Core is designed to be a flexible TCG playtable that can work with any custom frontend, as long as it integrates with Sanctum Core's Network Attribute system. Once players connect to a lobby and start the game, everything is controlled through this network-based system. This setup ensures smooth and consistent communication between the playtable and any frontend, making it easy to create and manage games across different platforms.

### Network Attribute
The `NetworkAttribute` class is the foundation for attributes that can be networked in our system. Think of it as a blueprint for attributes that can hold and manage values, as well as track changes. It includes:

-   **Unique Identifier**: Each `NetworkAttribute` has a unique ID for identification.
-   **Value Management**: It can store a value of a specific type and provides methods to set, clear, and manage that value.
-   **Serialization**: Attributes can be serialized to a string format, useful for network transmission.
-   **Event Handling**: It supports events that trigger when the attribute's value changes, allowing other parts of the system to react to these changes.

### `NetworkAttribute<T>` Class

`NetworkAttribute<T>` extends `NetworkAttribute` to support strongly-typed attributes. It adds:

-   **Strongly-Typed Value**: It works with a specific type `T`, making type handling safer and more predictable.
-   **Change Notification**: It fires events when the value changes or when it's updated non-networked, helping synchronize state across different parts of the application.
-   **Serialization Efficiency**: It handles serialization of the value efficiently, updating only when necessary.

### `NetworkAttributeFactory` Class

The `NetworkAttributeFactory` is the orchestrator of `NetworkAttribute` instances. Its main functions include:

-   **Attribute Management**: It creates and stores various network attributes, allowing you to manage them by their IDs.
-   **Handling Network Changes**: It processes incoming network data to update attributes based on serialized data received over the network.
-   **Event Handling**: It connects attribute changes to network events, ensuring that updates are propagated throughout the system.
-   **Cleaning Up**: It provides methods to clear attributes and their associated listeners, maintaining a clean state.

### How It All Fits Together

In essence, `NetworkAttribute` and its derived class `NetworkAttribute<T>` provide a structured way to handle attributes that can be updated over a network. `NetworkAttributeFactory` ties everything together by managing these attributes, processing network updates, and ensuring that changes are communicated effectively.

When an attribute’s value changes, it can be serialized and sent over the network. The factory handles the deserialization and updates the attribute, triggering events to notify other parts of the system about the change. This approach ensures that your application remains synchronized and responsive to network changes.

To undestand which Network Attributes need to be implemtented, the Sanctum Core Network Attributes should be examined. 

# Sanctum Server
### Overview

The Sanctum Core Server listens for incoming TCP connections on a specified port (default is 51522). It supports various commands such as creating or joining lobbies and managing player connections. Here’s how you can connect and issue commands to the server.
### Sending Data with the Server

To communicate with the Sanctum Core Server, you will send data in the form of `NetworkCommand` instances. Each command is identified by an operation code (`opCode`) and carries a specific instruction associated with that command.

#### NetworkCommand Class

The `NetworkCommand` class encapsulates the data needed to send a command to the server. It consists of two primary properties:

- `opCode`: An integer that represents the command type, corresponding to the `NetworkInstruction` enum.
  - The valid opcodes are as follows `    public enum NetworkInstruction
    {
        CreateLobby, JoinLobby, PlayersInLobby, InvalidCommand, LobbyDescription, StartGame, NetworkAttribute
    }`
- `instruction`: A string that contains the additional data or parameters related to the command.

Here’s a quick look at the `NetworkCommand` class:

```csharp
public class NetworkCommand
{
    public readonly int opCode;
    public readonly string instruction;

    public NetworkCommand(int opCode, string instruction)
    {
        this.opCode = opCode;
        this.instruction = instruction;
    }

    public override string ToString()
    {
        return $"{(NetworkInstruction)this.opCode} - {this.instruction}";
    }
}
```

### Connecting to the Server
1. **Client Setup**:
   - **Connect to the Server**: Establish a TCP connection using the port number.
   - **Example**:
     ```csharp
     TcpClient client = new TcpClient("127.0.0.1", 51522);
     NetworkStream stream = client.GetStream();
     ```

### Joining a Lobby

To join a lobby, follow these steps:

1. **Send a Join Command**:
   - You need to provide your name and the lobby code to join an existing lobby.
   - **Example**:
     ```csharp
     string joinLobbyPayload = "YourName|ABCD"; // Replace with your name and lobby code
     Server.SendMessage(stream, NetworkInstruction.JoinLobby, joinLobbyPayload);
     ```

2. **Handle Responses**:
   - The server will respond with status with the user's assigned UUID and the lobby size in the form 
   - ```"{uuid}|{lobbySize}"```

### Creating a Lobby

To create a new lobby, follow these steps:

1. **Send a Create Command**:
   - Provide the number of players and a unique lobby code.
   - **Example**:
     ```csharp
     string createLobbyPayload = "2|ABCD"; // Replace with player count and lobby code
     Server.SendMessage(stream, NetworkInstruction.CreateLobby, createLobbyPayload);
     ```

2. **Handle Responses**:
   - The server will respond with status with the user's assigned UUID and the lobby code in the form 
   - ```"{uuid}|{lobbyCode}"```

### Error Handling

- **Invalid Commands**: If the server receives an invalid command, it will respond with an `InvalidCommand` message. Check your command format and data.
- **Connection Issues**: Handle exceptions and errors during data transmission to ensure a stable connection.

### Example Code

Here is an example of creating a lobby:

```csharp
// Connect to the server
TcpClient client = new TcpClient("127.0.0.1", 51522);
NetworkStream stream = client.GetStream();

// Create a lobby with 2 players and a lobby code "ABCD"
string createLobbyPayload = "2|ABCD";
Server.SendMessage(stream, NetworkInstruction.CreateLobby, createLobbyPayload);

// Handle server responses (implement based on your application's logic)
```

After all players have connected to the lobby, the server will create a playtable, and all further networking is done via the Network Attribute system. 