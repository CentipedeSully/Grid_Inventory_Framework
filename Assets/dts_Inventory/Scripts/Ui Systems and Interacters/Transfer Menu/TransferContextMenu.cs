using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace dtsInventory
{
    public class TransferContextMenu : MonoBehaviour, IGridUiElement
    {
        //Declarations
        [SerializeField] private GameObject _containerOptionPrefab;
        [SerializeField] private Transform _activeOptionsContainer;
        [SerializeField]private List<GridInvButton> _activeButtons = new();
        [SerializeField]private int _currentHoveredBtnIndex = -1;
        GridInvButton _hoveredBtn;
        //private 
        [SerializeField] private Transform _unusedOptionsContainer;
        [SerializeField] private UiDarkener _uiDarkener;
        private int _contextStackSize;
        private ItemData _itemContext;
        private bool _isShowing = false;
        private bool _isFocused = false;
        private List<InvGrid> _gridContext = new();
        private GameObject _tempGameObject;
        private TransferOptionDefinition _tempOptionDef;

        public UnityEvent OnUiShown;
        public UnityEvent OnUiHidden;
        public UnityEvent OnUiFocused;
        public UnityEvent OnUiUnfocused;

        [Tooltip("This gets called when the Ui has rebuilt itself and is ready to be displayed")]
        public UnityEvent OnTransferMenuBuilt;
        public UnityEvent<InvGrid, int> OnOptionSelected;









        //internals
        private void SetButtonAsHovered(int index)
        {

            //ignore if index out of range
            if (_activeButtons.Count == 0 || index >= _activeButtons.Count || index < 0)
                return;

            //ignore if the btn's already hovered
            if (_activeButtons[index] == _hoveredBtn)
                return;

            //Clear the previously-hovered btn
            ClearHoveredButton();

            //ensure the new btn is hovered
            _hoveredBtn = _activeButtons[index];
            if (_hoveredBtn.GetCurrentButtonState()!= GIButtonState.Disabled)
                _hoveredBtn.SetButtonState(GIButtonState.Highlighted);
            else
            {
                //show feedback for when the hovered button is disabled explaining why it's disabled
                //...
            }
            _currentHoveredBtnIndex = index;

        }

        private void ClearHoveredButton()
        {
            //exit the previous-hovered btn (if it exists)
            if (_hoveredBtn != null)
            {
                if (_hoveredBtn.GetCurrentButtonState() != GIButtonState.Disabled)
                    _hoveredBtn.SetButtonState(GIButtonState.Normal);
                else
                {
                    //Clear the feedback for a highlighted, disabled button
                    //...
                }
                _hoveredBtn = null;
                
            }
            _currentHoveredBtnIndex = -1;
        }
        private void RebuildMenu()
        {
            int availableContextContainers = _gridContext.Count;


            if (availableContextContainers == 0)
                return;

            int optionsBuilt = 0;
            _activeButtons.Clear();

            //overwrite all preexisting options [and remove any the remainders]
            for (int i = _activeOptionsContainer.childCount - 1; i >= 0; i--)
            {
                //only handle children with the relevant optionDefinition
                _tempOptionDef = _activeOptionsContainer.GetChild(i).GetComponent<TransferOptionDefinition>();
                if (_tempOptionDef == null)
                    continue;

                //remove extra options
                if (optionsBuilt == availableContextContainers)
                {
                    _tempOptionDef.gameObject.SetActive(false);
                    _tempOptionDef.transform.SetParent(_unusedOptionsContainer, false);
                }

                //otherwise overwrite the options that exist
                else
                {
                    //overwrite the button's grid reference
                    _tempOptionDef.SetInvGridReference(_gridContext[optionsBuilt]);

                    //overwrite the btn's text
                    _tempOptionDef.SetButtonText(_gridContext[optionsBuilt].name);

                    _tempOptionDef.SetTransferMenu(this);
                    _tempOptionDef.gameObject.SetActive(true);


                    //determine the capacity of the grid

                    //we'll create queries starting with the greatest value (in descending order) until we get a valid response.
                    //no valid response means this particular grid has no available space for the item.

                    //start with the max transferrable value +1 [the do-while will decrement it on the first pass]
                    int availableSpace = _contextStackSize + 1;

                    //init the other utilities that we'll need
                    List<InvGrid.ItemQuery> itemQueries = new();
                    List<InvGrid.ItemQueryResponse> queryResponses = new();

                    do
                    {
                        availableSpace--;

                        //rebuild the item query
                        InvGrid.ItemQuery itemQuery = new(_itemContext, availableSpace);

                        //reset the queryList [InvGrids only take lists of queries XD]
                        itemQueries.Clear();
                        itemQueries.Add(itemQuery);

                        //check if the current number of items can fit in the grid
                        queryResponses.Clear();

                        if (availableSpace > 0)
                            queryResponses = _gridContext[optionsBuilt].FindSpaceForItems(itemQueries);


                    }

                    //keep checking if there's space in the grid for the shrinking amount of items.
                    //break from checking when either we've found space, or we've reached zero
                    while (queryResponses.Count == 0 && availableSpace > 0);


                    GridInvButton button =  _tempOptionDef.GetComponent<GridInvButton>();

                    if (!_activeButtons.Contains(button))
                        _activeButtons.Add(button);

                    //disable the selection of grids that don't have enough space for the contextual item.
                    if (availableSpace == 0)
                    {
                        button.SetButtonState(GIButtonState.Disabled);
                        _tempOptionDef.SetDetectedAvaialableItemSpace(availableSpace);
                    }
                    else
                    {
                        button.SetButtonState(GIButtonState.Normal);
                        _tempOptionDef.SetDetectedAvaialableItemSpace(availableSpace);
                    }

                    optionsBuilt++;
                }
            }

            //if we've crawled through the active children, but still need more, 
            //then we'll make the ones we're missing
            while (optionsBuilt < availableContextContainers)
            {
                //attempt to recycle the unused options, if any exist
                if (_unusedOptionsContainer.childCount > 0)
                {
                    //get the latest child's optionDefinition
                    //[This should exist on every child in the container. If it doesn't then you've added something to the _unusedContainer you shouldn't have]
                    _tempOptionDef = _unusedOptionsContainer.GetChild(_unusedOptionsContainer.childCount - 1).GetComponent<TransferOptionDefinition>();

                    //move the option to the active options list
                    _tempOptionDef.transform.SetParent(_activeOptionsContainer, false);
                    _tempOptionDef.gameObject.SetActive(true);
                }

                //no unusedContainer exist. Create a new object.
                else
                {
                    _tempGameObject = Instantiate(_containerOptionPrefab, _activeOptionsContainer.transform);
                    _tempGameObject.SetActive(true);

                    _tempOptionDef = _tempGameObject.GetComponent<TransferOptionDefinition>();
                }


                //set the built option's grid reference & btn text
                _tempOptionDef.SetInvGridReference(_gridContext[optionsBuilt]);
                _tempOptionDef.SetButtonText(_gridContext[optionsBuilt].name);
                _tempOptionDef.SetTransferMenu(this);


                //determine the capacity of the grid

                //start with the max transferrable value +1 [the do-while will decrement it on the first pass]
                int availableSpace = _contextStackSize + 1;

                //init the other utilities that we'll need
                List<InvGrid.ItemQuery> itemQueries = new();
                List<InvGrid.ItemQueryResponse> queryResponses = new();

                do
                {
                    availableSpace--;

                    //rebuild the item query
                    InvGrid.ItemQuery itemQuery = new(_itemContext, availableSpace);

                    //reset the queryList [InvGrids only take lists of queries XD]
                    itemQueries.Clear();
                    itemQueries.Add(itemQuery);

                    //check if the current number of items can fit in the grid
                    queryResponses.Clear();

                    if (availableSpace > 0)
                        queryResponses = _gridContext[optionsBuilt].FindSpaceForItems(itemQueries);


                }

                //keep checking if there's space in the grid for the shrinking amount of items.
                //break from checking when either we've found space, or we've reached zero
                while (queryResponses.Count == 0 && availableSpace > 0);


                GridInvButton button = _tempGameObject.GetComponent<GridInvButton>();

                if (!_activeButtons.Contains(button))
                    _activeButtons.Add(button);

                //disable the selection of grids that don't have enough space for the contextual item.
                if (availableSpace == 0)
                {
                    button.SetButtonState(GIButtonState.Disabled);
                    _tempOptionDef.SetDetectedAvaialableItemSpace(availableSpace);
                }
                else
                {
                    button.SetButtonState(GIButtonState.Normal);
                    _tempOptionDef.SetDetectedAvaialableItemSpace(availableSpace);
                }

                
                optionsBuilt++;
            }

            //Raise the OnUiSet event. The menu should be built from the interal grid Context
            OnTransferMenuBuilt?.Invoke();
        }
        private void ClearContext()
        {
            ClearHoveredButton();

            _gridContext.Clear();
            _itemContext = null;
            _contextStackSize = 0;
            _activeButtons.Clear();
        }


        //Externals
        /// <summary>
        /// Sets what options should exist in the menu. Use this method before attempting to request a TransferMenu. Does not trigger
        /// building a menu. Null and empty lists are ignored.
        /// </summary>
        /// <param name="gridsToSelectFrom"></param>
        public void SetGridContext(ItemData itemContext, int contextStackSize,List<InvGrid> gridsToSelectFrom)
        {
            if (gridsToSelectFrom == null)
                return;
            if (gridsToSelectFrom.Count == 0)
                return;
            if (itemContext == null)
                return;

            //Debug.Log($"Received grid contexts: {gridsToSelectFrom.Count}");

            _gridContext = gridsToSelectFrom;
            _itemContext = itemContext;
            _contextStackSize = contextStackSize;
        }
        public void RespondToTransferMenuRequest(ItemData itemContext, int stackSize,List<InvGrid> gridsToSelectFrom)
        {
            string gridContextsDebugString = "";
            foreach (InvGrid gridOption in gridsToSelectFrom)
                gridContextsDebugString += $"{gridOption.name}\n";

            //Debug.Log($"Provided Context:\nitem : {itemContext.name} [{stackSize}]\npossible grid Contexts {gridContextsDebugString}");
            SetGridContext(itemContext, stackSize, gridsToSelectFrom);
            //can't build a menu from a null gridContext
            //[Don't know how this would be possible, since it's instantiated to empty by default]
            //[ === "null" and "empty" aren't the same. Null means its value doesn't exist. Empty means its value is empty === ]
            if (_gridContext == null)
            {
                Debug.LogWarning($"Attempted to summon a transfer menu with a NULL gridContext. Try calling '{nameof(SetGridContext)}' before" +
                    $" summoning a menu. Doing this informs the menu of all the possible options.");
                return;
            }

            //Refuse to build empty menus 
            if (_gridContext.Count == 0)
            {
                Debug.LogWarning($"Attempted to summon a transfer menu with an EMPTY gridContext. Try calling '{nameof(SetGridContext)}' before" +
                    $" summoning a menu. Doing this informs the menu of all the possible options.");
                return;
            }

            if (_itemContext == null)
            {
                Debug.LogWarning($"Attempted to summon a transfer menu with a null itemContext. This is necessary to determine if the contextual item can" +
                    $"actually fit in any of the provided grid options.");
                return;
            }

            if (stackSize <= 0)
            {
                Debug.LogWarning($"Detected a request to transfer a stack of zero or less items. How did this even happen? Ignoring transfer menu request.");
                return;
            }

            RebuildMenu();
        }
        public void RespondToMenuOptionSelection(TransferOptionDefinition selectedOption)
        {
            //only respond if the Ui is active
            if (_isFocused && _isShowing)
                OnOptionSelected?.Invoke(selectedOption.GetInvGridReference(), selectedOption.GetAvailableItemSpace());
        }



        public void ShowUi()
        {
            if (!_isShowing)
            {
                _isShowing = true;
                gameObject.SetActive(true);
                OnUiShown?.Invoke();
                UpdateGIMOnShown(this);
            }
        }
        public void UnfocusOnUi()
        {
            if (_isFocused && _isShowing)
            {
                _isFocused = false;
                OnUiUnfocused?.Invoke();
            }
        }
        public void FocusOnUi()
        {
            if (!_isFocused && _isShowing)
            {
                _isFocused = true;
                OnUiFocused?.Invoke();
            }
        }
        public GameObject GetGameObject()
        {
            return gameObject;
        }
        public void HideUi()
        {
            if (_isShowing)
            {
                _isShowing = false;
                gameObject.SetActive(false);
                ClearContext();
                UpdateGIMOnHidden(this);
                OnUiHidden?.Invoke();
            }
        }
        public bool IsFocused()
        {
            return _isFocused;
        }
        public bool IsShown()
        {
            return _isShowing;
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
        public void RespondToCancelInput()
        {
            if (!_isShowing || !_isFocused)
                return;

            HideUi();
        }
        public void RespondToConfirmInput()
        {
            if (!_isShowing || !_isFocused)
                return;

            if (_hoveredBtn == null || _currentHoveredBtnIndex == -1)
                return;

            if (_hoveredBtn.GetCurrentButtonState() != GIButtonState.Disabled)
            {
                _hoveredBtn.onClick?.Invoke();
            }
        }
        public void RespondToEditHotkey()
        {
            //...
        }
        public void RespondToHeavyLeftAction()
        {
            //...
        }
        public void RespondToHeavyRightAction()
        {
            //...
        }
        public void RespondToJumpHotkey()
        {
            //...
        }
        public void RespondToLightLeftAction()
        {
            //...
        }
        public void RespondToLightRightAction()
        {
            //...
        }
        public void RespondToPrimaryDirectionalInput(Vector2 input)
        {
            if (!_isShowing || !_isFocused)
                return;

            if (_activeButtons.Count == 0)
                return;

            //read input
            //move the hover effect up when up is pressed
            if (input.y > .1f)
            {
                //default to the bottom
                if (_currentHoveredBtnIndex == -1 || _currentHoveredBtnIndex - 1 < 0)
                {
                    SetButtonAsHovered(_activeButtons.Count - 1);
                } 
                //if we can go up without needing to wrap, do it
                else
                    SetButtonAsHovered(_currentHoveredBtnIndex - 1);
            }

            //move the hover effect down when down is pressed
            else if (input.y < -.1f)
            {
                //default to the top of the collection for good feels
                if (_currentHoveredBtnIndex == -1 || _currentHoveredBtnIndex + 1 > _activeButtons.Count - 1)
                    SetButtonAsHovered(0);
                else
                    SetButtonAsHovered(_currentHoveredBtnIndex +1);
            }
        }
        public void RespondToSecondaryDirectionalInput(Vector2 input)
        {
            //...
        }
        public void RespondToTertiaryDirectionalInput(Vector2 input)
        {
            //...
        }


        public void UpdateGIMOnHidden(IGridUiElement self)
        {
            GIMHelper.UpdateGIMOnHidden(this);
        }
        public void UpdateGIMOnShown(IGridUiElement self)
        {
            GIMHelper.UpdateGIMOnShown(this);
        }


    }
}
