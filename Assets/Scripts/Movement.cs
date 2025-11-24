using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 4.0f;

    private Vector2 velocity = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movementVector = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            movementVector += Vector2.left;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movementVector += Vector2.down;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movementVector += Vector2.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movementVector += Vector2.up;
        }
        velocity = movementVector.normalized * moveSpeed * Time.deltaTime;
        gameObject.transform.position += new Vector3(velocity.x, 0.0f, velocity.y);
    }
}
