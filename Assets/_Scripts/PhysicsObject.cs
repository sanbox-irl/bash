using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    public float minGroundNormalY = .65f;
    public float GravityModifierUp = 1f;
    public float GravityModifierDown = 1f;

    protected Vector2 m_TargetVelocity;
    protected bool m_IsGrounded;
    protected Vector2 m_GroundNormal;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D m_ContactFilter2D;
    protected RaycastHit2D[] m_HitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> m_HitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    private void OnEnable() {
        rb2d = GetComponent<Rigidbody2D>();
        m_ContactFilter2D.useTriggers = false;
        m_ContactFilter2D.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_ContactFilter2D.useLayerMask = true;
    }

    protected virtual void ComputeVelocity() {}

    void Update() {
        m_TargetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    void FixedUpdate() {

        // Core Gravity Movement
        float thisGravity = velocity.y >= 0 ? GravityModifierUp : GravityModifierDown;
        velocity += thisGravity * Physics2D.gravity * Time.deltaTime;
        velocity.x = m_TargetVelocity.x;
        m_IsGrounded = false;

        // DeltaPos
        Vector2 deltaPos = velocity * Time.deltaTime;

        // Horizontal
        Vector2 moveAlongGround = new Vector2(m_GroundNormal.y, -m_GroundNormal.x);
        Vector2 move = moveAlongGround * deltaPos.x;
        Movement(move, false);

        // Vertical
        Vector2 ourMove = Vector2.up * deltaPos.y;
        Movement(ourMove, true);
    }

    void Movement(Vector2 move, bool yMovement) {

        // Check if we Should Move At all
        float distance = move.magnitude;
        if (distance > minMoveDistance) {
            // Do collisions
            int numberOfContacts = rb2d.Cast(move, m_ContactFilter2D, m_HitBuffer, distance + shellRadius);
            m_HitBufferList.Clear();

            // Add our Objects into the List
            for (int i = 0; i < numberOfContacts; i++)
                m_HitBufferList.Add(m_HitBuffer[i]);

            // Loop over the List
            for (int i = 0; i < m_HitBufferList.Count; i++) {
                Vector2 currentNormal = m_HitBufferList[i].normal;
                if (currentNormal.y > minGroundNormalY) {
                    m_IsGrounded = true;

                    if (yMovement) {
                        m_GroundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                // Does our Velocity work?
                float projection = Vector2.Dot(velocity, currentNormal);
                if (projection < 0) {
                    velocity = velocity - projection * currentNormal;
                }

                // Should we Update our Distance?
                float modifiedDistance = m_HitBufferList[i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }

        }
        // Actually set the position of our RigidBody
        rb2d.position = rb2d.position + move.normalized * distance;
    }
}