using System;
using System.Collections.Generic;
using UnityEngine;

public class GameDebug : MonoBehaviour {

    public static GameDebug Current { get; private set; }

    [SerializeField]
    Rect _windowRect = new Rect(5, 5, 200, 200);

    [SerializeField]
    List<DebugMessage> debugMessages = new List<DebugMessage>();

    [SerializeField]
    Vector2 _scrollBar = Vector2.zero;
    Vector2 _scrollBar2 = Vector2.zero;

    [SerializeField]
    bool _lockedToBottom = true;

    void OnEnable() {
        Current = this;
    }

    void OnGUI() {
        _windowRect = GUI.Window(0, _windowRect, DebugWindow, "Debug Console");

        if(_windowRect.xMax > Screen.width) _windowRect.x = Screen.width - _windowRect.width;
        if(_windowRect.yMax > Screen.height) _windowRect.y = Screen.height - _windowRect.height;

        if(_windowRect.x < 0) _windowRect.x = 0;
        if(_windowRect.y < 0) _windowRect.y = 0;
    }

    public void Add(string message, Func<string> customMessage = null) {
        Add(new DebugMessage(message, customMessage));
    }
    public void Add(DebugMessage message) {
        debugMessages.Add(message);
    }

    public void Clear() {
        debugMessages.Clear();
    }

    void DebugWindow(int id) {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height((_windowRect.height - 55) * .5f));
        {
            _scrollBar2 = GUILayout.BeginScrollView(_scrollBar2);
            {
                foreach(var debugMessage in debugMessages)
                    if(debugMessage.customMessage != null)
                        GUILayout.Label(debugMessage.customMessage(), GUI.skin.box);
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height((_windowRect.height - 55) * .5f));
        {
            _scrollBar = GUILayout.BeginScrollView(_scrollBar);
            {
                foreach(var debugMessage in debugMessages)
                    if(debugMessage.customMessage == null)
                        GUILayout.Label(debugMessage.message, GUI.skin.box);
            }
            GUILayout.EndScrollView();
        }
        GUILayout.EndVertical();

        _lockedToBottom = GUILayout.Toggle(_lockedToBottom, "Auto Scroll");

        if(_lockedToBottom)
            _scrollBar.y = Mathf.Infinity;

        GUI.DragWindow();
    }

    [System.Serializable]
    public struct DebugMessage {
        public string message;
        public Func<string> customMessage;

        public DebugMessage(string message, Func<string> customMessage) {
            this.message = message;
            this.customMessage = customMessage;
        }
    }
}