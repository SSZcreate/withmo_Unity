using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRMLoader vrmLoader;
    [SerializeField] private DefaultAnimationController defaultAnimationController;

    /// <summary>
    /// ExitScreenアニメーションを両方のコントローラーでトリガーします。
    /// </summary>
    public void TriggerExitScreenAnimation()
    {
        if (vrmLoader != null)
        {
            vrmLoader.TriggerExitScreenAnimation();
        }
        else
        {
            Debug.LogWarning("VRMLoaderが割り当てられていません。");
        }

        if (defaultAnimationController != null)
        {
            defaultAnimationController.TriggerExitScreenAnimation();
        }
        else
        {
            Debug.LogWarning("DefaultAnimationControllerが割り当てられていません。");
        }
    }

    /// <summary>
    /// EnterScreenアニメーションを両方のコントローラーでトリガーします。
    /// </summary>
    public void TriggerEnterScreenAnimation()
    {
        if (vrmLoader != null)
        {
            vrmLoader.TriggerEnterScreenAnimation();
        }
        else
        {
            Debug.LogWarning("VRMLoaderが割り当てられていません。");
        }

        if (defaultAnimationController != null)
        {
            defaultAnimationController.TriggerEnterScreenAnimation();
        }
        else
        {
            Debug.LogWarning("DefaultAnimationControllerが割り当てられていません。");
        }
    }

    /// <summary>
    /// その他のメソッドも同様に実装可能です。
    /// 例: タッチアニメーションのトリガー
    /// </summary>
    public void TriggerTouchAnimation()
    {
        if (vrmLoader != null)
        {
            vrmLoader.TriggerTouchAnimation();
        }
        else
        {
            Debug.LogWarning("VRMLoaderが割り当てられていません。");
        }

        if (defaultAnimationController != null)
        {
            defaultAnimationController.TriggerTouchAnimation();
        }
        else
        {
            Debug.LogWarning("DefaultAnimationControllerが割り当てられていません。");
        }
    }

    // 必要に応じて、他のメソッドも追加してください。
}
