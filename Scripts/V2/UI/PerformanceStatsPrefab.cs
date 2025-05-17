using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace V2.UI
{
    /// <summary>
    /// Creates a performance stats UI panel at runtime using the WindowManager.
    /// </summary>
    public class PerformanceStatsPrefab : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private bool createOnStart = true;
        
        private void Start()
        {
            if (createOnStart)
            {
                CreatePerformanceStatsUI();
            }
        }
        
        /// <summary>
        /// Creates a performance stats UI panel using the WindowManager.
        /// </summary>
        public void CreatePerformanceStatsUI()
        {
            // Ensure WindowManager exists
            if (WindowManager.Instance == null)
            {
                Debug.LogError("WindowManager not found. Please add a WindowManager to the scene.");
                return;
            }
            
            // Add PerformanceStatsUI component to this GameObject
            PerformanceStatsUI statsUI = gameObject.AddComponent<PerformanceStatsUI>();
            
            // The PerformanceStatsUI will create its own window through the WindowManager in its Start method
        }
    }
}