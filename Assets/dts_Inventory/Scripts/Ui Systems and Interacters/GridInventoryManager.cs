using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



namespace dtsInventory
{
    public interface IGridUiElement
    {
        GameObject GetGameObject();
        void UpdateGIMOnShown(IGridUiElement self);
        void UpdateGIMOnHidden(IGridUiElement self);
        void ShowUi();
        void HideUi();
        void FocusOnUi(); 
        void UnfocusOnUi();

        bool IsShown();
        bool IsFocused();

        //elements dont NEED to respond to every binding if you're building your own elements.
        //Confirm, Back, and a single Directional response are highly recommended for a general navigation & selection experience, tho.
        void RespondToPrimaryDirectionalInput(Vector2 input); //right analog stick && directional Keys
        void RespondToSecondaryDirectionalInput(Vector2 input); //left analog stick && [whatever binding you desire]
        void RespondToTertiaryDirectionalInput(Vector2 input); //d pad && [whatever binding you desire. Might not always imply directional navigation (waepon wheels, for example)]
        void RespondToLightLeftAction(); //left bumper ("previous" thing)
        void RespondToHeavyLeftAction(); //left trigger ("previous" thing, but different)
        void RespondToLightRightAction(); //right bumper ("next" thing)
        void RespondToHeavyRightAction(); //right trigger ("next" thing, but different)

        void RespondToConfirmInput(); //make the selection
        void RespondToCancelInput(); // go back from selection

        void RespondToJumpHotkey(); //how to quickly jump to out-of-reach elements
        void RespondToEditHotkey(); //need a way to quickly enter edit mode?

        void ReadAlphaInput(bool input); //hold this to modify other inputs
        void ReadBetaInput(bool input); //hold this to modify other inputs
        void ReadGammaInput(bool input); //hold this to modify other inputs
    }

    /// <summary>
    /// Manages the interdependent ui elements of the whole grid inventory. Feeds inputs to the current-focused ui element (without relying on 
    /// Unity's Navigation systems). Always defaults to the provided default element if the ui is showing. 
    /// 
    /// Completely unity-event driven for customizability.
    /// </summary>
    public class GridInventoryManager : MonoBehaviour
    {
        [Header("State Values")]
        private IGridUiElement _focusedElement = null;
        private bool _isInventoryShowing = false;
        private List<IGridUiElement> _focusStack = new List<IGridUiElement>();
        private List<InvGrid> _openedGrids = new List<InvGrid>();
        private List<InvGrid> _openedMerchantGrids = new List<InvGrid>();
        private List<InvGrid> _personalGrids = new List<InvGrid>();
        private InvGrid _tempGrid = null;
        private int _lastGridIndex = -1;

        //unity events
        [Tooltip("What should run when the main 'show inventory' event is triggered?")]
        public UnityEvent OnShowGridUi;
        [Tooltip("What should run when the main 'hide inventory' event is triggered?")]
        public UnityEvent OnHideGridUi;

        [Tooltip("Should anything external run whenever an element gets focused on?")]
        public UnityEvent<IGridUiElement> OnFocusedElementEntered;
        [Tooltip("Should anything external run whenever an element gets unfocused on?")]
        public UnityEvent<IGridUiElement> OnFocusedElementExited;
        [Tooltip("Should anything external run whenever an element gets opened?")]
        public UnityEvent<IGridUiElement> OnElementOpened;
        [Tooltip("Should anything external run whenever an element gets closed?")]
        public UnityEvent<IGridUiElement> OnElementClosed;


        //monobehaviours
        private void Awake()
        {
            GIMHelper.SetGIM(this);
        }





