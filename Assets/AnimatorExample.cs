using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorExample : MonoBehaviour
{
    public Animator Animator { get { return GetComponent<Animator>(); } }

    [SerializeField]
    private Transform _lookTarget;
    //[SerializeField]
    //private Transform _leftHandIkTarget;
    [SerializeField, Range(0, 1)]
    private float _ikWeight;

    private void OnAnimatorIK(int layerIndex)
    {
        if (_lookTarget != null) {
            Animator.SetLookAtWeight(_ikWeight);
            Animator.SetLookAtPosition(_lookTarget.position);
        }

        // if (_leftHandIkTarget != null) {
        //     Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _ikWeight);
        //     Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _ikWeight);
        //     Animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIkTarget.position);
        //     Animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandIkTarget.rotation);
        // }
    }
}
