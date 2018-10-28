using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PhysicsObject {

    // Design Fields
    [SerializeField] private float MaxSpeed = 0;
    [SerializeField] private float MaximumJumpSpeed = 0;
    [SerializeField] private int AdjustRawInputExponentially = 0;
    [SerializeField] private int AdjustRawInputAmplitude = 0;
    [SerializeField] private float CoyoteDuration = 0;
    [SerializeField] private float BufferedDuration = 0;
    [Tooltip("This is the minimum force of JumpTakeOff speed that a jump makes, i.e. how high tapping jump will take you.")]
    [Range(0f, 1f)]
    [SerializeField] private float MinimumJumpPercentage = 0;

    // Temp
    [SerializeField] private bool DrawJumpTypeIndicators = false;
    [SerializeField] private GameObject JumpIndicator = null;

    // Privates
    private SpriteRenderer spriteRenderer = null;
    private bool isJumping = false;
    private bool isBuffered = false;
    private bool didBuffer = false;
    private bool isCoyote = false;
    private bool didCoyote = false;
    private bool checkJumpCancel = false;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void ComputeVelocity() {
        Vector2 move = Vector2.zero;

        // Horizontal Inputs

        float rawHInput = Input.GetAxis("Horizontal");
        float adjustedHInput = Mathf.Pow(Mathf.Abs(rawHInput), AdjustRawInputExponentially) * AdjustRawInputAmplitude;
        adjustedHInput *= Mathf.Sign(rawHInput);
        move.x = rawHInput;

        // Register Coyote Time
        if (!isJumping && !m_IsGrounded && !isCoyote && (CoyoteDuration > 0f)) {
            if (!didCoyote) {
                isCoyote = true;
                didCoyote = true;
                StartCoroutine(EndCoyote(CoyoteDuration));
            }
        } else if (m_IsGrounded) {
            isJumping = false;
            didCoyote = false;
        }

        // Vertical Jump     
        if (Input.GetButtonDown("Jump") || isBuffered) {
            // Do a Jump!
            if (m_IsGrounded || isCoyote) {
                // Debug
                CreateJumpIndicator();

                // Set variables
                velocity.y = MaximumJumpSpeed;
                isCoyote = false;
                didBuffer = isBuffered;
                isBuffered = false;
                isJumping = true;
            }
            // Input Buffered Jump
            else {
                if (isBuffered == false) {
                    isBuffered = true;
                    StartCoroutine(EndQueue(BufferedDuration));
                }
            }
        }

        // We separate these checks out so a user isn't locked to lower jump on a buffer.
        if (Input.GetButtonUp("Jump") && !didBuffer) {
            checkJumpCancel = true;
            didBuffer = false;
        }

        if (didBuffer && !Input.GetButton("Jump")) {
            checkJumpCancel = true;
            didBuffer = false;
        }

        if (checkJumpCancel) {
            if (velocity.y < ((1 - MinimumJumpPercentage) * MaximumJumpSpeed)) {
                velocity.y *= 0.5f;
                checkJumpCancel = false;
            }
        }

        // Flip Sprite
        bool flipSprite = spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f);
        if (flipSprite) {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        // Set our Velocity
        m_TargetVelocity = move * MaxSpeed;
    }

    // Coroutines

    private IEnumerator EndQueue(float delay) {
        yield return new WaitForSeconds(delay);
        isBuffered = false;
    }

    private IEnumerator EndCoyote(float delay) {
        yield return new WaitForSeconds(delay);
        isCoyote = false;
    }

    IEnumerator DeactivateThisObject(GameObject thisObject, float delayTime) {
        yield return new WaitForSeconds(delayTime);
        thisObject.SetActive(false);
    }

    // Debug

    private void CreateJumpIndicator() {
        // Debug our jumps!
        if (DrawJumpTypeIndicators) {
            GameObject thisInd = Instantiate(JumpIndicator, transform.position, Quaternion.identity);
            Animator thisAnim = thisInd.GetComponent<Animator>();

            if (m_IsGrounded) {
                thisAnim.SetInteger("Frame", 0);
            }
            if (isCoyote) {
                thisAnim.SetInteger("Frame", 1);
            }
            if (isBuffered) {
                thisAnim.SetInteger("Frame", 2);
            }
            StartCoroutine(DeactivateThisObject(thisInd, 3f));
        }
    }
}