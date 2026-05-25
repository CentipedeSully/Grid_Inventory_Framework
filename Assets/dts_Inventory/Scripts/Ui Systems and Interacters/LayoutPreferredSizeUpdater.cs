
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace dtsInventory
{
    public class LayoutPreferredSizeUpdater : MonoBehaviour
    {
        [SerializeField] private RectTransform _layoutTransformToWatch;
        ILayoutElement _detectedLayoutElement;
        [SerializeField] private LayoutElement _layoutElementToUpdate;

        [Tooltip("Listens for any changes in this tmpro component. AutoUpdates the preferred size when a change is detected.")]
        [SerializeField] private TextMeshProUGUI _watchedTmproComponent;
        [SerializeField] private float _maxPreferredHeight;

        public UnityEvent OnLayoutResized;




        void OnEnable()
        {
            // Subscribe to the global internal change event
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        void OnTextChanged(Object obj)
        {
            // Verify if the event was fired by YOUR specific text component
            if (obj == _watchedTmproComponent)
            {
                UpdateLayoutPreferredHeight();
            }
        }

        [ContextMenu("Update Preferred Height")]
        public void UpdateLayoutPreferredHeight()
        {
            if (_layoutTransformToWatch == null)
                return;

            _detectedLayoutElement = _layoutTransformToWatch.GetComponent<ILayoutElement>();
            if (_layoutElementToUpdate != null && _detectedLayoutElement != null)
            {
                if (_layoutElementToUpdate.preferredHeight != _detectedLayoutElement.preferredHeight)
                    _layoutElementToUpdate.preferredHeight = _detectedLayoutElement.preferredHeight;

                if (_layoutElementToUpdate.preferredHeight > _maxPreferredHeight)
                    _layoutElementToUpdate.preferredHeight = _maxPreferredHeight;

                OnLayoutResized?.Invoke();

            }
        }
    }
}
