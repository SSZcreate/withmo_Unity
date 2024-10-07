using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniVRM10;
using TMPro;
using DG.Tweening;
using Unity.VisualScripting.Antlr3.Runtime;

public class VRMLoader : MonoBehaviour
{
    public GameObject kurukuru;
    public GameObject defo;
    public Slider slider; 

    private Animator animator;

    [SerializeField] private int animationRange; // アニメーション範囲を設定
    private bool vrmLoaded = false; // VRMがロードされたかどうかを示すフラグ
    public Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Vrm10Instance vrm10Instance;

    //モデルの大きさ
    float scale;

    private bool isTouchAnimationPlaying = false; // タッチアニメーションが再生中かのフラグ
    private static readonly string TouchParam = "Touch";
    
    [SerializeField] private float touchAnimationDuration = 2.0f; // タッチアニメーションの継続時間

    public async void ReceiveVRMFilePath(string path)
    {   
        //vrmがあればvrmを削除
        if (vrmLoaded){
            Destroy(vrm10Instance.gameObject);
            vrmLoaded = false;
        }
        //デフォルトモデルの削除
        Destroy(defo);
        //ロード画面on
        kurukuru.SetActive(true);
        //スライダーリセット
        slider.value = 1;

        try{
            //pathからVRM読み込み
            vrm10Instance = await Vrm10.LoadPathAsync(path);
            // 読み込んだVRMインスタンスの位置を設定
            vrm10Instance.gameObject.transform.position = Vector3.zero;
            // 表情操作
            vrm10RuntimeExpression = vrm10Instance.Runtime.Expression;
            // 読み込んだVRMインスタンスのAnimatorコンポーネントにアクセス
            animator = vrm10Instance.GetComponent<Animator>();
            //Facialをアタッチ
            vrm10Instance.gameObject.AddComponent<Facial>();
        } catch(System.Exception e) {
            //MethodCで投げられた例外はここで処理される
            Debug.LogWarning($"Meshをアタッチしてください。 : {e}");
            SceneManager.LoadScene (SceneManager.GetActiveScene().name);

        }

        //ロード画面off
        kurukuru.SetActive(false);

        if (animator != null)
        {
            // アニメーションコントローラのファイル名のみ指定
            animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("motion");
        }
        vrmLoaded = true; // VRMがロードされたことを示すフラグを設定
    }

    //Sliderでスケール変更
    public void ScaleAdjust()
    {   
        if (!vrmLoaded)
            return;

        float scale = slider.value;
        vrm10Instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    //テスト用
    public void testcase()
    {
        ReceiveVRMFilePath("C:\\Users\\ok122\\Downloads\\shiroko_VRM\\shiroko_VRM\\ba_shiroko.vrm");
    }

    void Start(){

    }

    void Update()
    {
        if (animator != null)
        {
            // 通常のアニメーションをランダムで再生
            if (!isTouchAnimationPlaying)
            {
                animator.SetInteger("random", Random.Range(1, animationRange));
            }

            // タッチ入力があるかを確認
            if (Input.touchCount > 0)
            {
                // タッチが発生したら、現在のアニメーションをスキップしてタッチアニメーションに遷移
                ForceTransitionToTouchAnimation();
            }
        }
    }

    // 現在のアニメーションを強制的にスキップし、Touchアニメーションに強制的に遷移させるメソッド
    private void ForceTransitionToTouchAnimation()
    {
        if (isTouchAnimationPlaying) return; // 既にタッチアニメーションが再生中なら処理しない

        // 現在のアニメーションを取得
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // レイヤー0のステート情報を取得
        int currentStateHash = stateInfo.fullPathHash; // 現在のステートのハッシュ値を取得

        // 現在のアニメーションを強制的に終了させ、再生時間を1に設定（最終フレームにジャンプ）
        animator.Play(currentStateHash, 0, 1.0f); // レイヤー0、再生時間1（終わり）

        // Touchアニメーションを再生
        animator.SetBool(TouchParam, true); // Touch パラメータをオンにする

        isTouchAnimationPlaying = true;

        // コルーチンを使って、一定時間後にTouchアニメーションを終了
        StartCoroutine(ResetTouchAfterDelay(touchAnimationDuration));
    }

    // 一定時間後にTouchアニメーションを終了させるコルーチン
    private IEnumerator ResetTouchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Touch パラメータをオフにし、通常のアニメーション再生に戻す
        animator.SetBool(TouchParam, false);
        isTouchAnimationPlaying = false;
    }
}
