using System;
using System.Collections;
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
        
        public Tile BeginPairTile { get; private set; }
        #endregion

        #region Private Variable
        private GridManager _gridManager;
        private Tile _rootPair;
        #endregion
        
        #region States

        public StateMachine<TileState> TileStateMachine { get; private set; }

        public WaitingState WaitingTileState { get; private set; }
        public FallingRootState FallingRootTileState { get; private set; }
        public FallingChildState FallingTileChildState { get; private set; }
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
        public void InitializeTile(GridManager gridManager, TileData tileData, Vector2Int initialGridPosition, bool isRoot)
        {
            //this.OwnerGridID = gridManager.GridID;
            this.Data = tileData;
            this.IsRoot = isRoot;
            this._gridManager = gridManager;
            _tileSpriteRenderer.sprite = tileData.DefaultSprite;
            SetTemporaryGridPosition(initialGridPosition);

            TileStateMachine = new StateMachine<TileState>();

            WaitingTileState = new WaitingState(this, TileStateMachine, _gridManager);
            FallingRootTileState = new FallingRootState(this, TileStateMachine, _gridManager);
            FallingTileChildState = new FallingChildState(this, TileStateMachine, _gridManager);
            PlacedOnGridTileState = new PlacedOnGridState(this, TileStateMachine, gridManager);
            MatchedTileState = new MatchedState(this, TileStateMachine);
            GridGameOverTileState = new GameOverTileState(this, TileStateMachine, gridManager);

            TileStateMachine.Initialize(WaitingTileState);
        }
        
        public void SetBeginPair(Tile tile)
        {
            BeginPairTile = tile;
        }

        public void Destroy()
        {
            EventManager.InvokeEventTileDestroyed(_gridManager.GridID);
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
            if(GridPosition.HasValue)
                transform.position = Utilities.GridUtilities.GetGridCellWorldPosition(_gridManager.Grid, GridPosition.Value);
            TileStateMachine.ChangeState(PlacedOnGridTileState);
        }
        public void SetTemporaryGridPosition(Vector2Int gridPosition)
        {
            TemporaryGridPosition = gridPosition;
            GridPosition = null;
        }

        public void StartToMoveHorizontal(float moveDuration, float targetHorizontalPosition)
        {
            StartCoroutine(MoveToHorizontal(moveDuration, targetHorizontalPosition));
        }
        
        IEnumerator MoveToHorizontal(float moveDuration, float targetHorizontalPosition)
        {
            Vector3 initialPosition = transform.position;
            float elapsedTime = 0;

            while (elapsedTime < moveDuration)
            {
               float t = elapsedTime / moveDuration;
               var target = new Vector3(targetHorizontalPosition, transform.position.y, 0);
               transform.position = Vector3.Lerp(initialPosition, target, t);
               elapsedTime += Time.deltaTime;
               yield return null;
            }
            transform.position =  new Vector3(targetHorizontalPosition, transform.position.y, 0);
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