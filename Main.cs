using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Multiplayer;
using UnityEngine;

namespace ms_hff_tweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Main : BaseUnityPlugin
{
    public static Main instance;
    private static Rect mainWindowRect;
    private static Rect whitelistWindowRect;
    public static bool applyWhitelist = false;
    public static bool applyBlacklist = false;
    public static bool oldApplyWhitelist = false;
    public static bool oldApplyBlacklist = false;
    private List<string> whitelistStrings = new List<string>(100);
    private List<string> blacklistStrings = new List<string>(100);
    private bool syncVisibility = true;
    void Start()
    {
        instance = this;
        Shell.RegisterCommand("hfftweaks", new Action<string>(ShellFunction), null);
        Shell.RegisterCommand("ht", new Action<string>(ShellFunction), null);
        whitelistStrings.Add("");
        blacklistStrings.Add("");
        mainWindowRect = new Rect(1090f, 180f, 0f, 0f);
        whitelistWindowRect = new Rect(1090f, 400f, 0f, 0f);
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }
    void ShellFunction(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            Debug.Log("Invalid argument");
            return;
        }
        string[] args = arg.Split(' ');
        if (args.Length == 0)
        {
            Debug.Log("Invalid argument");
            return;
        }
        switch (args[0].ToLower())
        {
            case "applywhitelist":
            case "awl":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref applyWhitelist, nameof(applyWhitelist), "");
                        if (applyWhitelist)
                        {
                            applyBlacklist = false;
                        }
                        return;
                    }
                    SetBooleanCommand(ref applyWhitelist, nameof(applyWhitelist), args[1]);
                    if (applyWhitelist)
                    {
                        applyBlacklist = false;
                    }
                    return;
                }
            case "applyblacklist":
            case "abl":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref applyBlacklist, nameof(applyBlacklist), "");
                        if (applyBlacklist)
                        {
                            applyWhitelist = false;
                        }
                        return;
                    }
                    SetBooleanCommand(ref applyBlacklist, nameof(applyBlacklist), args[1]);
                    if (applyBlacklist)
                    {
                        applyWhitelist = false;
                    }
                    return;
                }
            default:
                {
                    Debug.Log("Invalid argument");
                    return;
                }
        }
    }
    public static void SetBooleanCommand(ref bool boolean, string name, string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            boolean = !boolean;
        }
        else if (Regex.IsMatch(arg, @"^(t(rue)?|1|on)$", RegexOptions.IgnoreCase))
        {
            boolean = true;
        }
        else if (Regex.IsMatch(arg, @"^(f(alse)?|0|off)$", RegexOptions.IgnoreCase))
        {
            boolean = false;
        }
        else
        {
            Debug.Log("Invalid argument");
        }
        Debug.Log($"Set \"{name}\" to \"{boolean}\"");
    }
    private static GUILayoutOption[] expandOptions;
    private static GUIStyle noWrapToggleStyle;
    private static GUIStyle noWrapTextFieldStyle;
    private static GUIStyle noWrapButtonStyle;
    public static bool guiOpened = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            guiOpened = !guiOpened;
        }
        if (oldApplyWhitelist = !applyWhitelist)
        {
            mainWindowRect.height = 0;
            whitelistWindowRect.width = whitelistWindowRect.height = 0;
        }
        if (oldApplyBlacklist = !applyBlacklist)
        {
            mainWindowRect.height = 0;
            whitelistWindowRect.width = whitelistWindowRect.height = 0;
        }
        if (applyWhitelist)
        {
            CreateStringsList(ref whitelistStrings, ref whitelistWindowRect, true);
        }
        if (applyBlacklist)
        {
            CreateStringsList(ref blacklistStrings, ref whitelistWindowRect, true);
        }
    }
    public static void CreateStringsList(ref List<string> strings, ref Rect rect, bool usingGUILayout = false)
    {
        if (strings.Count >= 2)
        {
            bool flag = true;
            for (int i = 0; i < strings.Count - 1; i++)
            {
                if (string.IsNullOrEmpty(strings[i]))
                {
                    strings.RemoveAt(i);
                    if (rect != null && usingGUILayout)
                    {
                        rect.width = rect.height = 0;
                    }
                    flag = false;
                }
            }
            if (flag && !string.IsNullOrEmpty(strings[strings.Count - 1]))
            {
                strings.Add("");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(strings[0]))
            {
                strings.Add("");
            }
        }
    }
    void OnGUI()
    {
        if (expandOptions == null)
        {
            expandOptions =
            [
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true),
                GUILayout.MaxWidth(Screen.width)
            ];
            noWrapToggleStyle = new GUIStyle(GUI.skin.toggle)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            noWrapTextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                fixedWidth = 128
            };
            noWrapButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
        }
        if (guiOpened)
        {
            mainWindowRect = GUILayout.Window(1524676900, mainWindowRect, WindowFunction, "M's HFF Tweaks", expandOptions);
        }
        if ((guiOpened && (applyWhitelist || applyBlacklist)) || (!guiOpened && (applyWhitelist || applyBlacklist) && !syncVisibility))
        {
            whitelistWindowRect = GUILayout.Window(1524676901, whitelistWindowRect, WindowFunction, applyWhitelist ? "Whitelist" : "Blacklist", expandOptions);
        }
    }
    void WindowFunction(int windowId)
    {
        switch (windowId)
        {
            case 1524676900:
                {
                    if (!applyBlacklist)
                    {
                        applyWhitelist = GUILayout.Toggle(applyWhitelist, "Apply whitelist", noWrapToggleStyle, expandOptions);
                    }
                    if (!applyWhitelist)
                    {
                        applyBlacklist = GUILayout.Toggle(applyBlacklist, "Apply blacklist", noWrapToggleStyle, expandOptions);
                    }
                    break;
                }
            case 1524676901:
                {
                    syncVisibility = GUILayout.Toggle(syncVisibility, "Sync visibility", noWrapToggleStyle, expandOptions);
                    int loops= applyWhitelist ? whitelistStrings.Count : blacklistStrings.Count;
                    using (var horizontalScope = new GUILayout.HorizontalScope())
                    {
                        using (var verticalScope = new GUILayout.VerticalScope())
                        {
                            for (int i = 0; i < loops; i++)
                            {
                                if (applyWhitelist)
                                {
                                    whitelistStrings[i] = GUILayout.TextArea(whitelistStrings[i], noWrapTextFieldStyle, expandOptions);
                                }
                                else
                                {
                                    blacklistStrings[i] = GUILayout.TextArea(blacklistStrings[i], noWrapTextFieldStyle, expandOptions);
                                }
                            }
                        }
                        using (var verticalScope = new GUILayout.VerticalScope())
                        {
                            for (int i = 0; i < loops - 1; i++)
                            {
                                if (GUILayout.Button("Clear", noWrapButtonStyle))
                                {
                                    if (applyWhitelist)
                                    {
                                        whitelistStrings[i] = string.Empty;
                                    }
                                    else
                                    {
                                        blacklistStrings[i] = string.Empty;
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            default:
                {
                    return;
                }
        }
        GUI.DragWindow();
    }

    [HarmonyPatch(typeof(NetGame), nameof(NetGame.AddServerPlayer))]
    static class JoinObservation
    {
        private static int oldLobbyMaxPlayers;
        private static bool flag = true;
        [HarmonyPrefix]
        public static void Prefix(ref NetHost owner, ref string userId, ref bool isLocal)
        {
            Debug.Log("invoked prefix");
            if (App.isServer)
            {
                flag = true;
                oldLobbyMaxPlayers = Options.lobbyMaxPlayers;
                if (applyWhitelist)
                {
                    if (!instance.whitelistStrings.Contains(owner.name) && !instance.whitelistStrings.Contains(userId) && !isLocal)
                    {
                        flag = false;
                        Debug.Log($"Blocked {owner.name} ({userId})");
                        Options.lobbyMaxPlayers = NetGame.instance.players.Count;
                    }
                }
                if (applyBlacklist)
                {
                    if ((instance.blacklistStrings.Contains(owner.name) || instance.blacklistStrings.Contains(userId)) && !isLocal)
                    {
                        flag = false;
                        Debug.Log($"Blocked {owner.name} ({userId})");
                        Options.lobbyMaxPlayers = NetGame.instance.players.Count;
                    }
                }
                if (flag)
                {
                    Debug.Log($"Player {owner.name} ({userId}) joined");
                }
            }
        }
        [HarmonyPostfix]
        public static void Postfix()
        {
            Debug.Log("invoked postfix");
            if (App.isServer && !flag)
            {
                Options.lobbyMaxPlayers++;
            }
        }
    }
}
