using UnityEngine;
using UnityEngine.UI;

namespace SaveSystem
{
    public class SaveLoadUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button saveButton;
        public Button exitButton;
        public Button loadButton;
        public Button newGameButton;
        public Text statusText;
    
        private SaveManager _saveManager;
    
        private void Start()
        {
            _saveManager = SaveManager.Instance;
        
            if (_saveManager == null)
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

            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(NewGame);
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

        private void ExitGame()
        {
            // Quit the application
            Debug.Log("Exiting game");
            Application.Quit();
        
            // This line will only be reached in the Unity Editor since Application.Quit() doesn't work there
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void NewGame()
        {
            // Load the game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
        }

        private void SaveGame()
        {
            if (_saveManager != null)
            {
                _saveManager.SaveGame();
            
                if (statusText != null)
                {
                    statusText.text = "Game saved successfully!";
                    Invoke("ClearStatusText", 3f);
                }
            }
        }
    
        private void LoadGame()
        {
            if (_saveManager != null)
            {
                _saveManager.LoadGame();

                if (statusText == null) return;
                statusText.text = "Game loaded successfully!";
                Invoke("ClearStatusText", 3f);
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
}