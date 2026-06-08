using UnityEngine;

namespace RMC.d.Gameplay.Match3
{
    public class Match3BoardController : MonoBehaviour
    {
        [SerializeField] private Match3LevelConfig _levelConfig;

        public Vector2Int BoardSize
        {
            get
            {
                if (_levelConfig == null)
                {
                    return new Vector2Int(8, 8);
                }
                return new Vector2Int(_levelConfig.width, _levelConfig.height);
            }
        }

        private void Start()
        {
            Debug.Log($"Initializing Match-3 board {BoardSize.x}x{BoardSize.y}.");
        }
    }
}
