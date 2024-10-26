using UnityEngine;
using System.Reflection;
using System.Collections;

public class AnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRMLoader vrmLoader;
    [SerializeField] private DefaultAnimationController defaultAnimationController;

    [SerializeField] private GameObject targetObject;

    [Header("Animator Example")]
    [SerializeField] private AnimatorExample animatorExample; // AnimatorExample への参照

    [Header("IK Animation Settings")]
    [SerializeField, Tooltip("IK ウェイトのフェードイン持続時間（秒）")]
    private float ikFadeInDuration = 1f; // フェードインの持続時間

    [SerializeField, Tooltip("IK ウェイトの保持持続時間（秒）")]
    private float ikHoldDuration = 1f; // 保持の持続時間

    [SerializeField, Tooltip("IK ウェイトのフェードアウト持続時間（秒）")]
    private float ikFadeOutDuration = 1f; // フェードアウトの持続時間

    private Coroutine ikWeightCoroutine;

    // vrmLoaded の状態を追跡
    private bool previousVrmLoaded = false;
    private bool _vrmload = false;

    /// <summary>
    /// Unity の Update メソッドで vrmLoaded の変化を監視します。
    /// </summary>
    private void Update()
    {
        if (vrmLoader == null)
        {
            Debug.LogError("VRMLoader の参照が AnimationController に設定されていません。");
            return;
        }

        // vrmLoaded が false から true に変わったかをチェック
        if (!previousVrmLoaded && vrmLoader.vrmLoaded)
        {
            // VRM がロードされた
            AssignAnimatorExample();
            _vrmload = true;
        }

        // 前回の状態を更新
        previousVrmLoaded = vrmLoader.vrmLoaded;
    }

    /// <summary>
    /// ロードされた VRM モデルから AnimatorExample を取得して参照を設定します。
    /// </summary>
    private void AssignAnimatorExample()
    {
        // "VRM1" という名前のオブジェクトを探す
        GameObject vrm1Object = GameObject.Find("VRM1");
        if (vrm1Object != null)
        {
            AnimatorExample newAnimatorExample = vrm1Object.GetComponent<AnimatorExample>();
            if (newAnimatorExample != null)
            {
                animatorExample = newAnimatorExample;
                Debug.Log("AnimatorExample が正常に設定されました。");

                // 既存の IK ウェイトアニメーションのコルーチンを停止
                if (ikWeightCoroutine != null)
                {
                    StopCoroutine(ikWeightCoroutine);
                    ikWeightCoroutine = null;
                }
            }
            else
            {
                Debug.LogError("VRM1 オブジェクトに AnimatorExample コンポーネントが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("名前が 'VRM1' のオブジェクトがシーン内に存在しません。");
        }
    }

    /// <summary>
    /// オブジェクトを指定された比率に基づいて移動し、_ikWeight をアニメーションさせる関数。
    /// </summary>
    /// <param name="ratioString">"x,y" 形式の文字列 (例: "0.45,0.31")</param>
    public void MoveLookat(string ratioString)
    {
        // VRMLoaderのcanTriggerTouchAnimationがtrueかチェック
        if (vrmLoader == null)
        {
            Debug.LogError("VRMLoader の参照が AnimationController に設定されていません。");
            return;
        }

        if (!_vrmload)
        {
            if (!defaultAnimationController.canmove)
            {
                Debug.LogWarning("MoveObject を呼び出す条件が満たされていません。canTriggerTouchAnimation が false です。");
                return;
            }
        }
        else
        {
            if (!vrmLoader.canmove)
            {
                Debug.LogWarning("MoveObject を呼び出す条件が満たされていません。canTriggerTouchAnimation が false です。");
                return;
            }
        }

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

            // 既存のコルーチンがあれば停止
            if (ikWeightCoroutine != null)
            {
                StopCoroutine(ikWeightCoroutine);
            }
            // IK ウェイトアニメーションのコルーチンを開始
            ikWeightCoroutine = StartCoroutine(AnimateIKWeight(ikFadeInDuration, ikHoldDuration, ikFadeOutDuration));
        }
        else
        {
            Debug.LogError("Failed to parse ratio values. Please ensure they are valid numbers.");
        }
    }

    /// <summary>
    /// _ikWeight を 0 から 1 へフェードインし、1 を一定時間保持し、1 から 0 へフェードアウトさせるコルーチン。
    /// </summary>
    /// <param name="fadeIn">フェードインの持続時間（秒）</param>
    /// <param name="hold">保持の持続時間（秒）</param>
    /// <param name="fadeOut">フェードアウトの持続時間（秒）</param>
    private IEnumerator AnimateIKWeight(float fadeIn, float hold, float fadeOut)
    {
        if (animatorExample == null)
        {
            Debug.LogError("AnimatorExample reference is not set.");
            yield break;
        }

        // Reflection を使ってプライベートフィールド _ikWeight にアクセス
        FieldInfo ikWeightField = typeof(AnimatorExample).GetField("_ikWeight", BindingFlags.NonPublic | BindingFlags.Instance);

        if (ikWeightField == null)
        {
            Debug.LogError("Failed to find _ikWeight field in AnimatorExample.");
            yield break;
        }

        // フェードイン: 0 → 1
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            float value = Mathf.Lerp(0f, 1f, elapsed / fadeIn);
            ikWeightField.SetValue(animatorExample, value);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ikWeightField.SetValue(animatorExample, 1f);

        // 1 を保持
        yield return new WaitForSeconds(hold);

        // フェードアウト: 1 → 0
        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            float value = Mathf.Lerp(1f, 0f, elapsed / fadeOut);
            ikWeightField.SetValue(animatorExample, value);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ikWeightField.SetValue(animatorExample, 0f);
    }

    /// <summary>
    /// VRMLoader と DefaultAnimationController の ExitScreen アニメーションをトリガーします。
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
    /// VRMLoader と DefaultAnimationController の EnterScreen アニメーションをトリガーします。
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
    /// VRMLoader と DefaultAnimationController の Touch アニメーションをトリガーします。
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

    // 他のメソッドも必要に応じて追加してください。
}
