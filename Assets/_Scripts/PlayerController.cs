using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PhysicsObject {

    // Design Fields
    [SerializeField] private float MaxSpeed;
    [SerializeField] private float JumpTakeOffSpeed;
    [SerializeField] private int AdjustRawInputExponentially;
    [SerializeField] private int AdjustRawInputAmplitude;
    [SerializeField] private float CoyoteDuration;
    [SerializeField] private float BufferedDuration;

    // Temp
    [SerializeField] private bool DrawJumpTypeIndicators;
    [SerializeField] private GameObject JumpIndicator;

    // Privates
    private SpriteRenderer spriteRenderer;
    private bool isJumping = false;
    private bool isBuffered = false;
    private bool didBuffer = false;
    private bool isCoyote = false;
    private bool didCoyote = false;

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
        if (isBuffered) print("IsBuffered");

        // Vertical Jump
        if (Input.GetButtonDown("Jump") || isBuffered) {
            // Do a Jump!
            if (m_IsGrounded || isCoyote) {
                // Debug
                CreateJumpIndicator();

                // Set variables
                velocity.y = JumpTakeOffSpeed;
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
        } else if (Input.GetButtonUp("Jump") || didBuffer) {
            if (velocity.y > 0)
                velocity.y = velocity.y * 0.5f;
        }

        // Flip Sprite
        bool flipSprite = spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f);
        if (flipSprite) {
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        // Set our Velocity
        m_TargetVelocity = move * MaxSpeed;
    }

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