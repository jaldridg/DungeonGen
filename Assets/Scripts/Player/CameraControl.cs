using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    GameObject player;
    [SerializeField] float cameraSnapiness = 2.0f;
    [SerializeField] float cursorLimiting = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // Camera will get pulled towards player
        Vector3 cam2D = new Vector3(transform.localPosition.x, 0.0f, transform.localPosition.z);
        Vector3 offsetVector = player.transform.localPosition - cam2D;
        Vector3 offsetVector2D = new Vector3(offsetVector.x, 0.0f, offsetVector.z).normalized;
        Vector3 playerForce = offsetVector2D * offsetVector.magnitude;
    
        // Camera will get pulled by cursor
        offsetVector = player.transform.position - GetMousePos();
        offsetVector2D = new Vector3(-offsetVector.z, 0.0f, offsetVector.x).normalized;
        Vector3 cameraForce = offsetVector2D * offsetVector.magnitude / cursorLimiting;

        // Combine forces
        transform.localPosition += (playerForce + cameraForce) * cameraSnapiness * Time.deltaTime;
    }

    Vector3 GetMousePos()
    {
        Plane p = new Plane(Vector3.up, 0);
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        float d;
        Vector3 mousePos = player.transform.position;
        if (p.Raycast(r, out d))
        {
            mousePos = r.GetPoint(d);
        }
        return mousePos;
    }
}
