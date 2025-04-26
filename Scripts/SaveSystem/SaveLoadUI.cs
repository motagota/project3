using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUI : MonoBehaviour
{
    [Header("UI References")]
    public Button saveButton;
    public Button loadButton;
    public Text statusText;
    
    private SaveManager saveManager;
    
    private void Start()
    {
        saveManager = SaveManager.Instance;
        
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found!");
            return;
        }
        
        // Set up button listeners
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveGame);
        }
        
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadGame);
        }
        
        // Clear status text
        if (statusText != null)
        {
            statusText.text = "";
        }
    }
    
    private void SaveGame()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame();
            
            if (statusText != null)
            {
                statusText.text = "Game saved successfully!";
                Invoke("ClearStatusText", 3f);
            }
        }
    }
    
    private void LoadGame()
    {
        if (saveManager != null)
        {
            saveManager.LoadGame();
            
            if (statusText != null)
            {
                statusText.text = "Game loaded successfully!";
                Invoke("ClearStatusText", 3f);
            }
        }
    }
    
    private void ClearStatusText()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }
}