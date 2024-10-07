using UnityEngine;
using UniVRM10;

public class DefaultAnimationController : MonoBehaviour
{
    // VRMの表情操作用ランタイムインスタンス
    private Vrm10RuntimeExpression vrm10RuntimeExpression;

    // Animatorコンポーネントへの参照
    private Animator animator;

    [SerializeField] private Vrm10Instance vrm; // Inspectorで割り当てるVRMインスタンス
    [SerializeField] private int animationRange = 6; // ランダムアニメーションの範囲（例：1～6）
    [SerializeField] private float randomAnimationInterval = 5.0f; // ランダムアニメーション間のインターバル

    // タッチアニメーションが再生中かを示すフラグ
    private bool isTouchAnimationPlaying = false;

    // Animator Controllerで設定したパラメータのハッシュ
    private static readonly int RandomParamHash = Animator.StringToHash("random");
    private static readonly int TouchParamHash = Animator.StringToHash("Touch");

    private float randomAnimationTimer = 0f;

    // 前回のアニメーションステートを保持する変数
    private string previousStateName = "";

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
        }
        else
        {
            Debug.LogError("VRMインスタンスが割り当てられていません。");
        }
    }

    void Update()
    {
        if (animator != null)
        {
            // 現在のアニメーターのステートを取得
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            string currentStateName = currentState.IsName("TouchAnimation") ? "TouchAnimation" :
                                      currentState.IsName("Idle") ? "Idle" :
                                      currentState.shortNameHash != 0 ? currentState.ToString() : "Other";

            // ステートが変更された場合のみ処理を行う
            if (currentStateName != previousStateName)
            {
                Debug.Log($"Animation state changed from {previousStateName} to {currentStateName}");

                // ステート変更時の処理
                if (currentStateName == "TouchAnimation")
                {
                    isTouchAnimationPlaying = true;
                }
                else if (currentStateName == "Idle")
                {
                    // Idleステートに遷移した場合のみリセット
                    // アニメーターが遷移中でないことを確認
                    if (!animator.IsInTransition(0))
                    {
                        animator.SetBool(TouchParamHash, false);
                        isTouchAnimationPlaying = false;
                        Debug.Log("Idle state detected. Touch parameter reset and flag cleared.");
                    }
                }
                else
                {
                    isTouchAnimationPlaying = false;
                }

                // 前回のステート名を更新
                previousStateName = currentStateName;
            }

            // ランダムアニメーションのタイマー更新
            if (!isTouchAnimationPlaying)
            {
                randomAnimationTimer += Time.deltaTime;
                if (randomAnimationTimer >= randomAnimationInterval)
                {
                    int randomAnim = Random.Range(1, animationRange + 1); // 1～animationRangeの範囲
                    animator.SetInteger(RandomParamHash, randomAnim);
                    randomAnimationTimer = 0f;
                }
            }

            // タッチアニメーションが再生中でない場合のみタッチ入力を検出
            if (!isTouchAnimationPlaying)
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

            // その他の処理が必要な場合はここに追加
        }
    }

    /// <summary>
    /// 現在のアニメーションを強制的にスキップし、Touchアニメーションに遷移させるメソッド
    /// </summary>
    private void ForceTransitionToTouchAnimation()
    {
        // タッチアニメーションが再生中かどうかを確認
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
        animator.SetBool(TouchParamHash, true); // Touch パラメータをオンにする

        Debug.Log("Touch animation started.");
    }

    /// <summary>
    /// タッチアニメーションをリセットするメソッド
    /// </summary>
    private void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(TouchParamHash, false);
            isTouchAnimationPlaying = false;
            Debug.Log("TouchAnimation ended and Touch parameter reset.");
        }
    }

    /// <summary>
    /// タッチアニメーション終了後に呼ばれる関数（AnimatorStateInfoを使用しない場合は未使用）
    /// </summary>
    public void OnTouchAnimationEnd()
    {
        // このメソッドはコルーチンのみを使用する場合は未使用です。
        // AnimatorStateInfoを使用してアニメーション終了を検知する場合に実装します。

        // Touch パラメータをオフにし、通常のアニメーション再生に戻す
        animator.SetBool(TouchParamHash, false);
        isTouchAnimationPlaying = false;

        Debug.Log("TouchAnimation ended via AnimatorStateInfo and Touch parameter reset.");
    }
}
