using UnityEngine;

namespace V2.Data
{
    public class UPSMonitor
    {
        private long frameCount;
        private float  lastSecondTime;
        private int ups;
    
        public UPSMonitor() {
            frameCount = 0;
            ups = 0;
        }
    
        public void update() {
            frameCount++;
        
            float  currentTime = Time.realtimeSinceStartup;
            if (currentTime - lastSecondTime >= 1) {
                ups = (int) frameCount;
                frameCount = 0;
                lastSecondTime = currentTime;
            }
        }
    
        public int getUPS() {
            return ups;
        }
    }
}