using Bluspur.Collectables;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bluspur.Movement
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Cached References")]
        [SerializeField] private Rigidbody2D playerRigidBody = null;
        [SerializeField] private Collider2D playerHitBox = null;
        [SerializeField] private Animator playerAnimator = null;

        [Header("Unlocks")]
        [SerializeField] private bool multiJumpUnlocked = true;
        [SerializeField] private bool dashUnlocked = true;
        [SerializeField] private bool pogoUnlocked = true;

        [Header("Horizontal Movement")]
        [SerializeField] private float movementSpeed = 1f;
        [SerializeField, Range(0f, 100f)] private float inAirMovementPercentage = 100f;

        [Header("Jumping")]
        [SerializeField] private LayerMask jumpableLayers;
        [Tooltip("The height above the ground that the engine will consider the player to be grounded at")]
        [SerializeField] private float groundDetectionGraceHeight = 0.5f;
        [Tooltip("The time in seconds that the player has to make a \"grounded\" jump after they left the ground")]
        [SerializeField] private float coyoteTime = 0.3f;
        [Space(20)]
        [SerializeField] private float minimumJumpForce = 1f;
        [SerializeField] private float maximumJumpHoldTime = 0.2f;
        [Space(20)]
        [SerializeField] private int numberInAirJumps = 0;

        [Header("Dashing")]
        [SerializeField] private float dashForce = 30f;
        [SerializeField] private float dashDuration = 0.5f;
        [SerializeField] private float minimumVelocityToDash = 0.1f;
        [SerializeField] private float dashCooldown = 0.5f;

        [Header("Pickaxe Drop")]
        [SerializeField] private LayerMask bouncableLayers;
        [SerializeField] private LayerMask slideableLayers;
        [SerializeField] private LayerMask platformLayers;
        [SerializeField] private float buttSlideSpeed = 20f;
        [SerializeField] private int buttSlideGraceFrames = 10;

        [Header("Platform Deactivation")]
        [SerializeField] private float pushForce = 10f;

        [Header("Coin Throwing")]
        [SerializeField] private GameObject throwableCoinPrefab = null;
        [SerializeField] private float cooldownBetweenThrows = 0.1f;

        private bool movementEnabled = true;

        private float horizontalInput = 0f;
        
        private int jumpsRemaining = 0;
        private bool isGrounded = false;
        private bool jumpScheduled = false;
        private bool isJumping = false;
        private float coyoteTimeCounter = 0f;
        private float lastSuccessfulJumpTime = 0f;

        private bool dashing = false;
        private float lastDashTime = 0f;

        private bool dropModifierKeyHeld;
        private bool sliding = false;
        private bool pogoing = false;
        private bool downPushScheduled = false;
        private bool repulseScheduled = false;
        private bool onSlope = false;

        private Bounceable lastBounceData = null;
        private DeactivatablePlatform platformUnderfoot = null;
        private Vector2 slopeTangent;
        private int buttSlideGraceFramesRemaining = 0;

        private GameManager manager = null;
        private bool throwKeyHeld = false;
        private float lastThrowTime = 0f;

        #region EventHandlers

        public void OnHorizontalInput(InputAction.CallbackContext context)
        {
            if(movementEnabled)
            {
                horizontalInput = context.ReadValue<float>();
            }
        }

        public void OnJumpInput(InputAction.CallbackContext context)
        {
            if(movementEnabled)
            {
                if (context.started)
                {
                    HandleJumpInput();
                }

                if (context.performed)
                {
                    jumpScheduled = false;
                }
            }
        }

        public void OnDashInput(InputAction.CallbackContext context)
        {
            if(movementEnabled)
            {
                if (context.started)
                {
                    HandleDash();
                }
            }
        }

        public void OnDropInput(InputAction.CallbackContext context)
        {
            if(movementEnabled)
            {
                if (context.started)
                {
                    dropModifierKeyHeld = true;
                }
                if (context.performed)
                {
                    dropModifierKeyHeld = false;
                    sliding = false;
                    pogoing = false;
                }
            }
        }

        public void OnThrowInput(InputAction.CallbackContext context)
        {
            if(movementEnabled)
            {
                if(context.started)
                {
                    throwKeyHeld = true;
                }
                if(context.performed)
                {
                    throwKeyHeld = false;
                }
            }
        }

        #endregion

        #region UnityCallbacks

        private void Start()
        {
            manager = FindObjectOfType<GameManager>();
        }

        private void FixedUpdate()
        {
            slopeTangent = GetSlopeTangent();

            if(downPushScheduled)
            {
                DownPush();
            }

            if(repulseScheduled)
            {
                DropRepulse();
            }

            if(sliding)
            {
                if(slopeTangent != Vector2.zero)
                {
                    Slide();
                }
            }
            else
            {
                HandleHorizontalMovement();
            }

            bool withinJumpHoldTime = (Time.time - lastSuccessfulJumpTime < maximumJumpHoldTime);

            if (jumpScheduled)
            {
                isJumping = EvaluateJump();
                jumpScheduled = false;
            }

            if (isJumping && withinJumpHoldTime)
            {
                DoJump();
            }

            bool isDashComplete = (Time.time - lastDashTime > dashDuration);

            if (dashing && !isDashComplete)
            {
                Dash();
            }
            else if (isDashComplete)
            {
                dashing = false;
            }
        }

        private void Update()
        {
            FlipSprite();
            HandleAnimationStates();

            if (isGrounded = IsGrounded())
            {
                ResetJumps();
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (dropModifierKeyHeld)
            {
                HandleDrop();

                if(pogoing && !repulseScheduled)
                {
                    repulseScheduled = GetDropCollision();
                }

                if(sliding)
                {
                    if(isGrounded)
                    {
                        buttSlideGraceFramesRemaining--;
                        if (buttSlideGraceFramesRemaining <= 0)
                        {
                            sliding = false;
                        }
                    }
                }
            }

            if(throwKeyHeld)
            {
                ThrowCoin();
            }
        }

        #endregion

        #region HorizontalMovement

        private void HandleHorizontalMovement()
        {
            float speedModifier = 1f;

            if (!isGrounded)
            {
                speedModifier = inAirMovementPercentage / 100;
            }

            playerRigidBody.velocity = new Vector2(horizontalInput * movementSpeed * speedModifier, playerRigidBody.velocity.y);

        }

        #endregion

        #region Jumping

        private void HandleJumpInput()
        {
            if (dropModifierKeyHeld)
            {
                platformUnderfoot = GetPlatformUnderfoot();
                if(platformUnderfoot != null)
                {
                    platformUnderfoot.DeactivatePlatform();
                    downPushScheduled = true;
                }
                else
                {
                    jumpScheduled = true;
                }
            }
            else
            {
                jumpScheduled = true;
            }
        }

        private bool EvaluateJump()
        {
            if(coyoteTimeCounter > 0)
            {
                lastSuccessfulJumpTime = Time.time;
                coyoteTimeCounter = 0f;
                return true;
            }
            else if (jumpsRemaining > 0 && multiJumpUnlocked)
            {
                jumpsRemaining--;
                lastSuccessfulJumpTime = Time.time;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DoJump()
        {
            playerRigidBody.velocity = new Vector2(playerRigidBody.velocity.x, minimumJumpForce);
        }
        
        private void ResetJumps()
        {
            jumpsRemaining = numberInAirJumps;
        }

        #endregion

        #region Dashing

        private void HandleDash()
        {
            bool meetsVelocityThreshold = Mathf.Abs(playerRigidBody.velocity.x) > minimumVelocityToDash;
            bool cooldownComplete = Time.time - lastDashTime > dashCooldown;

            if (meetsVelocityThreshold && cooldownComplete && dashUnlocked)
            {
                dashing = true;
                lastDashTime = Time.time;
            }
        }

        private void Dash()
        {
            float directionalDashForce = Mathf.Sign(transform.localScale.x) * dashForce;
            playerRigidBody.velocity = new Vector2(directionalDashForce, 0f);
        }
        #endregion

        #region Drop

        private void HandleDrop()
        {
            if (!downPushScheduled)
            {
                if(isGrounded)
                {
                    if(onSlope)
                    {
                        sliding = true;
                        buttSlideGraceFramesRemaining = buttSlideGraceFrames;
                    }

                    pogoing = false;
                }
                else
                {
                    if(pogoUnlocked)
                    {
                        pogoing = true;
                    }
                }
            }
        }

        private bool GetDropCollision()
        {
            Collider2D contact = Physics2D.OverlapBox(
                playerHitBox.bounds.center - new Vector3(0f, groundDetectionGraceHeight, 0f),
                playerHitBox.bounds.size - new Vector3(0.1f, 0f, 0f),
                0f,
                bouncableLayers);

            if (contact == null) { return false; }

            Bounceable bounceData = contact.GetComponentInParent<Bounceable>();

            if (bounceData != null)
            {
                lastBounceData = bounceData;
            }

            return bounceData != null;
        }

        private void DropRepulse()
        {
            float bounceForce = Mathf.Clamp(
                (playerRigidBody.velocity.y * -1 * lastBounceData.BounceMultiplier),
                lastBounceData.BounceMinimumForce,
                lastBounceData.BounceMaximumForce);
            playerRigidBody.velocity = new Vector2(
                playerRigidBody.velocity.x,
                bounceForce);
            repulseScheduled = false;
            lastBounceData.HandleOnBounce();
        }

        private void DownPush()
        {
            playerRigidBody.velocity = new Vector2(playerRigidBody.velocity.x, -pushForce);
            downPushScheduled = false;
        }

        private void Slide()
        {
            playerRigidBody.velocity = new Vector2(
                Mathf.Clamp(playerRigidBody.velocity.x + slopeTangent.normalized.x * buttSlideSpeed, -buttSlideSpeed, buttSlideSpeed),
                Mathf.Clamp(playerRigidBody.velocity.y + slopeTangent.normalized.y * buttSlideSpeed, -buttSlideSpeed, buttSlideSpeed));
        }

        private Vector2 GetSlopeTangent()
        {
            RaycastHit2D raycastCenter = Physics2D.Raycast(playerHitBox.bounds.center, Vector2.down, playerHitBox.bounds.size.y + groundDetectionGraceHeight, slideableLayers);
            RaycastHit2D raycastLeft = Physics2D.Raycast(new Vector2(playerHitBox.bounds.center.x - playerHitBox.bounds.size.x / 2, playerHitBox.bounds.center.y), Vector2.down, playerHitBox.bounds.size.y + groundDetectionGraceHeight, slideableLayers);
            RaycastHit2D raycastRight = Physics2D.Raycast(new Vector2(playerHitBox.bounds.center.x + playerHitBox.bounds.size.x / 2, playerHitBox.bounds.center.y), Vector2.down, playerHitBox.bounds.size.y + groundDetectionGraceHeight, slideableLayers);

            if (raycastCenter.collider != null && raycastLeft.collider != null && raycastRight.collider != null)
            {
                Vector2 tangent = Vector2.Perpendicular(raycastCenter.normal);

                if(IsVectorRoughlyEqual(tangent, Vector2.left) || IsVectorRoughlyEqual(tangent, Vector2.zero))
                {
                    onSlope = false;
                }
                else
                {
                    onSlope = true;
                }

                if (tangent.y > 0)
                {
                    tangent *= -1;
                }

                if(Mathf.Sign(playerRigidBody.velocity.x) != Mathf.Sign(tangent.x))
                {
                    tangent.x *= -1;
                }

                if(Mathf.Abs(tangent.x) < 0.01)
                {
                    tangent.x = 0f;
                }

                if (Mathf.Abs(tangent.y) < 0.01)
                {
                    tangent.y = 0f;
                }

                Debug.DrawRay(playerHitBox.bounds.center, tangent, Color.red);

                return tangent;
            }
            else
            {
                onSlope = false;

                return Vector2.zero;
            }
        }

        private DeactivatablePlatform GetPlatformUnderfoot()
        {
            RaycastHit2D raycastCenter = Physics2D.Raycast(playerHitBox.bounds.center, Vector2.down, playerHitBox.bounds.size.y + groundDetectionGraceHeight, platformLayers);
            RaycastHit2D raycastLeft = Physics2D.Raycast(new Vector2(playerHitBox.bounds.center.x - playerHitBox.bounds.size.x / 2, playerHitBox.bounds.center.y), Vector2.down, playerHitBox.bounds.size.y + groundDetectionGraceHeight, platformLayers);
            RaycastHit2D raycastRight = Physics2D.Raycast(new Vector2(playerHitBox.bounds.center.x + playerHitBox.bounds.size.x / 2, playerHitBox.bounds.center.y), Vector2.down, playerHitBox.bounds.size.y + groundDetectionGraceHeight, platformLayers);

            DeactivatablePlatform platform = null;

            if (raycastCenter.collider != null && raycastLeft.collider != null && raycastRight.collider != null)
            {
                if(raycastCenter.collider.gameObject.GetComponentInParent<DeactivatablePlatform>())
                {
                    platform = raycastCenter.collider.gameObject.GetComponentInParent<DeactivatablePlatform>();
                }
            }

            return platform;
        }

        #endregion

        #region UnlockTriggers

        public void UnlockMultiJump()
        {
            multiJumpUnlocked = true;
        }

        public void UnlockDash()
        {
            dashUnlocked = true;
        }

        public void UnlockPogo()
        {
            pogoUnlocked = true;
        }

        public void EnableMovement()
        {
            playerAnimator.SetBool("InElevator", false);
            playerRigidBody.isKinematic = false;
            movementEnabled = true;
        }

        public void DisableMovement()
        {
            playerRigidBody.isKinematic = true;
            playerRigidBody.velocity = Vector2.zero;

            movementEnabled = false;

            playerAnimator.SetBool("InElevator", true);
        }

        #endregion

        #region Throw

        private void ThrowCoin()
        {
            if(manager.CollectedCoins > 0)
            {
                if (Time.time - lastThrowTime > cooldownBetweenThrows)
                {
                    lastThrowTime = Time.time;
                    Collectable.AddCoins(-1);
                    GameObject coinInstance = Instantiate(throwableCoinPrefab, transform.position, Quaternion.identity);
                    Vector2 launchForce = new Vector2(UnityEngine.Random.Range(-5, 5), 7f);
                    coinInstance.GetComponent<Rigidbody2D>().AddForce(launchForce, ForceMode2D.Impulse);
                }
            }
        }

        #endregion

        #region AnimationHandlers

        private void FlipSprite()
        {
            bool hasXVelocity = Mathf.Abs(playerRigidBody.velocity.x) > 0.1f;

            if (hasXVelocity)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Mathf.Sign(playerRigidBody.velocity.x), transform.localScale.y);
            }
        }

        private void HandleAnimationStates()
        {
            bool isRunning = Mathf.Abs(horizontalInput) > 0f;
            bool isJumping = playerRigidBody.velocity.y > 0 && !isGrounded;
            bool isFalling = playerRigidBody.velocity.y < 0 && !isGrounded;
            bool isDropping = pogoing || sliding;
            float verticalVelocity = playerRigidBody.velocity.y;
            playerAnimator.SetBool("IsRunning", isRunning);
            playerAnimator.SetBool("IsJumping", isJumping);
            playerAnimator.SetBool("IsFalling", isFalling);
            playerAnimator.SetBool("IsDropping", isDropping);
            playerAnimator.SetFloat("VerticalVelocity", verticalVelocity);
        }

        #endregion

        #region Helpers

        private bool IsVectorRoughlyEqual (Vector2 vectorOne, Vector2 vectorTwo)
        {
            float tolerance = 0.01f;
            bool xEquals = Mathf.Abs(vectorTwo.x - vectorOne.x) < tolerance;
            bool yEquals = Mathf.Abs(vectorTwo.y - vectorOne.y) < tolerance;
            return xEquals && yEquals;
        }

        private bool IsGrounded()
        {
            Collider2D contact = Physics2D.OverlapBox(
                playerHitBox.bounds.center - new Vector3(0f, groundDetectionGraceHeight, 0f),
                playerHitBox.bounds.size - new Vector3(0.1f, 0f, 0f),
                0f,
                jumpableLayers);
            return contact != null;
        }

        #endregion

    }
}

