using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformTransitioner : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTranform;
    [SerializeField] private Vector3 _hiddenLocalOrigin;
    [SerializeField] private Vector3 _showingLocalOrigin;
    [SerializeField] private float _transitionDuration = .2f;
    [SerializeField] private bool _startShowing = false;
    private float _currentTransitionDuration=0;
    private bool _isTransitioning = false;
    private Vector3 _targetPosition;
    private Vector3 _startPosition;
    [Header("Debug")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private bool _cmdMoveToHiddenPosition = false;
    [SerializeField] private bool _cmdMoveToShowingPosition = false;


    private void Start()
    {
        if (_startShowing)
            MoveToShowingPosition();
        else MoveToHiddenPosition();
    }

    private void Update()
    {
        if (_isDebugActive)
            ListenForDebugCommands();

        if (_isTransitioning)
            TickTransition();
    }

    private void TickTransition()
    {
        if (_rectTranform == null)
        {
            ClearLerpUtilities();

            //default the transform to wherever it started, assuming the lerp failed along the way
            _rectTranform.localPosition = _startPosition; 
            return;
        }

        _currentTransitionDuration += Time.deltaTime;
        _rectTranform.localPosition = Vector3.Lerp(_startPosition, _targetPosition, _currentTransitionDuration / _transitionDuration);

        if (_rectTranform.localPosition == _targetPosition)
        {
            ClearLerpUtilities();
        }
    }

    private void ClearLerpUtilities()
    {
        _isTransitioning = false;
        _currentTransitionDuration = 0;
        _startPosition = _rectTranform.position;
        _targetPosition = _startPosition;
    }
    private void SetupLerpUtilities(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        _startPosition = _rectTranform.localPosition;

        _currentTransitionDuration = 0;
        _isTransitioning = true;
    }

    public void MoveToShowingPosition()
    {
        float midProgress = 0;
        //adjust the starting tick time if we're already inbetween points
        if (_isTransitioning && _targetPosition == _hiddenLocalOrigin)
            midProgress = _transitionDuration - _currentTransitionDuration;

        SetupLerpUtilities(_showingLocalOrigin);
        _currentTransitionDuration += midProgress;
    }
    public void MoveToHiddenPosition()
    {
        float midProgress = 0;
        //adjust the starting tick time if we're already inbetween points
        if (_isTransitioning && _targetPosition == _showingLocalOrigin)
            midProgress = _transitionDuration - _currentTransitionDuration;

        SetupLerpUtilities(_hiddenLocalOrigin);
        _currentTransitionDuration += midProgress;
    }


    private void ListenForDebugCommands()
    {
        if (_cmdMoveToHiddenPosition)
        {
            _cmdMoveToHiddenPosition = false;
            MoveToHiddenPosition();
        }

        if (_cmdMoveToShowingPosition)
        {
            _cmdMoveToShowingPosition = false;
            MoveToShowingPosition();
        }
    }


}
