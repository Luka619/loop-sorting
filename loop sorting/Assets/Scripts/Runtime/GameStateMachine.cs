namespace LoopSorting
{
    public interface IGameStateHost
    {
        void EnterMenuState();
        void EnterPlayingState();
        void ExitState(GameStateMachine.State from);
    }

    public sealed class GameStateMachine
    {
        public enum State
        {
            None = 0,
            Menu = 1,
            Playing = 2
        }

        private State _state;
        private readonly IGameStateHost _host;

        public GameStateMachine(IGameStateHost host)
        {
            _host = host;
        }

        public State Current => _state;

        public void EnterMenu()
        {
            TransitionTo(State.Menu);
        }

        public void EnterPlaying()
        {
            TransitionTo(State.Playing);
        }

        private void TransitionTo(State next)
        {
            if (_state == next) return;

            var prev = _state;
            if (prev != State.None)
            {
                _host.ExitState(prev);
            }

            _state = next;
            switch (_state)
            {
                case State.Menu:
                    _host.EnterMenuState();
                    break;
                case State.Playing:
                    _host.EnterPlayingState();
                    break;
            }
        }
    }
}
