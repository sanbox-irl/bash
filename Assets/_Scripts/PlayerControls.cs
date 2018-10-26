using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour {
    [SerializeField] private PlayerBehaviors PlayerBehaviors;
    [SerializeField] private float m_HorizontalMovement = 0f;
    [SerializeField] private bool m_DoJump = false;
    [SerializeField] private float m_RunSpeed = 20f;
    [SerializeField] private int m_AdjustRaw = 1;
    [SerializeField] private int m_AdjustAmplitude = 1;
    [SerializeField] private float m_CoyoteTime = 0.1f;
    [SerializeField] private float m_BufferedJumpTime = 0.1f;

    private bool isCoyoteTime = false;
    public bool queuedJump = false;

    private void Update() {
        float rawHInput = Input.GetAxis("Horizontal");
        // Do our Game Design
        float storeSign = Mathf.Sign(rawHInput);
        rawHInput = Mathf.Pow(Mathf.Abs(rawHInput), m_AdjustRaw) * m_AdjustAmplitude;
        // Return the Sign
        rawHInput *= storeSign;
        m_HorizontalMovement = rawHInput * m_RunSpeed;
        if (Input.GetButtonDown("Jump")) {
            m_DoJump = true;
        };
    }

    private void FixedUpdate() {
        bool didJump = PlayerBehaviors.Move(m_HorizontalMovement * Time.deltaTime, m_DoJump, isCoyoteTime, queuedJump);

        // If we didn't jump, 
        if (!didJump) {
            // If we tried to jump: QUEUE
            if (m_DoJump) {
                queuedJump = true;
                Invoke("EndQueueJump", m_BufferedJumpTime);
            }
        } else {
            if (queuedJump == true) queuedJump = false;
        }
        m_DoJump = false;
    }

    public void StartCoyoteTime() {
        isCoyoteTime = true;
        Invoke("EndCoyoteTime", m_CoyoteTime);
    }

    public void EndCoyoteTime() {
        isCoyoteTime = false;
    }

    public void EndQueueJump() {
        queuedJump = false;
    }
}