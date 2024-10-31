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

    [Header("MoveLookat Settings")]
    [SerializeField, Tooltip("移動にかける時間（秒）")]
    private float moveDuration = 1f;

    [SerializeField, Tooltip("移動後にその位置にとどまる時間（秒）")]
    private float holdDuration = 1f;

    [SerializeField, Tooltip("戻る際の参照オブジェクト")]
    private GameObject referenceObject;

    [Header("IK Weight Settings")]
    [SerializeField, Tooltip("IKウェイトのフェードにかける時間（秒）")]
    private float ikFadeDuration = 0.5f;

    // リフレクション用のFieldInfoをキャッシュ
    private FieldInfo ikWeightFieldInfo;

    // IKウェイトのコルーチンを管理するための参照
    private Coroutine ikWeightCoroutine;

    // 移動中かどうかを判定するフラグ
    private bool isMoving = false;

    // VRMLoader のロード状態を追跡
    private bool previousVrmLoaded = false;
    private bool _vrmload = false;

    /// <summary>
    /// Unity の Awake メソッドでリフレクションを初期化します。
    /// </summary>
    private void Awake()
    {
        if (animatorExample != null)
        {
            ikWeightFieldInfo = typeof(AnimatorExample).GetField("_ikWeight", BindingFlags.NonPublic | BindingFlags.Instance);
            if (ikWeightFieldInfo == null)
            {
                Debug.LogError("AnimatorExample クラス内に '_ikWeight' フィールドが見つかりません。");
            }
        }
        else
        {
            Debug.LogError("AnimatorExample がアサインされていません。");
        }
    }

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

                // リフレクションを再初期化
                ikWeightFieldInfo = typeof(AnimatorExample).GetField("_ikWeight", BindingFlags.NonPublic | BindingFlags.Instance);
                if (ikWeightFieldInfo == null)
                {
                    Debug.LogError("AnimatorExample クラス内に '_ikWeight' フィールドが見つかりません。");
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
    /// オブジェクトを指定された比率に基づいて移動し、指定された秒数で移動し、その位置に指定された時間とどまり、指定オブジェクトの同じx,yポジションに戻ります。
    /// </summary>
    /// <param name="ratioString">"x,y" 形式の文字列 (例: "0.45,0.31")</param>
    public void MoveLookat(string ratioString)
    {
        // 既に移動中の場合は無視
        if (isMoving)
        {
            Debug.LogWarning("現在移動中のため、MoveLookat 呼び出しは無視されました。");
            return;
        }

        // VRMLoaderのcanmoveがtrueかチェック
        if (vrmLoader == null)
        {
            Debug.LogError("VRMLoader の参照が AnimationController に設定されていません。");
            return;
        }

        if (!_vrmload)
        {
            if (!defaultAnimationController.canmove)
            {
                Debug.LogWarning("MoveLookat を呼び出す条件が満たされていません。canmove が false です。");
                SmoothSetIKWeight(0f, ikFadeDuration);
                return;
            }
        }
        else
        {
            if (!vrmLoader.canmove)
            {
                Debug.LogWarning("MoveLookat を呼び出す条件が満たされていません。canmove が false です。");
                SmoothSetIKWeight(0f, ikFadeDuration);
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
            // ビューポート座標を計算（左下が (0,0)、右上が (1,1)）
            Vector3 viewportPosition = new Vector3(xRatio, yRatio, Camera.main.WorldToViewportPoint(targetObject.transform.position).z);

            // ビューポート座標をワールド座標に変換
            Vector3 targetPosition = Camera.main.ViewportToWorldPoint(viewportPosition);

            // 現在の位置を保存
            Vector3 originalPosition = targetObject.transform.position;

            // 参照オブジェクトが指定されていない場合はエラーログを出す
            if (referenceObject == null)
            {
                Debug.LogError("ReferenceObject が指定されていません。");
                return;
            }

            // IKウェイトを1に滑らかに変更
            SmoothSetIKWeight(1f, ikFadeDuration);

            // 移動中フラグを立てる
            isMoving = true;

            // コルーチンを開始
            StartCoroutine(MoveAndHold(targetObject, originalPosition, targetPosition, moveDuration, holdDuration, referenceObject));
        }
        else
        {
            Debug.LogError("Failed to parse ratio values. Please ensure they are valid numbers.");
        }
    }

    /// <summary>
    /// IKウェイトを滑らかに変更するメソッド
    /// </summary>
    /// <param name="targetWeight">目標とするIKウェイトの値（0～1）</param>
    /// <param name="duration">変更にかける時間（秒）</param>
    private void SmoothSetIKWeight(float targetWeight, float duration)
    {
        if (animatorExample == null)
        {
            Debug.LogError("AnimatorExample reference is not set.");
            return;
        }

        if (ikWeightFieldInfo == null)
        {
            Debug.LogError("IKWeightFieldInfo is not initialized.");
            return;
        }

        // 既存のコルーチンがあれば停止
        if (ikWeightCoroutine != null)
        {
            StopCoroutine(ikWeightCoroutine);
        }

        // コルーチンを開始
        ikWeightCoroutine = StartCoroutine(FadeIKWeight(targetWeight, duration));
    }

    /// <summary>
    /// IKウェイトを滑らかにフェードさせるコルーチン
    /// </summary>
    /// <param name="targetWeight">目標とするIKウェイトの値（0～1）</param>
    /// <param name="duration">フェードにかける時間（秒）</param>
    /// <returns></returns>
    private IEnumerator FadeIKWeight(float targetWeight, float duration)
    {
        float elapsedTime = 0f;

        // 現在のIKウェイトの値を取得
        float currentWeight = (float)ikWeightFieldInfo.GetValue(animatorExample);

        while (elapsedTime < duration)
        {
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, elapsedTime / duration);
            ikWeightFieldInfo.SetValue(animatorExample, newWeight);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最終的な値を設定
        ikWeightFieldInfo.SetValue(animatorExample, targetWeight);
        ikWeightCoroutine = null;
    }

    /// <summary>
    /// ターゲットオブジェクトを指定された位置に移動し、指定時間保持後に参照オブジェクトのx,yポジションに戻ります。
    /// </summary>
    /// <param name="obj">移動対象のオブジェクト</param>
    /// <param name="originalPosition">元の位置</param>
    /// <param name="targetPosition">移動先の位置</param>
    /// <param name="moveDuration">移動にかける時間（秒）</param>
    /// <param name="holdDuration">その位置にとどまる時間（秒）</param>
    /// <param name="referenceObject">戻る際の参照オブジェクト</param>
    /// <returns></returns>
    private IEnumerator MoveAndHold(GameObject obj, Vector3 originalPosition, Vector3 targetPosition, float moveDuration, float holdDuration, GameObject referenceObject)
    {
        float elapsedTime = 0f;

        // 移動処理: 現在の位置からターゲット位置へ
        while (elapsedTime < moveDuration)
        {
            obj.transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = targetPosition;

        // 指定された時間だけその位置にとどまる
        yield return new WaitForSeconds(holdDuration);

        // 戻り位置を計算: 参照オブジェクトのx,y座標、元のz座標
        Vector3 referencePosition = referenceObject.transform.position;
        Vector3 returnPosition = new Vector3(referencePosition.x, referencePosition.y, originalPosition.z);

        // 戻り処理: ターゲット位置から戻り位置へ
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            obj.transform.position = Vector3.Lerp(targetPosition, returnPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = returnPosition;

        // 移動完了フラグをリセット
        isMoving = false;
    }

    /// <summary>
    /// VRMLoader と DefaultAnimationController の ExitScreen アニメーションをトリガーします。
    /// </summary>
    public void TriggerExitScreenAnimation()
    {
        if (vrmLoader != null)
        {
            vrmLoader.StartExitProcess();
        }
        else
        {
            Debug.LogWarning("VRMLoaderが割り当てられていません。");
        }

        if (defaultAnimationController != null)
        {
            defaultAnimationController.StartExitProcess();
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
            vrmLoader.StartEnterProcess();
        }
        else
        {
            Debug.LogWarning("VRMLoaderが割り当てられていません。");
        }

        if (defaultAnimationController != null)
        {
            defaultAnimationController.StartEnterProcess();
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
