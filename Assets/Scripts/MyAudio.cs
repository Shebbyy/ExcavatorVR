using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyAudio : MonoBehaviour
{

    public AudioClip myAudio;

    private AudioSource audioS;
    // Start is called before the first frame update
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        audioS.PlayOneShot(myAudio, 0.7F);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
