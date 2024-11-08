using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI restartButtonText;
    public TextMeshProUGUI quitButtonText;
    public Button restartButton;
    public Button quitButton;

    private void Start()
    {
        gameOverPanel.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        Debug.Log("Button click listeners added.");
    }

    public void ShowGameOverScreen(Team winningTeam)
    {
        gameOverPanel.SetActive(true);
        winnerText.text = winningTeam == Team.WHITE ? "White Wins!" : "Black Wins!";
        winnerText.color = winningTeam == Team.WHITE ? Color.black : Color.white;
        gameOverPanel.GetComponent<Image>().color = winningTeam == Team.WHITE ? Color.white : Color.black;

        Color buttonTextColor = winningTeam == Team.WHITE ? Color.black : Color.white;
        restartButtonText.color = buttonTextColor;
        quitButtonText.color = buttonTextColor;


        Color buttonBackgroundColor = winningTeam == Team.WHITE ? Color.white : Color.black;
        restartButton.GetComponent<Image>().color = buttonBackgroundColor;
        quitButton.GetComponent<Image>().color = buttonBackgroundColor;
    }

    public void RestartGame()
    {
        Debug.Log("RestartGame called.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame called.");
#if UNITY_EDITOR
        // Stop play mode in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // Quit the application in a built game
    Application.Quit();
#endif
    }
}
