using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PredatorBehavior : Behavior
{
    //any walls nearby? if so move away
    //any prey in sight? if so chase nearest one
    //wander
    public bool targetSighted = false;
    public bool nearWall = false;
    public bool nearPred = false;
    public Vector3 awayFromWall = Vector3.zero;
    public Vector3 awayFromPred = Vector3.zero;
    public float viewRadius = 20f;
    public float viewAngle = 90f;
    public float wallViewRadius = 2f;
    public LayerMask preyMask;
    public LayerMask wallMask;
    public LayerMask predMask;

    Transform target;

    public override void Start()
    {
        StartCoroutine(ContinualCheckFOV());
        base.Start();
    }

    public override MovementState StateUpdate(MovementState movementState)
    {
        //if near a wall, turn to move away from that wall
        if (nearWall)
        {
            movementState = MovementState.AvoidWall;
        }
        //otherwise if a target is visible, hunt it down
        else if (targetSighted)
        {
            movementState = MovementState.Hunt;
        }
        //finally if near a brother, move away from the brother
        else if (nearPred)
        {
            movementState = MovementState.AvoidBrother;
        }
        return base.StateUpdate(movementState);
    }

    private IEnumerator ContinualCheckFOV()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        while (true)
        {
            yield return wait;
            SingleCheckFOV();
        }
    }

    private void SingleCheckFOV()
    {
        Collider[] preyCheck = Physics.OverlapSphere(transform.position, viewRadius, preyMask);
        Collider[] wallCheck = Physics.OverlapSphere(transform.position, wallViewRadius, wallMask);
        Collider[] predCheck = Physics.OverlapSphere(transform.position, wallViewRadius, predMask);

        if (preyCheck.Length != 0)
        {
            //find nearest target
            target = preyCheck[0].transform;
            float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            foreach (Collider agent in preyCheck)
            {
                float distanceToAgent = Vector3.Distance(agent.transform.position, transform.position);
                if (distanceToAgent < distanceToTarget)
                {
                    target = agent.transform;
                    distanceToTarget = distanceToAgent;
                }
            }

            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.right, directionToTarget) < viewAngle / 2)
            {
                targetSighted = !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, wallMask);
            }
            else targetSighted = false;
        }
        else targetSighted = false;

        nearWall = wallCheck.Length > 0;
        if (nearWall)
        {
            //find nearest wall
            Transform nearWall = wallCheck[0].transform;
            float distanceToNearWall = Vector3.Distance(nearWall.transform.position, transform.position);
            foreach (Collider wall in wallCheck)
            {
                float distanceToCurrentWall = Vector3.Distance(wall.transform.position, transform.position);
                if (distanceToCurrentWall < distanceToNearWall)
                {
                    nearWall = wall.transform;
                    distanceToNearWall = distanceToCurrentWall;
                }
            }

            awayFromWall = (transform.position - nearWall.position).normalized;
        }

        nearPred = predCheck.Length > 1;
        if (nearPred)
        {
            //find nearest wall
            Transform nearPred = predCheck[1].transform;
            float distanceToNearPred = Vector3.Distance(nearPred.transform.position, transform.position);
            foreach (Collider pred in predCheck)
            {
                float distanceToCurrentPred = Vector3.Distance(pred.transform.position, transform.position);
                if (distanceToCurrentPred < distanceToNearPred && pred.transform.root != transform)
                {
                    nearPred = pred.transform;
                    distanceToNearPred = distanceToCurrentPred;
                }
            }

            awayFromPred = (transform.position - nearPred.position).normalized;
        }
    }

    public override float Hunt(Vector3 direction)
    {
        //adjust angle to point towards prey
        //get the angle, in degrees, between this.right and the vector from this.position to target.position
        Vector3 targetVector = target.transform.position - transform.position;
        return Vector3.Angle(targetVector, direction);
    }

    public override bool StillVisible()
    {
        return targetSighted;
    }

    public override bool NearWall()
    {
        return nearWall;
    }

    public override bool NearBrother()
    {
        return nearPred;
    }

    public override Vector3 AwayFromWall()
    {
        return awayFromWall;
    }

    public override Vector3 AwayFromBrother()
    {
        return awayFromPred;
    }
}
