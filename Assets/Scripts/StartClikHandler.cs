using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartClikHandler : MonoBehaviour
{
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadScence);
    }
    private void LoadScence()
    {
        Debug.Log("µã»÷");
        SceneManager.LoadScene("SampleScene");
    }
}
