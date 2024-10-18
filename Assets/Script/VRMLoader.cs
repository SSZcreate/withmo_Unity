using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniVRM10;
using TMPro;
using DG.Tweening;

public class VRMLoader : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject kurukuru; // ロード画面のUI
    public GameObject defo;      // デフォルトモデル
    public Slider slider;        // スケール調整用スライダー

    [Header("Animation Settings")]
    [SerializeField] private int animationRange = 6;
    [SerializeField] private float randomAnimationInterval = 5.0f;
    [SerializeField] private string touchAnimationStateName = "TouchAnimation";
    [SerializeField] private string idleStateName = "Idle";

    [Header("Touch Animation Settings")]
    [SerializeField] private string touchTriggerParamName = "TouchTrigger"; // トリガー名に変更

    [Header("Camera Settings")]
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private string cameraTriggerParamName = "CameraTrigger";

    private Animator animator;
    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Vrm10Instance vrm10Instance;

    // Animator パラメータのハッシュ
    private static readonly int TouchTriggerParamHash = Animator.StringToHash("TouchTrigger");
    private static readonly int RandomParamHash = Animator.StringToHash("random");
    private int cameraTriggerParamHash;

    private float randomAnimationTimer = 0f;
    private string previousStateName = "";
    private string currentStateName = "";

    private bool isLoading = false;
    private bool vrmLoaded = false;
    private bool canTriggerTouchAnimation = false; // 一度だけトリガーできるフラグ

    [SerializeField] private float touchCooldown = 2.0f; // タッチ後のクールダウン時間（秒）
    private bool isCooldown = false; // クールダウン中かどうかのフラグ

    void Start()
    {
        // Animator パラメータのハッシュを初期化
        cameraTriggerParamHash = Animator.StringToHash(cameraTriggerParamName);

        // デフォルトモデルをアクティブにする
        if (defo != null)
        {
            defo.SetActive(true);
        }

        Debug.Log("VRMLoader started.");
    }

    void Update()
    {
        if (animator != null)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            currentStateName = GetCurrentStateName(currentState);

            // ステートが変更された場合の処理
            if (currentStateName != previousStateName)
            {
                Debug.Log($"アニメーションステートが {previousStateName} から {currentStateName} に変更されました。");
                HandleStateChange(currentStateName);
                previousStateName = currentStateName;
            }

            // Idle ステートでランダムアニメーションを実行
            if (currentStateName == "Idle")
            {
                randomAnimationTimer += Time.deltaTime;
                if (randomAnimationTimer >= randomAnimationInterval)
                {
                    TriggerRandomAnimation();
                    randomAnimationTimer = 0f;
                }

                // Idle に戻ったらタッチアニメーションが再度実行可能にする
                canTriggerTouchAnimation = true;
            }
            else
            {
                randomAnimationTimer = 0f;
            }
        }
    }

    /// <summary>
    /// 現在のアニメーションステート名を取得します。
    /// </summary>
    /// <param name="stateInfo">現在のステート情報。</param>
    /// <returns>ステート名の文字列。</returns>
    private string GetCurrentStateName(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName(touchAnimationStateName))
            return "TouchAnimation";
        else if (stateInfo.IsName(idleStateName))
            return "Idle";
        else
            return "Other";
    }

    /// <summary>
    /// ステートが変更された際の処理を行います。
    /// </summary>
    /// <param name="currentStateName">新しいステート名。</param>
    private void HandleStateChange(string currentStateName)
    {
        switch (currentStateName)
        {
            case "TouchAnimation":
                Debug.Log("タッチアニメーションが開始されました。");
                break;

            case "Idle":
                if (!animator.IsInTransition(0))
                    ResetTouchAnimation();
                break;
        }
    }

    /// <summary>
    /// ランダムアニメーションをトリガーします。
    /// </summary>
    private void TriggerRandomAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator が null のため、ランダムアニメーションをトリガーできません。");
            return;
        }

        int randomAnim = Random.Range(1, animationRange + 1);
        animator.SetInteger(RandomParamHash, randomAnim);
        Debug.Log($"ランダムアニメーション {randomAnim} をトリガーしました。");
    }

    /// <summary>
    /// タッチアニメーションをトリガーします。
    /// 現在のアニメーションステートが Idle の時のみ実行可能です。
    /// </summary>
    public void TriggerTouchAnimation()
    {
        // クールダウン中またはトリガー不可の場合は処理を中断
        if (!canTriggerTouchAnimation || isCooldown)
        {
            Debug.LogWarning("現在タッチアニメーションをトリガーできません。");
            return;
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator が null のため、タッチアニメーションをトリガーできません。");
            return;
        }

        // 現在のステートが Idle であることを確認
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(idleStateName))
        {
            Debug.LogWarning("タッチアニメーションは Idle ステートの時のみトリガーできます。");
            return;
        }

        // Idle ステートの時だけアニメーションを最終フレームにスキップしてタッチアニメーションを実行
        animator.Play(stateInfo.fullPathHash, 0, 1.0f); // 最終フレームへスキップ
        Debug.Log("Idle アニメーションを最終フレームにスキップしました。");

        // カメラアニメーションをトリガー
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger(cameraTriggerParamHash);
            Debug.Log("カメラアニメーションをトリガーしました。");
        }
        else
        {
            Debug.LogWarning("Camera Animator が割り当てられていません。");
        }

        // Trigger を使用してタッチアニメーションを開始
        animator.SetTrigger(TouchTriggerParamHash);
        Debug.Log("タッチアニメーションをトリガーしました。");

        // タッチアニメーションを再度トリガーできないようにフラグを設定
        canTriggerTouchAnimation = false;

        // クールダウンを開始
        StartCoroutine(StartCooldown());
    }

    /// <summary>
    /// タッチアニメーションをリセットします。
    /// </summary>
    private void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(TouchTriggerParamHash); // トリガーをリセット
            Debug.Log("タッチアニメーションをリセットしました。");
        }
    }

    /// <summary>
    /// クールダウン期間を開始します。
    /// </summary>
    /// <returns>IEnumerator コルーチン。</returns>
    private IEnumerator StartCooldown()
    {
        isCooldown = true; // クールダウン中に設定
        yield return new WaitForSeconds(touchCooldown); // クールダウン時間待機
        isCooldown = false; // クールダウン解除
        Debug.Log("クールダウンが終了しました。タッチアニメーションを再度トリガーできます。");
    }

    /// <summary>
    /// VRMファイルのパスを受け取り、モデルをロードします。
    /// </summary>
    /// <param name="path">VRMファイルのパス。</param>
    public async void ReceiveVRMFilePath(string path)
    {
        if (isLoading)
        {
            Debug.LogWarning("現在VRMをロード中です。しばらくお待ちください。");
            return;
        }

        isLoading = true;

        // 既存のVRMインスタンスを破棄
        if (vrm10Instance != null)
        {
            Destroy(vrm10Instance.gameObject);
            vrm10Instance = null;
            vrmLoaded = false;
            Debug.Log("既存のVRMインスタンスを破棄しました。");
        }

        // デフォルトモデルを破棄
        if (defo != null)
        {
            Destroy(defo);
            Debug.Log("デフォルトモデルを破棄しました。");
        }

        // ロード画面を表示
        if (kurukuru != null)
        {
            kurukuru.SetActive(true);
            Debug.Log("ロード画面を表示しました。");
        }

        // スライダーをリセット
        if (slider != null)
        {
            slider.value = 1;
            Debug.Log("スライダーをリセットしました。");
        }

        try
        {
            vrm10Instance = await Vrm10.LoadPathAsync(path);
            Debug.Log($"パス {path} からVRMをロードしました。");

            // ロードしたモデルの位置をリセット
            vrm10Instance.gameObject.transform.position = Vector3.zero;

            vrm10RuntimeExpression = vrm10Instance.Runtime.Expression;
            animator = vrm10Instance.GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError("VRMインスタンスにAnimatorコンポーネントが見つかりません。");
            }
            else
            {
                // Animator Controller を割り当て
                var controller = Resources.Load<RuntimeAnimatorController>("motion");
                if (controller != null)
                {
                    animator.runtimeAnimatorController = controller;
                    Debug.Log("RuntimeAnimatorControllerをAnimatorに割り当てました。");
                }
                else
                {
                    Debug.LogError("Resources/motion からRuntimeAnimatorControllerのロードに失敗しました。");
                }
            }

            // Facialコンポーネントを追加
            vrm10Instance.gameObject.AddComponent<Facial>();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"VRMのロード中に例外が発生しました: {e}");
            // 現在のシーンを再読み込みして状態をリセット
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        // ロード画面を非表示にする
        if (kurukuru != null)
        {
            kurukuru.SetActive(false);
            Debug.Log("ロード画面を非表示にしました。");
        }

        vrmLoaded = true;
        isLoading = false;
        Debug.Log("VRMのロードが完了しました。");
    }

    /// <summary>
    /// ロードしたVRMモデルのスケールを調整します。
    /// </summary>
    public void ScaleAdjust()
    {
        if (!vrmLoaded || vrm10Instance == null)
        {
            Debug.LogWarning("VRMがロードされていません。スケールを調整できません。");
            return;
        }

        float scale = slider.value;
        vrm10Instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);
        Debug.Log($"VRMのスケールを {scale} に調整しました。");
    }

    /// <summary>
    /// テストケースとして特定のVRMファイルをロードします。
    /// </summary>
    public void TestCase()
    {
        ReceiveVRMFilePath("C:/Users/ok122/Downloads/Subaru_FanmadeModel_MMD+VRM/VRM/Subaru_Hiyoko_inner.vrm");
    }

    /// <summary>
    /// UIボタンからタッチアニメーションを手動でトリガーします。
    /// </summary>
    public void OnTouchButtonPressed()
    {
        TriggerTouchAnimation();
    }
}
