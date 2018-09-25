using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class Loader : MonoBehaviour
    {

        public GameObject gameManagerPrefab;          //GameManager prefab to instantiate.

        void Awake()
        {
            if (GameManager.instance == null) {
                Instantiate(gameManagerPrefab);
            }
        }

    }

}