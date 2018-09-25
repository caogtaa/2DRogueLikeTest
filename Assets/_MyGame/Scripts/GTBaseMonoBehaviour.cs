using System;
using UnityEngine;
using System.Collections;

namespace MyGame
{
    public class GTBaseMonoBehaviour : MonoBehaviour
    {
        public delegate void CallbackDelegate();

        public delegate bool ActionDelegate();

        public delegate void HitCallback<T>(T t);

        public IEnumerator CombineCoroutineWithCallback(IEnumerator routine, CallbackDelegate callback)
        {
            yield return StartCoroutine(routine);
            if (callback != null)
                callback();
        }
    }
}
