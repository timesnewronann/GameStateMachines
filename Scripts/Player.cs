using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script will handle our player's behaviour 
// Player should behave in the following fashion:
// Moving slowly until the speed reaches the fast threshold. While moving slowly the angle of the player can change immmediately and a dive can be initiated.
// Once the speed in a specific direction has increased past the threshold, the player will begin moving quickly.

// Moving quickly the player cannot turn immediately outside of a small threshold. If the player moves the mouse outside that directional range, the player will continue in the original direction but begin slowing down.
// When the player gets below the speed threshold it will drop back to moving slowly.
// Dive when the left mouse button is pressed. Similar to hop this should be a visible but quick movement. This is the only state when Mips can be caught.
// Recovery afterwards where no movement is possilbe, followed by transitioning to the slow move state.

public class Player : MonoBehaviour
{
    // External tunables.
    static public float m_fMaxSpeed = 0.015f; // Max speed player will move at
    public float m_fSlowSpeed = m_fMaxSpeed * 0.66f; // Speed player moves at when they move slowly
    public float m_fIncSpeed = 0.00002f; // How much speed increases as player moves 
    public float m_fMagnitudeFast = 0.6f; // How long it takes to move fast 
    public float m_fMagnitudeSlow = 0.06f; // How long it takes to move slowly
    public float m_fFastRotateSpeed = 0.2f; // Speed when rotating
    public float m_fFastRotateMax = 10.0f; // Max rotation
    public float m_fDiveTime = 0.3f; // Time it takes to dive 
    public float m_fDiveRecoveryTime = 0.5f; // How long it takes to recover from a dive 
    public float m_fDiveDistance = 3.0f; // How far we can dive 

    // Internal variables.
    public Vector3 m_vDiveStartPos; // Start position of a dive 
    public Vector3 m_vDiveEndPos; // End position of a dive 
    public float m_fAngle; // Current player's angle 
    public float m_fSpeed; // Current speed of player 
    public float m_fTargetSpeed; // Current speed of target 
    public float m_fTargetAngle; // Target angle for player 
    public eState m_nState; // current player state 
    public float m_fDiveStartTime; // time when the dive started 

    // Using enum to have the different player state 
    public enum eState : int
    {
        kMoveSlow, // state where player is moving slowly
        kMoveFast, // state where player is moving fast 
        kDiving, // state where player is diving 
        kRecovering, // state where player is recovering
        kNumStates // total number of states 
    }

    // Colors to visualize what state the player is in
    private Color[] stateColors = new Color[(int)eState.kNumStates]
    {
        new Color(0,     0,   0), // black moving slow 
        new Color(255, 255, 255), // white moving fast 
        new Color(0,     0, 255), // blue diving
        new Color(0,   255,   0), // green recovering
    };

    // check if we are diving 
    public bool IsDiving()
    {
        return (m_nState == eState.kDiving);
    }

    void CheckForDive()
    {
        if (Input.GetMouseButton(0) && (m_nState != eState.kDiving && m_nState != eState.kRecovering))
        {
            // Start the dive operation
            m_nState = eState.kDiving;
            m_fSpeed = 0.0f;

            // Store starting parameters.
            m_vDiveStartPos = transform.position;
            m_vDiveEndPos = m_vDiveStartPos - (transform.right * m_fDiveDistance);
            m_fDiveStartTime = Time.time;
        }
    }

    void Start()
    {
        // Initialize variables.
        m_fAngle = 0;
        m_fSpeed = 0;
        m_nState = eState.kMoveSlow;
    }

    void UpdateDirectionAndSpeed()
    {
        // Get relative positions between the mouse and player
        Vector3 vScreenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vScreenSize = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 vOffset = new Vector2(transform.position.x - vScreenPos.x, transform.position.y - vScreenPos.y);

        // Find the target angle being requested.
        m_fTargetAngle = Mathf.Atan2(vOffset.y, vOffset.x) * Mathf.Rad2Deg;

        // Calculate how far away from the player the mouse is.
        float fMouseMagnitude = vOffset.magnitude / vScreenSize.magnitude;

        // Based on distance, calculate the speed the player is requesting.
        if (fMouseMagnitude > m_fMagnitudeFast)
        {
            m_fTargetSpeed = m_fMaxSpeed;
        }
        else if (fMouseMagnitude > m_fMagnitudeSlow)
        {
            m_fTargetSpeed = m_fSlowSpeed;
        }
        else
        {
            m_fTargetSpeed = 0.0f;
        }
    }


