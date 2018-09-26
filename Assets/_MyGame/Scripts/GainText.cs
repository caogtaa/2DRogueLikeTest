using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame
{
    public class GainText : MonoBehaviour
    {
        public Text content;

        private void Awake()
        {
            content = GetComponentInChildren<Text>();
        }
    }
}