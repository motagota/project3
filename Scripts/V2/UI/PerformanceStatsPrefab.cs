using UnityEngine;
using UnityEngine.UI;

namespace V2.UI
{
    /// <summary>
    /// Helper class to create a performance stats UI prefab at runtime
    /// </summary>
    public class PerformanceStatsPrefab : MonoBehaviour
    {
        [SerializeField] private Font uiFont;
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private Vector2 position = new Vector2(10, 10);
        
        private static GameObject _statsPanel;
        
        private void Start()
        {
            if (createOnStart)
            {
                CreatePerformanceStatsUI();
            }
        }
        
        /// <summary>
        /// Creates a performance stats UI panel in the scene
        /// </summary>
        public GameObject CreatePerformanceStatsUI()
        {
            if (_statsPanel != null) return _statsPanel;
            
            // Create canvas if it doesn't exist
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("StatsCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            
            // Create stats panel
            _statsPanel = new GameObject("PerformanceStatsPanel");
            _statsPanel.transform.SetParent(canvas.transform, false);
            
            // Add panel components
            RectTransform rectTransform = _statsPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(300, 200);
            
            // Add background image
            Image bgImage = _statsPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            
            // Add text component
            GameObject textObj = new GameObject("StatsText");
            textObj.transform.SetParent(_statsPanel.transform, false);
            
            RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = new Vector2(10, 10);
            textRectTransform.offsetMax = new Vector2(-10, -10);
            
            Text text = textObj.AddComponent<Text>();
            text.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.supportRichText = true;
            text.raycastTarget = false;
            text.alignment = TextAnchor.UpperLeft;
            text.text = "Performance Stats Loading...";
            
            // Add the stats component
            PerformanceStatsUI statsUI = _statsPanel.AddComponent<PerformanceStatsUI>();
            statsUI.statsText = text;
            
            return _statsPanel;
        }
        
        /// <summary>
        /// Toggles the visibility of the performance stats UI
        /// </summary>
        public void ToggleStatsVisibility()
        {
            if (_statsPanel != null)
            {
                _statsPanel.SetActive(!_statsPanel.activeSelf);
            }
        }
    }
}