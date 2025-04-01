using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;
using HutongGames.PlayMaker.Actions;
using System.Linq;

namespace ibaldurPractice
{

    public class FrameCounter : MonoBehaviour
    {
        public int frameCount;
        List<float> fpsTracker = new List<float>();
        public void Start() {

        }
        public void FixedUpdate() {
            frameCount++;
        }

        public void Update() {
            fpsTracker.Add(1f/Time.unscaledDeltaTime);
            if (fpsTracker.Count > 10) {
                fpsTracker.RemoveAt(0);
            }
        }

        public float GetFPS() {
            return fpsTracker.Average();
        }
    }
}