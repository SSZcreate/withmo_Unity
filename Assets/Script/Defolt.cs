using UnityEngine;
using UniVRM10;
using System.Collections;

public class DefaultAnimationController : MonoBehaviour
{
    [Header("VRM Settings")]
    [SerializeField] private Vrm10Instance vrm;

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
    private static readonly int TouchTriggerParamHash = Animator.StringToHash("TouchTrigger"); // トリガー用ハッシュ
    private int cameraTriggerParamHash;

    private float randomAnimationTimer = 0f;
    private bool canTriggerTouchAnimation = false; // タッチアニメーションを一度だけ許可するフラグ

    [SerializeField] private float cooldownTime = 2.0f; // タッチ後のクールダウン時間（秒）
    private bool isCooldown = false; // クールダウン中かどうかのフラグ

    void Start()
    {
        if (vrm != null)
        {
            animator = vrm.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("AnimatorがVRMにアタッチされていません。");
                return;
            }

            var controller = Resources.Load<RuntimeAnimatorController>("motion");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogError("Resources/motion から RuntimeAnimatorController のロードに失敗しました。");
            }
        }
        else
        {
            Debug.LogError("VRMインスタンスが割り当てられていません。");
        }

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
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentState.IsName(idleStateName))
            {
                randomAnimationTimer += Time.deltaTime;
                if (randomAnimationTimer >= randomAnimationInterval)
                {
                    TriggerRandomAnimation();
                    randomAnimationTimer = 0f;
                }

                canTriggerTouchAnimation = true; // Idleに戻ったら再度タッチアニメーションが可能に
            }
            else
            {
                randomAnimationTimer = 0f;
            }
        }
    }

    /// <summary>
    /// タッチアニメーションをIdleステートから遷移させる
    /// </summary>
    public void TriggerTouchAnimation()
    {
        // 現在のステートがIdleであるか、かつクールダウン中でないことを確認
        if (!canTriggerTouchAnimation || isCooldown)
        {
            Debug.LogWarning("タッチアニメーションはクールダウン中のため利用できません。");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animatorが見つかりません。");
            return;
        }

        // 現在のステートがIdleであることを確認
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(idleStateName))
        {
            Debug.LogWarning("タッチアニメーションはIdleステートの時のみトリガーできます。");
            return;
        }

        // Idle ステートの時だけアニメーションをスキップしてタッチアニメーションを実行
        animator.Play(stateInfo.fullPathHash, 0, 1.0f); // 最終フレームへスキップ
        Debug.Log("Idleアニメーションを最終フレームにスキップしました。");

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger(cameraTriggerParamHash);
            Debug.Log("カメラアニメーションをトリガーしました。");
        }

        // トリガーを使用してタッチアニメーションを開始
        animator.SetTrigger(TouchTriggerParamHash);
        Debug.Log("タッチアニメーションをトリガーしました。");

        canTriggerTouchAnimation = false; // 一度実行後は無効化
        StartCoroutine(StartCooldown()); // クールダウン開始
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true; // クールダウン中に設定
        yield return new WaitForSeconds(cooldownTime); // クールダウン時間待機
        isCooldown = false; // クールダウン解除
        Debug.Log("クールダウンが終了しました。タッチアニメーションを再度トリガーできます。");
    }

    /// <summary>
    /// ランダムアニメーションをトリガーする
    /// </summary>
    private void TriggerRandomAnimation()
    {
        if (animator != null)
        {
            int randomAnim = Random.Range(1, animationRange + 1);
            animator.SetInteger("random", randomAnim);
            //Debug.Log($"Random animation triggered: {randomAnim}");
        }
    }

    /// <summary>
    /// タッチアニメーションをリセットする
    /// </summary>
    public void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(TouchTriggerParamHash); // トリガーをリセット
            Debug.Log("タッチアニメーションをリセットしました。");
        }
    }
}
