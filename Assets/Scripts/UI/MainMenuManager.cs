using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Battle");
    }
    public void GoToShop()
    {
        SceneManager.LoadScene("Shop");
    }
    // "게임 종료" 버튼에 연결할 함수
    public void GoToInventory()
    {
        SceneManager.LoadScene("Inventory");
    }
    public void GoToCampaign()
    {
        SceneManager.LoadScene("Campaign");
    }
    public void QuitGame()
    {
        // 에디터에서 테스트할 때와 실제 빌드에서 모두 작동하도록 처리합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}