using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegateCameraTip : StateMachineBehaviour
{
    public bool Exit = false;
    public float StartTime = 0;
    public float FadeTime = 0;
    float Weight = 1;
    TiltForCamera tipComponent;
    public override void OnStateEnter (Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        tipComponent = animator.GetComponent<TiltForCamera>();
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (tipComponent && animatorStateInfo.normalizedTime >= StartTime)
        {
            if (FadeTime == 0)
            {
                if (animatorStateInfo.normalizedTime < StartTime)
                    Weight = 1;
                else
                    Weight = 0;
            }
            else
                Weight = Mathf.Clamp01((animatorStateInfo.normalizedTime - StartTime) / FadeTime);
            if (!Exit) Weight = 1 - Weight;

            tipComponent.TipWeight = Weight;
        }
    }
}
