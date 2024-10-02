using UnityEngine;

public class HideObjectAfterAnimation : MonoBehaviour
{
    public GameObject objectToHide; // 表示したいオブジェクトの参照

    public void ShowObject()
    {
        // アニメーションが終了したらオブジェクトを非表示にする
        objectToHide.SetActive(true);
    }
}
