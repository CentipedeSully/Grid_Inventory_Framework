using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MerchantHelperUtilities
{

    /// <summary>
    /// Calculates what should be paid for the item(s) in question.
    /// </summary>
    /// <param name="item">The item[Data] in question</param>
    /// <param name="amount">The amount of items to charge for</param>
    /// <param name="priceMultiplier">The seller's markup/discount percentage</param>
    /// <returns>An int (rounded up) that's the calculated price of all the items</returns>
    public static int CalculatePrice(ItemData item, int amount, float priceMultiplier)
    {
        if (item == null)
            return 0;

        //make sure the merchant isn't ripping off the player via rounding errors ^_^
        int individualItemSale = (int)Mathf.Ceil(item.ItemValue() * priceMultiplier);

        return individualItemSale * amount;
    }

}
