using UnityEngine;
using UnityEngine.UI;

namespace MatchThreeEngine
{
    public sealed class TypeTile : MonoBehaviour
    {
        public int x;
        public int y;
        public Image icon;
        public Button button;
        private TypeAssetTile _type;
        public TileData Data => new TileData(x, y, _type.id);
       
        public void Initialize(int x, int y, TypeAssetTile type)
        {
            this.x = x;
            this.y = y;
            Type = type;
            button.onClick.AddListener(OnTileClicked);
        }
        public TypeAssetTile Type
        {
            get => _type;

            set
            {
                if (_type == value) return;

                _type = value;

                icon.sprite = _type.sprite;
            }
        }
        private void OnTileClicked()
        {
            // Логика обработки клика по плитке
            Debug.Log($"Tile clicked at ({x}, {y}) with type {_type.id}");
        }
    }
}