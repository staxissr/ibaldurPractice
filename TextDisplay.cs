
using System;
using UnityEngine;
using Modding;
using System.IO;
using UnityEngine.UI;

namespace ibaldurPractice
{
    public class TextDisplay
    {
        GameObject textObj;
        static GameObject canvas;
        Vector2 size;
        public bool active;

        public TextDisplay(Vector2 pos, Vector2 size, string text, int fontSize) {
            this.size = size;
            if (!canvas) { CreateCanvas(); }

            textObj = new GameObject();
            textObj.transform.SetParent(canvas.transform, false);
            textObj.AddComponent<CanvasRenderer>();
            RectTransform textTransform = textObj.AddComponent<RectTransform>();
            textTransform.sizeDelta = size;
            

            CanvasGroup group  = textObj.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            Text t = textObj.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.text = text;
            t.fontSize = fontSize;
            t.color = Color.white;
            Vector2 position = new Vector2((pos.x + size.x / 2f) / size.x, (size.y - (pos.y + size.y / 2f)) / size.y);
            textTransform.anchorMin = position;
            textTransform.anchorMax = position;
            GameObject.DontDestroyOnLoad(textObj);
            active = true;
            
        }

        void CreateCanvas() {
            canvas = new GameObject();
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(size.x, size.y);
            canvas.AddComponent<GraphicRaycaster>();
            GameObject.DontDestroyOnLoad(canvas);
        }

        public void UpdateText(string text) {
            textObj.GetComponent<Text>().text = text;
        }
    }

}