using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    public AudioSource soundToPlay;
    public int groundLayerNum =8;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != groundLayerNum){
            soundToPlay.Stop();
            soundToPlay.pitch = Random.Range(0.1f , 1.2f);

            soundToPlay.Play();
        }
    }
}
