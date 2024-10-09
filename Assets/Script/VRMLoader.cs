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
    [SerializeField] private int animationRange = 6; // アニメーション範囲（例：1～6）
    [SerializeField] private float randomAnimationInterval = 5.0f; // ランダムアニメーションの間隔（秒）
    [SerializeField] private string touchAnimationStateName = "TouchAnimation"; // タッチアニメーションステート名
    [SerializeField] private string idleStateName = "Idle"; // Idleステート名

    [Header("Touch Animation Settings")]
    [SerializeField] private string touchParamName = "Touch"; // Touchパラメータ名

    [Header("Camera Settings")]
    [SerializeField] private Animator cameraAnimator; // InspectorでカメラのAnimatorを割り当てる
    [SerializeField] private string cameraTriggerParamName = "CameraTrigger"; // カメラアニメーション用のTriggerパラメータ名

    private Animator animator;
    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Vrm10Instance vrm10Instance;

    // Animator Controllerで設定したパラメータのハッシュ
    private int touchParamHash;
    private int randomParamHash;
    private int cameraTriggerParamHash;

    private float randomAnimationTimer = 0f;

    // 前回のアニメーションステートを保持する変数
    private string previousStateName = "";
    private string currentStateName = "";

    void Start()
    {
        // Animatorパラメータのハッシュを取得
        touchParamHash = Animator.StringToHash(touchParamName);
        randomParamHash = Animator.StringToHash("random");
        cameraTriggerParamHash = Animator.StringToHash(cameraTriggerParamName);

        // デフォルトモデルが存在する場合は保持
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
            // 現在のアニメーションステートを取得
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            currentStateName = GetCurrentStateName(currentState);

            // ステートが変更された場合のみ処理を行う
            if (currentStateName != previousStateName)
            {
                Debug.Log($"Animation state changed from {previousStateName} to {currentStateName}");
                HandleStateChange(currentStateName, currentState);
                previousStateName = currentStateName;
            }

            // ランダムアニメーションのタイマー更新
            if (currentStateName == "Idle")
            {
                randomAnimationTimer += Time.deltaTime;
                if (randomAnimationTimer >= randomAnimationInterval)
                {
                    TriggerRandomAnimation();
                    randomAnimationTimer = 0f;
                }
            }
            else
            {
                // Idleステートでない場合、ランダムアニメーションのタイマーをリセット
                randomAnimationTimer = 0f;
            }

            // タッチアニメーションをIdleステートのときだけ有効にする
            HandleTouchInput(currentStateName);
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
                Debug.Log("Touch animation started.");
                break;

            case "Idle":
                // アニメーターが遷移中でないことを確認
                if (!animator.IsInTransition(0))
                {
                    ResetTouchAnimation();
                }
                break;

            default:
                // 他のステートに遷移した場合の処理（必要に応じて追加）
                break;
        }
    }

    /// <summary>
    /// ランダムアニメーションをトリガーするメソッド
    /// </summary>
    private void TriggerRandomAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator is null. Cannot trigger random animation.");
            return;
        }

        int randomAnim = Random.Range(1, animationRange + 1); // 1～animationRangeの範囲
        animator.SetInteger(randomParamHash, randomAnim);
        Debug.Log($"Random animation triggered: {randomAnim}");
    }

    /// <summary>
    /// タッチ入力を処理するメソッド
    /// タッチアニメーションはIdleステートのときだけ有効
    /// </summary>
    /// <param name="currentStateName">現在のステート名</param>
    private void HandleTouchInput(string currentStateName)
    {
        // 現在のステートがIdleでない場合、タッチアニメーションを無効にする
        if (currentStateName != "Idle")
        {
            // もしタッチパラメータがtrueになっている場合、falseにリセット
            if (animator.GetBool(touchParamHash))
            {
                animator.SetBool(touchParamHash, false);
                Debug.Log("Current state is not Idle. Touch parameter reset to false.");
            }
            return;
        }

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
                Debug.Log("Touch input detected within valid range and state is Idle.");
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
                Debug.Log("Mouse click detected within valid range and state is Idle.");
                ForceTransitionToTouchAnimation();
            }
        }
        #endif
    }

    /// <summary>
    /// 現在のアニメーションを強制的にスキップし、Touchアニメーションに遷移させるメソッド
    /// 同時にカメラアニメーションもトリガーします
    /// </summary>
    private void ForceTransitionToTouchAnimation()
    {
        // Touchパラメータをオンにする前に、現在のアニメーションをスキップ
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int currentStateHash = stateInfo.fullPathHash;

        // 現在のアニメーションを最終フレームにジャンプさせる
        animator.Play(currentStateHash, 0, 1.0f); // レイヤー0、再生時間1（終わり）
        Debug.Log("Current animation forced to its final frame.");

        // Touchアニメーションを再生
        animator.SetBool(touchParamHash, true); // Touch パラメータをオンにする
        Debug.Log("Touch animation triggered.");

        // カメラアニメーションをトリガー
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger(cameraTriggerParamHash);
            Debug.Log("Camera animation triggered.");
        }
        else
        {
            Debug.LogWarning("Camera Animator is not assigned. Cannot trigger camera animation.");
        }
    }

    /// <summary>
    /// Touchアニメーションをリセットするメソッド
    /// </summary>
    private void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(touchParamHash, false);
            Debug.Log("Touch parameter reset to false.");
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
            Debug.Log("Existing VRM instance destroyed.");
        }

        // デフォルトモデルの削除
        if (defo != null)
        {
            Destroy(defo);
            Debug.Log("Default model destroyed.");
        }

        // ロード画面を表示
        if (kurukuru != null)
        {
            kurukuru.SetActive(true);
            Debug.Log("Loading screen activated.");
        }

        // スライダーリセット
        if (slider != null)
        {
            slider.value = 1;
            Debug.Log("Slider reset to 1.");
        }

        try
        {
            // パスからVRMを非同期で読み込み
            vrm10Instance = await Vrm10.LoadPathAsync(path);
            Debug.Log($"VRM loaded from path: {path}");

            // 読み込んだVRMインスタンスの位置を設定
            vrm10Instance.gameObject.transform.position = Vector3.zero;
            Debug.Log("VRM instance position set to Vector3.zero.");

            // 表情操作
            vrm10RuntimeExpression = vrm10Instance.Runtime.Expression;

            // Animatorコンポーネントにアクセス
            animator = vrm10Instance.GetComponent<Animator>();

            if (animator == null)
            {
                Debug.LogError("AnimatorコンポーネントがVRMにアタッチされていません。");
            }
            else
            {
                // Animator Controllerを設定
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("motion");
                Debug.Log("Animator Controller 'motion' loaded and assigned.");
            }

            // Facialをアタッチ
            vrm10Instance.gameObject.AddComponent<Facial>();
            Debug.Log("Facial component added to VRM instance.");
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
            Debug.Log("Loading screen deactivated.");
        }

        vrmLoaded = true; // VRMがロードされたことを示すフラグを設定
        Debug.Log("VRM loaded flag set to true.");
    }

    /// <summary>
    /// スケールをスライダーで調整するメソッド
    /// </summary>
    public void ScaleAdjust()
    {   
        if (!vrmLoaded || vrm10Instance == null)
        {
            Debug.LogWarning("VRM not loaded or vrm10Instance is null. Cannot adjust scale.");
            return;
        }

        float scale = slider.value;
        vrm10Instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);
        Debug.Log($"VRM scale adjusted to: {scale}");
    }

    /// <summary>
    /// テスト用のメソッド
    /// </summary>
    public void testcase()
    {
        ReceiveVRMFilePath("C:/Users/ok122/Downloads/ba_hoshino_s (1)/ba_hoshino_s.vrm");
    }

    // VRMがロードされたかどうかを示すフラグ
    private bool vrmLoaded = false; // VRMがロードされたかどうか
}
