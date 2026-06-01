using dtsInventory;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace dtsInventory
{
    public class ContextMenuController : MonoBehaviour, IGridUiElement
    {
        [SerializeField] private GameObject _contextMenu;
        [SerializeField] private RectTransform _btnOptionsContainer;

        //unity Events
        public UnityEvent OnContextMenuShown;
        public UnityEvent OnContextMenuHidden;

        public UnityEvent OnUiEnteredAsFocused;
        public UnityEvent OnUiExitedFromFocused;

        public UnityEvent OnMenuRebuilt;
        public UnityEvent OnContextSet;

        private HashSet<ContextOption> _setOptions = new();
        private List<GridInvButton> _activeBtnOptions = new();
        private List<GridInvButton> _allBtnOptions = new();
        private GridInvButton _hoveredBtn;
        private bool _isFocused = false;
        [SerializeField] private bool _isShowing = false;

        private (int, int) _positionContext = (-1,-1);
        private InvGrid _gridContext;
        [SerializeField] private int _currentHoveredBtnIndex = -1;

        private void Awake()
        {
            DetectAllPossibleBtnOptions();
        }




        //internals
        private void DetectAllPossibleBtnOptions()
        {
            //Collect all possible btn options
            for (int i = 0; i < _btnOptionsContainer.childCount; i++)
            {
                Transform child = _btnOptionsContainer.GetChild(i);
                ContextualOptionDefinition context = child.GetComponent<ContextualOptionDefinition>();
                GridInvButton btn = child.GetComponent<GridInvButton>();

                if (context != null && !_allBtnOptions.Contains(btn))
                    _allBtnOptions.Add(btn);

            }
        }
        private bool BuildMenu()
        {
            int optionCount = 0;
            _activeBtnOptions.Clear();

            //show all matching context buttons, and hide all buttons that don't match the context
            for (int i = 0; i < _allBtnOptions.Count; i++)
            {
                
                ContextualOptionDefinition context = _allBtnOptions[i].gameObject.GetComponent<ContextualOptionDefinition>();

                if (_setOptions.Contains(context.GetContextOption()))
                {
                    context.gameObject.SetActive(true);
                    optionCount++;
                    _activeBtnOptions.Add(_allBtnOptions[i]);
                }
                else
                {
                    context.gameObject.SetActive(false);
                }
            }

            

            //return success if the menu has contexts available
            if (optionCount > 0)
            {
                OnMenuRebuilt?.Invoke();
                return true;
            }
            else return false;
        }
        private void ClearContext()
        {
            _gridContext = null;
            _positionContext = (-1, -1);
            _setOptions.Clear();

            foreach (GridInvButton btn in _activeBtnOptions)
                btn.gameObject.SetActive(false);

            _activeBtnOptions.Clear();
        }
        private void ResetNav()
        {
            _currentHoveredBtnIndex = -1;

        }





        //internals
        private void SetButtonAsHovered(int index)
        {

            //ignore if index out of range
            if (_activeBtnOptions.Count == 0 || index >= _activeBtnOptions.Count || index < 0)
                return;

            //ignore if the btn's already hovered
            if (_activeBtnOptions[index] == _hoveredBtn)
                return;

            //Clear the previously-hovered btn
            ClearHoveredButton();

            //ensure the new btn is hovered
            _hoveredBtn = _activeBtnOptions[index];
            _hoveredBtn.SetButtonState(GIButtonState.Highlighted);
            _currentHoveredBtnIndex = index;

        }

        private void ClearHoveredButton()
        {
            //exit the previous-hovered btn (if it exists)
            if (_hoveredBtn != null)
            {
                _hoveredBtn.SetButtonState(GIButtonState.Normal);
                _hoveredBtn = null;
                _currentHoveredBtnIndex = -1;
            }
        }

        
        private void CalculateHoveredBtnFromTwoColumnNavInput(Vector2 direction)
        {
            //default the menu to the first element
            if (_currentHoveredBtnIndex == -1)
            {
                _currentHoveredBtnIndex = 0;
                SetButtonAsHovered(_currentHoveredBtnIndex);
                return;
            }


            //Imagine we're stepping through a list that's rendered as 2 columns:
            //     Btn 0    Btn 1
            //     Btn 2    Btn 3
            //     Btn 4    Btn 5
            //     ...

            // moving left or right only moves 1 index (unity knows where each btn is, we only need to know the indexes)
            // moving up or down visually would logically jump over an index position.
            // Below we're calculating the logical index jump from whatever input direction we receive


            int stepCount = 0;
            
            //left move : -1
            if (direction.x < -0.1f)
                stepCount += -1;

            //right move : +1
            if (direction.x > 0.1f)
                stepCount += 1;

            //up move : -2
            if (direction.y > 0.1f)
                stepCount += -2;

            //down move : +2
            if (direction.y < -0.1f)
                stepCount += 2;


            int newHoverBtnIndex = _currentHoveredBtnIndex + stepCount;

            //ensure the while loops don't repeat forever, under any circumstance
            int iterationsMax = 10;
            int currentIterations = 0;

            while (newHoverBtnIndex < 0 && currentIterations < iterationsMax)  //
            {
                //offset by the list's btn count.
                newHoverBtnIndex += _activeBtnOptions.Count;
                currentIterations++;
            }

            currentIterations = 0;
            while (newHoverBtnIndex >= _activeBtnOptions.Count && currentIterations < iterationsMax)
            {
                //offset by the list's btn count.
                newHoverBtnIndex -= _activeBtnOptions.Count;
                currentIterations++;
            }

            //detect if an infinite while was caught
            if (currentIterations >= iterationsMax && (newHoverBtnIndex < 0 || newHoverBtnIndex >= _activeBtnOptions.Count))
            {
                Debug.LogWarning("Failed to calculate the step count. An infinite loop was possibly detected. Ignoring directional navigation.");
                return;
            }

            _currentHoveredBtnIndex = newHoverBtnIndex;
            SetButtonAsHovered(_currentHoveredBtnIndex);


        }




        //externals
        public void RespondToPointerHoverOnBtn(GridInvButton hoveredBtn)
        {

            //don't respond if the menu isn't showing or isn't the ui's focus
            if (!_isShowing || !_isFocused)
                return;

            if (_hoveredBtn == hoveredBtn)
                return;

            //update the current hovered btn if it exists within the active btn list
            if (_activeBtnOptions.Contains(hoveredBtn))
            {
                int index = _activeBtnOptions.IndexOf(hoveredBtn);
                SetButtonAsHovered(index);
                
            }
        }
        public void RespondToPointerExitedBtnHover(GridInvButton exitedBtn)
        {

            //don't respond if the menu isn't showing or isn't the ui's focus
            if (!_isShowing || !_isFocused)
                return;

            //update the exited button, regardless of it's current state
            exitedBtn.SetButtonState(GIButtonState.Normal);

            //if this exited button is our currently hovered btn, update our internal hovered-btn state
            if (_hoveredBtn == exitedBtn)
            {
                _hoveredBtn = null;
                _currentHoveredBtnIndex = -1;
            }


        }
        public void SetContext(HashSet<ContextOption> options,(int,int) position, InvGrid grid)
        {
            if (options == null)
                return;
            if (options.Count < 1)
                return;

            _setOptions.Clear();

            //copy the provided set, to avoid accidental editing of the reference
            foreach (ContextOption option in options)
                _setOptions.Add(option);


            if (BuildMenu())
            {
                _gridContext = grid;
                _positionContext = position;
                SetMenuPosition(position, grid);
                OnContextSet?.Invoke();
            }
            else
                ClearContext();


        }
        public void SetMenuPosition((int,int) position, InvGrid grid)
        {
            RectTransform cellRectTransform = grid.GetCellObject(position).GetComponent<RectTransform>();
            Vector3 screenPosition = cellRectTransform.transform.position; //canvas is an overlay; transform.position == screen postiion
            transform.position = screenPosition;
        }
        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void ShowUi()
        {
            if (!_isShowing)
            {
                _isShowing = true;
                _contextMenu.SetActive(true);
                ResetNav();
                OnContextMenuShown?.Invoke();
            }
        }

        public void HideUi()
        {
            if (_isShowing)
            {
                _isShowing= false;
                _contextMenu.SetActive(false);

                ClearContext();
                ResetNav();
                OnContextMenuHidden?.Invoke();
            }
        }

        public void FocusOnUi()
        {
            if (!_isFocused)
            {
                _isFocused = true;
                GIMHelper.FocusOnGridElement(this);
                OnUiEnteredAsFocused?.Invoke();
            }
        }

        public void UnfocusOnUi()
        {
            if (_isFocused)
            {
                _isFocused = false;
                GIMHelper.ClearElementFromFocus(this);
                OnUiExitedFromFocused?.Invoke();
            }
        }

        public void RespondToPrimaryDirectionalInput(Vector2 input)
        {
            CalculateHoveredBtnFromTwoColumnNavInput(input);
        }

        public void RespondToSecondaryDirectionalInput(Vector2 input)
        {
            //...
        }

        public void RespondToTertiaryDirectionalInput(Vector2 input)
        {
            //...
        }

        public void RespondToLightLeftAction()
        {
            //...
        }

        public void RespondToHeavyLeftAction()
        {
            //...
        }

        public void RespondToLightRightAction()
        {
            //...
        }

        public void RespondToHeavyRightAction()
        {
            //...
        }

        public void RespondToConfirmInput()
        {
            //...
        }

        public void RespondToCancelInput()
        {
            HideUi();
        }

        public void RespondToJumpHotkey()
        {
            //...
        }

        public void RespondToEditHotkey()
        {
            //...
        }

        public void ReadAlphaInput(bool input)
        {
            //...
        }

        public void ReadBetaInput(bool input)
        {
            //...
        }

        public void ReadGammaInput(bool input)
        {
            //...
        }
    }
}

