using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/RotationObject")]
public class RotationObject : MonoBehaviour
{
    [SerializeField] private Vector3 RotationSpeed = new Vector3();
    [SerializeField] private bool Rigid = true;
    [SerializeField] private bool Gravity = true;

    void Start()
    {
        if (Rigid)
        {
            Rigidbody rigidbody;
            if (GetComponent<Rigidbody>() == null) rigidbody = gameObject.AddComponent<Rigidbody>();
            else rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = Gravity;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        transform.eulerAngles = transform.eulerAngles + RotationSpeed;
    }
}
