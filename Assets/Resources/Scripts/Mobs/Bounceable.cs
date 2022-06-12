using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounceable : MonoBehaviour
{
    [SerializeField] private bool destroyOnBounce = true;
    [SerializeField] private float bounceMultiplier = 0.9f;
    [SerializeField] private float bounceMinimumForce = 15f;
    [SerializeField] private float bounceMaximumForce = 20f;

    public float BounceMinimumForce
    {
        get { return bounceMinimumForce; }
        set { bounceMinimumForce = value; }
    }
    public float BounceMaximumForce
    {
        get { return bounceMaximumForce; }
        set { bounceMaximumForce = value; }
    }
    public float BounceMultiplier
    {
        get { return bounceMultiplier; }
        set { bounceMultiplier = value; }
    }

    public virtual void HandleOnBounce()
    {
        if(destroyOnBounce)
        {
            Destroy(gameObject);
        }
    }
}
