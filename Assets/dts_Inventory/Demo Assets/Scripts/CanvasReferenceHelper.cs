using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CanvasReferenceHelper
{

    private static Canvas _canvas;
    private static SetupCanvasHelper _setupCanvasHelper;
    public static void SetCanvas(Canvas newCanvas) { _canvas = newCanvas; }
    public static void SetSetupHelper(SetupCanvasHelper helper) { _setupCanvasHelper =helper; }
    public static Canvas GetCanvas() { return _canvas; }
    public static Camera GetUiCamera() 
    {
        if (_setupCanvasHelper == null)
            return null;
        return _setupCanvasHelper._uiCamera; 
    }
}
