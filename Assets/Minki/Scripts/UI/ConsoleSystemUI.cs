using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SPTr.CustomCollections;
using SPTr.DeveloperConsole;
using System;
using Unity.VisualScripting;
using System.Collections;
using System.IO;
using SPTr.CMD;
using System.Text.RegularExpressions;



#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SPTr.UI
{
    public class ConsoleSystemUI : MonoBehaviour
    {
        public static ConsoleSystemUI instance;

        public RectTransform ConsoleWindow;
        public InputField TextField;
        public InputField ConsoleLog;
        public Text SuggestionTexts;
        public Text CurrentSuggestionText;
        public Image SuggestionsPanel;
        public TrieTree SuggestionsTree;

        private const int MAX_SUGGETIONS_COUNT = 15;

        private const string COLOR_ERROR = "#F78181";
        private const string COLOR_INFO = "#F7BE81";
        private const string COLOR_VALUE = "#F5DA81";

        private List<string> _lastInputText = new List<string>();
        private List<string> _suggestions = new List<string>();
        private StringBuilder _sb = new StringBuilder();

        public static ConsoleCommand help = new ConsoleCommand("help",
            (string args) =>
            {
                var split = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (split.Length > 1)
                {
                    Debug.Log($"<color={COLOR_ERROR}>인자는 하나만 입력해주세요</color>");
                    return;
                }

                var cmd = DevConsole.FindCommand(split[0]);
                if (cmd != null)
                {
                    if (cmd.Description == null || cmd.Description.Length > 1)
                    {
                        Debug.Log($"<color={COLOR_ERROR}>명령어의 설명이 존재하지 않습니다.</color>");
                        return;
                    }

                    Debug.Log($"<color={COLOR_INFO}>{(cmd.Description[0] != '/' ? cmd.Description : cmd.Description[1..])}</color>");
                }
                else
                {
                    Debug.Log($"<color={COLOR_ERROR}>해당하는 이름의 콘솔 명령어가 존재하지 않습니다.</color>");
                }
            }, "콘솔 명령어의 설명을 출력합니다. \n- help <콘솔 명령어>");

        public static ConsoleCommand clear = new ConsoleCommand("clear", () => { instance.ConsoleLog.text = ""; instance._lastInputText.Clear(); },
            "/콘솔 로그를 전부 지웁니다.");

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);

                instance.SuggestionsTree = new TrieTree();

                DevConsole.OnCmdAdded += AddToSuggestionTree;
                DevConsole.OnCmdRemoved += RemoveToSuggestionTree;
                Application.logMessageReceived += HandleLog;

                DevConsole.AddAllCommandInAssembly();

                BindCMD.OnExcuteBinding += UpdateSuggestions;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        public void Start()
        {
            ConsoleWindow.gameObject.SetActive(false);
        }

        public void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (ConsoleWindow.gameObject.activeSelf && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                ExcuteConsoleText();
                TextField.onSubmit.Invoke("");
            }
            if (Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                ConsoleWindow.gameObject.SetActive(!ConsoleWindow.gameObject.activeSelf);
            }
            if (TextField.isFocused && CurrentSuggestionText.text != "" && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                TextField.text = CurrentSuggestionText.text;
                TextField.caretPosition = CurrentSuggestionText.text.Length;
                UpdateSuggestions();
            }
#else
        if (ConsoleWindow.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            ExcuteConsoleText();
        }
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            ConsoleWindow.gameObject.SetActive(!ConsoleWindow.gameObject.activeSelf);
        }
        if (TextField.isFocused && CurrentSuggestionText.text != "" && Input.GetKeyDown(KeyCode.Tab))
        {
            TextField.text = CurrentSuggestionText.text;
            TextField.caretPosition = CurrentSuggestionText.text.Length;
            UpdateSuggestions();
        }
