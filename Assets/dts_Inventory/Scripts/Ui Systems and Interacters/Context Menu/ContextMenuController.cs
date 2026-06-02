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
        private HashSet<ContextOption> _setOptions = new();
        private List<GridInvButton> _activeBtnOptions = new();
        private List<GridInvButton> _allBtnOptions = new();
        private GridInvButton _hoveredBtn;
        [SerializeField] private bool _isFocused = false;
        [SerializeField] private bool _isShowing = false;
        [SerializeField] private int _currentHoveredBtnIndex = -1;


        //unity Events
        [Header("Core Events")]
        public UnityEvent OnContextMenuShown;
        public UnityEvent OnContextMenuHidden;

        public UnityEvent OnUiFocused;
        public UnityEvent OnUiUnfocused;

        public UnityEvent OnMenuRebuilt;
        public UnityEvent OnContextSet;

        public UnityEvent<ContextOption,int,int> OnNumericalSelectorRequested;

        [Header("Contextual Events")]
        public UnityEvent<int> OnOrganize;
        public UnityEvent<int> OnUse;
        public UnityEvent<int> OnDiscard;
        public UnityEvent<int> OnBuy;
        public UnityEvent<int> OnSell;
        public UnityEvent<int> OnTransfer;
        public UnityEvent<int> OnTake;
        

        private (int, int) _positionContext = (-1,-1);
        private InvGrid _gridContext;
        private ContextOption _selectedContextOption = ContextOption.None;
        private Vector2Int _interactionRange = Vector2Int.one;
        private int _interactionAmount = 0;
        

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
            _selectedContextOption = ContextOption.None;
            _interactionAmount = 0;
            _interactionRange = Vector2Int.one;

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

            if (_activeBtnOptions.Count <= 1)
                return;

            //Imagine we're stepping through a list that's rendered as 2 columns:
            //     Btn 0    Btn 1
            //     Btn 2    Btn 3
            //     Btn 4    Btn 5
            //     ...

            // moving left or right only moves 1 index (unity knows where each btn is, we only need to know the indexes)
            // moving up or down visually would logically jump over an index position.
            // Below we're calculating the logical index jump from whatever input direction we receive


            
            //left move : -1
            if (direction.x < -0.1f)
            {
                //if our current position is even, wrap to the other side of the menu
                if (_currentHoveredBtnIndex % 2 == 0)
                {
                    //wrap to [currentPosition + 1] position (assuming it exists)
                    if (_currentHoveredBtnIndex + 1 < _activeBtnOptions.Count)
                        SetButtonAsHovered(_currentHoveredBtnIndex + 1);
                }

                //just go to the left
                else SetButtonAsHovered(_currentHoveredBtnIndex - 1);
            }
                

            //right move : +1
            if (direction.x > 0.1f)
            {
                //if our current position is odd, wrap to the other side of the menu
                if (_currentHoveredBtnIndex % 2 == 1)
                {
                    SetButtonAsHovered(_currentHoveredBtnIndex - 1);

                }

                //just go to the right (assuming the position exists)
                else if (_currentHoveredBtnIndex + 1 < _activeBtnOptions.Count)
                    SetButtonAsHovered(_currentHoveredBtnIndex + 1);
            }

            //up move : -2
            if (direction.y > 0.1f)
            {
                //only calculate vertical movement if we have more than 1 row
                if (_activeBtnOptions.Count > 2)
                {
                    //if we can go up without needing to wrap, do it
                    if (_currentHoveredBtnIndex - 2 >= 0)
                        SetButtonAsHovered(_currentHoveredBtnIndex - 2);

                    else
                    {
                        //calculate the wrapped index
                        int wrapIndex = _activeBtnOptions.Count + (_currentHoveredBtnIndex - 2);

                        //if the number of menu options are even, then we can wrap naturally
                        if (_activeBtnOptions.Count % 2 == 0)
                            SetButtonAsHovered(wrapIndex);

                        else
                        {
                            if (_currentHoveredBtnIndex % 2 == 0)
                                SetButtonAsHovered(wrapIndex + 1); // offset the wrap to remain on the evens side
                            else SetButtonAsHovered(wrapIndex - 1); // offset the wrap to remain on the odds side
                        }
                        
                    }
                }
            }

            //down move : +2
            if (direction.y < -0.1f)
            {
                //only calculate vertical movement if we have more than 1 row
                if (_activeBtnOptions.Count > 2)
                {
                    //if we can go down without needing to wrap, do it
                    if (_currentHoveredBtnIndex + 2 <= _activeBtnOptions.Count)
                        SetButtonAsHovered(_currentHoveredBtnIndex + 2);

                    else
                    {
                        //calculate the wrapped index
                        int wrapIndex = (_currentHoveredBtnIndex + 2) - _activeBtnOptions.Count;

                        //if the number of menu options are even, then we can wrap naturally
                        if (_activeBtnOptions.Count % 2 == 0)
                            SetButtonAsHovered(wrapIndex);

                        else
                        {
                            if (_currentHoveredBtnIndex % 2 == 0)
                                SetButtonAsHovered(wrapIndex - 1); // offset the wrap to remain on the evens side
                            else SetButtonAsHovered(wrapIndex + 1); // offset the wrap to remain on the odds side
                        }

                    }
                }
            }



        }

        private void TriggerInteractionEvent(int amountToInteractWith)
        {
            //ebug.Log($"Triggering interaction event: {_selectedContextOption}");
            switch (_selectedContextOption)
            {
                case ContextOption.OrganizeItem:
                    //Debug.Log($"Organize Contextual Event firing");
                    OnOrganize?.Invoke(amountToInteractWith);
                    break;

                case ContextOption.UseItem:
                    //Debug.Log($"Use Contextual Event firing");
                    OnUse?.Invoke(amountToInteractWith);
                    break;

                case ContextOption.DiscardItem:
                    //Debug.Log($"Discard Contextual Event firing");
                    OnDiscard?.Invoke(amountToInteractWith);
                    break;

                case ContextOption.BuyItem:
                    OnBuy?.Invoke(amountToInteractWith);
                    break;

                case ContextOption.SellItem:
                    OnSell?.Invoke(amountToInteractWith);
                    break;

                case ContextOption.TransferItem:
                    OnTransfer?.Invoke(amountToInteractWith);
                    break;

                case ContextOption.TakeItem:
                    OnTake?.Invoke(amountToInteractWith);
                    break;

            }
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
        public void RespondToContextSelection(ContextualOptionDefinition contextOption)
        {
            //ignore any selections that're made if the contextMenu isn't the active Ui focus
            if (!_isShowing || !_isFocused)
                return;

            _selectedContextOption = contextOption.GetContextOption();

            //setup the numerical selector
            _interactionRange.x = 1;
            _interactionRange.y = _gridContext.GetStackValue(_positionContext);

            //auto complete the interaction if you can only interact with 1
            if (_interactionRange.y == 1)
            {
                _interactionAmount = 1;
                TriggerInteractionEvent(_interactionAmount);
            }

            //otherwise show the numerical selector. Let the user choose how many to interact with.
            else OnNumericalSelectorRequested?.Invoke(_selectedContextOption, _interactionRange.x, _interactionRange.y);

        }

        public void RespondToNumericalSelection(int selectedValue)
        {
            //Debug.Log($"Caught Submitted number; {selectedValue}");
            TriggerInteractionEvent(selectedValue);
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
                UpdateGIMOnShown(this);
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
                UpdateGIMOnHidden(this);
                OnContextMenuHidden?.Invoke();
            }
        }

        public void UpdateGIMOnShown(IGridUiElement self) { GIMHelper.UpdateGIMOnShown(this); }
        public void UpdateGIMOnHidden(IGridUiElement self) { GIMHelper.UpdateGIMOnHidden(this); }

        public void FocusOnUi()
        {
            if (!_isFocused)
            {
                _isFocused = true;
                OnUiFocused?.Invoke();
            }
        }

        public void UnfocusOnUi()
        {
            if (_isFocused)
            {
                _isFocused = false;
                OnUiUnfocused?.Invoke();
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
            if (!_isShowing || !_isFocused)
                return;

            if (_currentHoveredBtnIndex > -1 && _currentHoveredBtnIndex < _activeBtnOptions.Count)
            {
                _activeBtnOptions[_currentHoveredBtnIndex].onClick?.Invoke();
            }
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

        public bool IsShown()
        {
            return _isShowing;
        }
        public bool IsFocused() {  return _isFocused; }
    }
}

