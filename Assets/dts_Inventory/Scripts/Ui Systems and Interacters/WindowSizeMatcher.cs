using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowSizeMatcher : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransformToWatch;
    [SerializeField] private RectTransform _mimicRectTransform;
    [SerializeField] private Vector2 _sizeOffset;
    private IEnumerator _endOfFrameRunner;
    [SerializeField] private bool _runDuringUpdate = false;





    private void Update()
    {
        if (_runDuringUpdate)
        {
            if (_rectTransformToWatch == null || _mimicRectTransform == null)
                return;

            if (_mimicRectTransform.sizeDelta != _rectTransformToWatch.sizeDelta + _sizeOffset)
            {
                UpdateMimic();
            }
        }
    }




    [ContextMenu("Update Mimic RectTransform")]
    public void UpdateMimic()
    {
        if (_rectTransformToWatch != null && _mimicRectTransform != null && _endOfFrameRunner == null)
        {
            if (Application.isPlaying)
            {
                _endOfFrameRunner = UpdateAtEndOfFrame();
                StartCoroutine(_endOfFrameRunner);
            }

            else _mimicRectTransform.sizeDelta = _rectTransformToWatch.sizeDelta + _sizeOffset;


        }
            
    }

    private IEnumerator UpdateAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _mimicRectTransform.sizeDelta = _rectTransformToWatch.sizeDelta + _sizeOffset;
        _endOfFrameRunner = null;
    }
}
