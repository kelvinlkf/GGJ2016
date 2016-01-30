using UnityEngine;
using System.Collections;

public class GlobeRotation : MonoBehaviour {

    float rotationSpeed = 45f;
    Vector3 directionUp = Vector3.up;

    void Awake()
    {
    }

	void FixedUpdate () {
        transform.Rotate(directionUp * rotationSpeed * Time.deltaTime);
    }
}
