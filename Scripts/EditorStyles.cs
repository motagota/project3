using UnityEngine;

// This class provides runtime equivalents of editor styles
public static class EditorStyles
{
    private static GUIStyle _boldLabel;
    
    public static GUIStyle boldLabel
    {
        get
        {
            if (_boldLabel == null)
            {
                _boldLabel = new GUIStyle(GUI.skin.label);
                _boldLabel.fontStyle = FontStyle.Bold;
            }
            return _boldLabel;
        }
    }
}