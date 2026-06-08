using R3;
using RMC.d.Scenes;
using UnityEngine;
using UnityEngine.UIElements;

namespace RMC.d.UI
{
    /// <summary>
    /// Renders user interface elements onto the Unity screen.
    /// </summary>
    public class GameView : MonoBehaviour
    {
        //  Properties ------------------------------------
        public Label LivesLabel { get { return _uiDocument?.rootVisualElement.Q<Label>("UpperLeftLabel"); }}
        public Label ScoreLabel { get { return _uiDocument?.rootVisualElement.Q<Label>("UpperRightLabel"); }}
        public VisualElement InstructionsContainer { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("LowerLeftContainer"); } }
        public Label InstructionsLabel { get { return _uiDocument?.rootVisualElement.Q<Label>("LowerLeftLabel"); }}
        public VisualElement TitleContainer { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("LowerRightContainer"); } }
        public Label TitleLabel { get { return _uiDocument?.rootVisualElement.Q<Label>("LowerRightLabel"); }}
        public VisualElement CenterContainer { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("CenterContainer"); } }
        public Label CenterLabel { get { return _uiDocument?.rootVisualElement.Q<Label>("CenterLabel"); } }
        public VisualElement TouchControlsContainer { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("TouchControlsContainer"); } }
        public VisualElement TouchMovePad { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("TouchMovePad"); } }
        public VisualElement TouchMoveKnob { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("TouchMoveKnob"); } }
        public VisualElement TouchJumpButton { get { return _uiDocument?.rootVisualElement.Q<VisualElement>("TouchJumpButton"); } }
        public Vector2 TouchMoveInput { get { return _touchMoveInput; } }

        //  Fields ----------------------------------------
        [SerializeField]
        private UIDocument _uiDocument;
        private GameModel _gameModel;
        private CompositeDisposable _disposable = new CompositeDisposable();
        private Vector2 _touchMoveInput = Vector2.zero;
        private int _touchMovePointerId = -1;
        private bool _touchJumpRequested = false;
        private bool _isTouchControlsConfigured = false;
        public bool IsInitialized { get; private set; }

        //  Unity Methods ---------------------------------

        protected void OnDestroy()
        {
            Debug.Log($"{GetType().Name}.Dispose()");
            _disposable?.Dispose();
            IsInitialized = false;
        }

        //  Methods ---------------------------------------
        public void Initialize(GameModel gameModel)
        {
            Debug.Log($"{GetType().Name}.Initialize()");

            if (IsInitialized)
            {
                return;
            }

            IsInitialized = true;

            // Model
            _gameModel = gameModel;
            _gameModel.Lives.Subscribe(GameModel_OnLivesChanged).AddTo(_disposable);
            _gameModel.Score.Subscribe(GameModel_OnScoreChanged).AddTo(_disposable);
            _gameModel.Title.Subscribe(GameModel_OnTitleChanged).AddTo(_disposable);
            _gameModel.Instructions.Subscribe(GameModel_OnInstructionsChanged).AddTo(_disposable);
            _gameModel.State.Subscribe(GameModel_OnStateChanged).AddTo(_disposable);
            _gameModel.Prompt.Subscribe(GameModel_OnPromptChanged).AddTo(_disposable);

            // Initialize current UI state immediately
            ConfigureTouchControls();
            GameModel_OnLivesChanged(_gameModel.Lives.Value);
            GameModel_OnScoreChanged(_gameModel.Score.Value);
            GameModel_OnTitleChanged(_gameModel.Title.Value);
            GameModel_OnInstructionsChanged(_gameModel.Instructions.Value);
            GameModel_OnStateChanged(_gameModel.State.Value);
            GameModel_OnPromptChanged(_gameModel.Prompt.Value);
        }

        public bool ConsumeTouchJumpRequest()
        {
            bool touchJumpRequested = _touchJumpRequested;
            _touchJumpRequested = false;
            return touchJumpRequested;
        }

        public void ResetTouchInput()
        {
            _touchJumpRequested = false;

            if (TouchJumpButton != null)
            {
                TouchJumpButton.RemoveFromClassList("touch-jump-button--pressed");
            }

            if (TouchMovePad == null || TouchMoveKnob == null)
            {
                _touchMoveInput = Vector2.zero;
                _touchMovePointerId = -1;
                return;
            }

            ReleaseTouchMovePointer();
        }

        //  Event Handlers --------------------------------

        private void GameModel_OnInstructionsChanged(string value)
        {
            if (ShouldShowTouchControls())
            {
                if (InstructionsContainer != null)
                {
                    InstructionsContainer.style.display = DisplayStyle.None;
                }
                return;
            }

            if (InstructionsContainer != null)
            {
                InstructionsContainer.style.display = DisplayStyle.Flex;
            }

            if (InstructionsLabel != null)
            {
                InstructionsLabel.text = $"{value}";
            }
        }

        private void GameModel_OnTitleChanged(string value)
        {
            if (ShouldShowTouchControls())
            {
                if (TitleContainer != null)
                {
                    TitleContainer.style.display = DisplayStyle.None;
                }
                return;
            }

            if (TitleContainer != null)
            {
                TitleContainer.style.display = DisplayStyle.Flex;
            }

            if (TitleLabel != null)
            {
                TitleLabel.text = $"{value}";
            }
        }

        private void GameModel_OnScoreChanged(int value)
        {
            ScoreLabel.text = $"Score: {_gameModel.Score.Value:000}/{GameModel.ScoreMax:000}";
        }

        private void GameModel_OnLivesChanged(int value)
        {
            LivesLabel.text = $"Lives: {_gameModel.Lives.Value:000}/{GameModel.LivesMax:000}";
        }

        private void GameModel_OnStateChanged(GameState state)
        {
            // Show/Hide the centered overlay during start/stop phases
            if (CenterContainer == null)
            {
                return;
            }

            bool isVisible = state == GameState.GameStarting || state == GameState.GameStopping;
            CenterContainer.style.opacity = isVisible ? 1 : 0;
        }

        private void GameModel_OnPromptChanged(string text)
        {
            if (CenterLabel != null)
            {
                CenterLabel.text = text ?? string.Empty;
            }
        }

        private void ConfigureTouchControls()
        {
            if (_isTouchControlsConfigured)
            {
                return;
            }

            _isTouchControlsConfigured = true;

            if (TouchControlsContainer == null || TouchMovePad == null || TouchMoveKnob == null || TouchJumpButton == null)
            {
                return;
            }

            TouchControlsContainer.pickingMode = PickingMode.Ignore;

            bool showTouchControls = ShouldShowTouchControls();
            TouchControlsContainer.style.display = showTouchControls ? DisplayStyle.Flex : DisplayStyle.None;

            if (!showTouchControls)
            {
                return;
            }

            TouchMovePad.pickingMode = PickingMode.Position;
            TouchJumpButton.pickingMode = PickingMode.Position;

            TouchMovePad.RegisterCallback<GeometryChangedEvent>(TouchMovePad_OnGeometryChanged);
            TouchMovePad.RegisterCallback<PointerDownEvent>(TouchMovePad_OnPointerDown);
            TouchMovePad.RegisterCallback<PointerMoveEvent>(TouchMovePad_OnPointerMove);
            TouchMovePad.RegisterCallback<PointerUpEvent>(TouchMovePad_OnPointerUp);
            TouchMovePad.RegisterCallback<PointerCancelEvent>(TouchMovePad_OnPointerCancel);

            TouchJumpButton.RegisterCallback<PointerDownEvent>(TouchJumpButton_OnPointerDown);
            TouchJumpButton.RegisterCallback<PointerUpEvent>(TouchJumpButton_OnPointerUp);
            TouchJumpButton.RegisterCallback<PointerCancelEvent>(TouchJumpButton_OnPointerCancel);

            ResetTouchMoveKnob();
        }

        private bool ShouldShowTouchControls()
        {
            return Application.isMobilePlatform && TouchControlsContainer != null;
        }

        private void TouchMovePad_OnGeometryChanged(GeometryChangedEvent evt)
        {
            ResetTouchMoveKnob();
        }

        private void TouchMovePad_OnPointerDown(PointerDownEvent evt)
        {
            if (_touchMovePointerId != -1)
            {
                return;
            }

            _touchMovePointerId = evt.pointerId;
            TouchMovePad.CapturePointer(evt.pointerId);
            UpdateTouchMove(evt.localPosition);
            evt.StopPropagation();
        }

        private void TouchMovePad_OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _touchMovePointerId)
            {
                return;
            }

            UpdateTouchMove(evt.localPosition);
            evt.StopPropagation();
        }

        private void TouchMovePad_OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _touchMovePointerId)
            {
                return;
            }

            ReleaseTouchMovePointer();
            evt.StopPropagation();
        }

        private void TouchMovePad_OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != _touchMovePointerId)
            {
                return;
            }

            ReleaseTouchMovePointer();
            evt.StopPropagation();
        }

        private void TouchJumpButton_OnPointerDown(PointerDownEvent evt)
        {
            _touchJumpRequested = true;
            TouchJumpButton.AddToClassList("touch-jump-button--pressed");
            evt.StopPropagation();
        }

        private void TouchJumpButton_OnPointerUp(PointerUpEvent evt)
        {
            TouchJumpButton.RemoveFromClassList("touch-jump-button--pressed");
            evt.StopPropagation();
        }

        private void TouchJumpButton_OnPointerCancel(PointerCancelEvent evt)
        {
            TouchJumpButton.RemoveFromClassList("touch-jump-button--pressed");
            evt.StopPropagation();
        }

        private void UpdateTouchMove(Vector2 localPosition)
        {
            float padWidth = TouchMovePad.resolvedStyle.width;
            float padHeight = TouchMovePad.resolvedStyle.height;
            float padRadius = Mathf.Min(padWidth, padHeight) * 0.5f;

            if (padRadius <= 0)
            {
                _touchMoveInput = Vector2.zero;
                return;
            }

            Vector2 padCenter = new Vector2(padWidth * 0.5f, padHeight * 0.5f);
            Vector2 delta = new Vector2(localPosition.x - padCenter.x, padCenter.y - localPosition.y);
            _touchMoveInput = Vector2.ClampMagnitude(delta / padRadius, 1f);

            float knobTravel = padRadius * 0.55f;
            Vector2 knobOffset = new Vector2(_touchMoveInput.x, -_touchMoveInput.y) * knobTravel;
            PositionTouchMoveKnob(knobOffset);
        }

        private void ReleaseTouchMovePointer()
        {
            if (_touchMovePointerId != -1)
            {
                TouchMovePad.ReleasePointer(_touchMovePointerId);
            }

            _touchMovePointerId = -1;
            _touchMoveInput = Vector2.zero;
            ResetTouchMoveKnob();
        }

        private void ResetTouchMoveKnob()
        {
            PositionTouchMoveKnob(Vector2.zero);
        }

        private void PositionTouchMoveKnob(Vector2 knobOffset)
        {
            float knobHalfWidth = TouchMoveKnob.resolvedStyle.width * 0.5f;
            float knobHalfHeight = TouchMoveKnob.resolvedStyle.height * 0.5f;
            float padHalfWidth = TouchMovePad.resolvedStyle.width * 0.5f;
            float padHalfHeight = TouchMovePad.resolvedStyle.height * 0.5f;

            if (padHalfWidth <= 0 || padHalfHeight <= 0 || knobHalfWidth <= 0 || knobHalfHeight <= 0)
            {
                return;
            }

            TouchMoveKnob.style.left = padHalfWidth - knobHalfWidth + knobOffset.x;
            TouchMoveKnob.style.top = padHalfHeight - knobHalfHeight + knobOffset.y;
        }
    }
}
