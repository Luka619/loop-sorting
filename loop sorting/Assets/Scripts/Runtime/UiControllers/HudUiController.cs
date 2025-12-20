using UnityEngine;

namespace LoopSorting
{
    public sealed class HudUiController
    {
        private readonly GameRuntimeController _host;

        public HudUiController(GameRuntimeController host)
        {
            _host = host;
        }
    }
}
