﻿using UnityEngine;
using System.Collections;
using System.IO;
public class GoldDrop : MonoBehaviour
{

    // Use this for initialization
    public GameObject Theplayer;
    public GameObject SackofGOld;
    GameObject FloatingBag;
    public Canvas pauseMenu;
    public GameObject GoldDropPosition;
    public GameObject RespawnPosition;
    public int Golddrop;
    public int DrppedGold;
    //File SaveGold;
    StreamWriter SaveGold;
    StreamReader LoadGold;
    Vector3 Position;
    void Start()
    {
        Theplayer = GameObject.Find("Player");
        // Theplayer = GameObject.FindGameObjectWithTag("Player");
      
    }

    // Update is called once per frame
    void Update()
    {

        if (Theplayer.GetComponent<PlayerHealth>().CurHealth <= 0)
        {

          
            Golddrop = Theplayer.GetComponent<PlayerGold>().Gold;
            Position = transform.position;
            if (File.Exists("DeathGold.txt"))
                File.Delete("DeathGold.txt");

            if (File.Exists("Location.txt"))
                File.Delete("Location.txt");

            File.WriteAllText("DeathGold.txt", Golddrop.ToString());
            File.WriteAllText("Location.txt", Position.ToString());
         
            
            Application.LoadLevel(0);
        }

    }









void OnTriggerEnter(Collider other)
{
    Theplayer = GameObject.FindGameObjectWithTag("Player");
    if (other.tag == "Player")
    {
        Debug.Log("I HIT HERE ");
        LoadGold = new StreamReader("DeathGold.txt", true);
        string line = LoadGold.ReadLine();
        LoadGold.Close();

        Debug.Log(line);
        int temp = System.Int32.Parse(line);
        Debug.Log(temp);
        Theplayer.GetComponent<PlayerGold>().Gold += temp;
        Debug.Log(Theplayer.GetComponent<PlayerGold>().Gold);
        this.enabled = false;
        Destroy(this);
        SackofGOld.SetActive(false);


    }



}




    }