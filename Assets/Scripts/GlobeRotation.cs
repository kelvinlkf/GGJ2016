using UnityEngine;
using System.Collections;

public class GlobeRotation : MonoBehaviour {

    float rotationSpeed = 15f;
    Vector3 directionUp = Vector3.up;
    Vector3 directionRight = Vector3.right;

	void FixedUpdate () {
        transform.Rotate(directionUp * rotationSpeed * Time.deltaTime);
        transform.Rotate(directionRight * rotationSpeed * Time.deltaTime);
    }
}
