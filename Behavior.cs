using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Behavior : MonoBehaviour
{
    Rigidbody body;
    float stateTime;

    const float STEP_DIST = 150f;
    const float STATE_TIME_LOCK = 0.5f;
    const float TURN_AMOUNT = 90f;

    public enum MovementState
    {
        WanderLeft,
        WanderRight,
        WanderStraight,
        Hunt,
        Flee,
        AvoidWall,
        AvoidBrother
    };
    [SerializeField] MovementState moveState;

    // Start is called before the first frame update
    public virtual void Start()
    {
        moveState = MovementState.WanderStraight;
        stateTime = 0f;
        body = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //update state values
        moveState = StateUpdate(moveState); //default Behavior class; movement state should always be one of three Wanders

        //step based on current state
        Step(moveState);
    }

    public virtual MovementState StateUpdate(MovementState movementState)
    {
        MovementState newState = movementState;

        switch (movementState) {
            //if near wall, continue if still near wall
            case MovementState.AvoidWall:
                stateTime = 0;
                if (NearWall()) break; else goto default;

            //if hunting or fleeing, continue if target is still visible
            case MovementState.Hunt:
                stateTime = 0;
                if (StillVisible()) break; else goto default;
            case MovementState.Flee:
                stateTime = 0;
                if (StillVisible()) break; else goto default;

            //if near brother, separate from brother
            case MovementState.AvoidBrother:
                stateTime = 0;
                if (NearBrother()) break; else goto default;

            case MovementState.WanderLeft:
            case MovementState.WanderStraight:
            case MovementState.WanderRight:
            default:
                //check if it's been in current state for awhile
                if (stateTime >= STATE_TIME_LOCK 
                 || movementState == MovementState.AvoidWall 
                 || movementState == MovementState.AvoidBrother
                 || movementState == MovementState.Hunt 
                 || movementState == MovementState.Flee)
                {
                    //select a new wandering state at random
                    float randFloat = Random.value;
                    if (randFloat < 0.34f)
                    {
                        newState = MovementState.WanderLeft;
                    }
                    else if (randFloat < 0.67f)
                    {
                        newState = MovementState.WanderStraight;
                    }
                    else
                    {
                        newState = MovementState.WanderRight;
                    }

                    //reset stateTime
                    stateTime = 0;
                }
                break;
        }
        return newState;
    }
    

    void Step(MovementState movementState)
    {
        //increment counter of time spent in this state
        stateTime += Time.deltaTime;
        float turnSinceLastTick = Time.deltaTime * TURN_AMOUNT;
        float rotateAngle;
        switch (movementState)
        {
            case MovementState.AvoidWall:
                //rotate away from the wall
                rotateAngle = Vector3.Angle(transform.right, AwayFromWall());
                rotateAngle *= (Vector3.Angle(transform.forward, AwayFromWall()) < 90) ? -1 : 1; 
                transform.Rotate(new Vector3(0, rotateAngle / 5f, 0));
                break;

            case MovementState.AvoidBrother:
                //rotate away from brother
                rotateAngle = Vector3.Angle(transform.right, AwayFromBrother());
                rotateAngle *= (Vector3.Angle(transform.forward, AwayFromBrother()) < 90) ? -1 : 1;
                transform.Rotate(new Vector3(0, rotateAngle / 7f, 0));
                break;

            case MovementState.WanderLeft:
                //rotate slightly left
                transform.Rotate(new Vector3(0, turnSinceLastTick, 0));
                break;

            case MovementState.WanderStraight:
                //do not rotate
                break;

            case MovementState.WanderRight:
                //rotate slightly right
                transform.Rotate(new Vector3(0, -1 * turnSinceLastTick, 0));
                break;

            case MovementState.Hunt:
                //call child angle finder
                rotateAngle = Hunt(transform.right);
                rotateAngle *= (Hunt(transform.forward) < 90) ? -1 : 1;
                transform.Rotate(new Vector3(0, rotateAngle / 10f, 0));
                break;

            case MovementState.Flee:
                //call child angle finder
                rotateAngle = Flee(transform.right);
                rotateAngle *= (Flee(transform.forward) < 90) ? -1 : 1;
                transform.Rotate(new Vector3(0, rotateAngle / 5f, 0));
                break;
            
        }
        //step forward 
        body.velocity = STEP_DIST * Time.deltaTime * transform.right;
    }

    //implemented in Predator class
    public virtual float Hunt(Vector3 direction) { return 0f; }

    //implemented in Prey class
    public virtual float Flee(Vector3 direction) { return 0f; }

    //implemented in children classes
    public virtual bool StillVisible() { return false; }
    public virtual bool NearWall() { return false; }
    public virtual bool NearBrother() { return false; }
    public virtual Vector3 AwayFromWall() { return Vector3.zero; }
    public virtual Vector3 AwayFromBrother() { return Vector3.zero; }

}
