using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public Button saveButton;
    public Button loadButton;
    public Button continueButton;
    public Button exitButton;
    public Text statusText;
    
    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;
    
    private SaveManager saveManager;
    private bool isPaused = false;
    
    private void Start()
    {
        // Get reference to SaveManager
        saveManager = SaveManager.Instance;
        
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found!");
        }
        
        // Make sure pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
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
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGame);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        
        // Clear status text
        if (statusText != null)
        {
            statusText.text = "";
        }
    }
    
    private void Update()
    {
        // Toggle pause menu with Escape key
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePauseMenu();
        }
    }
    
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        // Show/hide pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
        
        // Pause/unpause game
        Time.timeScale = isPaused ? 0f : 1f;
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
            // Unpause before loading
            Time.timeScale = 1f;
            
            saveManager.LoadGame();
            
            if (statusText != null)
            {
                statusText.text = "Game loaded successfully!";
                Invoke("ClearStatusText", 3f);
            }
            
            // Close pause menu after loading
            isPaused = false;
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
        }
    }
    
    private void ContinueGame()
    {
        // Simply close the pause menu and resume game
        isPaused = false;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        Time.timeScale = 1f;
    }
    
    private void ExitGame()
    {
        // Unpause before exiting
        Time.timeScale = 1f;
        
        // Load the main menu scene
        // You can change "MainMenu" to the name of your menu scene
        SceneManager.LoadScene("StartScene");
    }
    
    private void ClearStatusText()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }
    
    // Make sure to reset timeScale when this script is disabled or destroyed
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}