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
        public void Start() {

        }
        public void FixedUpdate() {
            frameCount++;
        }
    }
}