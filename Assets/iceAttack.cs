﻿using UnityEngine;
using System.Collections;

public class iceAttack : MonoBehaviour
{

    // Use this for initialization
    public Renderer myRenderer;
    float timer = 1.5f;
    public GameObject[] spikes;
    public BoxCollider myCollider;
    bool activated = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        myRenderer.material.color = Color.Lerp(Color.red, Color.white, timer);
        timer -= Time.deltaTime;
        if (timer <= 0 && !activated)
        {
            foreach (GameObject spike in spikes)
            {
                if (spike)
                    spike.SetActive(true);
            }
            myCollider.enabled = true;
            Destroy(gameObject, 0.5f);
        }
    }
}