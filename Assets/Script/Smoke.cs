using UnityEngine;

public class Smoke : MonoBehaviour
{
    public new ParticleSystem particleSystem;
    public GameObject smoke;

    private void OnAnimationEnd()
    {
        // パーティクルを再生する
        particleSystem.Play();
        smoke.SetActive(true);
    }

    void HideObject()
    {
        // アニメーションが終了したらオブジェクトを非表示にする
        this.gameObject.SetActive (false);
    }

}
