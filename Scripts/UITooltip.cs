using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITooltip : MonoBehaviour
{
    public static UITooltip Instance { get; private set; }
    
    [SerializeField] private RectTransform background;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private Vector2 padding = new Vector2(20, 20);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Create Canvas if it doesn't exist
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("TooltipCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Setup tooltip
        gameObject.transform.SetParent(canvas.transform, false);

        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        background = bgObj.AddComponent<RectTransform>();
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        bgImage.raycastTarget = false;

        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bgObj.transform, false);
        tooltipText = textObj.AddComponent<TextMeshProUGUI>();
        tooltipText.color = Color.white;
        tooltipText.fontSize = 14;
        tooltipText.alignment = TextAlignmentOptions.Left;
        tooltipText.raycastTarget = false;

        // Setup RectTransforms
        RectTransform textRect = tooltipText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);

        // Position the tooltip panel in a fixed position (bottom left corner)
        background.anchorMin = new Vector2(0, 0);
        background.anchorMax = new Vector2(0, 0);
        background.pivot = new Vector2(0, 0);
        background.anchoredPosition = new Vector2(20, 20);
        background.sizeDelta = new Vector2(200, 100); // Set a default size

        // Hide initially
        gameObject.SetActive(false);
    }

    // Modified to ignore position parameter
    public void Show(string text, Vector2 position = default)
    {
        gameObject.SetActive(true);
        tooltipText.text = text;
        
        // Resize based on content
        Vector2 textSize = tooltipText.GetPreferredValues();
        background.sizeDelta = textSize + padding;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}