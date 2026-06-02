using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dtsInventory
{
    public class ContextualOptionDefinition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private ContextOption _optionType = ContextOption.None;
        //[SerializeField] private ContextWindowController _contextWindowController;

        public UnityEvent<GridInvButton> OnPointerEnter;
        public UnityEvent<GridInvButton> OnPointerExit;
        public UnityEvent<ContextualOptionDefinition> OnContextSelected;

       

        public ContextOption GetContextOption() { return _optionType; }



        //refernced via unity event in the Contextual option prefab
        public void PerformSelectionOfThisOption() 
        {
            //if (InvManagerHelper.IsInvSystemLocked())
            //    return;

            //_contextWindowController.MarkOptionAsSelected(GetComponent<Button>()); 
            //_contextWindowController.SpecifyAmount(_optionType); 

            
            OnContextSelected?.Invoke(this);


        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnter?.Invoke(GetComponent<GridInvButton>());
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            OnPointerExit?.Invoke(GetComponent<GridInvButton>());
        }
    }
}

