using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UObject = UnityEngine.Object;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker;
using System.Linq;
using GlobalEnums;

namespace ibaldurPractice
{

    
    public class ibaldurPractice : Mod
    {

        public int shadeAttackStart = 0;
        public List<int> exitedRange = new List<int>();
        public List<int> enteredRange = new List<int>();
        public List<int> baldurOpens = new List<int>();
        public List<int> baldurCloses = new List<int>();
        public float hitPos;
        public int damage = 0;
        public int slash = 0;
        public int shadeWaiting = 0;
        FrameCounter frameCounter;
        TextDisplay textDisplay;
        internal static ibaldurPractice Instance { get; private set; }

        public ibaldurPractice() { Instance = this; }

        public override string GetVersion()
        {
            return "1.0";
        }

        public override void Initialize()
        {
            Log("Initializing ibaldurPractice");
            Instance = this;
            // ModHooks.Instance.SceneChanged += SceneLoaded;
            ModHooks.Instance.SceneChanged += (string sceneName) => {
                if (sceneName != "Crossroads_11_alt") { textDisplay.UpdateText(""); };
            };
            ModHooks.Instance.AttackHook += (AttackDirection dir) => {
                if (frameCounter.frameCount - slash >= 200 && frameCounter.frameCount - damage < 50) {
                    slash = frameCounter.frameCount;
                    SearchForAttempt();
                }
            };
            ModHooks.Instance.AfterTakeDamageHook += (int i1, int i2) => {
                damage = frameCounter.frameCount;
                hitPos = HeroController.instance.transform.position.x;
                return i2;
            };
            ModHooks.Instance.GetPlayerIntHook += PlayerIntHook;
            
            GameObject frameCounterGO = new GameObject("FrameCounter");
            frameCounter = frameCounterGO.AddComponent<FrameCounter>();
            GameObject.DontDestroyOnLoad(frameCounterGO);
            textDisplay = new TextDisplay(new Vector2(Screen.width - 300, 10), new Vector2(1920, 1080), "", 20);
            Log("Initialized");
        }

        public void SearchForAttempt() {
            if (exitedRange.Count < 2 || enteredRange.Count < 2) {
                return;
            }
            int cur_frame = frameCounter.frameCount;
            if (cur_frame - exitedRange[exitedRange.Count - 1] > 500 || cur_frame - enteredRange[enteredRange.Count - 2] > 500 || cur_frame - shadeAttackStart > 500) {
                return;
            }
            DisplayData();
        }

        public void DisplayData() {
            string newText = "";
            int leftBaldur1 = exitedRange[exitedRange.Count-2];
            int enteredBaldur1 = enteredRange[enteredRange.Count-2];
            int leftBaldur2 = exitedRange[exitedRange.Count-1];
            int enteredBaldur2 = enteredRange[enteredRange.Count-1];
            int baldurOpens1 = baldurOpens[baldurOpens.Count - 2];
            int baldurCloses1 = baldurCloses[baldurCloses.Count-1];
            int baldurOpens2 = baldurCloses1 + 20;
            newText += "Left range: t" + (leftBaldur1 - shadeAttackStart) + "\n";
            newText += "Shade attack: t-0 \n";
            // newText += "Entered range: t+" + (enteredBaldur1 - shadeAttackStart) + "\n";
            // newText += "Re-left range: t+" + (leftBaldur2 - shadeAttackStart) + "\n";
            // newText += "Got hit: t+" + (damage - shadeAttackStart) + "\n";
            newText += $"Hit Pos {hitPos:f2}\n";
            // newText += "Re-entered range: t+" + (enteredBaldur2 - shadeAttackStart) + "\n";
            newText += "Slash delay: " + (slash - damage) + "\n";
            newText += "Wiggle duration: " + (enteredBaldur1 - leftBaldur1) + "\n";
            newText += "Earliest possible reentry: " + (baldurOpens2 - enteredBaldur2 > 0 ? "+" : "") + (baldurOpens2 - enteredBaldur2) + "\n";
            newText += "Late Shade attack: " + (shadeAttackStart - shadeWaiting == 0 ? "Yes" : "No") + "\n";
            // newText += "Baldur Close time: t+" + (baldurCloses[baldurCloses.Count -1] - shadeAttackStart) + "\n";
            // newText += "Baldur Open time: t+" + (baldurOpens[baldurOpens.Count -1] - shadeAttackStart) + "\n";
            // newText += "Opened1: t+" + (baldurOpens[baldurOpens.Count -2] - shadeAttackStart) + "\n";
            // newText += "Closed1: t+" + (baldurCloses[baldurCloses.Count -2] - shadeAttackStart) + "\n";
            // newText += "Opened2: t+" + (baldurOpens[baldurOpens.Count -1] - shadeAttackStart) + "\n";
            // newText += "Closed2: t+" + (baldurCloses[baldurCloses.Count -1] - shadeAttackStart) + "\n";
            // newText += "opened1rel: t+" + (baldurOpens[baldurOpens.Count -2] - leftBaldur1) + "\n";
            // newText += "closed1rel: t+" + (baldurCloses[baldurCloses.Count -2] - leftBaldur1) + "\n";
            // newText += "opened2rel: t+" + (baldurOpens[baldurOpens.Count -1] - leftBaldur1) + "\n";
            // newText += "closed2rel: t+" + (baldurCloses[baldurCloses.Count -1] - leftBaldur1) + "\n";
            textDisplay.UpdateText(newText);
        }
        
