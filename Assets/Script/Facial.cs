using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UniVRM10;

public class Facial : MonoBehaviour
{   
    // VRMモデルのVrm10Instanceコンポーネントへの参照
    private Vrm10Instance vrmInstance;

    // VRMモデルのVrm10RuntimeExpressionコンポーネントへの参照
    private Vrm10RuntimeExpression vrm10RuntimeExpression;
    
    // Start is called before the first frame update
    void Start(){
         vrmInstance = gameObject.GetComponent<Vrm10Instance>();
        // Vrm10InstanceからVrm10RuntimeExpressionコンポーネントを取得
        vrm10RuntimeExpression = vrmInstance.Runtime.Expression;
    } 
    //笑顔
    void Smile(){
        vrm10RuntimeExpression.SetWeight(ExpressionKey.Happy, 1);
    }
    //真顔
    void NotSmile(){
        vrm10RuntimeExpression.SetWeight(ExpressionKey.Happy, 0);
    }

    //目閉じる
    void Blink()
    {
        vrm10RuntimeExpression.SetWeight(ExpressionKey.Blink, 1);
    }
    //目開ける
    void BlinkOpen()
    {
        vrm10RuntimeExpression.SetWeight(ExpressionKey.Blink, 0);
    }
}
