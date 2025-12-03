using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    GameObject player;
    [SerializeField] float cameraSnapiness = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cam2D = new Vector3(transform.position.x, 0.0f, transform.position.z);
        Vector3 offsetVector = player.transform.position - cam2D;
        Vector3 offsetVector2D = new Vector3(offsetVector.z, 0.0f, -offsetVector.x).normalized;
        transform.localPosition += offsetVector2D * offsetVector.magnitude * cameraSnapiness * Time.deltaTime;
    }
}
