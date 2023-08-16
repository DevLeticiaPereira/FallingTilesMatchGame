namespace Grid.TileStates
{
    public class MatchedState : TileState
    {
        public MatchedState(Tile tileOwner, StateMachine<TileState> tileStateMachine) : base(tileOwner,
            tileStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            var animation = TileOwner.Data.DeathAnimation;
            _stateTimer = animation.length;
            TileOwner.PlayDeathAnimation();
        }

        public override void Update()
        {
            base.Update();
            if (_stateTimer <= 0) TileOwner.Destroy();
        }
    }
}