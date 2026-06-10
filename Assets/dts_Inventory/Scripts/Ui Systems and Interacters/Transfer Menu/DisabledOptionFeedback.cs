using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DisabledOptionFeedback : MonoBehaviour
{
    [SerializeField] private GameObject _disabledOverlay;
    [SerializeField] private Image _disabledImage;
    [SerializeField] private TextMeshProUGUI _disabledLabel;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _highlightedColor;

    [SerializeField] private float _transitionDuration = .15f;
    private float _currentTime = 0;
    private Color _startColor;
    private Color _endColor;
    private bool _isTransitioning = false;


    private void Start()
    {
        if (_disabledImage != null)
            _disabledImage.color = _normalColor;
    }

    private void Update()
    {
        if (_isTransitioning)
            TickTransition();
    }


    private void TickTransition()
    {
        _currentTime += Time.deltaTime;
        _disabledImage.color = Vector4.Lerp(_startColor,_endColor, _currentTime/_transitionDuration);

        if (_currentTime >= _transitionDuration)
        {
            _currentTime = 0;
            _isTransitioning = false;
        }
    }

    public void HighlightOverlay()
    {
        if (_disabledImage == null)
            return;

        //ignore if it's already highlighted
        if (_disabledImage.color == _highlightedColor)
            return;

        //start fresh if we're idle
        if (!_isTransitioning)
        {
            _isTransitioning = true;
            _startColor = _disabledImage.color;
            _endColor = _highlightedColor;
            _currentTime = 0;
        }

        //reverse the lerp direction if we're currently ending a highlight
        else if (_endColor != _highlightedColor)
        {
            //keep the current progress
            _currentTime = _transitionDuration - _currentTime;

            //swap the beginning and end values
            _endColor = _highlightedColor;
            _startColor = _normalColor;
        }
    }
    public void UnHighlightOverlay()
    {
        if (_disabledImage == null)
            return;

        //ignore if it's not highlighted
        if (_disabledImage.color == _normalColor)
            return;

        //start fresh if we're idle
        if (!_isTransitioning)
        {
            _isTransitioning = true;
            _startColor = _disabledImage.color;
            _endColor = _normalColor;
            _currentTime = 0;
        }

        //reverse the lerp direction if we're currently starting a highlight
        else if (_endColor != _normalColor)
        {
            //keep the current progress
            _currentTime = _transitionDuration - _currentTime;

            //swap the beginning and end values
            _endColor = _normalColor;
            _startColor = _highlightedColor;
        }
    }

    public void SetDisabledFeedback(bool newState)
    {
        if (_disabledOverlay == null)
            return;

        _disabledOverlay.SetActive(newState);
    }

    public void SetReasonLabel(string reason)
    {
        if (_disabledLabel == null)
            return;

        _disabledLabel.text = reason;
    }
}
