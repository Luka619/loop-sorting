using UnityEngine;

namespace LoopSorting
{
    public sealed class GameProgressState
    {
        public int Coins;
        public int Lives;
        public int BoosterSortCount;
        public int BoosterShuffleCount;
        public int SavedFlowIndex;
        public int SavedHighestUnlockedFlowIndex;

        public void ClampEconomy()
        {
            Coins = Mathf.Max(0, Coins);
            Lives = Mathf.Max(0, Lives);
            BoosterSortCount = Mathf.Clamp(BoosterSortCount, 0, 99);
            BoosterShuffleCount = Mathf.Clamp(BoosterShuffleCount, 0, 99);
        }
    }
}
