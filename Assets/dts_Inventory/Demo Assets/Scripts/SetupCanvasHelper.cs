using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupCanvasHelper : MonoBehaviour
{
    [SerializeField] public Camera _uiCamera;
    private void Awake()
    {
        CanvasReferenceHelper.SetCanvas(GetComponent<Canvas>());
        CanvasReferenceHelper.SetSetupHelper(this);
    }
}
