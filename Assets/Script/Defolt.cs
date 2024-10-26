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
    [SerializeField] private string touchTriggerParamName = "TouchTrigger";

    [Header("Camera Settings")]
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private string cameraExitStateName = "CameraExit"; // Exit用カメラアニメーション
    [SerializeField] private string cameraEnterStateName = "CameraEnter"; // Enter用カメラアニメーション
    [SerializeField] private string cameraTouchStateName = "Touchcamera"; // Enter用カメラアニメーション

    [Header("Screen Transition Animation Settings")]
    [SerializeField] private string exitScreenStateName = "ExitScreen"; 
    [SerializeField] private string enterScreenStateName = "EnterScreen"; 

    private Animator animator;
    private static readonly int TouchTriggerParamHash = Animator.StringToHash("TouchTrigger");

    private float randomAnimationTimer = 0f;
    private bool canTriggerTouchAnimation = false;

    public bool canmove = false;

    [SerializeField] private float cooldownTime = 2.0f;
    private bool isCooldown = false;
    private bool isExitCooldown = false;
    private bool isEnterCooldown = false;

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

        if (cameraAnimator == null)
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
                canmove = true;
                canTriggerTouchAnimation = true;
            }
            else
            {
                randomAnimationTimer = 0f;
                canmove = false;
            }
        }
    }

    public void TriggerTouchAnimation()
    {
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

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(idleStateName))
        {
            Debug.LogWarning("タッチアニメーションはIdleステートの時のみトリガーできます。");
            return;
        }

        animator.Play(stateInfo.fullPathHash, 0, 1.0f);
        Debug.Log("Idleアニメーションを最終フレームにスキップしました。");

        animator.SetTrigger(TouchTriggerParamHash);
        Debug.Log("タッチアニメーションをトリガーしました。");

        cameraAnimator.Play(cameraTouchStateName);
        Debug.Log("カメラタッチアニメーションをトリガーしました。");
        canTriggerTouchAnimation = false;
        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
        Debug.Log("クールダウンが終了しました。タッチアニメーションを再度トリガーできます。");
    }

    private void TriggerRandomAnimation()
    {
        if (animator != null)
        {
            int randomAnim = Random.Range(1, animationRange + 1);
            animator.SetInteger("random", randomAnim);
        }
    }

    public void ResetTouchAnimation()
    {
        if (animator != null)
        {
            animator.ResetTrigger(TouchTriggerParamHash);
            Debug.Log("タッチアニメーションをリセットしました。");
        }
    }

    public void TriggerExitScreenAnimation()
    {
        if (animator == null)
        {
            Debug.LogError("Animatorが見つかりません。");
            return;
        }

        animator.Play(exitScreenStateName);
        Debug.Log("ExitScreenアニメーションがトリガーされました。");

        if (cameraAnimator != null)
        {
            cameraAnimator.Play(cameraExitStateName);
            Debug.Log("CameraExitアニメーションが再生されました。");
        }
    }

    public void TriggerEnterScreenAnimation()
    {
        if (animator == null)
        {
            //Debug.LogError("Animatorが見つかりません。");
            return;
        }

        animator.Play(enterScreenStateName);
        Debug.Log("EnterScreenアニメーションがトリガーされました。");

        if (cameraAnimator != null)
        {
            cameraAnimator.Play(cameraEnterStateName);
            Debug.Log("CameraEnterアニメーションが再生されました。");
        }
    }

    public void StartExitProcess()
    {
        if (isExitCooldown)
        {
            Debug.LogWarning("ExitScreenアニメーションはクールダウン中です。");
            return;
        }

        TriggerExitScreenAnimation();
        StartCoroutine(StartExitCooldown());
    }

    public void StartEnterProcess()
    {
        if (isEnterCooldown)
        {
            Debug.LogWarning("EnterScreenアニメーションはクールダウン中です。");
            return;
        }

        TriggerEnterScreenAnimation();
        StartCoroutine(StartEnterCooldown());
    }

    private IEnumerator StartExitCooldown()
    {
        isExitCooldown = true;
        yield return new WaitForSeconds(1.0f); 
        isExitCooldown = false;
        Debug.Log("ExitScreenアニメーションのクールダウンが終了しました。");
    }

    private IEnumerator StartEnterCooldown()
    {
        isEnterCooldown = true;
        yield return new WaitForSeconds(1.0f); 
        isEnterCooldown = false;
        Debug.Log("EnterScreenアニメーションのクールダウンが終了しました。");
    }
}
