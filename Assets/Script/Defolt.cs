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
    [SerializeField] private string touchParamName = "Touch";

    [Header("Camera Settings")]
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private string cameraTriggerParamName = "CameraTrigger";

    private Animator animator;
    private static readonly int TouchParamHash = Animator.StringToHash("Touch");
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

            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("motion");
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
        if (!canTriggerTouchAnimation || isCooldown)
        {
            Debug.LogWarning("Touch animation is not available during cooldown.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animatorが見つかりません。");
            return;
        }

        // Idle ステートの時だけアニメーションをスキップしてタッチアニメーションを実行
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Idle"))
        {
            int currentStateHash = stateInfo.fullPathHash;
            animator.Play(currentStateHash, 0, 1.0f); // 最終フレームへスキップ
            Debug.Log("Idle animation forced to its final frame.");
            if (cameraAnimator != null)
            {
                cameraAnimator.SetTrigger(cameraTriggerParamHash);
                Debug.Log("Camera animation triggered.");
            }
        }

        animator.SetBool(TouchParamHash, true); // タッチアニメーションのトリガー
        Debug.Log("Touch animation triggered.");

        canTriggerTouchAnimation = false; // 一度実行後は無効化
        StartCoroutine(StartCooldown()); // クールダウン開始
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true; // クールダウン中に設定
        yield return new WaitForSeconds(cooldownTime); // 2秒待機
        isCooldown = false; // クールダウン解除
        Debug.Log("Cooldown finished. Touch animation can be triggered again.");
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
            animator.SetBool(TouchParamHash, false);
            Debug.Log("Touch animation reset.");
        }
    }
}
