using System.Text;
using UnityEngine;
using TMPro;
using V2.Data;
using UnityEngine.UI;

namespace V2.UI
{
    public class PerformanceStatsUI : MonoBehaviour
    {
        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool showDetailedStats = true;
        
        private float _timer;
        private StringBuilder _stringBuilder = new StringBuilder();
        private TextMeshProUGUI _statsText;
        private string _windowId = "PerformanceStats";
        private GameObject _windowObject;
        
        private void Start()
        {
            // Create window using WindowManager
            CreateStatsWindow();
        }
        
        private void CreateStatsWindow()
        {
            // Create a window for performance stats
            RectTransform contentArea = WindowManager.Instance.CreateWindow(_windowId, "Performance Stats");
            _windowObject = contentArea.transform.parent.gameObject;
            
            // Create stats text element
            GameObject textObject = new GameObject("StatsText");
            textObject.transform.SetParent(contentArea, false);
            
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);
            
            _statsText = textObject.AddComponent<TextMeshProUGUI>();
            _statsText.fontSize = 14;
            _statsText.alignment = TextAlignmentOptions.TopLeft;
            _statsText.enableWordWrapping = true;
            
            // Add reset button
            GameObject resetButton = new GameObject("ResetButton");
            resetButton.transform.SetParent(contentArea, false);
            
            RectTransform buttonRect = resetButton.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1, 0);
            buttonRect.anchorMax = new Vector2(1, 0);
            buttonRect.pivot = new Vector2(1, 0);
            buttonRect.anchoredPosition = new Vector2(-10, 10);
            buttonRect.sizeDelta = new Vector2(100, 30);
            
            Image buttonImage = resetButton.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            Button button = resetButton.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(ResetStats);
            
            // Add button text
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(resetButton.transform, false);
            
            RectTransform textRect = buttonTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Reset Stats";
            buttonText.fontSize = 12;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
        }
        
        private void Update()
        {
            _timer += Time.deltaTime;
            
            if (_timer >= updateInterval)
            {
                _timer = 0f;
                UpdateStatsDisplay();
            }
            
            // Check if window was closed
            if (_windowObject == null)
            {
                // Recreate window if it was closed
                CreateStatsWindow();
            }
        }
        
        private void UpdateStatsDisplay()
        {
            if (_statsText == null) return;
            
            _stringBuilder.Clear();
            var stats = ChunkData.PerformanceStats;
            
            _stringBuilder.AppendLine("<b>PERFORMANCE STATS (ms)</b>");
            
            // Machines stats
            AppendComponentStats("Machines", stats.MachineStats);
            
            // Connectors stats
            AppendComponentStats("Connectors", stats.ConnectorStats);
            
            // Belts stats
            AppendComponentStats("Belts", stats.BeltStats);
            
            // Total stats
            _stringBuilder.AppendLine("\n<b>Total:</b>");
            _stringBuilder.AppendFormat("  Min: {0:F3} | Max: {1:F3} | Avg: {2:F3}", 
                stats.TotalStats.MinTime, 
                stats.TotalStats.MaxTime, 
                stats.TotalStats.GetRecentAverage());
            
            _statsText.text = _stringBuilder.ToString();
        }
        
        private void AppendComponentStats(string name, ComponentStats stats)
        {
            _stringBuilder.AppendFormat("<b>{0}:</b>\n", name);
            _stringBuilder.AppendFormat("  Min: {0:F3} | Max: {1:F3} | Avg: {2:F3}", 
                stats.MinTime, 
                stats.MaxTime, 
                stats.GetRecentAverage());
            
            if (showDetailedStats)
            {
                _stringBuilder.AppendFormat("\n  All-time Avg: {0:F3} | Samples: {1}", 
                    stats.AverageTime, 
                    stats.SampleCount);
            }
            
            _stringBuilder.AppendLine();
        }
        
        public void ResetStats()
        {
            ChunkData.PerformanceStats.Reset();
        }
        
        private void OnDestroy()
        {
            // Close the window when this component is destroyed
            if (WindowManager.Instance != null)
            {
                WindowManager.Instance.CloseWindow(_windowId);
            }
        }
    }
}