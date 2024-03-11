using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallSpin : MonoBehaviour
{
    public bool xMovement;
    public bool zMovement;
    float gameTime;
    float radius = 3f;
    Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        gameTime = 0f;
        startPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameTime += Time.deltaTime;

        transform.position = new Vector3(startPosition.x + Mathf.Cos(gameTime) * radius * (xMovement ? 1 : 0), 
                                         0.5f, 
                                         startPosition.z + Mathf.Sin(gameTime) * radius * (zMovement ? 1 : 0));
    }
}
