using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;




public class MerchantSettings : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InvGrid _grid;

    [Header("Stock Settings")]
    [SerializeField] private List<LootRoll> _stockChancesList;
    [SerializeField] private bool _showRestockResults;
    private IEnumerator _wareRegenerator;

    [Header("Pricing")]
    [Tooltip("What's this merchant's price adjustment when selling to the player")]
    [SerializeField] private float _merchantPriceAdjustment = 1;
    [Tooltip("What's this merchant's price adjustment when buying items from the player")]
    [SerializeField] private float _exchangePriceAdjustment = .5f;

    [Header("Payment")]
    [SerializeField] private ItemData _currency;




    [Header("Debug")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private bool _cmdRerollStock;


    [Header("Events")]
    public UnityEvent<InvGrid> OnMerchantWaresRegenerated;

    //monobehaviours
    private void Awake()
    {
        //attempt to connect to a default grid
        if (_grid == null)
            _grid = GetComponent<InvGrid>();
    }
    private void Update()
    {
        if (_isDebugActive)
            ListenForDebugCommands();
    }



    //internals
    private IEnumerator RegenStockAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        RepopulateMerchantContainer();
        _wareRegenerator = null;
        OnMerchantWaresRegenerated?.Invoke(_grid);
    }
    private void RepopulateMerchantContainer()
    {
        if (_grid == null)
        {
            Debug.LogWarning("Attempted to regenerate merchant wares no grid current is set.");
            return;
        }
        //first, remove all items from the merchant's container
        int removalIterations = 0;
        while (_grid.GetAllStacks().Count > 0 && removalIterations <= 1000) //this shouldn't take 1000 iterations. This demo doesn't contain 1000 different itemDatas
        {
            ItemData chosenItem = _grid.GetAllStacks().First().Value;
            _grid.RemoveItem(chosenItem, _grid.CountItem(chosenItem));
            removalIterations++;
        }

        if (removalIterations > 1000 && _grid.GetAllStacks().Count > 0)
        {
            Debug.LogWarning($"Failsafe detected: averted an infinite while loop while attempting to remove all items from a merchant's container {_grid.name}. Aborting operation.");
            return;
        }

        //now reroll the merchant's loot
        List<bool> lootRollResults = ContainerController.RollForLoot(_stockChancesList, _showRestockResults);

        //add all successfully-rolled items to the container
        for (int i = 0; i < _stockChancesList.Count; i++)
        {

            if (lootRollResults[i] == true)
            {
                int amountToAdd = UnityEngine.Random.Range(_stockChancesList[i].minAmount, _stockChancesList[i].maxAmount + 1);

                if (_showRestockResults)
                    Debug.Log($"Rolled Amount to add: {amountToAdd}");

                _grid.AddItem(_stockChancesList[i].itemdata, amountToAdd);
            }

        }
    }


    //externals
    public void RegenerateWares()
    {
        //only attempt to regen if we aren't already regen'ing
        if (_wareRegenerator == null)
        {
            _wareRegenerator = RegenStockAtEndOfFrame();
            StartCoroutine(_wareRegenerator);
        }
    }

    public float GetMerchantPriceAdjustment() { return _merchantPriceAdjustment; }
    public void SetMerchantPriceAdjustment(float newPriceModifier) { _merchantPriceAdjustment= newPriceModifier; }

    public float GetExchangePriceAdjustment() { return _exchangePriceAdjustment; }
    public void SetExchangePriceAdjustment(float newPriceModifier) { _exchangePriceAdjustment= newPriceModifier; }

    public InvGrid GetMerchantGrid() { return _grid; }
    public void SetMerchantGrid(InvGrid merchantGrid) {  _grid = merchantGrid; }

    public ItemData GetCurrency() { return _currency; }


    //Debug
    private void ListenForDebugCommands()
    {
        if (_cmdRerollStock)
        {
            _cmdRerollStock = false;
            RegenerateWares();
        }
    }


}
