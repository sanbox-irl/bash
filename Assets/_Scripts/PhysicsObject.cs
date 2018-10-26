using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    public float GravityModifier = 1f;

    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D m_ContactFilter2D;
    protected RaycastHit2D[] m_HitBuffer = new RaycastHit2D[16];

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;
   

    private void OnEnable() {
        rb2d = GetComponent<Rigidbody2D>();
        m_ContactFilter2D.useTriggers = false;
        m_ContactFilter2D.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_ContactFilter2D.useLayerMask = true;
    }

    void FixedUpdate() {

        // Core Gravity Movement
        velocity += GravityModifier * Physics2D.gravity * Time.deltaTime;
        Vector2 deltaPos = velocity * Time.deltaTime;
        Vector2 ourMove = Vector2.up * deltaPos.y;

        Movement(ourMove);
    }

    void Movement(Vector2 thisMove) {

        // Check if we Should Move At all
        float distance = thisMove.magnitude;
        if (distance > minMoveDistance) {
            // 
            int numberOfContacts = rb2d.Cast(thisMove,m_ContactFilter2D, m_HitBuffer, distance + shellRadius);


        }
        rb2d.position = rb2d.position + thisMove;
    }
}