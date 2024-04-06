using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script will handle the transitions between states
// We will use this script for idle, hopping, and catching the bunny
// Color will change depending on the state 

public class Target : MonoBehaviour
{
    public Player m_player;
    public enum eState : int // We will use enumeration to define the different states 
    {
        // Idle state 
        kIdle,

        // Start hopping state
        kHopStart,

        // Currently hopping state
        kHop,

        // Caught by player state
        kCaught,

        // Total number of states 
        kNumStates
    }


    // Colors for the different states 
    // Will indicate what state we are in
    private Color[] stateColors = new Color[(int)eState.kNumStates]
   {
        new Color(255, 0,   0), // red for idle
        new Color(0,   255, 0), // green for start hopping
        new Color(0,   0,   255), // blue for hopping 
        new Color(255, 255, 255) // white for caught 
   };

    // External tunables.
    public float m_fHopTime = 0.2f; // how long it takes to hop
    public float m_fHopSpeed = 6.0f; // hop speed
    public float m_fScaredDistance = 3.0f; // how far the player can be from the target before it is scared
    public int m_nMaxMoveAttempts = 50; // max attempts to move 

    // Internal variables.
    public eState m_nState; // current state 
    public float m_fHopStart; // when the hop starts
    public Vector3 m_vHopStartPos; // where the hop starts
    public Vector3 m_vHopEndPos; // where the position ends 

    void Start()
    {
        // Setup the initial state and get the player GO.
        m_nState = eState.kIdle;
        m_player = GameObject.FindObjectOfType(typeof(Player)) as Player;
    }

    void Update()
    {
        switch (m_nState)
        {
            // state if rabbit is idle
            case eState.kIdle:
                // See how far the rabbit is to the player
                // Use .Distance from Vector3
                // Check if the player distance is less than the rabbits scared distance

                // Professor's idle state
                if (Vector3.Distance(transform.position, m_player.transform.position) < m_fScaredDistance) {
                    // Start rabbits hopping, transition between states
                    m_nState = eState.kHopStart;
                }
                break;

            // state if rabbit starts hopping
            case eState.kHopStart:
                // Professor Fodor's implementation
                m_fHopStart = Time.time;
                bool bIsOffScreen; // bool to check if MIPS is offscreen
                bool bIsUnSafe; // Check if MIPS is safe or not
                int numberOfAttempts = 0; // counter for how many attempts MIPS has moved
                Quaternion originalRotation = transform.rotation;

                do {
                    do {
                        // calculate the angle the bunny will spin from
                        float fNewAngle = Random.Range(0,360);

                        // set rotation
                        transform.rotation = Quaternion.Euler(0,0, fNewAngle);

                        // set ending hop position
                        m_vHopEndPos = transform.position + (transform.up * m_fHopTime *m_fHopSpeed);

                        // Set the screen posotion so bunny doesn't go off screen
                        var vScreenPos = Camera.main.WorldToViewportPoint(m_vHopEndPos);
                        // fixed viewport issue 
                        bIsOffScreen = (vScreenPos.x <= 0 || vScreenPos.x >= 1 || vScreenPos.y <= 0 | vScreenPos.y >= 1);

                    } while (bIsOffScreen);


                        bIsUnSafe = Vector3.Distance(m_vHopEndPos, m_player.transform.position) < m_fScaredDistance;
                        numberOfAttempts += 1;
                    } while (bIsUnSafe && (numberOfAttempts < m_nMaxMoveAttempts));
                

                // if attempts is 50
                if (false) {
                    transform.rotation = originalRotation;
                    m_nState = eState.kIdle;
                }
                else {
                    // switch states to hop
                    m_vHopStartPos = transform.position;
                    m_nState = eState.kHop;
                }

                break;


                // OLD CODE 

                // // Determine hop direction away from the player
                // Vector3 direction = (transform.position - m_player.transform.position).normalized;

                // // Make the bunny face the hop direction
                // // Calculate angle towards hop direction
                // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // // Adjust the angle for the bunny's orientation if needed
                // transform.rotation = Quaternion.Euler(0, 0, angle);

                // // Calculate a tentative end position before clamping
                // Vector3 tentativeEndPos = transform.position + direction * m_fHopSpeed;

                // // Convert tentative end position to viewport coordinates
                // Vector3 viewportPoint = Camera.main.WorldToViewportPoint(tentativeEndPos);

                // // Clamp the viewportPoint to ensure it's within the screen bounds
                // viewportPoint.x = Mathf.Clamp(viewportPoint.x, 0.05f, 0.95f); // Prevent hopping off-screen horizontally
                // viewportPoint.y = Mathf.Clamp(viewportPoint.y, 0.05f, 0.95f); // Prevent hopping off-screen vertically

                // // Convert the clamped viewport coordinates back to world coordinates
                // Vector3 clampedWorldPosition = Camera.main.ViewportToWorldPoint(viewportPoint);
                // clampedWorldPosition.z = transform.position.z; // Maintain the original z position

                // // Set the hop start position to the current position
                // m_vHopStartPos = transform.position;

                // // Update the end position for the hop to the clamped position
                // m_vHopEndPos = clampedWorldPosition;

                // // Record the start time for the hop
                // m_fHopStart = Time.time;

                // // Change state to hopping
                // m_nState = eState.kHop;
                // break;

            
            case eState.kHop:
                    // From professor fodor 
                    // Calculate how far through the hop we are, as a fraction.
                    float fHopFraction = (Time.time - m_fHopStart) / m_fHopTime;
                    // Clamp the fraction to ensure it doesn't exceed 1.
                    fHopFraction = Mathf.Clamp(fHopFraction, 0.0f, 1.0f);
                    // Move the entity along the hop path.
                    transform.position = Vector3.Lerp(m_vHopStartPos, m_vHopEndPos, fHopFraction);

                    // Check if the hop is complete.
                    if (fHopFraction == 1.0f)
                    {
                        // If so, return to idle state.
                        m_nState = eState.kIdle;
                    }
                break;

                // OLD CODE
                // // Rabbit hops, interpolate position between start and end 
                // float timeSinceStart = (Time.time - m_fHopStart) / m_fHopTime;


                // // If < 100% of hop time passed, interpolate position of start and end 
                // if (timeSinceStart < 1.0f) {
                //     transform.position = Vector3.Lerp(m_vHopStartPos, m_vHopEndPos, timeSinceStart);
                // }
                // else {
                //     // after hop is completed, go back to idle state
                //     m_nState = eState.kIdle;
                // }
                // break;

            case eState.kCaught:
                // case is handled on OnTriggerStay2D
                break;

        }
        // update color depending on the current state, helps us visualize what state we're in
        GetComponent<Renderer>().material.color = stateColors[(int)m_nState];
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // Check if this is the player (in this situation it should be!)
        if (collision.gameObject == GameObject.Find("Player"))
        {
            // If the player is diving, it's a catch!
            if (m_player.IsDiving())
            {
                m_nState = eState.kCaught;
                transform.parent = m_player.transform;
                transform.localPosition = new Vector3(0.0f, -0.5f, 0.0f);
            }
        }
    }
}