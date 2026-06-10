using dtsInventory;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dtsInventory
{
    public class NumericalSelectorController : MonoBehaviour, IGridUiElement
    {
        //Declarations
        [SerializeField] private int _maxNumber = 9999;
        [SerializeField] private int _minNumber = 0;
        [SerializeField] private int _number = 0;
        [SerializeField] private TextMeshProUGUI _textDisplay;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private GameObject _textNavigationTarget;
        [SerializeField] private AudioClip _onValueChangedAudioClip;
        [SerializeField] private float _confirmInputDelay = .1f;
        [SerializeField] private Button _pointerConfirmBtn;
        private bool _isShowing = false;
        private bool _isFocused = false;
        private float _currentDelayCount = 0;
        private bool _confirmReady = false;
        private AudioSource _audioSource;
        private ContextOption _contextOption;

        [Header("Unity Events")]
        public UnityEvent OnUiShown;
        public UnityEvent OnUiHidden;
        public UnityEvent OnUiFocused;
        public UnityEvent OnUiUnfocused;
        public UnityEvent<int> OnValueSubmitted;



        //monobehaviours

        private void Update()
        {
            TickDelay();
        }


        //internals
        private void RenderNumbertoDisplay()
        {
            _textDisplay.text = _number.ToString();
            _inputField.text = _number.ToString();
        }
        private void TickDelay()
        {
            if (!_confirmReady)
            {
                _currentDelayCount += Time.deltaTime;
                if (_currentDelayCount >= _confirmInputDelay)
                {
                    _confirmReady = true;
                    _currentDelayCount = 0;
                }
            }
        }
        private void ResetConfirmDelay()
        {
            _confirmReady = false;
            _currentDelayCount = 0;
        }




        //externals
        public void IncrementNumber()
        {
            _number++;

            if (_number > _maxNumber)
                _number = _minNumber;

            RenderNumbertoDisplay();

        }
        public void DecrementNumber()
        {
            _number--;

            if (_number < _minNumber)
                _number = _maxNumber;

            RenderNumbertoDisplay();
        }
        public void ResetNumber()
        {
            _number = _minNumber;
            RenderNumbertoDisplay();
        }
        public void SetNumber(int number)
        {
            _number = number;
            RenderNumbertoDisplay();
        }
        public int GetNumber() { return _number; }
        public int GetMax() { return _maxNumber; }
        public int GetMin() { return _minNumber; }
        public void SetContext(ContextOption context, int minValue, int maxValue)
        {
            _contextOption = context;
            _minNumber = minValue;
            _maxNumber = maxValue;
        }
        public void RespondToNumericalSelectRequest(ContextOption context, int minValue, int maxValue,Vector2 menuPosition)
        {
            SetContext(context, minValue, maxValue);
            GetComponent<RectTransform>().position = menuPosition;
            ShowUi();
        }

        public RectTransform GetRectTransform() { return GetComponent<RectTransform>(); }
        public void SubmitNumber()
        {
            if (_isShowing && _confirmReady)
            {
                //Debug.Log($"submitting number: {_number}");
                OnValueSubmitted.Invoke(_number);
            }
        }
        public bool IsNumericalSelectorOpen()
        {
            return gameObject.activeSelf;
        }

        public void PlayValueChangeAudioFeedback()
        {
            /*
            if (_audioSource == null)
                _audioSource = InvManagerHelper.GetInvInteracterAudiosource();

            if (_audioSource != null && _onValueChangedAudioClip != null)
            {
                _audioSource.clip = _onValueChangedAudioClip;
                _audioSource.Play();
            }*/
        }
        public void VerifyInputOnEnd()
        {
            int number = int.Parse(_textDisplay.text);
            _number = Mathf.Clamp(number, _minNumber, _maxNumber);
            RenderNumbertoDisplay();
        }

        public void ActivateInputEditing()
        {
            _inputField.ActivateInputField();
        }

        public GameObject GetGameObject(){return gameObject;}

        public void ShowUi()
        {
            if (!_isShowing)
            {
                _isShowing = true;
                gameObject.SetActive(true);
                ResetNumber();
                ResetConfirmDelay();
                OnUiShown?.Invoke();
                UpdateGIMOnShown(this);
                
            }
        }

        public void HideUi()
        {
            if (_isShowing)
            {
                _isShowing = false;
                ResetNumber();
                gameObject.SetActive(false);
                UpdateGIMOnHidden(this);
                OnUiHidden?.Invoke();
            }
        }

        public void UpdateGIMOnShown(IGridUiElement self) { GIMHelper.UpdateGIMOnShown(this); }
        public void UpdateGIMOnHidden(IGridUiElement self) { GIMHelper.UpdateGIMOnHidden(this); }
        public void FocusOnUi()
        {
            if (!_isFocused)
            {
                _isFocused = true;
                OnUiFocused?.Invoke();
            }
        }

        public void UnfocusOnUi()
        {
            if (_isFocused)
            {
                _isFocused = false;
                OnUiUnfocused?.Invoke();
            }
        }

        public void RespondToPrimaryDirectionalInput(Vector2 input)
        {
            if (input.y > .1f)
            {
                IncrementNumber();
            }
            else if (input.y < -.1f)
            {
                DecrementNumber();
            }
        }

        public void RespondToSecondaryDirectionalInput(Vector2 input)
        {
            //...
        }

        public void RespondToTertiaryDirectionalInput(Vector2 input)
        {
            //...
        }

        public void RespondToLightLeftAction()
        {
            //...
        }

        public void RespondToHeavyLeftAction()
        {
            //...
        }

        public void RespondToLightRightAction()
        {
            //...
        }

        public void RespondToHeavyRightAction()
        {
            //...
        }

        public void RespondToConfirmInput()
        {
            SubmitNumber();
        }

        public void RespondToCancelInput()
        {
            HideUi();
        }

        public void RespondToJumpHotkey()
        {
            //...
        }

        public void RespondToEditHotkey()
        {
            //...
        }

        public void ReadAlphaInput(bool input)
        {
            //...
        }

        public void ReadBetaInput(bool input)
        {
            //...
        }

        public void ReadGammaInput(bool input)
        {
            //...
        }

        public bool IsShown(){return _isShowing; }
        public bool IsFocused() { return _isFocused; }
        

    }
}
