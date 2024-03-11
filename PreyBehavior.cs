using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PreyBehavior : Behavior
{
    //any walls nearby? if so move away
    //any preds in sight? if so run from nearest one
    //wander
    public bool predatorSighted = false;
    public bool nearWall = false;
    public bool nearPrey = false;
    public Vector3 awayFromWall = Vector3.zero;
    public Vector3 awayFromPrey = Vector3.zero;
    public float viewRadius = 2.5f;
    public float wallViewRadius = 2f;
    public GameObject[] respawnPoints;
    public LayerMask predatorMask;
    public LayerMask wallMask;
    public LayerMask preyMask;

    Transform seenPredator;

    public override void Start()
    {
        StartCoroutine(ContinualCheckFOV());
        base.Start();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("Predator"))
        {
            GameObject respawnPoint = respawnPoints[Mathf.FloorToInt(respawnPoints.Length * Random.value)];
            gameObject.transform.position = new Vector3(respawnPoint.transform.position.x, 0.5f, respawnPoint.transform.position.z);
        }
    }

    public override MovementState StateUpdate(MovementState movementState)
    {
        //if near a wall, turn to move away from that wall
        if (nearWall)
        {
            movementState = MovementState.AvoidWall;
        }
        //otherwise if a target is visible, hunt it down
        else if (predatorSighted)
        {
            movementState = MovementState.Flee;
        }
        //finally if near a brother, move away from the brother
        else if (nearPrey)
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
        Collider[] predatorCheck = Physics.OverlapSphere(transform.position, viewRadius, predatorMask);
        Collider[] wallCheck = Physics.OverlapSphere(transform.position, wallViewRadius, wallMask);
        Collider[] preyCheck = Physics.OverlapSphere(transform.position, wallViewRadius, preyMask);

        if (predatorCheck.Length != 0)
        {
            //find nearest pred
            seenPredator = predatorCheck[0].transform;
            float distanceToPred = Vector3.Distance(seenPredator.transform.position, transform.position);
            foreach (Collider agent in predatorCheck)
            {
                float distanceToAgent = Vector3.Distance(agent.transform.position, transform.position);
                if (distanceToAgent < distanceToPred)
                {
                    seenPredator = agent.transform;
                    distanceToPred = distanceToAgent;
                }
            }

            Vector3 directionToPred = (seenPredator.position - transform.position).normalized;
            predatorSighted = !Physics.Raycast(transform.position, directionToPred, distanceToPred, wallMask);
        }
        else predatorSighted = false;

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

        nearPrey = preyCheck.Length > 1;
        if (nearPrey)
        {
            //find nearest prey
            Transform nearPrey = preyCheck[1].transform;
            float distanceToNearPrey = Vector3.Distance(nearPrey.transform.position, transform.position);
            foreach (Collider prey in preyCheck)
            {
                float distanceToCurrentPrey = Vector3.Distance(prey.transform.position, transform.position);
                if (distanceToCurrentPrey < distanceToNearPrey && prey.transform.root != transform)
                {
                    nearPrey = prey.transform;
                    distanceToNearPrey = distanceToCurrentPrey;
                }
            }

            awayFromPrey = (transform.position - nearPrey.position).normalized;
        }
    }

    public override float Flee(Vector3 direction)
    {
        //adjust angle to point away from predator
        //get the angle, in degrees, between this.right and the vector from this.position to target.position
        Vector3 targetVector = transform.position - seenPredator.transform.position;
        return Vector3.Angle(targetVector, direction);
    }

    public override bool StillVisible()
    {
        return predatorSighted;
    }

    public override bool NearWall()
    {
        return nearWall;
    }

    public override bool NearBrother()
    {
        return nearPrey;
    }

    public override Vector3 AwayFromWall()
    {
        return awayFromWall;
    }

    public override Vector3 AwayFromBrother()
    {
        return awayFromPrey;
    }
}
