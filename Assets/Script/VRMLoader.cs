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
    // ロードの進捗状況を管理する
    public GameObject kurukuru;
    public GameObject defo;
    public Slider slider; 

    private Animator animator;
    private bool vrmLoaded = false; // VRMがロードされたかどうかを示すフラグ
    public Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Vrm10Instance vrm10Instance;
    
    //モデルの大きさ
    float scale;
    

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

    //delay秒遅らせる
    IEnumerator InvokeAfterDelay(System.Action action, float delay)
    {   
        yield return new WaitForSeconds(delay);
        action();
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
        // VRMがロードされていない場合は何もしない
        if (!vrmLoaded)
            return;

        // animatorがnullでないことを確認してから処理を実行
        if (animator != null)
        {   
            //1~6のアニメーションを流す
            animator.SetInteger("random", Random.Range(1, 6));
        }
    }
}

