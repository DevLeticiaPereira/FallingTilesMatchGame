using System;
using UnityEngine;
using Grid.TileStates;

namespace Grid
{
    public class Tile : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private SpriteRenderer _tileSpriteRenderer;
        [SerializeField] private Animation _animationComponent;
        #endregion

        #region Public Variables
        public TileData Data { get; private set; }
        public Vector2Int? GridPosition { get; private set; }
        public Vector2Int? TemporaryGridPosition { get; private set; }
        public bool IsRoot { get; private set; }
        #endregion

        #region Private Variable
        private GridManager _gridManager;
        private Tile _rootPair;
        #endregion
        
        #region States

        public StateMachine<TileState> TileStateMachine { get; private set; }

        public WaitingState WaitingTileState { get; private set; }
        public FallingState FallingTileState { get; private set; }
        public PlacedOnGridState PlacedOnGridTileState { get; private set; }
        public GameOverTileState GridGameOverTileState { get; private set; }
        public MatchedState MatchedTileState { get; private set; }

        #endregion

        #region MonoBehavior
        private void OnEnable()
        {
            EventManager.EventGridGameOver += OnGridGameOver;
        }
        private void OnDisable()
        {
            EventManager.EventGridGameOver -= OnGridGameOver;
        }
        private void Update()
        {
            TileStateMachine.CurrentState.Update();
        }
        #endregion

        #region Initialize and Destroy
        public void InitializeTile(GridManager gridManager, TileData tileData, Tile BeginPair)
        {
            //this.OwnerGridID = gridManager.GridID;
            this.Data = tileData;
            this.IsRoot = BeginPair == null;
            this._gridManager = gridManager;
            _tileSpriteRenderer.sprite = tileData.DefaultSprite;

            TileStateMachine = new StateMachine<TileState>();

            WaitingTileState = new WaitingState(this, TileStateMachine, _gridManager.GridID);
            FallingTileState = new FallingState(this, TileStateMachine, _gridManager);
            PlacedOnGridTileState = new PlacedOnGridState(this, TileStateMachine, gridManager);
            MatchedTileState = new MatchedState(this, TileStateMachine);
            GridGameOverTileState = new GameOverTileState(this, TileStateMachine, gridManager);

            TileStateMachine.Initialize(WaitingTileState);
        }
        public void Destroy()
        {
            EventManager.InvokeGridEventTileDestroyed(_gridManager.GridID);
            Destroy(this.gameObject);
        }
        #endregion
        
        #region EventsCallback
        private void OnGridGameOver(Guid gridID)
        {
            if (gridID != _gridManager.GridID)
            {
                return;
            }
            TileStateMachine.ChangeState(GridGameOverTileState);
        }
        #endregion

        #region Grid Related
        public void SetGridPosition(Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            TemporaryGridPosition = null;
            if(GridPosition.HasValue)
                transform.position = Utilities.GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, GridPosition.Value);
            TileStateMachine.ChangeState(PlacedOnGridTileState);
        }
        public void SetTemporaryGridPosition(Vector2Int gridPosition, bool updateWorldPosition)
        {
            TemporaryGridPosition = gridPosition;
            GridPosition = null;
            if(TemporaryGridPosition.HasValue && updateWorldPosition)
                transform.position = Utilities.GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, TemporaryGridPosition.Value);
        }
        #endregion
        
        #region Sprites and Animations
        public void SetFallingRootSprite()
        {
            _tileSpriteRenderer.sprite = Data.RootTileSprite;
        }
        public void SetDefaultSprite()
        {
            _tileSpriteRenderer.sprite = Data.DefaultSprite;
        }
        public void UpdateTileSpriteWithConnections(TileData.TileConnections connections)
        {
            _tileSpriteRenderer.sprite = Data.GetSpriteForConnection(connections);
        }
        public void PlayDeathAnimation()
        {
            //_animationComponent.Play(Data.DeathAnimation.name.ToString());
        }
        #endregion
    }
}