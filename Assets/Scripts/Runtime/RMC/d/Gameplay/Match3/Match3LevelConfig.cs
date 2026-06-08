using UnityEngine;

namespace RMC.d.Gameplay.Match3
{
    [CreateAssetMenu(menuName = "RMC/d/Match3 Level Config")]
    public class Match3LevelConfig : ScriptableObject
    {
        [Min(4)] public int width = 8;
        [Min(4)] public int height = 8;
        [Min(1)] public int moveLimit = 24;
        [Min(1)] public int targetScore = 1500;
    }
}
