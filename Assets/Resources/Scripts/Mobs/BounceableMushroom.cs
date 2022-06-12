using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceableMushroom : Bounceable
{
    [SerializeField] private Animator animator = null;

    public override void HandleOnBounce()
    {
        animator.SetTrigger("Bounced");
        base.HandleOnBounce();
    }
}
