using Godot;
using System;
using SignalLost.Network;

namespace SignalLost.UI
{
    [GlobalClass]
    public partial class NetworkUI : Control
    {
        // UI elements
        private LineEdit _nameInput;
        private LineEdit _ipInput;
        private Button _hostButton;
        private Button _joinButton;
        private Button _disconnectButton;
        private ItemList _playerList;
        private RichTextLabel _chatLog;
        private LineEdit _chatInput;
        private Button _sendButton;
        private Label _statusLabel;

        // Network manager reference
        private NetworkManager _networkManager;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get UI elements
            _nameInput = GetNode<LineEdit>("Panel/VBoxContainer/GridContainer/NameInput");
            _ipInput = GetNode<LineEdit>("Panel/VBoxContainer/GridContainer/IpInput");
            _hostButton = GetNode<Button>("Panel/VBoxContainer/ButtonsContainer/HostButton");
            _joinButton = GetNode<Button>("Panel/VBoxContainer/ButtonsContainer/JoinButton");
            _disconnectButton = GetNode<Button>("Panel/VBoxContainer/ButtonsContainer/DisconnectButton");
            _playerList = GetNode<ItemList>("Panel/VBoxContainer/HBoxContainer/PlayerList");
            _chatLog = GetNode<RichTextLabel>("Panel/VBoxContainer/HBoxContainer/VBoxContainer/ChatLog");
            _chatInput = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer/VBoxContainer/HBoxContainer/ChatInput");
            _sendButton = GetNode<Button>("Panel/VBoxContainer/HBoxContainer/VBoxContainer/HBoxContainer/SendButton");
            _statusLabel = GetNode<Label>("Panel/VBoxContainer/StatusLabel");

            // Get network manager
            _networkManager = GetNode<NetworkManager>("/root/NetworkManager");
            if (_networkManager == null)
            {
                GD.PrintErr("NetworkUI: Failed to get NetworkManager reference");
                return;
            }

            // Connect signals
            _hostButton.Pressed += OnHostButtonPressed;
            _joinButton.Pressed += OnJoinButtonPressed;
            _disconnectButton.Pressed += OnDisconnectButtonPressed;
            _sendButton.Pressed += OnSendButtonPressed;
            _chatInput.TextSubmitted += OnChatInputSubmitted;

            _networkManager.NetworkStateChanged += OnNetworkStateChanged;
            _networkManager.PlayerListChanged += OnPlayerListChanged;
            _networkManager.MessageReceived += OnMessageReceived;
            _networkManager.ConnectionFailed += OnConnectionFailed;

            // Initialize UI
            UpdateUI();
        }

        // Update UI based on connection state
        private void UpdateUI()
        {
            bool connected = _networkManager.IsConnected();
            bool isServer = _networkManager.IsServer();

            // Update buttons
            _hostButton.Disabled = connected;
            _joinButton.Disabled = connected;
            _disconnectButton.Disabled = !connected;
            _nameInput.Editable = !connected;
            _ipInput.Editable = !connected;
            _chatInput.Editable = connected;
            _sendButton.Disabled = !connected;

            // Update status label
            if (connected)
            {
                _statusLabel.Text = isServer ? "Status: Hosting" : "Status: Connected";
                _statusLabel.Modulate = new Color(0.0f, 1.0f, 0.0f); // Green
            }
            else
            {
                _statusLabel.Text = "Status: Disconnected";
                _statusLabel.Modulate = new Color(1.0f, 0.0f, 0.0f); // Red
            }

            // Update player list
            UpdatePlayerList();
        }

        // Update the player list
        private void UpdatePlayerList()
        {
            _playerList.Clear();

            if (_networkManager.IsConnected())
            {
                int[] playerIds = _networkManager.GetPlayerIds();
                foreach (int id in playerIds)
                {
                    string name = _networkManager.GetPlayerName(id);
                    string text = $"{name} (ID: {id})";
                    
                    // Highlight local player
                    if (id == _networkManager.GetLocalPlayerId())
                    {
                        text += " (You)";
                    }
                    
                    _playerList.AddItem(text);
                }
            }
        }

        // Add a message to the chat log
        private void AddChatMessage(string sender, string message)
        {
            string timestamp = Time.GetDatetimeStringFromSystem().Split(" ")[1]; // Get only the time part
            _chatLog.AppendText($"[{timestamp}] {sender}: {message}\n");
            
            // Auto-scroll to bottom
            _chatLog.ScrollToLine(_chatLog.GetLineCount() - 1);
        }

        // Event handlers
        private void OnHostButtonPressed()
        {
            // Set player name
            string playerName = _nameInput.Text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Host";
                _nameInput.Text = playerName;
            }
            _networkManager.SetPlayerName(playerName);

            // Create server
            Error error = _networkManager.CreateServer();
            if (error != Error.Ok)
            {
                _statusLabel.Text = $"Error: Failed to create server ({error})";
                _statusLabel.Modulate = new Color(1.0f, 0.0f, 0.0f); // Red
            }
        }

        private void OnJoinButtonPressed()
        {
            // Set player name
            string playerName = _nameInput.Text.Trim();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"Player {new Random().Next(1000, 9999)}";
                _nameInput.Text = playerName;
            }
            _networkManager.SetPlayerName(playerName);

            // Get server IP
            string serverIp = _ipInput.Text.Trim();
            if (string.IsNullOrEmpty(serverIp))
            {
                serverIp = "127.0.0.1";
                _ipInput.Text = serverIp;
            }
            _networkManager.ServerIp = serverIp;

            // Connect to server
            Error error = _networkManager.ConnectToServer();
            if (error != Error.Ok)
            {
                _statusLabel.Text = $"Error: Failed to connect to server ({error})";
                _statusLabel.Modulate = new Color(1.0f, 0.0f, 0.0f); // Red
            }
            else
            {
                _statusLabel.Text = "Status: Connecting...";
                _statusLabel.Modulate = new Color(1.0f, 1.0f, 0.0f); // Yellow
            }
        }

        private void OnDisconnectButtonPressed()
        {
            _networkManager.CloseConnection();
        }

        private void OnSendButtonPressed()
        {
            string message = _chatInput.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                _networkManager.SendMessage(message);
                _chatInput.Text = "";
            }
        }

        private void OnChatInputSubmitted(string text)
        {
            OnSendButtonPressed();
        }

        private void OnNetworkStateChanged(bool connected, bool isServer)
        {
            UpdateUI();

            if (connected)
            {
                AddChatMessage("System", isServer ? "Server started" : "Connected to server");
            }
            else
            {
                AddChatMessage("System", "Disconnected");
                _playerList.Clear();
            }
        }

        private void OnPlayerListChanged()
        {
            UpdatePlayerList();
        }

        private void OnMessageReceived(int senderId, string senderName, string message)
        {
            AddChatMessage(senderName, message);
        }

        private void OnConnectionFailed()
        {
            _statusLabel.Text = "Error: Connection failed";
            _statusLabel.Modulate = new Color(1.0f, 0.0f, 0.0f); // Red
            AddChatMessage("System", "Failed to connect to server");
        }
    }
}