    void Update()
    {
        UpdateDirectionAndSpeed();
        // Make new position to be used in all cases
        Vector3 newPos;
        switch (m_nState)
        {
            // state if mario is moving slow 
            // Need to clamp the player similar to the rabbit so it doesn't go out of bounds.
            case eState.kMoveSlow:
                //Calling this function updates m_fTargetAngle and m_fTargetSpeed based on mouse prediction.
                UpdateDirectionAndSpeed(); // Ensure direction and speed are updated 
                
                m_fAngle = m_fTargetAngle; // align the player's angle with the target angle 
                transform.rotation = Quaternion.Euler(0, 0, m_fAngle); // rotate position
                
                // Increase or decrease speed smoothly towards the target speed
                if (m_fSpeed < m_fTargetSpeed) {
                    m_fSpeed += m_fIncSpeed;
                    m_fSpeed = Mathf.Min(m_fSpeed, m_fMaxSpeed); // Cap the speed to the maximum speed
                    //Check our speed is increasing and how much it increases
                    //Debug.Log($"Increasing Speed: New m_fSpeed = {m_fSpeed}");
                } else if (m_fSpeed > m_fTargetSpeed) {
                    m_fSpeed -= m_fIncSpeed;
                    //Check if our speed decreases and how much it decreases
                    //Debug.Log($"Decreasing Speed: New m_fSpeed = {m_fSpeed}");
                }

                // Clamp the speed to the maximum speed
                // m_fSpeed = Mathf.Min(m_fSpeed, m_fMaxSpeed);

                // Move player at current speed
                transform.position  -= (transform.right * m_fSpeed);
                //Debug.Log($"Slow State: m_fSpeed = {m_fSpeed}, m_fTargetSpeed = {m_fTargetSpeed}, m_fSlowSpeed = {m_fSlowSpeed}");
            
            
                // Clamp the player position within the screen bounds
                newPos = Camera.main.WorldToViewportPoint(transform.position);
                newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
                newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
                //Only change the x and y position based on the clamped viewport values
                transform.position = new Vector3(Camera.main.ViewportToWorldPoint(newPos).x,
                                                Camera.main.ViewportToWorldPoint(newPos).y,
                                                transform.position.z); // Keep the original z position
                //transform.position = Camera.main.ViewportToWorldPoint(newPos);


                
                // Transition to fast movement if the speed exceeds the slow speed threshold
                if (m_fSpeed > m_fSlowSpeed) {
                    m_nState = eState.kMoveFast; // Player is moving past the threshold of slow so switch to fast state
                    
                }


                // Check if the player has dived
                CheckForDive(); // use the function 
                break;
            
            // state if mario is moving fast
        case eState.kMoveFast:
            // Update the direction and target speed based on mouse position
            UpdateDirectionAndSpeed();

            // Rotate player towards the target angle, but with a limited rotation speed
            float rotationStep = m_fFastRotateSpeed;
            m_fAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, m_fTargetAngle, rotationStep);
            transform.rotation = Quaternion.Euler(0, 0, m_fAngle);

            // Gradually adjust the player's current speed towards the target speed, but not exceeding the max speed
            if (m_fSpeed < m_fTargetSpeed) {
                m_fSpeed += m_fIncSpeed;
                m_fSpeed = Mathf.Min(m_fSpeed, m_fMaxSpeed);
                // Check our speed is increasing 
            } else if (m_fSpeed > m_fTargetSpeed) {
                m_fSpeed -= m_fIncSpeed;
            }

            // Ensure the player's speed does not drop below the slow speed threshold while in fast state
            m_fSpeed = Mathf.Max(m_fSpeed, m_fSlowSpeed);

            // Move the player in the current facing direction at the current speed
            transform.position  -= (transform.right * m_fSpeed);

            // Clamp the player's position to the screen boundaries to prevent going out of bounds
            newPos = Camera.main.WorldToViewportPoint(transform.position);
            newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
            newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
            transform.position = new Vector3(Camera.main.ViewportToWorldPoint(newPos).x,
                                                Camera.main.ViewportToWorldPoint(newPos).y,
                                                transform.position.z); // Keep the original z position
            //Debug.Log($"Unclamped Position: {transform.position}");

            // If the player's speed drops to the slow threshold, switch to the slow movement state
            if (m_fSpeed <= m_fSlowSpeed) {
                m_nState = eState.kMoveSlow;
                // Debug to see if we go to slow 
                //Debug.Log("Transitioning to Slow State");
            }

            // Check if the player initiates a dive
            CheckForDive();
            //Debug.Log("Moving fast");
            break;

        //  state if mario is diving
        case eState.kDiving:
            // Diving Logic
            // Don't need to call UpdateDirectionAndSpeed because it can mess up diving 

            // Need to rotate the player during the dive 
            // Calculate the dive progress
            float diveProgress = (Time.time - m_fDiveStartTime) / m_fDiveTime;
            if (diveProgress < 1.0f) {
                // Continue diving towards the target position
                transform.position = Vector3.Lerp(m_vDiveStartPos, m_vDiveEndPos, diveProgress);

                // We can rotate the player to face the diving direction if we want to
                Vector2 diveDirection = (m_vDiveEndPos - m_vDiveStartPos).normalized;
                float diveAngle = Mathf.Atan2(diveDirection.y, diveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, diveAngle + 180);

                // Clamp player to prevent player from going out of bounds 
                newPos = Camera.main.WorldToViewportPoint(transform.position);
                newPos.x = Mathf.Clamp(newPos.x, 0.05f, 0.95f);
                newPos.y = Mathf.Clamp(newPos.y, 0.05f, 0.95f);
                transform.position = Camera.main.ViewportToWorldPoint(newPos);
            } else {
                // switch to recovering
                m_nState = eState.kRecovering;
                // Debug to see if we recver
                //Debug.Log("Transitioning to Recovering State");
                m_fDiveStartTime = Time.time; // Reset timer for recovery phase
            }
            break;

            // state if mario is recovering from a dive
        case eState.kRecovering:
            // Ensure the player remains on screen during recovery
            Vector3 newPosRecovery = Camera.main.WorldToViewportPoint(transform.position);
            newPosRecovery.x = Mathf.Clamp(newPosRecovery.x, 0.05f, 0.95f);
            newPosRecovery.y = Mathf.Clamp(newPosRecovery.y, 0.05f, 0.95f);
            transform.position = Camera.main.ViewportToWorldPoint(newPosRecovery);

            // Adjust angle back to rarget angle to make sure the mouse and player are facing the correct direction
            //UpdateDirectionAndSpeed(); // update m_fTargetAngle 
            m_fAngle = m_fTargetAngle; // align player's angle with target angle
            transform.rotation = Quaternion.Euler(0,0, m_fAngle); // rotate it 

            // Transition back to slow movement after recovery time
            if ((Time.time - m_fDiveStartTime) >= m_fDiveRecoveryTime) {
                m_nState = eState.kMoveSlow; // Transition back to slow movement
                //Debug to see if it goes back to slow
                //Debug.Log("Transitioning to Slow State");
            }
            break;
        }

        // Adjusted Dive Check - At the end to prevent overriding state transitions
        if (Input.GetMouseButtonDown(0) && (m_nState == eState.kMoveSlow || m_nState == eState.kMoveFast))
        {
            m_nState = eState.kDiving;
            m_fSpeed = 0.0f;
            m_vDiveStartPos = transform.position;
            // Assuming forward direction for diving - adjust as necessary
            m_vDiveEndPos = transform.position + (transform.forward * m_fDiveDistance);
            m_fDiveStartTime = Time.time;
        }

        GetComponent<Renderer>().material.color = stateColors[(int)m_nState];
    }
}
