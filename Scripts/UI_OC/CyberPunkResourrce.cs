using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CyberpunkUI
{
    /// <summary>
    /// A cyberpunk-styled resource graph that displays production and consumption rates
    /// </summary>
    public class CyberpunkResourceGraph : MonoBehaviour
    {
        [Header("Graph Settings")]
        [SerializeField] private int graphResolution = 100;
        [SerializeField] private float graphUpdateInterval = 0.5f;
        [SerializeField] private int dataPointsToStore = 60;
        [SerializeField] private float graphHeight = 100f;
        [SerializeField] private float graphWidth = 400f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color productionColor = new Color(0f, 1f, 0.8f, 0.8f); // Cyan
        [SerializeField] private Color consumptionColor = new Color(1f, 0.3f, 0.7f, 0.8f); // Magenta
        [SerializeField] private Color gridColor = new Color(0.15f, 0.15f, 0.2f, 0.5f);
        [SerializeField] private Color backgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.9f);
        [SerializeField] private float lineThickness = 2f;
        [SerializeField] private bool useGlowEffect = true;
        [SerializeField] private float glowIntensity = 1.5f;
        
        [Header("References")]
        [SerializeField] private RectTransform graphContainer;
        [SerializeField] private RectTransform productionLineParent;
        [SerializeField] private RectTransform consumptionLineParent;
        [SerializeField] private RectTransform gridLinesParent;
        [SerializeField] private TextMeshProUGUI resourceNameText;
        [SerializeField] private TextMeshProUGUI productionValueText;
        [SerializeField] private TextMeshProUGUI consumptionValueText;
        [SerializeField] private TextMeshProUGUI netValueText;
        [SerializeField] private Image backgroundPanel;
        [SerializeField] private ParticleSystem glowParticles;
        
        [Header("Animation")]
        [SerializeField] private float animationSpeed = 5f;
        [SerializeField] private AnimationCurve dataTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        // Runtime data
        private List<float> productionData = new List<float>();
        private List<float> consumptionData = new List<float>();
        private List<Image> productionLineSegments = new List<Image>();
        private List<Image> consumptionLineSegments = new List<Image>();
        private float currentMaxValue = 10f;
        private float targetMaxValue = 10f;
        private float displayedProductionValue = 0f;
        private float displayedConsumptionValue = 0f;
        private float smoothedProductionValue = 0f;
        private float smoothedConsumptionValue = 0f;
        private float timeSinceLastUpdate = 0f;
        
        // Resource tracking
        private string resourceName = "Energy";
        private float currentProductionRate = 0f;
        private float currentConsumptionRate = 0f;
        
        // Prefabs and materials
        private GameObject lineSegmentPrefab;
        private Material glowMaterial;
        public Material GlowMat;
        private void Awake()
        {
            InitializeComponents();
            InitializeData();
            CreateGrid();
            StartCoroutine(AnimateStartup());
        }
        
        private void InitializeComponents()
        {
            // Create components if not assigned in inspector
            if (graphContainer == null)
            {
                GameObject container = new GameObject("Graph Container", typeof(RectTransform));
                container.transform.SetParent(transform);
                graphContainer = container.GetComponent<RectTransform>();
                graphContainer.sizeDelta = new Vector2(graphWidth, graphHeight);
                graphContainer.anchoredPosition = Vector2.zero;
            }
            
            if (backgroundPanel == null)
            {
                GameObject bgPanel = new GameObject("Background Panel", typeof(RectTransform), typeof(Image));
                bgPanel.transform.SetParent(graphContainer);
                backgroundPanel = bgPanel.GetComponent<Image>();
                backgroundPanel.color = backgroundColor;
                backgroundPanel.rectTransform.sizeDelta = new Vector2(graphWidth, graphHeight);
                backgroundPanel.rectTransform.anchoredPosition = Vector2.zero;
            }
            
            if (productionLineParent == null)
            {
                GameObject prodLines = new GameObject("Production Lines", typeof(RectTransform));
                prodLines.transform.SetParent(graphContainer);
                productionLineParent = prodLines.GetComponent<RectTransform>();
                productionLineParent.sizeDelta = new Vector2(graphWidth, graphHeight);
                productionLineParent.anchoredPosition = Vector2.zero;
            }
            
            if (consumptionLineParent == null)
            {
                GameObject consLines = new GameObject("Consumption Lines", typeof(RectTransform));
                consLines.transform.SetParent(graphContainer);
                consumptionLineParent = consLines.GetComponent<RectTransform>();
                consumptionLineParent.sizeDelta = new Vector2(graphWidth, graphHeight);
                consumptionLineParent.anchoredPosition = Vector2.zero;
            }
            
            if (gridLinesParent == null)
            {
                GameObject gridLines = new GameObject("Grid Lines", typeof(RectTransform));
                gridLines.transform.SetParent(graphContainer);
                gridLinesParent = gridLines.GetComponent<RectTransform>();
                gridLinesParent.sizeDelta = new Vector2(graphWidth, graphHeight);
                gridLinesParent.anchoredPosition = Vector2.zero;
            }
            
            // Create prefab for line segments
            lineSegmentPrefab = new GameObject("Line Segment", typeof(RectTransform), typeof(Image));
            lineSegmentPrefab.SetActive(false);
            
            // Create glow effect material
            if (useGlowEffect)
            {
                glowMaterial = new Material(GlowMat);
                InitializeGlowParticles();
            }
        }
        
        private void InitializeGlowParticles()
        {
            if (glowParticles == null)
            {
                GameObject particleObj = new GameObject("Glow Particles", typeof(ParticleSystem));
                particleObj.transform.SetParent(graphContainer);
                glowParticles = particleObj.GetComponent<ParticleSystem>();
                
                var main = glowParticles.main;
                main.startLifetime = 1.5f;
                main.startSize = 2f;
                main.startSpeed = 0.5f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                var emission = glowParticles.emission;
                emission.rateOverTime = 15f;
                
                var shape = glowParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Rectangle;
                shape.scale = new Vector3(graphWidth * 0.9f, graphHeight * 0.5f, 1f);
                shape.position = new Vector3(0f, 0f, -1f);
                
                var colorOverLifetime = glowParticles.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] 
                    { 
                        new GradientColorKey(productionColor, 0.0f),
                        new GradientColorKey(consumptionColor, 1.0f) 
                    },
                    new GradientAlphaKey[] 
                    { 
                        new GradientAlphaKey(0.0f, 0.0f),
                        new GradientAlphaKey(0.3f * glowIntensity, 0.3f),
                        new GradientAlphaKey(0.0f, 1.0f) 
                    }
                );
                colorOverLifetime.color = gradient;
                
                var renderer = glowParticles.GetComponent<ParticleSystemRenderer>();
                renderer.material = glowMaterial;
                renderer.sortingOrder = -1;
            }
        }
        
        private void InitializeData()
        {
            // Initialize with zero data
            for (int i = 0; i < dataPointsToStore; i++)
            {
                productionData.Add(0f);
                consumptionData.Add(0f);
            }
            
            // Create initial line segments
            CreateLineSegments();
        }
        
        private void CreateLineSegments()
        {
            // Clean up existing segments
            foreach (var segment in productionLineSegments)
            {
                if (segment != null)
                {
                    Destroy(segment.gameObject);
                }
            }
            productionLineSegments.Clear();
            
            foreach (var segment in consumptionLineSegments)
            {
                if (segment != null)
                {
                    Destroy(segment.gameObject);
                }
            }
            consumptionLineSegments.Clear();
            
            // Create new segments
            for (int i = 0; i < graphResolution - 1; i++)
            {
                // Production line segments
                GameObject prodSegObj = Instantiate(lineSegmentPrefab, productionLineParent);
                prodSegObj.SetActive(true);
                Image prodImage = prodSegObj.GetComponent<Image>();
                prodImage.color = productionColor;
                productionLineSegments.Add(prodImage);
                
                // Consumption line segments  
                GameObject consSegObj = Instantiate(lineSegmentPrefab, consumptionLineParent);
                consSegObj.SetActive(true);
                Image consImage = consSegObj.GetComponent<Image>();
                consImage.color = consumptionColor;
                consumptionLineSegments.Add(consImage);
            }
        }
        
        private void CreateGrid()
        {
            // Create horizontal grid lines
            int numHorizontalLines = 5;
            for (int i = 0; i <= numHorizontalLines; i++)
            {
                GameObject lineObj = new GameObject($"HGrid_{i}", typeof(RectTransform), typeof(Image));
                lineObj.transform.SetParent(gridLinesParent);
                Image lineImage = lineObj.GetComponent<Image>();
                lineImage.color = gridColor;
                
                RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
                float yPos = (graphHeight * i) / numHorizontalLines - graphHeight / 2f;
                rectTransform.anchoredPosition = new Vector2(0f, yPos);
                rectTransform.sizeDelta = new Vector2(graphWidth, 1f);
                
                // Add label
                GameObject labelObj = new GameObject($"Label_{i}", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(gridLinesParent);
                TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
                label.fontSize = 10;
                label.alignment = TextAlignmentOptions.MidlineRight;
                label.color = new Color(0.7f, 0.7f, 0.8f);
                
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchoredPosition = new Vector2(-graphWidth / 2f - 10f, yPos);
                labelRect.sizeDelta = new Vector2(40f, 15f);
            }
            
            // Create vertical grid lines
            int numVerticalLines = 6;
            for (int i = 0; i <= numVerticalLines; i++)
            {
                GameObject lineObj = new GameObject($"VGrid_{i}", typeof(RectTransform), typeof(Image));
                lineObj.transform.SetParent(gridLinesParent);
                Image lineImage = lineObj.GetComponent<Image>();
                lineImage.color = gridColor;
                
                RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
                float xPos = (graphWidth * i) / numVerticalLines - graphWidth / 2f;
                rectTransform.anchoredPosition = new Vector2(xPos, 0f);
                rectTransform.sizeDelta = new Vector2(1f, graphHeight);
            }
        }
        
        private IEnumerator AnimateStartup()
        {
            // Fade in background
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                backgroundPanel.color = new Color(
                    backgroundColor.r, 
                    backgroundColor.g, 
                    backgroundColor.b, 
                    Mathf.Lerp(0f, backgroundColor.a, t)
                );
                yield return null;
            }
            
            // Initialize text labels with typewriter effect
            if (resourceNameText != null)
            {
                StartCoroutine(TypewriterEffect(resourceNameText, resourceName, 0.05f));
            }
            
            // Simulate some data coming in
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(SimulateDataForDemo());
        }
        
        private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string finalText, float characterDelay)
        {
            textComponent.text = "";
            foreach (char c in finalText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(characterDelay);
            }
        }
        
        private IEnumerator SimulateDataForDemo()
        {
            // Just a demo simulation - replace with real data in production
            float time = 0;
            while (true)
            {
                time += Time.deltaTime;
                
                // Simulate some interesting patterns
                float prodBase = 10f + 5f * Mathf.Sin(time * 0.5f);
                float consBase = 8f + 4f * Mathf.Cos(time * 0.3f);
                
                // Add noise for realism
                float prodNoise = UnityEngine.Random.Range(-1f, 1f);
                float consNoise = UnityEngine.Random.Range(-0.8f, 0.8f);
                
                // Occasionally add spikes
                if (UnityEngine.Random.value > 0.95f)
                {
                    prodNoise += UnityEngine.Random.Range(0f, 5f);
                }
                
                if (UnityEngine.Random.value > 0.97f)
                {
                    consNoise += UnityEngine.Random.Range(0f, 6f);
                }
                
                // Update values
                UpdateResourceValues(prodBase + prodNoise, consBase + consNoise);
                
                yield return null;
            }
        }
        
        public void UpdateResourceValues(float production, float consumption)
        {
            currentProductionRate = production;
            currentConsumptionRate = consumption;
        }
        
        private void Update()
        {
            // Animate smoothing of values
            smoothedProductionValue = Mathf.Lerp(smoothedProductionValue, currentProductionRate, Time.deltaTime * animationSpeed);
            smoothedConsumptionValue = Mathf.Lerp(smoothedConsumptionValue, currentConsumptionRate, Time.deltaTime * animationSpeed);
            
            // Update text displays with cyberpunk formatting
            if (productionValueText != null)
            {
                displayedProductionValue = Mathf.Lerp(displayedProductionValue, smoothedProductionValue, Time.deltaTime * animationSpeed * 2f);
                productionValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(productionColor)}>▲</color> {displayedProductionValue:F1}";
            }
            
            if (consumptionValueText != null)
            {
                displayedConsumptionValue = Mathf.Lerp(displayedConsumptionValue, smoothedConsumptionValue, Time.deltaTime * animationSpeed * 2f);
                consumptionValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(consumptionColor)}>▼</color> {displayedConsumptionValue:F1}";
            }
            
            if (netValueText != null)
            {
                float net = displayedProductionValue - displayedConsumptionValue;
                string sign = net >= 0 ? "+" : "";
                Color netColor = net >= 0 ? new Color(0.2f, 0.8f, 0.4f) : new Color(0.8f, 0.2f, 0.2f);
                netValueText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(netColor)}>{sign}{net:F1}</color>";
            }
            
            // Only update graph at intervals to save performance
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= graphUpdateInterval)
            {
                timeSinceLastUpdate = 0f;
                UpdateGraphData();
                UpdateMaxValue();
                UpdateGraphVisuals();
            }
        }
        
        private void UpdateGraphData()
        {
            // Add new data points
            productionData.Add(smoothedProductionValue);
            consumptionData.Add(smoothedConsumptionValue);
            
            // Remove oldest data points if we have too many
            if (productionData.Count > dataPointsToStore)
            {
                productionData.RemoveAt(0);
                consumptionData.RemoveAt(0);
            }
        }
        
        private void UpdateMaxValue()
        {
            // Find the maximum value in our data to scale the graph properly
            float maxDataValue = 10f;  // Minimum scale
            
            foreach (float value in productionData)
            {
                maxDataValue = Mathf.Max(maxDataValue, value);
            }
            
            foreach (float value in consumptionData)
            {
                maxDataValue = Mathf.Max(maxDataValue, value);
            }
            
            // Add 20% headroom
            targetMaxValue = maxDataValue * 1.2f;
            
            // Smooth the scale transition
            currentMaxValue = Mathf.Lerp(currentMaxValue, targetMaxValue, Time.deltaTime * 2f);
            
            // Update grid labels
            UpdateGridLabels();
        }
        
        private void UpdateGridLabels()
        {
            // Update grid line labels to reflect current scale
            int numHorizontalLines = 5;
            for (int i = 0; i <= numHorizontalLines; i++)
            {
                Transform labelTransform = gridLinesParent.Find($"Label_{i}");
                if (labelTransform != null)
                {
                    TextMeshProUGUI label = labelTransform.GetComponent<TextMeshProUGUI>();
                    float value = (currentMaxValue * i) / numHorizontalLines;
                    label.text = value.ToString("F1");
                }
            }
        }
        
        private void UpdateGraphVisuals()
        {
            // Calculate steps for sampling the data
            float step = (float)(productionData.Count - 1) / (graphResolution - 1);
            
            // Update each line segment to connect the data points
            for (int i = 0; i < graphResolution - 1; i++)
            {
                // Calculate indices into our data lists
                float indexFloat = i * step;
                float nextIndexFloat = (i + 1) * step;
                
                int index = Mathf.FloorToInt(indexFloat);
                int nextIndex = Mathf.FloorToInt(nextIndexFloat);
                
                // Handle edge cases
                index = Mathf.Clamp(index, 0, productionData.Count - 1);
                nextIndex = Mathf.Clamp(nextIndex, 0, productionData.Count - 1);
                
                // Get heights based on data values, scaled to current max
                float prodHeight1 = (productionData[index] / currentMaxValue) * graphHeight;
                float prodHeight2 = (productionData[nextIndex] / currentMaxValue) * graphHeight;
                
                float consHeight1 = (consumptionData[index] / currentMaxValue) * graphHeight;
                float consHeight2 = (consumptionData[nextIndex] / currentMaxValue) * graphHeight;
                
                // Position and size the line segments
                UpdateLineSegment(productionLineSegments[i], i, prodHeight1, prodHeight2);
                UpdateLineSegment(consumptionLineSegments[i], i, consHeight1, consHeight2);
            }
        }
        
        private void UpdateLineSegment(Image lineSegment, int index, float height1, float height2)
        {
            // Calculate positions for the start and end of the line segment
            float segmentWidth = graphWidth / (graphResolution - 1);
            float xPos = index * segmentWidth - graphWidth / 2f + segmentWidth / 2f;
            
            // Calculate the midpoint between the two data points
            float yPos = (height1 + height2) / 4f - graphHeight / 2f; // Divide by 4 to center it
            
            // Calculate the length and angle of the line segment
            float deltaX = segmentWidth;
            float deltaY = height2 - height1;
            float length = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float angle = Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg;
            
            // Update the line segment's position, size, and rotation
            RectTransform rectTransform = lineSegment.rectTransform;
            rectTransform.anchoredPosition = new Vector2(xPos, yPos);
            rectTransform.sizeDelta = new Vector2(length, lineThickness);
            rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        
        // Public methods for external systems to update the graph
        public void SetResourceName(string name)
        {
            resourceName = name;
            if (resourceNameText != null)
            {
                resourceNameText.text = resourceName;
            }
        }
        
        public void ClearData()
        {
            productionData.Clear();
            consumptionData.Clear();
            
            for (int i = 0; i < dataPointsToStore; i++)
            {
                productionData.Add(0f);
                consumptionData.Add(0f);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            if (lineSegmentPrefab != null)
            {
                Destroy(lineSegmentPrefab);
            }
            
            if (glowMaterial != null)
            {
                Destroy(glowMaterial);
            }
        }
    }
}