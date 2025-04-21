using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.Network
{
    /// <summary>
    /// Manages network connectivity and synchronization between clients.
    /// </summary>
    [GlobalClass]
    public partial class NetworkManager : Node
    {
        // Singleton instance
        private static NetworkManager _instance;
        public static NetworkManager Instance => _instance;

        // Network configuration
        [Export] public int ServerPort { get; set; } = 28000;
        [Export] public int MaxPlayers { get; set; } = 4;
        [Export] public string ServerIp { get; set; } = "127.0.0.1";

        // Network state
        private bool _isServer = false;
        private bool _isConnected = false;
        private int _localPlayerId = 1;
        private Dictionary<int, string> _playerNames = new Dictionary<int, string>();
        private ENetMultiplayerPeer _peer;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Set up singleton
            if (_instance != null)
            {
                QueueFree();
                return;
            }
            _instance = this;

            // Initialize
            _peer = new ENetMultiplayerPeer();

            // Connect signals
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ConnectionFailed += OnConnectionFailed;
            Multiplayer.ServerDisconnected += OnServerDisconnected;

            GD.Print("NetworkManager initialized");
        }

        // Create a server
        public Error CreateServer()
        {
            // Close any existing connections
            if (_isConnected)
            {
                CloseConnection();
            }

            // Create server
            Error error = _peer.CreateServer(ServerPort, MaxPlayers);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create server: {error}");
                return error;
            }

            Multiplayer.MultiplayerPeer = _peer;
            _isServer = true;
            _isConnected = true;
            _localPlayerId = 1; // Server is always ID 1

            // Add local player
            _playerNames[_localPlayerId] = "Server";

            GD.Print("Server created successfully");
            EmitSignal(SignalName.NetworkStateChanged, true, true);
            return Error.Ok;
        }

        // Connect to a server
        public Error ConnectToServer(string ip = null)
        {
            // Close any existing connections
            if (_isConnected)
            {
                CloseConnection();
            }

            // Use provided IP or default
            string serverIp = ip ?? ServerIp;

            // Connect to server
            Error error = _peer.CreateClient(serverIp, ServerPort);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to connect to server: {error}");
                return error;
            }

            Multiplayer.MultiplayerPeer = _peer;
            _isServer = false;
            _isConnected = false; // Will be set to true when connected

            GD.Print($"Connecting to server at {serverIp}:{ServerPort}...");
            return Error.Ok;
        }

        // Close the connection
        public void CloseConnection()
        {
            if (_peer != null)
            {
                _peer.Close();
            }

            _isServer = false;
            _isConnected = false;
            _playerNames.Clear();

            GD.Print("Connection closed");
            EmitSignal(SignalName.NetworkStateChanged, false, false);
        }

        // Check if this is the server
        public bool IsServer()
        {
            return _isServer;
        }

        // Check if connected to a server
        public bool IsConnected()
        {
            return _isConnected;
        }

        // Get the local player ID
        public int GetLocalPlayerId()
        {
            return _localPlayerId;
        }

        // Get all connected player IDs
        public int[] GetPlayerIds()
        {
            int[] playerIds = new int[_playerNames.Count];
            int index = 0;
            foreach (int id in _playerNames.Keys)
            {
                playerIds[index++] = id;
            }
            return playerIds;
        }

        // Get a player's name by ID
        public string GetPlayerName(int playerId)
        {
            if (_playerNames.ContainsKey(playerId))
            {
                return _playerNames[playerId];
            }
            return "Unknown";
        }

        // Set the local player's name
        public void SetPlayerName(string name)
        {
            _playerNames[_localPlayerId] = name;

            // Broadcast to other players if connected
            if (_isConnected)
            {
                RpcId(1, nameof(RegisterPlayerName), _localPlayerId, name);
            }
        }

        // Register a player's name (called on server)
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void RegisterPlayerName(int playerId, string name)
        {
            if (_isServer)
            {
                // Server registers the name and broadcasts to all clients
                _playerNames[playerId] = name;
                Rpc(nameof(UpdatePlayerName), playerId, name);
                EmitSignal(SignalName.PlayerListChanged);
            }
        }

        // Update a player's name (called on clients)
        [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void UpdatePlayerName(int playerId, string name)
        {
            _playerNames[playerId] = name;
            EmitSignal(SignalName.PlayerListChanged);
        }

        // Synchronize a discovered location
        public void SyncDiscoveredLocation(string locationId)
        {
            if (_isConnected)
            {
                if (_isServer)
                {
                    // Server broadcasts to all clients
                    Rpc(nameof(OnLocationDiscovered), locationId);
                }
                else
                {
                    // Client sends to server
                    RpcId(1, nameof(OnLocationDiscovered), locationId);
                }
            }
        }

        // Handle discovered location
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void OnLocationDiscovered(string locationId)
        {
            if (_isServer)
            {
                // Server broadcasts to all clients
                Rpc(nameof(OnLocationDiscovered), locationId);
            }

            // Get the map system
            var mapSystem = GetNode<MapSystem>("/root/MapSystem");
            if (mapSystem != null)
            {
                mapSystem.DiscoverLocation(locationId);
            }
        }

        // Synchronize a radio signal
        public void SyncRadioSignal(string signalId, float frequency, float strength)
        {
            if (_isConnected)
            {
                if (_isServer)
                {
                    // Server broadcasts to all clients
                    Rpc(nameof(OnRadioSignalReceived), signalId, frequency, strength);
                }
                else
                {
                    // Client sends to server
                    RpcId(1, nameof(OnRadioSignalReceived), signalId, frequency, strength);
                }
            }
        }

        // Handle radio signal
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void OnRadioSignalReceived(string signalId, float frequency, float strength)
        {
            if (_isServer)
            {
                // Server broadcasts to all clients
                Rpc(nameof(OnRadioSignalReceived), signalId, frequency, strength);
            }

            // Get the radio system
            var radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            if (radioSystem != null)
            {
                // Add or update the signal
                radioSystem.AddOrUpdateSignal(signalId, frequency, strength);
            }
        }

        // Send a message to other players
        public void SendMessage(string message)
        {
            if (_isConnected)
            {
                if (_isServer)
                {
                    // Server broadcasts to all clients
                    Rpc(nameof(OnMessageReceived), _localPlayerId, message);
                }
                else
                {
                    // Client sends to server
                    RpcId(1, nameof(OnMessageReceived), _localPlayerId, message);
                }
            }
        }

        // Handle received message
        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        private void OnMessageReceived(int senderId, string message)
        {
            if (_isServer)
            {
                // Server broadcasts to all clients
                Rpc(nameof(OnMessageReceived), senderId, message);
            }

            // Get sender name
            string senderName = GetPlayerName(senderId);

            // Emit signal with message
            EmitSignal(SignalName.MessageReceived, senderId, senderName, message);
        }

        // Event handlers
        private void OnPeerConnected(long id)
        {
            GD.Print($"Peer connected: {id}");

            // Add player to list
            _playerNames[(int)id] = $"Player {id}";

            // If server, send current player list to new player
            if (_isServer)
            {
                foreach (var player in _playerNames)
                {
                    RpcId((int)id, nameof(UpdatePlayerName), player.Key, player.Value);
                }
            }

            EmitSignal(SignalName.PlayerConnected, (int)id);
            EmitSignal(SignalName.PlayerListChanged);
        }

        private void OnPeerDisconnected(long id)
        {
            GD.Print($"Peer disconnected: {id}");

            // Remove player from list
            _playerNames.Remove((int)id);

            EmitSignal(SignalName.PlayerDisconnected, (int)id);
            EmitSignal(SignalName.PlayerListChanged);
        }

        private void OnConnectedToServer()
        {
            GD.Print("Connected to server");
            _isConnected = true;
            _localPlayerId = Multiplayer.GetUniqueId();

            // Add local player
            _playerNames[_localPlayerId] = $"Player {_localPlayerId}";

            EmitSignal(SignalName.NetworkStateChanged, true, false);
        }

        private void OnConnectionFailed()
        {
            GD.Print("Connection failed");
            _isConnected = false;
            EmitSignal(SignalName.ConnectionFailed);
        }

        private void OnServerDisconnected()
        {
            GD.Print("Disconnected from server");
            _isConnected = false;
            _playerNames.Clear();
            EmitSignal(SignalName.NetworkStateChanged, false, false);
        }

        // Signals
        [Signal] public delegate void NetworkStateChangedEventHandler(bool connected, bool isServer);
        [Signal] public delegate void PlayerConnectedEventHandler(int playerId);
        [Signal] public delegate void PlayerDisconnectedEventHandler(int playerId);
        [Signal] public delegate void PlayerListChangedEventHandler();
        [Signal] public delegate void MessageReceivedEventHandler(int senderId, string senderName, string message);
        [Signal] public delegate void ConnectionFailedEventHandler();
    }
}
