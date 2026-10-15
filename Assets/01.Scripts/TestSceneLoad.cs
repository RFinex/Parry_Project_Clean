using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : InteractableBase
{
    public override bool CanInteract => true;

    public override void Interact(PlayerController interactor)
    {
        SceneLoadManager.Instance.LoadSceneAsync(SceneName.Stage1_Scene).Forget();
    }

}
