using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

public class Defolt : MonoBehaviour
{   
    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    private Animator animator;
    [SerializeField] private Vrm10Instance vrm;
    [SerializeField] private int animerange;

    //delay秒遅らせる
    IEnumerator InvokeAfterDelay(System.Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    //テスト用
    void Start()
    {
       
        vrm10RuntimeExpression = vrm.Runtime.Expression;
        animator = vrm.GetComponent<Animator>();

    }

    void Update()
    {
        // animatorがnullでないことを確認してから処理を実行
        if (animator != null)
        {   
            //1~6のアニメーションを流す
            animator.SetInteger("random", Random.Range(1, animerange));
        }
    }
}
