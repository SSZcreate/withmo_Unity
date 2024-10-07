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
    [SerializeField] private int animationRange = 6; // アニメーション範囲（例：1～6）
    [SerializeField] private float randomAnimationInterval = 5.0f; // ランダムアニメーションの間隔（秒）
    [SerializeField] private string touchAnimationStateName = "TouchAnimation"; // タッチアニメーションステート名
    [SerializeField] private string idleStateName = "Idle"; // Idleステート名

    [Header("Touch Animation Settings")]
    [SerializeField] private string touchParamName = "Touch"; // Touchパラメータ名
    [SerializeField] private float touchAnimationDuration = 2.0f; // タッチアニメーションの継続時間（秒）

    private Animator animator;
    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Vrm10Instance vrm10Instance;

    // フラグとタイマー
    private bool vrmLoaded = false; // VRMがロードされたかどうか
    private bool isTouchAnimationPlaying = false; // タッチアニメーションが再生中か
    private float randomAnimationTimer = 0f; // ランダムアニメーションのタイマー

    // ステート管理
    private string previousStateName = "";

    // Animatorパラメータのハッシュ
    private int touchParamHash;

    void Start()
    {
        // Animatorパラメータのハッシュを取得
        touchParamHash = Animator.StringToHash(touchParamName);

        // デフォルトモデルが存在する場合は保持
        if (defo != null)
        {
            defo.SetActive(true);
        }
    }

    void Update()
    {
        if (animator != null)
        {
            // 現在のアニメーションステートを取得
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            string currentStateName = GetCurrentStateName(currentState);

            // ステートが変更された場合のみ処理を行う
            if (currentStateName != previousStateName)
            {
                Debug.Log($"Animation state changed from {previousStateName} to {currentStateName}");

                HandleStateChange(currentStateName, currentState);
                previousStateName = currentStateName;
            }

            // ランダムアニメーションのタイマー更新
            if (!isTouchAnimationPlaying)
            {
                randomAnimationTimer += Time.deltaTime;
                if (randomAnimationTimer >= randomAnimationInterval)
                {
                    TriggerRandomAnimation();
                    randomAnimationTimer = 0f;
                }
            }

            // タッチアニメーションが再生中でない場合のみタッチ入力を検出
            if (!isTouchAnimationPlaying)
            {
                HandleTouchInput();
            }
        }
    }

    /// <summary>
    /// 現在のアニメーションステート名を取得するメソッド
    /// </summary>
    /// <param name="stateInfo">AnimatorStateInfo</param>
    /// <returns>ステート名</returns>
    private string GetCurrentStateName(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName(touchAnimationStateName))
        {
            return "TouchAnimation";
        }
        else if (stateInfo.IsName(idleStateName))
        {
            return "Idle";
        }
        else
        {
            return "Other";
        }
    }

    /// <summary>
    /// ステート変更時の処理を行うメソッド
    /// </summary>
    /// <param name="currentStateName">現在のステート名</param>
    /// <param name="currentState">AnimatorStateInfo</param>
    private void HandleStateChange(string currentStateName, AnimatorStateInfo currentState)
    {
        switch (currentStateName)
        {
            case "TouchAnimation":
                isTouchAnimationPlaying = true;
                break;

            case "Idle":
                // アニメーターが遷移中でないことを確認
                if (!animator.IsInTransition(0))
                {
                    ResetTouchAnimation();
                }
                break;

            default:
                isTouchAnimationPlaying = false;
                break;
        }
    }

    /// <summary>
    /// ランダムアニメーションをトリガーするメソッド
    /// </summary>
    private void TriggerRandomAnimation()
    {
        int randomAnim = Random.Range(1, animationRange + 1); // 1～animationRangeの範囲
        animator.SetInteger("random", randomAnim);
        Debug.Log($"Random animation triggered: {randomAnim}");
    }

    /// <summary>
    /// タッチ入力を処理するメソッド
    /// </summary>
    private void HandleTouchInput()
    {
        // モバイルプラットフォーム向けのタッチ入力
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // タッチ範囲の条件（上半分、左80%）
            bool isWithinValidRange = touchPosition.x <= screenWidth * 0.8f && touchPosition.y >= screenHeight * 0.5f;

            // 有効範囲内でタッチが開始された場合のみアニメーションをスキップ
            if (isWithinValidRange && touch.phase == TouchPhase.Began)
            {
                ForceTransitionToTouchAnimation();
            }
        }

        // PCプラットフォーム向けのマウス入力（オプション）
        #if !UNITY_ANDROID && !UNITY_IOS
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            bool isWithinValidRange = mousePosition.x <= screenWidth * 0.8f && mousePosition.y >= screenHeight * 0.5f;

            if (isWithinValidRange)
            {
                ForceTransitionToTouchAnimation();
            }
        }
        #endif
    }

    /// <summary>
    /// 現在のアニメーションを強制的にスキップし、Touchアニメーションに遷移させるメソッド
    /// </summary>
    private void ForceTransitionToTouchAnimation()
    {
        if (isTouchAnimationPlaying)
        {
            Debug.LogWarning("Touch animation is already playing. Ignoring new touch input.");
            return; // 既にタッチアニメーションが再生中なら処理しない
        }

        Debug.Log("Transitioning to TouchAnimation.");

        // 現在のアニメーションステート情報を取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int currentStateHash = stateInfo.fullPathHash;

        // 現在のアニメーションを強制的に終了させ、再生時間を1に設定（最終フレームにジャンプ）
        animator.Play(currentStateHash, 0, 1.0f); // レイヤー0、再生時間1（終わり）

        // Touchアニメーションを再生
        animator.SetBool(touchParamHash, true); // Touch パラメータをオンにする

        Debug.Log("Touch animation started.");
    }

    /// <summary>
    /// Touchアニメーションをリセットするメソッド
    /// </summary>
    private void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(touchParamHash, false);
            isTouchAnimationPlaying = false;
            Debug.Log("Idle state detected. Touch parameter reset and flag cleared.");
        }
    }

    /// <summary>
    /// VRMファイルのパスを受信してロードするメソッド
    /// </summary>
    /// <param name="path">VRMファイルのパス</param>
    public async void ReceiveVRMFilePath(string path)
    {   
        // VRMが既にロードされていれば削除
        if (vrmLoaded)
        {
            Destroy(vrm10Instance.gameObject);
            vrmLoaded = false;
        }

        // デフォルトモデルの削除
        if (defo != null)
        {
            Destroy(defo);
        }

        // ロード画面を表示
        if (kurukuru != null)
        {
            kurukuru.SetActive(true);
        }

        // スライダーリセット
        if (slider != null)
        {
            slider.value = 1;
        }

        try
        {
            // パスからVRMを非同期で読み込み
            vrm10Instance = await Vrm10.LoadPathAsync(path);

            // 読み込んだVRMインスタンスの位置を設定
            vrm10Instance.gameObject.transform.position = Vector3.zero;

            // 表情操作
            vrm10RuntimeExpression = vrm10Instance.Runtime.Expression;

            // Animatorコンポーネントにアクセス
            animator = vrm10Instance.GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError("AnimatorコンポーネントがVRMにアタッチされていません。");
            }

            // Facialをアタッチ
            vrm10Instance.gameObject.AddComponent<Facial>();
        }
        catch(System.Exception e)
        {
            // 例外処理
            Debug.LogWarning($"Meshをアタッチしてください。 : {e}");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // ロード画面を非表示
        if (kurukuru != null)
        {
            kurukuru.SetActive(false);
        }

        // Animator Controllerを設定
        if (animator != null)
        {
            // アニメーションコントローラのファイル名のみ指定
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("motion");
        }

        vrmLoaded = true; // VRMがロードされたことを示すフラグを設定
    }

    /// <summary>
    /// スケールをスライダーで調整するメソッド
    /// </summary>
    public void ScaleAdjust()
    {   
        if (!vrmLoaded || vrm10Instance == null)
            return;

        float scale = slider.value;
        vrm10Instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// テスト用のメソッド
    /// </summary>
    public void testcase()
    {
        ReceiveVRMFilePath("C:/Users/ok122/Downloads/T_MSF_v0.93/T式マイティーストライクフリーダム_v0.93.vrm");
    }
}
