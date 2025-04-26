using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI controls for terrain generation parameters
/// </summary>
public class TerrainGenerationUI : MonoBehaviour
{
    [Header("References")] public GridComputeManager gridManager;

    public TerrainGenerationParams terrainParams;
    public RawImage mapPreview;

    [Header("Seed Controls")] public TMP_InputField seedInputField;

    public Button randomSeedButton;

    [Header("Water Controls")] public Slider waterLevelSlider;

    public Slider waterFrequencySlider;
    public Slider sandBorderSlider;

    [Header("Resource Controls")] public Slider coalFrequencySlider;

    public Slider coalThresholdSlider;
    public Slider coalRichnessSlider;

    public Slider ironFrequencySlider;
    public Slider ironThresholdSlider;
    public Slider ironRichnessSlider;

    public Slider copperFrequencySlider;
    public Slider copperThresholdSlider;
    public Slider copperRichnessSlider;

    public Slider stoneFrequencySlider;
    public Slider stoneThresholdSlider;
    public Slider stoneRichnessSlider;

    [Header("Game Controls")] 
    public Button startGameButton;
    public Button backButton;

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("Required references are missing!");
            return;
        }
        
        InitializeUI();
        SetupListeners();
        UpdateAllTexts();

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGame);
        }
        
        if (startGameButton != null)
        {
           backButton.onClick.AddListener(OnBackGame);
        }
    }

    private void OnBackGame()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("SaveLoadScene");
    }

    private void InitializeUI()
    {
        // Set initial values from TerrainParams
        waterLevelSlider.value = terrainParams.waterLevel;
        waterFrequencySlider.value = terrainParams.waterFrequency;
        sandBorderSlider.value = terrainParams.sandBorderSize;

        // Coal
        coalFrequencySlider.value = terrainParams.coalFrequency;
        coalThresholdSlider.value = terrainParams.coalThreshold;
        coalRichnessSlider.value = terrainParams.coalRichness;

        // Iron
        ironFrequencySlider.value = terrainParams.ironFrequency;
        ironThresholdSlider.value = terrainParams.ironThreshold;
        ironRichnessSlider.value = terrainParams.ironRichness;

        // Copper
        copperFrequencySlider.value = terrainParams.copperFrequency;
        copperThresholdSlider.value = terrainParams.copperThreshold;
        copperRichnessSlider.value = terrainParams.copperRichness;

        // Stone
        stoneFrequencySlider.value = terrainParams.stoneFrequency;
        stoneThresholdSlider.value = terrainParams.stoneThreshold;
        stoneRichnessSlider.value = terrainParams.stoneRichness;

        // Update seed input field to use terrainParams instead of gridManager
        seedInputField.text = terrainParams.seed.ToString();
    }

    private bool ValidateReferences()
    {
        return gridManager != null && terrainParams != null;
    }
    
    private void SetupListeners()
    {
        if (seedInputField != null)
        {
            seedInputField.onEndEdit.AddListener(OnSeedChanged);
        }

        randomSeedButton.onClick.AddListener(() =>
        {
            terrainParams.seed = UnityEngine.Random.Range(-10000f, 10000f);
            seedInputField.text = terrainParams.seed.ToString();
            gridManager.GenerateNewMap(terrainParams);
        });

        // Water controls
        waterLevelSlider.onValueChanged.AddListener(OnWaterLevelChanged);
        waterFrequencySlider.onValueChanged.AddListener(OnWaterFrequencyChanged);
        sandBorderSlider.onValueChanged.AddListener(OnSandBorderChanged);

        // Resource controls

        // coal
        coalFrequencySlider.onValueChanged.AddListener((value) => OnResourceFrequencyChanged(0, value));
        coalThresholdSlider.onValueChanged.AddListener((value) => OnResourceThresholdChanged(0, value));
        coalRichnessSlider.onValueChanged.AddListener((value) => OnResourceRichnessChanged(0, value));

        // iron
        ironFrequencySlider.onValueChanged.AddListener((value) => OnResourceFrequencyChanged(1, value));
        ironThresholdSlider.onValueChanged.AddListener((value) => OnResourceThresholdChanged(1, value));
        ironRichnessSlider.onValueChanged.AddListener((value) => OnResourceRichnessChanged(1, value));
        // copper
        copperFrequencySlider.onValueChanged.AddListener((value) => OnResourceFrequencyChanged(2, value));
        copperThresholdSlider.onValueChanged.AddListener((value) => OnResourceThresholdChanged(2, value));
        copperRichnessSlider.onValueChanged.AddListener((value) => OnResourceRichnessChanged(2, value));
        // stone
        stoneFrequencySlider.onValueChanged.AddListener((value) => OnResourceFrequencyChanged(3, value));
        stoneThresholdSlider.onValueChanged.AddListener((value) => OnResourceThresholdChanged(3, value));
        stoneRichnessSlider.onValueChanged.AddListener((value) => OnResourceRichnessChanged(3, value));
    }

    private void UpdateAllTexts()
    {
        // Water controls
        if (waterLevelSlider.gameObject.GetComponentInChildren<TMP_Text>() != null)
        {
            waterLevelSlider.gameObject.GetComponentInChildren<TMP_Text>().text = $"Water Level: {waterLevelSlider.value:F2}";
        }
        if (waterFrequencySlider.gameObject.GetComponentInChildren<TMP_Text>() != null)
        {
            waterFrequencySlider.gameObject.GetComponentInChildren<TMP_Text>().text = $"Water Frequency: {waterFrequencySlider.value:F2}";
        }
        if (sandBorderSlider.gameObject.GetComponentInChildren<TMP_Text>() != null)
        {
            sandBorderSlider.gameObject.GetComponentInChildren<TMP_Text>().text = $"Sand Border: {sandBorderSlider.value:F2}";
        }

        // Coal
        UpdateResourceTexts(coalFrequencySlider, coalThresholdSlider, coalRichnessSlider, "Coal");
        
        // Iron
        UpdateResourceTexts(ironFrequencySlider, ironThresholdSlider, ironRichnessSlider, "Iron");
        
        // Copper
        UpdateResourceTexts(copperFrequencySlider, copperThresholdSlider, copperRichnessSlider, "Copper");
        
        // Stone
        UpdateResourceTexts(stoneFrequencySlider, stoneThresholdSlider, stoneRichnessSlider, "Stone");
    }

    private void UpdateResourceTexts(Slider frequencySlider, Slider thresholdSlider, Slider richnessSlider, string resourceName)
    {
        if (frequencySlider.gameObject.GetComponentInChildren<TMP_Text>() != null)
        {
            frequencySlider.gameObject.GetComponentInChildren<TMP_Text>().text = 
                $"{resourceName} Frequency: {frequencySlider.value:F2}";
        }
        if (thresholdSlider.gameObject.GetComponentInChildren<TMP_Text>() != null)
        {
            thresholdSlider.gameObject.GetComponentInChildren<TMP_Text>().text = 
                $"{resourceName} Size: {thresholdSlider.value:F2}";
        }
        if (richnessSlider.gameObject.GetComponentInChildren<TMP_Text>() != null)
        {
            richnessSlider.gameObject.GetComponentInChildren<TMP_Text>().text = 
                $"{resourceName} Richness: {richnessSlider.value:F2}";
        }
    }

    // Event Handlers
    private void OnSeedChanged(string value)
    {
        if (float.TryParse(value, out float newSeed))
        {
            terrainParams.seed = newSeed;
            gridManager.GenerateNewMap(terrainParams);
        }
    }


    private void OnRandomSeedClicked()
    {
        terrainParams.seed = UnityEngine.Random.Range(-10000f, 10000f);
        seedInputField.text = terrainParams.seed.ToString();
        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnWaterLevelChanged(float value)
    {
        terrainParams.waterLevel = value;
        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnWaterFrequencyChanged(float value)
    {
        terrainParams.waterFrequency = value;
        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnSandBorderChanged(float value)
    {
        terrainParams.sandBorderSize = value;
        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnResourceFrequencyChanged(int index, float value)
    {
        switch (index)
        {
            case 0:
                terrainParams.coalFrequency = value;
                break;
            case 1:
                terrainParams.ironFrequency = value;
                break;
            case 2:
                terrainParams.copperFrequency = value;
                break;
            case 3:
                terrainParams.stoneFrequency = value;
                break;
        }

        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnResourceThresholdChanged(int index, float value)
    {
        switch (index)
        {
            case 0:
                terrainParams.coalThreshold = value;
                break;
            case 1:
                terrainParams.ironThreshold = value;
                break;
            case 2:
                terrainParams.copperThreshold = value;
                break;
            case 3:
                terrainParams.stoneThreshold = value;
                break;
        }

        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnResourceRichnessChanged(int index, float value)
    {
        switch (index)
        {
            case 0:
                terrainParams.coalRichness = value;
                break;
            case 1:
                terrainParams.ironRichness = value;
                break;
            case 2:
                terrainParams.copperRichness = value;
                break;
            case 3:
                terrainParams.stoneRichness = value;
                break;
        }

        gridManager.GenerateNewMap(terrainParams);
    }

    private void OnStartGame()
    {
        // Save the current terrain parameters to PlayerPrefs
        string paramsJson = JsonUtility.ToJson(terrainParams);
        PlayerPrefs.SetString("TerrainParams", paramsJson);
        
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    private void OnDestroy()
    {
        // Save the terrain parameters when the UI is destroyed
        if (terrainParams != null)
        {
            string paramsJson = JsonUtility.ToJson(terrainParams);
            PlayerPrefs.SetString("TerrainParams", paramsJson);
            PlayerPrefs.Save();
        }
    }
}