using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace dtsInventory
{
    public class ItemInfoController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemDescText;
        [SerializeField] private TextMeshProUGUI _itemNameText;


        [SerializeField] private bool _isShowing = false;
        private bool _isItemPinned = false;
        [SerializeField] private ItemData _currentHoveredItemData;
        [SerializeField] private ItemData _currentShowingItemData;

        public UnityEvent OnItemInfoShown;
        public UnityEvent OnItemInfoHidden;
        public UnityEvent<ItemData> OnItemInfoSet;
        public UnityEvent OnItemInfoCleared;


        public void SetItem(ItemData itemTodisplay)
        {
            if (itemTodisplay != null)
            {
                _currentShowingItemData = itemTodisplay;

                //update the ui elements
                if (_itemDescText != null)
                    _itemDescText.text = itemTodisplay.Desc();
                if (_itemNameText != null)
                    _itemNameText.text = itemTodisplay.Name();

                OnItemInfoSet?.Invoke(_currentShowingItemData);
            }
        }
        public void ClearItem() { _currentShowingItemData = null; OnItemInfoCleared?.Invoke(); }
        
        /// <summary>
        /// Toggles the itemDisplay as showing. Will only respond if an item is set beforehand.
        /// </summary>
        public void ShowUi()
        {
            if (!_isShowing && _currentShowingItemData != null)
            {
                _isShowing = true;
                OnItemInfoShown?.Invoke();
            }
        }

        /// <summary>
        /// Toggles the itemDisplay as not showing and clears the currently-set item.
        /// </summary>
        public void HideUi()
        {
            if (_isShowing)
            {
                _isShowing = false;
                ClearItem();
                OnItemInfoHidden?.Invoke();
            }
        }

        public void RespondToGridCellSet(InvGrid grid, (int,int) position)
        {
            if (grid == null)
                return;


            if (!grid.IsCellOnGrid(position))
            {
                _currentHoveredItemData = null;
                HideUi();
                return;
            }

            if (!grid.IsCellOccupied(position))
            {
                _currentHoveredItemData = null;
                HideUi();
                return;
            }

            if (grid.IsCellOccupied(position))
            {
                _currentHoveredItemData = grid.GetStackItemData(position);
                if (!_isItemPinned)
                    SetItem(_currentHoveredItemData);

                ShowUi();
            }
        }
        public void RespondToStackPinned()
        {
            _isItemPinned = true;
            
        }
        public void RespondToPinnedStackLost()
        {
            _isItemPinned = false;
        }

    }
}
