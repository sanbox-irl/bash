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
    [SerializeField] private float CamelTime = 0.1f;

    private bool isCoyoteTime = false;

    private void Update() {
        float rawHInput = Input.GetAxis("Horizontal");
        // Do our Game Design
        float storeSign = Mathf.Sign(rawHInput);
        if (storeSign == -1) {

        }
        rawHInput = Mathf.Pow(Mathf.Abs(rawHInput), m_AdjustRaw) * m_AdjustAmplitude;
        // Return the Sign
        rawHInput *= storeSign;
        m_HorizontalMovement = rawHInput * m_RunSpeed;
        if (Input.GetButtonDown("Jump")) {
            m_DoJump = true;
        };
    }

    private void FixedUpdate() {
        PlayerBehaviors.Move(m_HorizontalMovement * Time.deltaTime, m_DoJump, isCoyoteTime);
        m_DoJump = false;
    }

    public void StartCoyoteTime() {
        isCoyoteTime = true;
        Invoke("EndCoyoteTime", CamelTime);
    }

    public void EndCoyoteTime() {
        isCoyoteTime = false;
    }
}