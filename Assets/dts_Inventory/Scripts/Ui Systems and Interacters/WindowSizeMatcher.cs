using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowSizeMatcher : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransformToWatch;
    [SerializeField] private RectTransform _mimicRectTransform;
    private IEnumerator _endOfFrameRunner;


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

            else _mimicRectTransform.sizeDelta = _rectTransformToWatch.sizeDelta;


        }
            
    }

    private IEnumerator UpdateAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _mimicRectTransform.sizeDelta = _rectTransformToWatch.sizeDelta;
        _endOfFrameRunner = null;
    }
}