        //internals
        private void PushCurrentFocusToStack()
        {
            if (_focusedElement != null)
            {
                //remove the element if it already exists as a waiting element [dunno how this'll happen]
                if (_focusStack.Contains(_focusedElement))
                    _focusStack.Remove(_focusedElement);
                _focusStack.Add(_focusedElement);
            }
        }
        private IGridUiElement PullPreviousFocusFromStack()
        {
            if (_focusStack.Count > 0)
            {
                IGridUiElement prevFocus = _focusStack[_focusStack.Count - 1];
                _focusStack.RemoveAt(_focusStack.Count -1);
                return prevFocus;
            }

            return null;
            
        }
        private void ClearFocusedElement()
        {
            if (_focusedElement == null)
                return;

            IGridUiElement exitedElement = _focusedElement;
            _focusedElement = null;
            exitedElement.UnfocusOnUi();

            //Debug.Log($"Unfocused element: {exitedElement.GetGameObject().name} ");
            OnFocusedElementExited?.Invoke(exitedElement);

        }
        private void FocusOnNextInStack()
        {
            if ( _focusedElement == null)
            {
                _focusedElement = PullPreviousFocusFromStack();

                if (_focusedElement != null)
                    SetFocusedElement(_focusedElement);
            }
            
        }
        private void TrackGrid(InvGrid detectedGrid)
        {
            if (detectedGrid == null)
                return;

            //only track new grids
            if (!_openedGrids.Contains(detectedGrid))
            {
                _openedGrids.Add(detectedGrid);
                Debug.Log($"Tracking grid: {detectedGrid.name}");

                if (detectedGrid.IsMerchant())
                {
                    _openedMerchantGrids.Add(detectedGrid);
                    Debug.Log("And it's a MERCHANT, too! Merchant tracked");
                }
            }
        }
        private void UntrackGrid(InvGrid grid)
        {
            if (_openedGrids.Contains(grid))
            {
                _openedGrids.Remove(grid);
                Debug.Log($"Removed grid from tracked grids: {grid.name}");
            }

            if (_openedMerchantGrids.Contains(grid))
            {
                _openedMerchantGrids.Remove(grid);
                Debug.Log($"And it's also a merchant. Merchant removed from tracked merchants {grid.name}");
            }
        }
        


        //externals
        public IGridUiElement GetCurrentFocus() { IGridUiElement focusCopy = _focusedElement; return focusCopy; }
        public int CountOpenedGrids() { return _openedGrids.Count; }
        public int CountOpenedMerchants() { return _openedMerchantGrids.Count; }
        public List<InvGrid> GetOpenedGridsList()
        {
            List<InvGrid> listCopy = new List<InvGrid>();

            for (int i = 0; i < _openedGrids.Count; i++)
                listCopy.Add(_openedGrids[i]);

            return listCopy;
        }
        public List<InvGrid> GetOpenedMerchantsList()
        {
            List<InvGrid> listCopy = new List<InvGrid>();

            for (int i = 0; i < _openedMerchantGrids.Count; i++)
                listCopy.Add(_openedMerchantGrids[i]);

            return listCopy;
        }
        public void FocusOnGrid(IGridUiElement grid)
        {
            //ignore null values
            if (grid == null) return;

            //ignore redundant values
            if (_focusedElement == grid)
                return;

            _tempGrid = grid.GetGameObject().GetComponent<InvGrid>();

            //ignore elements that aren't grids
            if (_tempGrid == null)
                return;

            //if we aren't yet tracking the grid, track it!
            if (!_openedGrids.Contains(_tempGrid))
                TrackGrid(_tempGrid);

            //Set the grid as the new focus
            SetFocusedElement(grid);

            //clear the temp variable
            _tempGrid = null;

        }
        public void FocusOnNextGrid()
        {
            if (_openedGrids.Count == 0)
                return;

            if (_openedGrids.Count == 1)
            {
                _lastGridIndex = 0;
                FocusOnGrid(_openedGrids[_lastGridIndex]);
            }

            if (_openedGrids.Count > 1)
            {
                //see if our focused element is a grid
                //if it's a grid, then go to that grid's neighbor (to the left)
                _tempGrid = _focusedElement.GetGameObject().GetComponent<InvGrid>();
                if (_tempGrid != null)
                {

                    //ensure we're wrapping back to the other end of the list if we cant go to the left anymore
                    if (_openedGrids.IndexOf(_tempGrid) == 0)
                        FocusOnGrid(_openedGrids[_openedGrids.Count - 1]);

                    else
                        FocusOnGrid(_openedGrids[_openedGrids.IndexOf(_tempGrid)-1]);
                }
            }
        }
        public void FocusOnPreviousGrid()
        {
            if (_openedGrids.Count == 0)
                return;

            if (_openedGrids.Count == 1)
            {
                _lastGridIndex = 0;
                FocusOnGrid(_openedGrids[_lastGridIndex]);
            }

            if (_openedGrids.Count > 1)
            {
                //see if our focused element is a grid
                //if it's a grid, then go to that grid's neighbor (to the RIGHT)
                _tempGrid = _focusedElement.GetGameObject().GetComponent<InvGrid>();
                if (_tempGrid != null)
                {

                    //ensure we're wrapping back to the other end of the list if we cant go to the RIGHT anymore
                    if (_openedGrids.IndexOf(_tempGrid) == _openedGrids.Count - 1)
                        FocusOnGrid(_openedGrids[0]);

                    else
                        FocusOnGrid(_openedGrids[_openedGrids.IndexOf(_tempGrid) + 1]);
                }
            }
        }
        public void SetGridAsPersonal(InvGrid grid)
        {
            if (grid == null)
                return;

            _personalGrids.Add(grid);
        }