        public int PlayerIntHook(string name) {
            
            if (name == "shadeSpecialType" && GameManager.instance.sceneName == "Crossroads_11_alt") {
                foreach (PlayMakerFSM fsm in PlayMakerFSM.FsmList) {
                    if (fsm.gameObject.name == "Hollow Shade(Clone)" && fsm.FsmName == "Shade Control") {
                        InsertAction(fsm, "Position", new InvokeMethod(() => {
                            shadeWaiting = frameCounter.frameCount;
                        }), 0);

                        InsertAction(fsm, "Slash Antic", new InvokeMethod(() => {
                            shadeAttackStart = frameCounter.frameCount;
                        }));

                        

                        // makes shade attack asap
                        // foreach (FsmState t in fsm.FsmStates)
                        // {
                        //     if (t.Name != "Fly") continue;
                        //     (t.Actions[5] as WaitRandom).timeMax = 1f;
                        //     break;
                        // }
                        
                    } else if (fsm.gameObject.name == "Attack Range" && fsm.FsmName == "Attack Range Detect") {
                        InsertAction(fsm, "In Range", new InvokeMethod(() => {
                            enteredRange.Add(frameCounter.frameCount);
                        }));
                        InsertAction(fsm, "Out of Range", new InvokeMethod(() => {
                            exitedRange.Add(frameCounter.frameCount);
                        }));
                    } else if (fsm.gameObject.name == "Blocker" && fsm.FsmName == "Blocker Control") {
                        InsertAction(fsm, "Close", new InvokeMethod(() => {
                            baldurCloses.Add(frameCounter.frameCount);
                        }));
                        InsertAction(fsm, "Open", new InvokeMethod(() => {
                            baldurOpens.Add(frameCounter.frameCount);
                        }));
                    }
                }

            } 
            return PlayerData.instance.GetIntInternal(name);
        }



        // shamelessly stolen from https://github.com/Kerr1291/ModCommon/blob/master/ModCommon/Util/FsmUtil.cs
        public void InsertAction(PlayMakerFSM fsm, string stateName, FsmStateAction action, int index = -1)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;
                if (index == -1) {
                    Array.Resize(ref actions, actions.Length + 1);
                    actions[actions.Length - 1] = action;
                } else {
                    List<FsmStateAction> newActions = actions.ToList();
                    newActions.Insert(index, action);
                    actions = newActions.ToArray();
                }

                t.Actions = actions;
                break;
            }
        }

    }

    public class InvokeMethod : FsmStateAction
    {
        private readonly Action _action;

        public InvokeMethod(Action a)
        {
            _action = a;
        }
        
        public override void OnEnter()
        {
            _action?.Invoke();
            Finish();
        }
    }

}