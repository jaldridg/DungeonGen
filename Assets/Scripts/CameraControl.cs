using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    GameObject player;
    [SerializeField] float cameraSnapiness = 2.0f;
    Vector3 cameraVelocity;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cam2D = new Vector3(transform.localPosition.x, 0.0f, transform.localPosition.z);
        Vector3 offsetVector = player.transform.localPosition - cam2D;
        Vector3 offsetVector2D = new Vector3(offsetVector.x, 0.0f, offsetVector.z).normalized;
        transform.localPosition += offsetVector2D * offsetVector.magnitude * cameraSnapiness * Time.deltaTime;
    }
}