        /// <summary>
        /// Sets an opened element as the current focus. If an element is already being focused on, then it is unfocused (but not closed)
        /// and is saved. More than one focus may be saved at a time. The latest saved focus get automatically refocused on when the current active focus gets cleared.
        /// </summary>
        /// <param name="element"></param>
        public void SetFocusedElement(IGridUiElement element)
        {
            if (element == null)
                return;
            if (!element.IsShown())
                return;

            //if the requested focus is already waiting in the stack
            //then remove that element from the stack.
            if (_focusStack.Contains(element))
            {
                //ensure all instances are removed
                while (_focusStack.Contains(element))
                    _focusStack.Remove(element);
            }    

            if (_focusedElement != null)
            {

                PushCurrentFocusToStack();
                ClearFocusedElement();
            }

            _focusedElement = element;
            _focusedElement.FocusOnUi();

            //Debug.Log($"New element set as focus: {_focusedElement.GetGameObject().name}\n");
            OnFocusedElementEntered?.Invoke(_focusedElement);
        }

        /// <summary>
        /// Clears the provided element from being the focus (only if that element is the current focus).
        /// If another previous focus still exists on the stack, then the waiting element is removed from the
        /// stack and regains the focus.
        /// </summary>
        /// <param name="element"></param>
        public void ClearElementFromCurrentFocus(IGridUiElement element)
        {
            if (element == null) 
                return;

            if (_focusedElement == element)
                ClearFocusedElement();
        }

        /// <summary>
        /// Automatically stops focusing on the hidden ui, and refocuses on the previously-opened Ui.
        /// If no previous ui exists, then this the default object will attempt to be focused on.
        /// </summary>
        /// <param name="element"></param>
        public void RespondToUiHidden(IGridUiElement element)
        {
            if (element == null)
                return;

            if (_focusStack.Contains(element))
            {
                //remove all occurences of the ui, if multiples exists somehow
                while (_focusStack.Contains(element))
                    _focusStack.Remove(element);
            }

            if (element == _focusedElement)
            {
                ClearElementFromCurrentFocus(element);
                FocusOnNextInStack();
            }

            //untrack the grid if the element is a grid
            _tempGrid = element.GetGameObject().GetComponent<InvGrid>();
            if (_tempGrid != null)
            {
                UntrackGrid(_tempGrid);
                _tempGrid = null;
            }

            OnElementClosed?.Invoke(element);
        }