#endif
        }

        public void LateUpdate()
        {
            if(!TextField.isFocused)
                BindCMD.InvokeBinding();

            if(Input.GetKey(KeyCode.K))
            Debug.Log("ㅁㅁㅇㅁㄴㅇ먼이만ㅇ미낭머ㅏ니어ㅑㄷㄹ빚아먼얌니엄닝ㅁㄴ러댕머니ㅏ얄넝미ㅏㅇ 냐머안밍 ㅑㅇ ㅓㄴ먀ㅣ ㅑㅇ ㅓ냐밍 ㅑ님야빚ㅇ");
        }

        public void AddToSuggestionTree(ConsoleCommand cmd)
        {
            SuggestionsTree.Add(cmd.Name);
        }

        public void RemoveToSuggestionTree(ConsoleCommand cmd)
        {
            SuggestionsTree.Remove(cmd.Name);
        }

        public void HandleLog(string logText, string stackTrace, LogType type)
        {
            string newText = logText;
            if (instance.ConsoleLog.text != "")
                newText = "\n" + newText;

            // 현재 콘솔 로그 텍스트와 새로 추가할 텍스트의 길이 합계
            int totalLength = instance.ConsoleLog.text.Length + newText.Length;

            // 최대 제한인 15000자를 초과하는 경우
            if (totalLength > 15000)
            {
                // 제거해야 할 문자 수 계산
                int charsToRemove = totalLength - 15000;

                // 현재 텍스트에서 앞부분을 잘라내기
                if (charsToRemove < instance.ConsoleLog.text.Length)
                {
                    instance.ConsoleLog.text = instance.ConsoleLog.text.Substring(charsToRemove);
                }
                else
                {
                    // 현재 텍스트보다 제거할 양이 많은 경우 텍스트를 비움
                    instance.ConsoleLog.text = "";
                }
            }

            // 새 로그 추가
            instance.ConsoleLog.text += newText;
        }

        public void SelectSuggestionFromIndex(int idx)
        {
            TextField.text = _suggestions[idx] + " ";
            UpdateSuggestions();

            TextField.ActivateInputField();

            StartCoroutine(MoveCaretToEnd());
        }

        public IEnumerator MoveCaretToEnd()
        {
            Color selectionColor = TextField.selectionColor;
            TextField.selectionColor = new Color { a = 0 };
            yield return null;

            TextField.caretPosition = TextField.text.Length;
            TextField.selectionAnchorPosition = TextField.caretPosition;
            TextField.selectionFocusPosition = TextField.caretPosition;
            TextField.ForceLabelUpdate();
            TextField.selectionColor = selectionColor;
        }

        public void UpdateSuggestions()
        {
            _sb.Clear();

            if (TextField.text == "" || !SuggestionsTree.TryLoadSuggestions(ref _suggestions, TextField.text))
            {
                if (SuggestionsPanel.enabled)
                    SuggestionsPanel.enabled = false;

                SuggestionTexts.text = "";
                CurrentSuggestionText.text = "";
                return;
            }

            if (!SuggestionsPanel.enabled)
                SuggestionsPanel.enabled = true;

            _suggestions.Sort();

            if (_suggestions.Count > 0)
            {
                for (int i = 0; i < _suggestions.Count; i++)
                {
                    if (i == MAX_SUGGETIONS_COUNT)
                    {
                        _sb.Append("...");
                        break;
                    }

                    _sb.Append(_suggestions[i]);
                    ConsoleCommand cmd = DevConsole.FindCommand(_suggestions[i]);

                    string trackVal = cmd.TrackedValue?.Invoke();

                    if (cmd.Type == DevConObjType.isBool)
                        trackVal = trackVal.ToLower();

                    _sb.Append($" {trackVal}");


                    if (i != _suggestions.Count - 1)
                        _sb.Append("\n");
                }
            }
            SuggestionTexts.text = _sb.ToString();
            CurrentSuggestionText.text = _suggestions[0];
        }

        public void ExcuteConsoleText()
        {
            if (TextField.text == "")
                return;

            TextField.text = Regex.Replace(TextField.text, @"<[^>]+>", string.Empty);

            Debug.Log($"> {TextField.text}");

            string[] splitText = TextField.text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (splitText[0] == "")
                return;

            var cmd = DevConsole.FindCommand(splitText[0]);

            if (cmd == null)
            {
                Debug.Log($"unknown command : {TextField.text}");
                TextField.text = "";
                return;
            }

            ProcessCMD(cmd, splitText[1..]);

            _lastInputText.Add(TextField.text);
            TextField.text = "";
        }

        public void ProcessCMD(ConsoleCommand cmd, string[] args)
        {
            if (args.Length < 1)
            {
                if (cmd.Type != DevConObjType.isVoid)
                {
                    string desc = (cmd.Description != "" && cmd.Description[0] != '/') ? $", <color={COLOR_INFO}>{cmd.Description}</color>" : string.Empty;
                    string value = cmd.TrackedValue != null ? $" = {cmd.TrackedValue.Invoke()}" : string.Empty;
                    string initValue = (cmd.InitValue != null && cmd.InitValue != cmd.TrackedValue.Invoke()) ? $" <color={COLOR_VALUE}>( init = {cmd.InitValue} )</color>" : string.Empty;

                    if (cmd.Type == DevConObjType.isBool)
                    {
                        value = value.ToLower();
                        initValue = initValue.ToLower();
                    }
                    Debug.Log($"<color=#9FF781>[{cmd.Name}{value}]</color>{initValue}{desc}");
                    return;
                }
                else if (cmd.Description != "" && cmd.Description[0] != '/')
                {
                    Debug.Log($"<color={COLOR_INFO}>{cmd.Description}</color>");
                }
                else if (cmd.Flag != ExecFlag.NONE
                    && (cmd.Flag & DevConsole.CurrentFlags) == 0)
                {
                    Debug.Log($"<color={COLOR_ERROR}>{cmd.Flag}플래그가 활성화 되어있지 않습니다.</color>");
                    return;
                }
                DevConsole.ExecuteCommand(cmd, string.Empty);
            }
            else if (args.Length > 1
                && cmd.Type != DevConObjType.isString
                && cmd.Type != DevConObjType.isVoid)
            {
                Debug.Log($"<color={COLOR_ERROR}>하나의 인자만 입력해야 합니다.</color>");
            }
            else
            {
                var cmdLine = args[0];

                if (cmd.Type == DevConObjType.isVoid && args[0] != "")
                {
                    Debug.Log($"<color={COLOR_ERROR}>{cmd.Name}은 매개변수가 없는 명령어입니다.</color>");
                    return;
                }

                if (cmd.Type == DevConObjType.isString)
                    cmdLine = string.Join(" ", args);

                if (!DevConsole.ExecuteCommand(cmd, cmdLine)
                    && cmd.Flag != ExecFlag.NONE
                    && (cmd.Flag & DevConsole.CurrentFlags) == 0)
                {
                    Debug.Log($"<color={COLOR_ERROR}>{cmd.Flag}플래그가 활성화 되어있지 않습니다.</color>");
                    return;
                }
            }
        }
        private void OnDestroy()
        {
            DevConsole.OnCmdAdded -= AddToSuggestionTree;
            DevConsole.OnCmdRemoved -= RemoveToSuggestionTree;
            Application.logMessageReceived -= HandleLog;
            BindCMD.OnExcuteBinding -= UpdateSuggestions;
        }
    }
}

