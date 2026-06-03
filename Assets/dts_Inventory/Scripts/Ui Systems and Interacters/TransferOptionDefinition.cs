using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



namespace dtsInventory
{
    public class TransferOptionDefinition : MonoBehaviour
    {
        [Header("Outdated")]
        [SerializeField] Text _buttonText;
        [SerializeField] private TransferMenuController _menuController;

        [Header("Production")]
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private InvGrid _invGridReference;
        [SerializeField] private TransferContextMenu _menu;



        public void SetInvGridReference(InvGrid gridReference) { _invGridReference = gridReference; }
        public void SetTransferMenuController(TransferMenuController controller) { _menuController = controller; }
        public void SetButtonText(string newText) { _text.text = newText; }
        public void SetTransferMenu(TransferContextMenu menu) { _menu = menu; }
        public void ConfirmThisSelection()
        {
            if (InvManagerHelper.IsInvSystemLocked())
                return;

            if (_menuController != null && _invGridReference != null)
            {
                Debug.Log($"Selection Confirmed:\nGridReference: {_invGridReference}\nSelectedOption: {this.gameObject}");
                _menuController.SetSelectedGrid(_invGridReference);
                _menuController.SaveSelectedOption(this.gameObject);
                _menuController.SubmitSelection();
            }
        }
        public void ConfirmSelection(InvGrid gridChoice)
        {
            if (_menu != null && _invGridReference != null)
                _menu.RespondToMenuOptionSelection(gridChoice);
            else
            {
                string menuName = "[NULL]";
                string gridReference = "[NULL]";
                if (_menu != null)
                    menuName = _menu.name;
                if (_invGridReference != null)
                    gridReference = _invGridReference.name;
                Debug.LogWarning($"Null TransferOption value(s) detected:\nTransferMenu reference: {menuName}\nInvGrid reference: {gridReference}");
            }
        }
    }
}



