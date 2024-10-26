using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorExample : MonoBehaviour
{
    public Animator Animator { get { return GetComponent<Animator>(); } }

    [SerializeField]
    private Transform _lookTarget;
    [SerializeField, Range(0, 1)]
    private float _ikWeight;
    private void Start()
    {
        // "Lookat"という名前のオブジェクトを探して_targetにセット
        GameObject lookAtObject = GameObject.Find("Lookat");
        if (lookAtObject != null)
        {
            _lookTarget = lookAtObject.transform;
        }
        else
        {
            Debug.LogWarning("Lookat object not found!");
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
            if (_lookTarget != null) {
            Animator.SetLookAtWeight(_ikWeight);
            Animator.SetLookAtPosition(_lookTarget.position);
        }

    }
}
