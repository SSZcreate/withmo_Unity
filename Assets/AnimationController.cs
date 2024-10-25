using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRMLoader vrmLoader;
    [SerializeField] private DefaultAnimationController defaultAnimationController;

    [SerializeField] private GameObject targetObject;

    // string型の比率を受け取り、オブジェクトを移動する関数
    public void MoveObject(string ratioString)
    {
        // 例: ratioString = "0.45,0.31"
        string[] values = ratioString.Split(',');

        if (values.Length != 2)
        {
            Debug.LogError("Invalid ratio format. Please provide in 'x,y' format.");
            return;
        }

        // 文字列を float に変換
        if (float.TryParse(values[0], out float xRatio) && float.TryParse(values[1], out float yRatio))
        {
            // ビューポート座標を計算（左上が (0,0)、右下が (1,1) となるように y を反転）
            Vector3 viewportPosition = new Vector3(xRatio, 1 - yRatio, Camera.main.WorldToViewportPoint(targetObject.transform.position).z);

            // ビューポート座標をワールド座標に変換
            Vector3 worldPosition = Camera.main.ViewportToWorldPoint(viewportPosition);

            // オブジェクトの位置を更新
            targetObject.transform.position = worldPosition;
        }
        else
        {
            Debug.LogError("Failed to parse ratio values. Please ensure they are valid numbers.");
        }
    }

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
