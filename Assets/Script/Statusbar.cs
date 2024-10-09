using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
{
    // ナビゲーションバー表示
    Screen.fullScreen = false;
    // ステータスバー表示
    StatusBarController.Show();
}


    // Update is called once per frame
    void Update()
    {
        
    }
}