        /// <summary>
        /// Automatically saves the current focus (if one exists) and begins focusing on the newly-opened ui.
        /// </summary>
        /// <param name="element"></param>
        public void RespondToUiShown(IGridUiElement element)
        {
            if (element == null)
                return;

            SetFocusedElement(element);

            //track the grid if the element is a grid
            _tempGrid = element.GetGameObject().GetComponent<InvGrid>();
            if (_tempGrid != null)
            {
                TrackGrid(_tempGrid);
                _tempGrid = null;
            }

            OnElementOpened?.Invoke(element);
        }

        public void RespondToInventoryHotkey()
        {
            if (!_isInventoryShowing)
            {
                _isInventoryShowing = true;
                OnShowGridUi?.Invoke();
            }
                
            else
            {
                _isInventoryShowing = false;
                ClearFocusedElement();
                OnHideGridUi?.Invoke();
            }
                
        }
        public bool IsInventoryShowing() { return _isInventoryShowing; }

        public void RelayPrimaryInputToFocusedElement(Vector2 directionalInput)
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToPrimaryDirectionalInput(directionalInput);
        }
        public void RelayLeftActionToFocusedElement()
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToLightLeftAction();
        }
        public void RelayRightActionToFocusedElement()
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToLightRightAction();
        }

        public void RelayConfirmToFocusedElement()
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToConfirmInput();
        }
        public void RelayCancelToFocusedElement()
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToCancelInput();
        }

        public void RelayJumpHotkeyToFocusedElement()
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToJumpHotkey();
        }
        public void RelayEditHotkeyToFocusedElement()
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.RespondToEditHotkey();
        }


        public void RelayAlphaToFocusedElement(bool input)
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.ReadAlphaInput(input);
        }
        public void RelayBetaToFocusedElement(bool input)
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.ReadBetaInput(input);
        }
        public void RelayGammaToFocusedElement(bool input)
        {
            if (_focusedElement == null)
                return;

            if (_isInventoryShowing)
                _focusedElement.ReadGammaInput(input);
        }
        
    }


    /// <summary>
    /// Provides immediate access to the Grid Inventory Manager. Commonly used to tell the manager if something needs to enter/exit 
    /// the manager's focus.
    /// </summary>
    public static class GIMHelper
    {
        private static GridInventoryManager _controller;


        public static void SetGIM(GridInventoryManager controller)
        {
            if (controller != null && _controller == null)
                _controller = controller;
        }
        public static void FocusOnGridElement(IGridUiElement element)
        {
            if (_controller == null)
                return;
            if (element == null)
                return;

            _controller.SetFocusedElement(element);
        }
        public static void ClearElementFromCurrentFocus(IGridUiElement element)
        {
            if (_controller == null)
                return;
            if (element == null)
                return;

            _controller.ClearElementFromCurrentFocus(element);
        }
        public static void UpdateGIMOnShown(IGridUiElement element)
        {
            if (_controller != null)
                _controller.RespondToUiShown(element);
        }
        public static void UpdateGIMOnHidden(IGridUiElement element)
        {
            if (_controller != null)
                _controller.RespondToUiHidden(element);
        }


        public static int CountOpenedGrids() { return _controller.CountOpenedGrids(); }
        public static int CountOpenedMerchants() { return _controller.CountOpenedMerchants(); }
        public static List<InvGrid> GetOpenedMerchantsList() { return _controller.GetOpenedMerchantsList(); }
        public static List<InvGrid> GetOpenedGridsList() { return _controller.GetOpenedGridsList(); }
        public static void FocusOnGrid(IGridUiElement gridElement) { _controller.FocusOnGrid(gridElement); }
        public static void FocusOnNextGrid() { _controller.FocusOnNextGrid(); }
        public static void FocusOnPreviousGrid() { _controller.FocusOnPreviousGrid(); }
        public static IGridUiElement GetCurrentFocus() { return _controller.GetCurrentFocus(); }
    }
}



