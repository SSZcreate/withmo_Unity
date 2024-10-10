using UnityEngine;
using UniVRM10;

public class DefaultAnimationController : MonoBehaviour
{
    [Header("VRM Settings")]
    [SerializeField] private Vrm10Instance vrm; // Inspectorで割り当てるVRMインスタンス

    [Header("Animation Settings")]
    [SerializeField] private int animationRange = 6; // ランダムアニメーションの範囲（例：1～6）
    [SerializeField] private float randomAnimationInterval = 5.0f; // ランダムアニメーション間のインターバル（秒）
    [SerializeField] private string touchAnimationStateName = "TouchAnimation"; // タッチアニメーションステート名
    [SerializeField] private string idleStateName = "Idle"; // Idleステート名

    [Header("Touch Animation Settings")]
    [SerializeField] private string touchParamName = "Touch"; // Touchパラメータ名

    [Header("Camera Settings")]
    [SerializeField] private Animator cameraAnimator; // InspectorでカメラのAnimatorを割り当てる
    [SerializeField] private string cameraTriggerParamName = "CameraTrigger"; // カメラアニメーション用のTriggerパラメータ名

    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Animator animator;

    // Animator Controllerで設定したパラメータのハッシュ
    private static readonly int RandomParamHash = Animator.StringToHash("random");
    private static readonly int TouchParamHash = Animator.StringToHash("Touch");

    private int cameraTriggerParamHash;

    private float randomAnimationTimer = 0f;

    // 前回のアニメーションステートを保持する変数
    private string previousStateName = "";
    private string currentStateName = "";

    float touchDuration = 0f; // Tracks the duration of the touch
float requiredHoldTime = 1f; // Required hold time in seconds (1 second)

    void Start()
    {
        // VRMインスタンスが割り当てられているか確認
        if (vrm != null)
        {
            vrm10RuntimeExpression = vrm.Runtime.Expression;
            animator = vrm.GetComponent<Animator>();

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
        }
        else
        {
            Debug.LogError("VRMインスタンスが割り当てられていません。");
        }

        // カメラAnimatorのパラメータハッシュを取得
        if (cameraAnimator != null)
        {
            cameraTriggerParamHash = Animator.StringToHash(cameraTriggerParamName);
        }
        else
        {
            Debug.LogError("Camera Animatorが割り当てられていません。");
        }
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
        animator.SetInteger(RandomParamHash, randomAnim);
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
            if (animator.GetBool(TouchParamHash))
            {
                animator.SetBool(TouchParamHash, false);
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

        // Condition for touch range (top half, left 80%)
        bool isWithinValidRange = touchPosition.x <= screenWidth * 0.8f && touchPosition.y >= screenHeight * 0.5f;

        if (isWithinValidRange)
        {
            if (touch.phase == TouchPhase.Began)
            {
                // Reset the touch duration timer when the touch begins
                touchDuration = 0f;
            }
            else if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                // Increment the touch duration while the touch is stationary or moving
                touchDuration += Time.deltaTime;

                // Check if the touch duration meets the required hold time
                if (touchDuration >= requiredHoldTime)
                {
                    Debug.Log("Touch input detected within valid range and state is Idle after holding for 1 second.");
                    ForceTransitionToTouchAnimation();
                    touchDuration = 0f; // Reset the touch duration after the transition
                }
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // Reset the touch duration if the touch ends or is canceled
                touchDuration = 0f;
            }
        }
        else
        {
            // Reset the touch duration if the touch is outside the valid range
            touchDuration = 0f;
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
        animator.SetBool(TouchParamHash, true); // Touch パラメータをオンにする
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
    /// タッチアニメーションをリセットするメソッド
    /// </summary>
    private void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(TouchParamHash, false);
            Debug.Log("Touch parameter reset to false.");
        }
    }
}
