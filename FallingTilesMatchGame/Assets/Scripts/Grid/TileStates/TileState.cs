using UnityEngine;

namespace Grid.TileStates
{
    public class TileState : BaseState
    {
        protected float _stateTimer;

        protected StateMachine<TileState> _tileStateMachine;

        public TileState(Tile tileOwner, StateMachine<TileState> tileStateMachine)
        {
            TileOwner = tileOwner;
            _tileStateMachine = tileStateMachine;
        }

        public Tile TileOwner { get; }

        public override void Enter()
        {
        }

        public override void Update()
        {
            _stateTimer -= Time.deltaTime;
        }

        public override void Exit()
        {
        }
    }
}