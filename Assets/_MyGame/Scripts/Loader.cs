using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class Loader : MonoBehaviour
    {

        public GameObject gameManagerPrefab;          //GameManager prefab to instantiate.
        public GameObject soundManager;         //SoundManager prefab to instantiate.

        void Awake()
        {
            if (GameManager.instance == null) {
                Instantiate(gameManagerPrefab);
            }

            //Check if a SoundManager has already been assigned to static variable GameManager.instance or if it's still null
            /*if (SoundManager.instance == null)
            {
                //Instantiate SoundManager prefab
                Instantiate(soundManager);
            }*/
        }

    }

}