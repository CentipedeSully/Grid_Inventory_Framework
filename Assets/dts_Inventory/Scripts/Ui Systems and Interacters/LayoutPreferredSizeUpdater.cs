
using System.Collections;
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
        [SerializeField] private RectTransform _rectTransformToUpdate;

        [SerializeField] private bool _updatePreferredWidth;
        [SerializeField] private bool _updatePreferredHeight;

        [Tooltip("Listens for any changes in this tmpro component. AutoUpdates the preferred size when a change is detected.")]
        [SerializeField] private TextMeshProUGUI _watchedTmproComponent;
        [SerializeField] private float _maxPreferredHeight;
        [SerializeField] private float _maxPreferredWidth;

        public UnityEvent OnLayoutPreferencesUpdated;
        public UnityEvent OnLayoutResized;

        private IEnumerator _updatePreferencesCoroutine = null;


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
                UpdateLayoutPreferredSize();
            }
        }


        private IEnumerator UpdatePreferredSizeNextFrame()
        {
            yield return new WaitForEndOfFrame();
            UpdateLayoutPreferredSize();
            _updatePreferencesCoroutine = null;
        }

        [ContextMenu("Update Preferred Size")]
        public void UpdateLayoutPreferredSize()
        {
            if (_updatePreferencesCoroutine == null && Application.isPlaying)
            {
                _updatePreferencesCoroutine = UpdatePreferredSizeNextFrame();
                StartCoroutine(_updatePreferencesCoroutine);
                return;
            }

            if (_layoutTransformToWatch == null)
                return;

            _detectedLayoutElement = _layoutTransformToWatch.GetComponent<ILayoutElement>();
            if (_layoutElementToUpdate != null && _detectedLayoutElement != null)
            {
                if (_updatePreferredHeight)
                {
                    if (_layoutElementToUpdate.preferredHeight != _detectedLayoutElement.preferredHeight)
                    {
                        //Debug.Log($"Detected Preferred Height: {_detectedLayoutElement.preferredHeight} [{_layoutTransformToWatch.name}]");
                        _layoutElementToUpdate.preferredHeight = _detectedLayoutElement.preferredHeight;
                    }

                    if (_layoutElementToUpdate.preferredHeight > _maxPreferredHeight)
                    {
                        _layoutElementToUpdate.preferredHeight = _maxPreferredHeight;
                    }
                }

                if (_updatePreferredWidth)
                {
                    if (_layoutElementToUpdate.preferredWidth != _detectedLayoutElement.preferredWidth)
                    {
                        //Debug.Log($"Detected Preferred Width: {_detectedLayoutElement.preferredWidth} [{_layoutTransformToWatch.name}]");
                        _layoutElementToUpdate.preferredWidth = _detectedLayoutElement.preferredWidth;
                    }

                    if (_layoutElementToUpdate.preferredWidth > _maxPreferredWidth)
                    {
                        _layoutElementToUpdate.preferredWidth = _maxPreferredWidth;
                    }
                }

                OnLayoutPreferencesUpdated?.Invoke();
            }
        }

        [ContextMenu("Resize RectTransorm")]
        public void ResizeRectTransformToMatchPreference()
        {
            if (_rectTransformToUpdate != null && _layoutElementToUpdate != null)
            {
                _rectTransformToUpdate.sizeDelta = new Vector2(_layoutElementToUpdate.preferredWidth, _layoutElementToUpdate.preferredHeight);
                OnLayoutResized?.Invoke();
            }
        }
    }
}
