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
using System.IO;

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
        public int lastDelayedAttack = 0;
        public bool shadeYWrong = false;
        public bool recentSuccess = false;
        public FrameCounter frameCounter;
        const string logPath = "ibaldurPracticeLog.csv";
        public string logText = "";
        TextDisplay textDisplay;
        internal static ibaldurPractice Instance { get; private set; }

        public ibaldurPractice() { Instance = this; }

        public override string GetVersion()
        {
            return "1.1";
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
            textDisplay = new TextDisplay(new Vector2(Screen.width - 300, 10), new Vector2(Screen.width, Screen.height), "", 20);

            if (!File.Exists(logPath)) {
                var fs = new FileStream(logPath, FileMode.Create);
                fs.Dispose();
                File.WriteAllText(logPath, "Succeeded,Left range,Hit pos,Slash delay,wiggle duration,earliest possible reentry,RNG late attack,Y pos late attack,fps" + Environment.NewLine);

            }
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
            frameCounter.StartCoroutine(LogAttempt());
            DisplayAttemptData();
        }

        public IEnumerator LogAttempt() {
            for (int i = 0; i < 5; i++ ) {
                yield return new WaitForFixedUpdate();
            }
            string attemptString = (recentSuccess ? "Succeeded," : "Failed, ") + logText + Environment.NewLine;
            File.AppendAllText(logPath, attemptString);
        }

        public void DisplayAttemptData() {
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
            newText += $"Hit Pos {hitPos:f2}\n";
            newText += "Slash delay: " + (slash - damage) + "\n";
            newText += "Wiggle duration: " + (enteredBaldur1 - leftBaldur1) + "\n";
            newText += "Earliest possible reentry: " + (baldurOpens2 - enteredBaldur2 > 0 ? "+" : "") + (baldurOpens2 - enteredBaldur2) + "\n";
            newText += "RNG Late attack: " + (shadeAttackStart - shadeWaiting == 0 ? "Yes" : "No") + "\n";
            newText += "Y pos Late attack: " + (shadeAttackStart - lastDelayedAttack < 3 ? "Yes" : "No") + "\n";
            textDisplay.UpdateText(newText);

            recentSuccess = false;
            logText = $"{leftBaldur1 - shadeAttackStart},{hitPos:f2},{slash - damage},{enteredBaldur1 - leftBaldur1},{(baldurOpens2 - enteredBaldur2 > 0 ? "+" : "") + (baldurOpens2 - enteredBaldur2)},{(shadeAttackStart - shadeWaiting == 0 ? "Yes" : "No")},{(shadeAttackStart - lastDelayedAttack < 3 ? "Yes" : "No")},{frameCounter.GetFPS():f0}";
        }
        
        public int PlayerIntHook(string name) {
            
            if (name == "shadeSpecialType" && GameManager.instance.sceneName == "Crossroads_11_alt") {
                foreach (PlayMakerFSM fsm in PlayMakerFSM.FsmList) {
                    if (fsm.gameObject.name == "Hollow Shade(Clone)" && fsm.FsmName == "Shade Control") {
                        InsertAction(fsm, "Position", new InvokeMethod(() => {
                            shadeWaiting = frameCounter.frameCount;
                        }), 0);

                        InsertAction(fsm, "Position", new InvokeMethod(() => {
                            if (fsm.FsmVariables.FindFsmBool("In Slash Range").Value && !fsm.FsmVariables.FindFsmBool("Same Y").Value) {
                                shadeYWrong = true;
                                lastDelayedAttack = frameCounter.frameCount;
                            }
                        }, everyFrame: true));

                        InsertAction(fsm, "Slash Antic", new InvokeMethod(() => {
                            shadeAttackStart = frameCounter.frameCount;
                        }));

                        
                    } else if (fsm.gameObject.name == "Attack Range" && fsm.FsmName == "Attack Range Detect") {
                        InsertAction(fsm, "In Range", new InvokeMethod(() => {
                        
                            if (enteredRange.Count == 0 || frameCounter.frameCount - enteredRange[enteredRange.Count - 1] < 30) {
                                shadeYWrong = false;
                            }
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
                        InsertAction(fsm, "Hit Pause", new InvokeMethod(() => {
                            recentSuccess = true;
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
        private readonly bool _everyFrame;

        public InvokeMethod(Action a, bool everyFrame = false)
        {
            _action = a;
            _everyFrame = everyFrame;
        }
        
        public override void OnEnter()
        {
            _action?.Invoke();
            if (!_everyFrame) {
                Finish();
            }
            
        }

        public override void OnFixedUpdate()
        {
            _action?.Invoke();
        }
    }

}