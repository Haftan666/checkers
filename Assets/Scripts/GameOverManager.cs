using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI restartButtonText;
    public TextMeshProUGUI quitButtonText;
    public Button restartButton;
    public Button quitButton;
    public float animationDuration = 1.0f;

    private void Start()
    {
        gameOverPanel.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        Debug.Log("Button click listeners added.");
    }

    public void ShowGameOverScreen(Team winningTeam)
    {
        winnerText.text = winningTeam == Team.WHITE ? "White Wins!" : "Black Wins!";
        winnerText.color = winningTeam == Team.WHITE ? Color.black : Color.white;
        gameOverPanel.GetComponent<Image>().color = winningTeam == Team.WHITE ? Color.white : Color.black;

        Color buttonTextColor = winningTeam == Team.WHITE ? Color.black : Color.white;
        restartButtonText.color = buttonTextColor;
        quitButtonText.color = buttonTextColor;

        Color buttonBackgroundColor = winningTeam == Team.WHITE ? Color.white : Color.black;
        restartButton.GetComponent<Image>().color = buttonBackgroundColor;
        quitButton.GetComponent<Image>().color = buttonBackgroundColor;

        Color borderColor = winningTeam == Team.WHITE ? Color.white : Color.black;
        restartButton.GetComponent<Image>().color = borderColor;
        quitButton.GetComponent<Image>().color = borderColor;

        StartCoroutine(AnimateGameOverScreen());
    }

    private IEnumerator AnimateGameOverScreen()
    {
        gameOverPanel.SetActive(true);

        RectTransform panelRectTransform = gameOverPanel.GetComponent<RectTransform>();
        Vector3 startPosition = new Vector3(panelRectTransform.anchoredPosition.x, -Screen.height, 0);
        Vector3 endPosition = new Vector3(panelRectTransform.anchoredPosition.x, 0, 0);
        CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0;

        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            panelRectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / animationDuration);
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panelRectTransform.anchoredPosition = endPosition;
        canvasGroup.alpha = 1;
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
