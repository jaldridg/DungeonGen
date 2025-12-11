using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float maxSpeed = 4.0f;
    [SerializeField] float acceleration = 6.0f;

    private Vector2 velocityVector = Vector2.zero;
    private Vector2 accelerationVector = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 accel = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            accel += Vector2.left;
        }
        if (Input.GetKey(KeyCode.A))
        {
            accel += Vector2.down;
        }
        if (Input.GetKey(KeyCode.S))
        {
            accel += Vector2.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            accel += Vector2.up;
        }

        accelerationVector = accel.normalized * acceleration;
        if (accelerationVector == Vector2.zero)
        {
            accelerationVector = -velocityVector.normalized * acceleration;
        }
        velocityVector += accelerationVector * Time.deltaTime;
        if (velocityVector.magnitude > maxSpeed)
        {
            velocityVector = velocityVector.normalized * maxSpeed;
        }
        gameObject.transform.position += new Vector3(velocityVector.x, 0.0f, velocityVector.y) * Time.deltaTime;
    }
}
