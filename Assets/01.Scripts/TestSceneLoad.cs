using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            SceneLoadManager.Instance.LoadSceneAsync(SceneName.Stage1_Scene).Forget();
            //SceneLoadManager.Instance.LoadStageScene();
        }
    }
}
