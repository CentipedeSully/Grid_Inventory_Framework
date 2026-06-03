using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TransferContextMenu : MonoBehaviour, IGridUiElement
{
    //Declarations
    [SerializeField] private GameObject _containerOptionPrefab;
    [SerializeField] private Transform _activeOptionsContainer;
    [SerializeField] private Transform _unusedOptionsContainer;
    [SerializeField] private UiDarkener _uiDarkener;
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
    public UnityEvent<InvGrid> OnOptionSelected;








    //internals
    private void RebuildMenu()
    {
        int availableContextContainers = _gridContext.Count;
        

        if (availableContextContainers == 0)
            return;

        int optionsBuilt = 0;

        //remove all preexisting options
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
                _tempOptionDef.transform.SetParent(_unusedOptionsContainer,false);
            }

            //otherwise overwrite the options that exist
            else
            {
                //overwrite the button's grid reference
                _tempOptionDef.SetInvGridReference(_gridContext[optionsBuilt]);

                //overwrite the btn's text
                _tempOptionDef.SetButtonText(_gridContext[optionsBuilt].name);

                _tempOptionDef.SetTransferMenu(this);

                _tempOptionDef.gameObject.SetActive(false);
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

            optionsBuilt++;
        }

        //Raise the OnUiSet event. The menu should be built from the interal grid Context
        OnTransferMenuBuilt?.Invoke();
    }
    private void ClearContext()
    {
        _gridContext.Clear();
    }


    //Externals
    /// <summary>
    /// Sets what options should exist in the menu. Use this method before attempting to request a TransferMenu. Does not trigger
    /// building a menu. Null and empty lists are ignored.
    /// </summary>
    /// <param name="gridsToSelectFrom"></param>
    public void SetGridContext(List<InvGrid> gridsToSelectFrom)
    {
        if (gridsToSelectFrom == null)
            return;
        if (gridsToSelectFrom.Count == 0)
            return;

        _gridContext = gridsToSelectFrom;
    }
    public void RespondToTransferMenuRequest(List<InvGrid> gridsToSelectFrom)
    {
        SetGridContext(gridsToSelectFrom);
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

        RebuildMenu();
    }
    public void RespondToMenuOptionSelection(InvGrid selection)
    {
        //only respond if the Ui is active
        if (_isFocused && _isShowing)
            OnOptionSelected?.Invoke(selection);
    }



    public void ShowUi()
    {
        if (!_isShowing)
        {
            _isShowing = true;
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
        return IsShown();
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
        HideUi();
    }
    public void RespondToConfirmInput()
    {
        
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
        //...
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
