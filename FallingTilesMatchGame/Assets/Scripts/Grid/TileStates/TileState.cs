using UnityEngine;

namespace Grid.TileStates
{
    public class TileState : BaseState
    {

        protected StateMachine<TileState> _tileStateMachine;
        public Tile TileOwner { get; private set; }
        protected float _stateTimer;

        public TileState(Tile tileOwner, StateMachine<TileState> tileStateMachine)
        {
            this.TileOwner = tileOwner;
            this._tileStateMachine = tileStateMachine;
        }

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