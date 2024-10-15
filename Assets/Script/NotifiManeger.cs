using UnityEngine;

public class HideObjectAfterAnimation : MonoBehaviour
{
    public GameObject objectToHide; // 表示したいオブジェクトの参照

    void Start() {
        if (objectToHide.activeSelf)
        {
            objectToHide.SetActive(false);
        }
    }

    public void ShowObject()
    {
        // オブジェクトが非表示の場合のみ表示する
        if (!objectToHide.activeSelf)
        {
            objectToHide.SetActive(true);
        }
    }

    public void HideObject()
    {
        // オブジェクトが非表示の場合のみ表示する
        if (objectToHide.activeSelf)
        {
            objectToHide.SetActive(false);
        }
    }
}
