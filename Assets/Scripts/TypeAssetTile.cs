using UnityEngine;

namespace MatchThreeEngine
{
	[CreateAssetMenu(menuName = "Match 3 Engine/Type Asset Tile")]
	public sealed class TypeAssetTile : ScriptableObject
	{
		public int id;

		public int value;

		public Sprite sprite;
	}
}
