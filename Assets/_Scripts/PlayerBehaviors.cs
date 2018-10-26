using UnityEngine;
using UnityEngine.Events;

public class PlayerBehaviors : MonoBehaviour {
    [SerializeField] private float m_JumpForce = 400f; // Amount of force added when the player jumps.
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f; // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false; // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround; // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck; // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck; // A position marking where to check for ceilings
    [SerializeField] private GameObject JumpIndicator;

    private bool m_DoJump = false;
    private bool m_Grounded; // Whether or not the player is grounded.
    private bool m_AlreadyGrounded = false;
    private bool m_Jumped = false;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true; // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded

    [Header("Events")]
    [Space]

    public UnityEvent OnJumpEvent;
    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;

    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnJumpEvent == null)
            OnJumpEvent = new UnityEvent();

    }

    private void FixedUpdate() {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        bool didCollide = Physics2D.Raycast(transform.position, Vector2.down, 1.0f, m_WhatIsGround);
        if (didCollide) {
            if (!wasGrounded && !m_AlreadyGrounded) {
                OnLandEvent.Invoke();
                m_AlreadyGrounded = true;
            }
            m_Grounded = true;
            m_Jumped = false;
        } else {
            if (wasGrounded && !m_Jumped) {
                OnFallEvent.Invoke();
            }
        }
    }

    public bool Move(float horizontalMove, bool jump, bool isCoyoteTime, bool isBuffered) {
        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl) {

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(horizontalMove * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (horizontalMove > 0 && !m_FacingRight) {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (horizontalMove < 0 && m_FacingRight) {
                // ... flip the player.
                Flip();
            }
        }

        // If the player should jump...
        if ((m_Grounded || isCoyoteTime) && (jump || isBuffered)) {
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));

            // Invoke any Jump stuff
            OnJumpEvent.Invoke();
            m_AlreadyGrounded = false;
            m_Jumped = true;

            // Debug Indicators!
            GameObject thisPing = Instantiate(JumpIndicator, transform.position, Quaternion.identity);
            Animator thisAnim = thisPing.GetComponent<Animator>();
            if (isCoyoteTime) {
                thisAnim.SetInteger("Frame", 1);
            } else {
                thisAnim.SetInteger("Frame", 0);
            }

            if (isBuffered) {
                thisAnim.SetInteger("Frame", 2);
            }

            return true;
        }

        return false;
    }

    private void Flip() {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}