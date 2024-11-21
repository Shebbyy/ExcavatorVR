using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class MyFirstScript : MonoBehaviour
{

    public GameObject thePrefab;

    public float speed = 5.0f;

    //private bool inAir = false;

    public float jumpHeight = 3.0f;

    private float initHeight;

    private bool falling;
    
    // Start is called before the first frame update
    void Start()
    {
        //Destroy(GameObject.Find("Tower1_A"), 0);

        //Vector3 pos = transform.position;
        
        //for (int i = 0; i < 5; i++)
        //{
        //    pos.x += 2;
        //    
        //    Instantiate(thePrefab, pos, transform.rotation);
        //}
        
        //initHeight = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        //float horiz = Input.GetAxis("Horizontal");
        //float vert = Input.GetAxis("Vertical");
        //transform.Translate(horiz, 0, vert);

        //bool growKey = Input.GetKey(("u"));
        //bool shrinkKey = Input.GetKey(("d"));
        //
        //if (growKey)
        //{
        //    
        //    transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
        //}
        //
        //if (shrinkKey && transform.localScale.x > 0.1f)
        //{
        //    transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
        //}

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(0, 0, -speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        
        // Problem: Physikberechnung nicht optimal; Daher eher AddForce verwenden
        //if (Input.GetKey(KeyCode.UpArrow))
        //{
        //    transform.position += Vector3.forward * Time.deltaTime * speed;
        //}
        //if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    transform.position += Vector3.back * Time.deltaTime * speed;
        //}
        //if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    transform.position += Vector3.right * Time.deltaTime * speed;
        //}
        //if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    transform.position += Vector3.left * Time.deltaTime * speed;
        //}

        //if (   Input.GetKey(KeyCode.Space)
        //    && !inAir)
        //{
        //    inAir = true;
        //    initHeight = transform.position.y;
        //}

        //if (   //inAir && 
        //       !falling
        //    && transform.position.y < initHeight + jumpHeight)
        //{
        //    transform.Translate(0, speed * Time.deltaTime, 0);
//
        //    if (transform.position.y >= initHeight + jumpHeight)
        //    {
        //        falling = true;
        //    }
        //} else if (falling)
        //{
        //    float yDelta = -speed * Time.deltaTime;
        //    if (transform.position.y + yDelta <= initHeight)
        //    {
        //        yDelta = initHeight - transform.position.y;
        //        falling = false;
        //        //inAir   = false;
        //    }
        //    transform.Translate(0, yDelta, 0);
        //}
    }
}
