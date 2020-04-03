using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpawnBehavior : StateMachineBehaviour
{
    public float NormalizedSpawnTime;
    public GameObject effect;
    bool spawned = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        spawned = false;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (!spawned && animatorStateInfo.normalizedTime >= NormalizedSpawnTime)
        {
            spawned = true;
            Instantiate(effect, animator.transform.position, animator.transform.rotation);
        }
    }
}
