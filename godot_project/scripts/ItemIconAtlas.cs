using Godot;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class ItemIconAtlas : Node
    {
        // The texture atlas containing all item icons
        private Texture2D _atlas;

        // The size of each icon in the atlas
        private Vector2I _iconSize = new Vector2I(32, 32);

        // The number of icons per row in the atlas
        private int _iconsPerRow = 8;

        // Dictionary mapping icon indices to atlas regions
        private Dictionary<int, Rect2> _iconRegions = new Dictionary<int, Rect2>();

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Load the atlas texture
            _atlas = ResourceLoader.Load<Texture2D>("res://assets/images/items/item_atlas.png");

            if (_atlas == null)
            {
                GD.PrintErr("ItemIconAtlas: Failed to load atlas texture");
                return;
            }

            // Calculate the total number of icons in the atlas
            int totalIcons = (_atlas.GetWidth() / _iconSize.X) * (_atlas.GetHeight() / _iconSize.Y);

            // Generate the icon regions
            for (int i = 0; i < totalIcons; i++)
            {
                int row = i / _iconsPerRow;
                int col = i % _iconsPerRow;

                float x = col * _iconSize.X;
                float y = row * _iconSize.Y;

                _iconRegions[i] = new Rect2(x, y, _iconSize.X, _iconSize.Y);
            }
        }

        // Get the atlas texture
        public Texture2D GetAtlasTexture()
        {
            return _atlas;
        }

        // Get the region for a specific icon index
        public Rect2 GetIconRegion(int iconIndex)
        {
            if (_iconRegions.ContainsKey(iconIndex))
            {
                return _iconRegions[iconIndex];
            }

            // Return the first icon region as a fallback
            return _iconRegions.ContainsKey(0) ? _iconRegions[0] : new Rect2(0, 0, _iconSize.X, _iconSize.Y);
        }

        // Get the icon size
        public Vector2I GetIconSize()
        {
            return _iconSize;
        }

        // Set the icon size
        public void SetIconSize(Vector2I size)
        {
            _iconSize = size;
            // Regenerate the icon regions
            _Ready();
        }

        // Set the number of icons per row
        public void SetIconsPerRow(int count)
        {
            _iconsPerRow = count;
            // Regenerate the icon regions
            _Ready();
        }
    }
}
