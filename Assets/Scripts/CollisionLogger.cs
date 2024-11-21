using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLogger : MonoBehaviour
{
    public float power = 500.0f;
    void Start()
    {
        this.GetComponent<Rigidbody>().AddForce(0, power, 0);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            Debug.Log("Hit the floor");
        }
        
        if (collision.gameObject.name == "Wall")
        {
            Debug.Log("Hit the Wall");
            GameObject.Find("Point light").GetComponent<Light>().enabled = !GameObject.Find("Point light").GetComponent<Light>().enabled;
            GameObject.Find("Point light").GetComponent<Light>().intensity = 5;
            if (!collision.gameObject.GetComponent<Rigidbody>())
            {
                collision.gameObject.AddComponent<Rigidbody>();
            }
        }
    }
}
