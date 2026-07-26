using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MerchantContextMenu : MonoBehaviour, IGridUiElement
{

    //Declarations
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _contextHeader;
    [SerializeField] private Image _sprite;
    [SerializeField] private NumericalSelectorController _numericalSelector;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Color _buyingPriceColor = Color.yellow;
    [SerializeField] private Color _exchangeReturnsColor = Color.green;
    private bool _isShowing = false;
    private bool _isFocused = false;

    private HashSet<ContextOption> _merchantOptions = new();
    private InvGrid _gridContext;
    private ItemData _itemContext;
    private ContextOption _contextOption;

    [Header("Events")]
    public UnityEvent OnUiShown;
    public UnityEvent OnUiHidden;
    public UnityEvent OnUiFocused;
    public UnityEvent OnUiUnfocused;

    //Monobehaviours
    private void Awake()
    {
        //ensure we know all the merchant-related context options
        _merchantOptions.Add(ContextOption.BuyItem);
        _merchantOptions.Add(ContextOption.SellItem);
    }



    //Internals
    private void ClearContext()
    {
        _gridContext = null;
        _contextOption = ContextOption.None;
        _itemContext = null;
    }



    //Externals
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

    public void RespondToPointerLClick() { }
    public void RespondToPointerRClick() { }
    public void RespondToPointerMClick() { }
    public void RespondToPointerScroll(Vector2 input) { }

    public void ReadAlphaInput(bool input)
    {
        //
    }

    public void ReadBetaInput(bool input)
    {
        //
    }

    public void ReadGammaInput(bool input)
    {
        //
    }

    public void RespondToCancelInput()
    {
        if (!_isShowing || !_isFocused)
            return;

        HideUi();
    }

    public void RespondToConfirmInput()
    {
        ///
    }

    public void RespondToEditHotkey()
    {
        //
    }

    public void RespondToHeavyLeftAction()
    {
        //
    }

    public void RespondToHeavyRightAction()
    {
       //
    }

    public void RespondToJumpHotkey()
    {
        //
    }

    public void RespondToLightLeftAction()
    {
        //
    }

    public void RespondToLightRightAction()
    {
        //
    }

    public void RespondToPrimaryDirectionalInput(Vector2 input)
    {
        //
    }

    public void RespondToSecondaryDirectionalInput(Vector2 input)
    {
       //
    }

    public void RespondToTertiaryDirectionalInput(Vector2 input)
    {
        //
    }



    public void RespondToMerchantContextRequest(ContextOption context, InvGrid merchantGrid, ItemData item, int min, int max)
    {
        if (merchantGrid == null || item == null)
            return;

        //do nothing if the context isn't a merchant context option
        if (!_merchantOptions.Contains(context))
            return;


        _gridContext = merchantGrid;
        _contextOption = context;
        _itemContext = item;


        //update all of the ui fields
        if (context == ContextOption.BuyItem)
            _contextHeader.text = "Buy?";
        else if (context == ContextOption.SellItem)
            _contextHeader.text = "Sell?";


        _sprite.sprite = item.Sprite();
        _numericalSelector.SetContext(context,min,max);

        UpdatePrice(_numericalSelector.GetNumber());


    }
    public void UpdatePrice(int amountOfItems)
    {
        if (_gridContext != null && _itemContext != null && _merchantOptions.Contains(_contextOption))
        {
            float priceModifier;
            if (_contextOption == ContextOption.BuyItem)
                priceModifier = _gridContext.GetComponent<MerchantSettings>().GetMerchantPriceAdjustment();
            else 
                priceModifier = _gridContext.GetComponent<MerchantSettings>().GetExchangePriceAdjustment();

            _priceText.text = MerchantHelperUtilities.CalculatePrice(_itemContext, amountOfItems, priceModifier).ToString();
        }
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
