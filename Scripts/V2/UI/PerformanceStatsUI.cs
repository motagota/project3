using System.Text;
using UnityEngine;
using UnityEngine.UI;
using V2.Data;

namespace V2.UI
{
    public class PerformanceStatsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Text statsText;
        
        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool showDetailedStats = true;
        
        private float _timer;
        private StringBuilder _stringBuilder = new StringBuilder();
        
        private void Update()
        {
            _timer += Time.deltaTime;
            
            if (_timer >= updateInterval)
            {
                _timer = 0f;
                UpdateStatsDisplay();
            }
        }
        
        private void UpdateStatsDisplay()
        {
            if (statsText == null) return;
            
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
            
            statsText.text = _stringBuilder.ToString();
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
    }
}