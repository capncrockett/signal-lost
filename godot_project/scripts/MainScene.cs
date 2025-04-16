using Godot;
using System;

namespace SignalLost
{
	[GlobalClass]
	public partial class MainScene : Node
	{
		// References to UI elements
		private Button _mapButton;
		private Control _mapUI;
		private Button _inventoryButton;
		private Control _inventoryUI;
		private Button _questButton;
		private Control _questUI;

		// Called when the node enters the scene tree for the first time
		public override void _Ready()
		{
			// Get references to UI elements
			_mapButton = GetNode<Button>("SidePanel/VBoxContainer/MapButton");
			_mapUI = GetNode<Control>("MapUI");
			_inventoryButton = GetNode<Button>("SidePanel/VBoxContainer/InventoryButton");
			_inventoryUI = GetNode<Control>("InventoryUI");
			_questButton = GetNode<Button>("SidePanel/VBoxContainer/QuestButton");
			_questUI = GetNode<Control>("QuestUI");

			// Connect signals
			_mapButton.Pressed += OnMapButtonPressed;
			_inventoryButton.Pressed += OnInventoryButtonPressed;
			_questButton.Pressed += OnQuestButtonPressed;

			// Hide the UIs initially
			_mapUI.Visible = false;
			_inventoryUI.Visible = false;
			_questUI.Visible = false;
		}

		// Event handlers
		private void OnMapButtonPressed()
		{
			// Toggle the map UI
			_mapUI.Visible = !_mapUI.Visible;

			// Hide other UIs if this UI is shown
			if (_mapUI.Visible)
			{
				_inventoryUI.Visible = false;
				_questUI.Visible = false;
			}
		}

		private void OnInventoryButtonPressed()
		{
			// Toggle the inventory UI
			_inventoryUI.Visible = !_inventoryUI.Visible;

			// Hide other UIs if this UI is shown
			if (_inventoryUI.Visible)
			{
				_mapUI.Visible = false;
				_questUI.Visible = false;
			}
		}

		private void OnQuestButtonPressed()
		{
			// Toggle the quest UI
			_questUI.Visible = !_questUI.Visible;

			// Hide other UIs if this UI is shown
			if (_questUI.Visible)
			{
				_mapUI.Visible = false;
				_inventoryUI.Visible = false;
			}
		}
	}
}
