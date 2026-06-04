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
        [Tooltip("Include any context options that require another external grid as a target to complete. For example:\n" +
            "'Buy' is triggered in a grid and requires another grid location to store bought goods.\n" +
            "'Sell' is triggered in a grid and requires another merchant grid to accept the goods. \n" +
            "'Transfer' requires a target grid to transfer the specified items to.\n" +
            "'Take' is triggered in nonPersonal grids, but still requires a reference to that personal grid to place the specified items.\n" +
            "You don't need to reference the prevous 4 mentioned above in the collection, since they're built-in by default.")]
        [SerializeField] private List<ContextOption> _otherMultiContainerContexts = new List<ContextOption>() { };
        private HashSet<ContextOption> _inferredContextOptions = new();
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
        public UnityEvent<List<InvGrid>> OnTransferMenuRequested;


        [Header("Contextual Events")]
        public UnityEvent<int> OnOrganize;
        public UnityEvent<int> OnUse;
        public UnityEvent<int> OnDiscard;
        [Tooltip("Given values are the amount bought, and where to send the bought goods")]
        public UnityEvent<int,InvGrid> OnBuy;
        [Tooltip("Given values are the amount sold, and where to send the sold goods")]
        public UnityEvent<int, InvGrid> OnSell;
        [Tooltip("Given values are the amount to transfer, and where to send the goods")]
        public UnityEvent<int, InvGrid> OnTransfer;
        [Tooltip("Given values are the amount to take, and where to send the taken goods. " +
            "Take implies that the goods are going to some main personal container, where Transfer is more" +
            " general and includes moving items from any container into any container.")]
        public UnityEvent<int, InvGrid> OnTake;

        private InvGrid _gridContext;
        private (int, int) _positionContext;
        private InvItem _itemContext;
        
        private InvGrid _selectedContextGridTarget;
        private List<InvGrid> _takeContextGridTargets = new();      //all possible contexts for the transfer menu, cached [if 'take' selected]
        private List<InvGrid> _transferContextGridTargets = new();  //all possible contexts for the transfer menu, cached [if 'transfer' selected]
        private List<InvGrid> _buyContextGridTargets = new();       //all possible contexts for the transfer menu, cached [if 'buy' selected]
        private List<InvGrid> _sellContextGridTargets = new();      //all possible contexts for the transfer menu, cached [if 'sell' selected]
        private ContextOption _selectedContextOption = ContextOption.None;
        private Vector2Int _interactionRange = Vector2Int.one;
        private int _interactionAmount = 0;
        private ContextOption[] _defaultMultiContainerContextsArry = { ContextOption.TakeItem, ContextOption.TransferItem, ContextOption.BuyItem, ContextOption.SellItem };
        private List<ContextOption> _defaultMultiContainerContexts = new();

        private void Awake()
        {
            DetectAllPossibleBtnOptions();

            //cache the default contexts list for readability later
            _defaultMultiContainerContexts.AddRange( _defaultMultiContainerContextsArry );
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

                if (_inferredContextOptions.Contains(context.GetContextOption()))
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
            _inferredContextOptions.Clear();

            _gridContext = null;
            _itemContext = null;
            _buyContextGridTargets.Clear();
            _sellContextGridTargets.Clear();
            _takeContextGridTargets.Clear();
            _transferContextGridTargets.Clear();

            _selectedContextGridTarget = null;
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
                    OnBuy?.Invoke(amountToInteractWith,_selectedContextGridTarget);
                    break;

                case ContextOption.SellItem:
                    OnSell?.Invoke(amountToInteractWith, _selectedContextGridTarget);
                    break;

                case ContextOption.TransferItem:
                    OnTransfer?.Invoke(amountToInteractWith, _selectedContextGridTarget);
                    break;

                case ContextOption.TakeItem:
                    OnTake?.Invoke(amountToInteractWith, _selectedContextGridTarget);
                    break;

            }
        }

        /// <summary>
        /// Reads the grid's personal contexts, the item's contexts at the given grid position, and creates a set of all the possible contexts.
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <param name="grid"></param>
        /// <returns>A set of all the possible contexts a user may choose.</returns>
        private HashSet<ContextOption> InferContextOptions(InvItem item, InvGrid grid)
        {
            HashSet<ContextOption> inferredOptions = new();

            // you can only buy items if they're in a merchant's inventory
            // you can't do anything else with them until you buy them
            if (grid.IsMerchant())
                inferredOptions.Add(ContextOption.BuyItem);

            //otherwise you do whatever you want
            //limited to the specific item's useability
            else
            {
                //start with all of the item's base contexts enabled
                foreach (ContextOption option in item.ItemData().ContextualOptions())
                    inferredOptions.Add(option);

                int openedGrids = GIMHelper.CountOpenedGrids();

                int openedMerchants = GIMHelper.CountOpenedMerchants();
                int openedNonMerchants = openedGrids - openedMerchants; //merchants are a subset of all opened grids. Subtract them.

                int openedPersonalGrids = GIMHelper.CountOpenedPersonalGrids();
                int openedNonPersonalGrids = openedGrids - openedPersonalGrids; //personal grids are a subset of all opened grids. Subtract them.

                //Personal grids and merchants grids should always be mutually exclusive
                //[Why would the player need to buy items from their own container?]


                //disable 'sell' if the context isn't right
                if (inferredOptions.Contains(ContextOption.SellItem))
                {
                    //disable selling if you can't sell items from this grid
                    //also disable selling if no opened merchant grids exist to receive the items
                    if (!grid.CanSellFromThisInventory() || openedMerchants < 1) //the selected grid will never be a merchant in this case
                        inferredOptions.Remove(ContextOption.SellItem);
                }

                //transfer & take must always be added manually.
                //Determine if either of those contexts are relevant.

                //enable 'take' if we're grabing something from a nonPersonal grid (assuming the player has a place to store their possessions)
                if (!grid.IsPersonal() && GIMHelper.CountPersonalGrids() > 0)
                {   
                    //save each personal grid as a potential 'take' context target
                    foreach (InvGrid personalGrid in GIMHelper.GetPersonalGridsList())
                        _takeContextGridTargets.Add(personalGrid);

                    inferredOptions.Add(ContextOption.TakeItem);
                }

                //enable transfer in the following cases:
                //if we're in a personal grid...
                if (grid.IsPersonal() )
                {
                    //Allow transferring between personal grids (if multiple exist)
                    if (GIMHelper.CountPersonalGrids() > 1)
                    {
                        foreach (InvGrid personalGrid in GIMHelper.GetPersonalGridsList())
                        {
                            //ignore the grid we're currently in
                            if (personalGrid == grid)
                                continue;

                            //you may transfer to any other personal grid. Why wouldn't you be able to?
                            if (!_transferContextGridTargets.Contains(personalGrid))
                                _transferContextGridTargets.Add(personalGrid);

                        }
                    }

                    //Allow transferring from the current personal grid to any opened nonPersonal, non merchant grid (that isn't the current grid)
                    if (openedGrids > 1)
                    {
                        foreach (InvGrid openedGrid in GIMHelper.GetOpenedGridsList())
                        {
                            //ignore the grid we're currently in
                            if (openedGrid == grid)
                                continue;

                            //ignore merchants
                            if (openedGrid.IsMerchant())
                                continue;

                            //personalGrids are definitely 'transfer' targets, but
                            //right now we're looking for nonPersonal grids
                            if (openedGrid.IsPersonal())
                                continue;

                            //save the opened nonPersonal grid as a valid transfer context
                            if (!_transferContextGridTargets.Contains(openedGrid))
                                _transferContextGridTargets.Add(openedGrid);

                        }
                    }

                    //add the transfer context if any transfer contexts exist
                    if (_transferContextGridTargets.Count > 0)
                        inferredOptions.Add(ContextOption.TransferItem);

                }

                //otherwise we're not in a personal grid, so
                //we'll need to select context targets differently...
                else
                {
                    //Only allow transferring from the current opened grid to another opened nonPersonal, non merchant grid
                    if (openedGrids > 1)
                    {
                        foreach (InvGrid openedGrid in GIMHelper.GetOpenedGridsList())
                        {
                            //ignore the grid we're currently in
                            if (openedGrid == grid)
                                continue;

                            //ignore merchants
                            if (openedGrid.IsMerchant())
                                continue;

                            //personalGrids aren't transfer targets in this case
                            if (openedGrid.IsPersonal())
                                continue;

                            //save the opened nonPersonal grid as a valid transfer context
                            if (!_transferContextGridTargets.Contains(openedGrid))
                                _transferContextGridTargets.Add(openedGrid);

                        }

                        //add the transfer context if any transfer contexts exist
                        if (_transferContextGridTargets.Count > 0)
                            inferredOptions.Add(ContextOption.TransferItem);
                    }
                }

            }


            return inferredOptions;
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

            
            //handle cases where we need further context. Check if the selected context is a multiContainer context,
            //and also ensure we summon the TransferMenu in case an 'other' grid context isn't yet set.
            if ( (_otherMultiContainerContexts.Contains(_selectedContextOption) || _defaultMultiContainerContexts.Contains(_selectedContextOption)) 
                && _selectedContextGridTarget == null)
            {
                //currently this isn't quite right, but it's enough for debugging the menu itself
                OnTransferMenuRequested?.Invoke(GIMHelper.GetOpenedGridsList());
                return;
            }

            //otherwise,we have everything we need. All we need now is an interaction amount.
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

        public void RespondToOtherGridContextSelection(InvGrid targetGrid)
        {
            if (!_isShowing)
                return;
            if (targetGrid == null)
                return;

            _selectedContextGridTarget = targetGrid;

            //determine if we need to also specify an amount
            //auto complete the interaction if you can only interact with 1
            //[our context should've already been set]
            if (_interactionRange.y == 1)
            {
                _interactionAmount = 1;
                TriggerInteractionEvent(_interactionAmount);
            }

            //otherwise show the numerical selector. Let the user choose how many to interact with.
            else OnNumericalSelectorRequested?.Invoke(_selectedContextOption, _interactionRange.x, _interactionRange.y);
        }

        /// <summary>
        /// Sets the context menu's context and then raises the OnContextSet event. 
        /// </summary>
        /// <param name="options">All valid options to show the user</param>
        /// <param name="position">where to show the menu on the grid</param>
        /// <param name="grid">the grid where the targeted item was chosen</param>
        /// <param name="otherTargetInvGrid">(optional) The other grid to send the item(s), if the context requires a target destination.
        ///  This may be ignored. If a context that requires another grid is selected, then another menu will show itself to allow the user
        ///  to specify further. If this parameter is given now, then the other menu won't need to show itself and the context will happen normally.</param>
        public void SetContext((int,int) position, InvGrid grid)
        {
            if (grid == null)
                return;
            if (!grid.IsCellOnGrid(position))
                return;

            _inferredContextOptions.Clear();
            _inferredContextOptions = InferContextOptions(grid.GetInvItemOnCell(position), grid);

            if (BuildMenu())//returns true if the build succeeded (no empty menus are allowed)
            {
                _gridContext = grid;
                _positionContext = position;
                _itemContext = grid.GetInvItemOnCell(position);

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
            if (!_isFocused && _isShowing)
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

