using UnityEngine;
using System.Collections;

public class meteorRotation : MonoBehaviour {

    float rotationSpeed = 15f;
    Vector3 directionUp = Vector3.up;

    void FixedUpdate()
    {
        transform.Rotate(directionUp * rotationSpeed * Time.deltaTime);
    }
}
