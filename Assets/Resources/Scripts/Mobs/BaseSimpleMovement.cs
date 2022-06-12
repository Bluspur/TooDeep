using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSimpleMovement : MonoBehaviour
{
    [Tooltip("Draw paths (Make sure Gizmos are turned on)")]
    [SerializeField] private bool drawPath = true;
    [Space(10)]
    [Tooltip("Should the object move vertically?")]
    [SerializeField] private bool movesVertically = false;
    [Tooltip("Should the object flip when it turns?")]
    [SerializeField] private bool flipWhenReversing = true;
    [Tooltip("How far the object will move to the right or up")]
    [SerializeField] private float positivePathDistance = 5f;
    [Tooltip("How far the object will move to the left or down")]
    [SerializeField] private float negativePathDistance = -5f;
    [Tooltip("How fast should the object move?")]
    [SerializeField] protected float movementSpeed = 5f;
    [Tooltip("How long should the object wait when it reaches its positive destination?")]
    [SerializeField] private float positivePositionHoldTime = 0f;
    [Tooltip("How long should the object wait when it reaches its negative destination?")]
    [SerializeField]private float negativePositionHoldTime = 0f;

    protected Rigidbody2D objectRigidBody = null;
    private int direction = 1;
    private Vector2 initialPosition;
    private Vector2 positivePosition;
    private Vector2 negativePosition;
    private Vector2 velocity;
    private bool waiting;
    private float currentHoldTime;
    private float lastTime;
    

    #region UnityCallbacks

    private void Awake()
    {
        objectRigidBody = GetComponent<Rigidbody2D>();
        CalculatePositions();
        velocity = new Vector2(
            movesVertically ? 0f : movementSpeed,
            movesVertically ? movementSpeed : 0f);
    }

    private void Update()
    {
        if(waiting)
        {
            waiting = Time.time - lastTime < currentHoldTime;
        }
    }

    private void FixedUpdate()
    {
        if(!waiting)
        {
            HandleMovement();
        }
    }

    #endregion

    #region ObjectMovement

    private void HandleMovement()
    {
        if(direction > 0)
        {
            if(!movesVertically)
            {
                if (transform.position.x >= positivePosition.x)
                {
                    Reverse();
                }
            }
            else
            {
                if (transform.position.y >= positivePosition.y)
                {
                    Reverse();
                }
            }
        }
        else
        {
            if (!movesVertically)
            {
                if (transform.position.x <= negativePosition.x)
                {
                    Reverse();
                }
            }
            else
            {
                if (transform.position.y <= negativePosition.y)
                {
                    Reverse();
                }
            }
        }

        MoveObject(velocity);
    }

    protected virtual void MoveObject(Vector2 velocity)
    {
        objectRigidBody.MovePosition(objectRigidBody.position + velocity * Time.fixedDeltaTime);
    }

    private void Reverse()
    {
        lastTime = Time.time;
        currentHoldTime = direction > 0 ? positivePositionHoldTime : negativePositionHoldTime;
        waiting = true;

        velocity *= -1;
        FlipDirection();
    }

    private void FlipDirection()
    {
        direction *= -1;

        if(!flipWhenReversing) { return; }

        if(movesVertically)
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                transform.localScale.y * -1,
                transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(
                transform.localScale.x * -1,
                transform.localScale.y,
                transform.localScale.z);
        }
    }

    #endregion

    #region Helpers

    private void OnDrawGizmos()
    {
        if(drawPath)
        {
            DrawPaths();
        }
    }

    private void DrawPaths()
    {
        int isHorizontal = movesVertically ? 0 : 1;
        int isVertical = movesVertically ? 1 : 0;
        Vector3 positiveEnd;
        Vector3 negativeEnd;

        if(!Application.isPlaying)
        {
            positiveEnd = new Vector3(
                transform.position.x + positivePathDistance * isHorizontal,
                transform.position.y + positivePathDistance * isVertical);
            negativeEnd = new Vector3(
                transform.position.x + negativePathDistance * isHorizontal,
                transform.position.y + negativePathDistance * isVertical);
        }
        else
        {
            positiveEnd = new Vector3(
                initialPosition.x + positivePathDistance * isHorizontal,
                initialPosition.y + positivePathDistance * isVertical);
            negativeEnd = new Vector3(
                initialPosition.x + negativePathDistance * isHorizontal,
                initialPosition.y + negativePathDistance * isVertical);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, positiveEnd);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, negativeEnd);
    }

    private void CalculatePositions()
    {
        int isHorizontal = movesVertically ? 0 : 1;
        int isVertical = movesVertically ? 1 : 0;

        initialPosition = transform.position;

        positivePosition = new Vector3(
            transform.position.x + positivePathDistance * isHorizontal,
            transform.position.y + positivePathDistance * isVertical,
            transform.position.z);

        negativePosition = new Vector3(
            transform.position.x + negativePathDistance * isHorizontal,
            transform.position.y + negativePathDistance * isVertical,
            transform.position.z);
    }

    private int GetDirection()
    {
        return (int)Mathf.Sign(movesVertically ? transform.localScale.y : transform.localScale.x);
    }

    #endregion
}
