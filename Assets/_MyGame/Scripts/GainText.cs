using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame
{
    public class GainText : MonoBehaviour
    {
        public Text content;

        void Awake()
        {
            content = GetComponentInChildren<Text>();
        }

        private void Start()
        {
            Animator animator = content.GetComponent<Animator>();
            AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
            Destroy(gameObject, clipInfos[0].clip.length);
        }
    }
}