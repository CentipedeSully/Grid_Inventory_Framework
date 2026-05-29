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
        void ShowUi();
        void HideUi();
        void FocusOnUi(); 
        void UnfocusOnUi();

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
        [SerializeField] private bool _isInventoryShowing = false;
        [Tooltip("The IGridUiElement component on this objects will get focused on automatically when this ui is shown " +
            "(and also exits the component when this ui is hidden).")]
        [SerializeField] private GameObject _defaultFocus;
        [SerializeField] private List<IGridUiElement> _focusStack = new List<IGridUiElement>();


        //unity events
        [Tooltip("What should run when the inv is shown?")]
        public UnityEvent OnShowGridUi;
        [Tooltip("What should run when the inv is hidden?")]
        public UnityEvent OnHideGridUi;

        [Tooltip("Should anything run whenever an element gets focused on?")]
        public UnityEvent<IGridUiElement> OnFocusedElementEntered;
        [Tooltip("Should anything run whenever an element gets exited?")]
        public UnityEvent<IGridUiElement> OnFocusedElementExited;



        private void Awake()
        {
            GIMHelper.SetGIM(this);
        }




        //externals


        //Event methods
        public void SetFocusedElement(IGridUiElement element)
        {
            if (element == null)
                return;
            if (element == _focusedElement)
                return;

            if (_focusedElement != null)
            {
                ClearFocusedElement();
            }

            _focusedElement = element;
            _focusedElement.FocusOnUi();
            OnFocusedElementEntered?.Invoke(_focusedElement);
        }
        public void ClearElementFromFocus(IGridUiElement element)
        {
            if (element == null) 
                return;

            if (_focusedElement == element)
            {
                ClearFocusedElement();
                
                //return focus to the default if it exists
                if (_defaultFocus != null)
                {
                    IGridUiElement defaultElement = _defaultFocus.GetComponent<IGridUiElement>();
                    if (defaultElement != element)
                        SetFocusedElement(defaultElement);
                }
            }
        }
        public void ClearFocusedElement()
        {
            if (_focusedElement == null)
                return;

            IGridUiElement exitedElement = _focusedElement;
            _focusedElement = null;
            exitedElement.UnfocusOnUi();
            OnFocusedElementExited?.Invoke(exitedElement);

        }


        public void RespondToInventoryHotkey()
        {
            if (!_isInventoryShowing)
            {
                _isInventoryShowing = true;

                if (_defaultFocus != null)
                    SetFocusedElement(_defaultFocus.GetComponent<IGridUiElement>());

                OnShowGridUi?.Invoke();
            }
                
            else
            {
                _isInventoryShowing = false;
                ClearFocusedElement();
                OnHideGridUi?.Invoke();
            }
                
        }

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
        public static void ClearElementFromFocus(IGridUiElement element)
        {
            if (_controller == null)
                return;
            if (element == null)
                return;

            _controller.ClearElementFromFocus(element);
        }
    }
}



