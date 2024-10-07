using System.Collections;
using UnityEngine;
using UniVRM10;

public class DefaultAnimationController : MonoBehaviour
{
    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Animator animator;
    [SerializeField] private Vrm10Instance vrm;
    [SerializeField] private int animationRange; // アニメーション範囲を設定
    private bool isTouchAnimationPlaying = false; // タッチアニメーションが再生中かのフラグ
    private static readonly string TouchParam = "Touch";
    [SerializeField] private float touchAnimationDuration = 2.0f; // タッチアニメーションの継続時間

    void Start()
    {
        if (vrm != null)
        {
            vrm10RuntimeExpression = vrm.Runtime.Expression;
            animator = vrm.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("VRM instance is not assigned.");
        }
    }

    void Update()
    {
        if (animator != null)
        {
            if (!isTouchAnimationPlaying)
            {
                animator.SetInteger("random", Random.Range(1, animationRange));
            }

            if (Input.touchCount > 0)
            {
                ForceTransitionToTouchAnimation();
            }
        }
        else
        {
            Debug.LogError("Animator not assigned or found on the VRM.");
        }
    }

    private void ForceTransitionToTouchAnimation()
    {
        if (isTouchAnimationPlaying) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int currentStateHash = stateInfo.fullPathHash;

        animator.Play(currentStateHash, 0, 1.0f); // 現在のアニメーションを終了
        animator.SetBool(TouchParam, true); // タッチアニメーションに遷移
        isTouchAnimationPlaying = true;

        // タッチアニメーションが一定時間経過後に終了する
        StartCoroutine(ResetTouchAfterDelay(touchAnimationDuration));
    }

    private IEnumerator ResetTouchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool(TouchParam, false);
        isTouchAnimationPlaying = false;
    }
}
