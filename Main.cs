using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HumanAPI;
using Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public static bool makeHumanInvincible = false;
    public static bool disableCheckpoints = false;
    public static bool oldDisableCheckpoints = false;
    public static bool disablePassTrigger = false;
    public static int checkpointSetting = 0;
    public static int checkpointCount;
    public static List<string> whitelistStrings = new List<string>(100);
    public static List<string> blacklistStrings = new List<string>(100);
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
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }
    void ShellFunction(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
        {
            Debug.Log("Invalid argument");
            return;
        }
        string[] args = arg.ToLower().Split(' ');
        if (args.Length == 0)
        {
            Debug.Log("Invalid argument");
            return;
        }
        switch (args[0])
        {
            case "whitelist":
            case "wl":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref applyWhitelist, nameof(applyWhitelist), "");
                        if (applyBlacklist)
                        {
                            applyBlacklist = false;
                        }
                        return;
                    }
                    switch (args[1])
                    {
                        case "apply":
                            {
                                if (args.Length == 2)
                                {
                                    SetBooleanCommand(ref applyWhitelist, nameof(applyWhitelist), "");
                                    if (applyBlacklist)
                                    {
                                        applyBlacklist = false;
                                    }
                                    return;
                                }
                                SetBooleanCommand(ref applyWhitelist, nameof(applyWhitelist), args[2]);
                                if (applyBlacklist)
                                {
                                    applyBlacklist = false;
                                }
                                return;
                            }
                        case "add":
                            {
                                if (args.Length == 2)
                                {
                                    Debug.Log("Invalid argument");
                                }
                                AddToList(ref whitelistStrings, nameof(whitelistStrings), args[2]);
                                return;
                            }
                        case "remove":
                        case "delete":
                            {
                                if (args.Length == 2)
                                {
                                    Debug.Log("Invalid argument");
                                }
                                if (int.TryParse(args[2], out int index))
                                {
                                    if (index < 1 || index > whitelistStrings.Count)
                                    {
                                        Debug.Log("Out of range");
                                    }
                                    whitelistStrings.RemoveAt(index - 1);
                                }
                                return;
                            }
                        case "clear":
                            {
                                whitelistStrings.Clear();
                                return;
                            }
                        default:
                            {
                                Debug.Log("Invalid argument");
                                return;
                            }
                    }
                }
            case "blacklist":
            case "bl":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref applyBlacklist, nameof(applyBlacklist), "");
                        if (applyWhitelist)
                        {
                            applyWhitelist = false;
                        }
                        return;
                    }
                    switch (args[1])
                    {
                        case "apply":
                            {
                                if (args.Length == 2)
                                {
                                    SetBooleanCommand(ref applyBlacklist, nameof(applyBlacklist), "");
                                    if (applyWhitelist)
                                    {
                                        applyWhitelist = false;
                                    }
                                    return;
                                }
                                SetBooleanCommand(ref applyBlacklist, nameof(applyBlacklist), args[2]);
                                if (applyWhitelist)
                                {
                                    applyWhitelist = false;
                                }
                                return;
                            }
                        case "add":
                            {
                                if (args.Length == 2)
                                {
                                    Debug.Log("Invalid argument");
                                }
                                AddToList(ref blacklistStrings, nameof(blacklistStrings), args[2]);
                                return;
                            }
                        case "remove":
                        case "delete":
                            {
                                if (args.Length == 2)
                                {
                                    Debug.Log("Invalid argument");
                                }
                                if (int.TryParse(args[2], out int index))
                                {
                                    if (index < 1 || index > blacklistStrings.Count)
                                    {
                                        Debug.Log("Out of range");
                                    }
                                    blacklistStrings.RemoveAt(index - 1);
                                }
                                return;
                            }
                        case "clear":
                            {
                                blacklistStrings.Clear();
                                return;
                            }
                        default:
                            {
                                Debug.Log("Invalid argument");
                                return;
                            }
                    }
                }
            case "invinciblehumans":
            case "ih":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref makeHumanInvincible, nameof(makeHumanInvincible), "");
                    }
                    SetBooleanCommand(ref makeHumanInvincible, nameof(makeHumanInvincible), args[1]);
                    return;
                }
            case "disablecheckpoints":
            case "dcp":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref disableCheckpoints, nameof(disableCheckpoints), "");
                    }
                    SetBooleanCommand(ref disableCheckpoints, nameof(disableCheckpoints), args[1]);
                    return;
                }
            case "disablepasstrigger":
            case "dpt":
                {
                    if (args.Length == 1)
                    {
                        SetBooleanCommand(ref disablePassTrigger, nameof(disablePassTrigger), "");
                    }
                    SetBooleanCommand(ref disablePassTrigger, nameof(disablePassTrigger), args[1]);
                    return;
                }
            default:
                {
                    Debug.Log("Invalid argument");
                    return;
                }
        }
    }

    void AddToList(ref List<string> strings, string name, string value)
    {
        if (Regex.IsMatch(value, @"^[0-9]{0,17}$", RegexOptions.IgnoreCase))
        {
            strings.Add(value);
        }
        else
        {
            Debug.Log("Invalid argument");
        }
        Debug.Log($"Added \"{name}\" to \"{value}\"");
        return;
    }
    public static void SetBooleanCommand(ref bool boolean, string name, string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
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
    private static GUIStyle noWrapListTextFieldStyle;
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
            for (int i = 0; i < whitelistStrings.Count; i++)
            {
                if (!Regex.IsMatch(whitelistStrings[i], @"^[0-9]{0,17}$"))
                {
                    whitelistStrings[i] = whitelistStrings[i].Substring(0, whitelistStrings[i].Length - 1);
                }
            }
        }
        if (applyBlacklist)
        {
            CreateStringsList(ref blacklistStrings, ref whitelistWindowRect, true);
            for (int i = 0; i < blacklistStrings.Count; i++)
            {
                if (!Regex.IsMatch(blacklistStrings[i], @"^[0-9]{0,17}$"))
                {
                    blacklistStrings[i] = blacklistStrings[i].Substring(0, blacklistStrings[i].Length - 1);
                }
            }
        }
        if (disableCheckpoints != oldDisableCheckpoints)
        {
            if (disableCheckpoints)
            {
                if (checkpointCount == 0)
                {
                    PreOnSceneLoaded.OnSceneLoaded();
                }
                checkpointSetting = Game.instance.currentCheckpointNumber;
            }
            oldDisableCheckpoints = disableCheckpoints;
        }
    }
    public static void CreateStringsList(ref List<string> strings, ref Rect rect, bool usingGUILayout = false)
    {
        if (strings.Count >= 2)
        {
            bool flag = true;
            for (int i = 0; i < strings.Count - 1; i++)
            {
                if (string.IsNullOrWhiteSpace(strings[i]))
                {
                    strings.RemoveAt(i);
                    if (rect != null && usingGUILayout)
                    {
                        rect.width = rect.height = 0;
                    }
                    flag = false;
                }
            }
            if (flag && !string.IsNullOrWhiteSpace(strings[strings.Count - 1]))
            {
                strings.Add("");
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(strings[0]))
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
            noWrapListTextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false,
                fixedWidth = 128
            };
            noWrapTextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
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
                    makeHumanInvincible = GUILayout.Toggle(makeHumanInvincible, "Invincible humans", noWrapToggleStyle, expandOptions);
                    disableCheckpoints = GUILayout.Toggle(disableCheckpoints, "Disable new checkpoints", noWrapToggleStyle, expandOptions);
                    if (disableCheckpoints)
                    {
                        using (var horizontalScope = new GUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("<<", noWrapButtonStyle))
                            {
                                checkpointSetting = 0;
                                Game.instance.currentCheckpointNumber = Game.instance.currentCheckpointSubObjectives = 0;
                                mainWindowRect.width = mainWindowRect.height = 0;
                                Game.instance.RestartCheckpoint();
                            }
                            if (GUILayout.Button("-", noWrapButtonStyle))
                            {
                                if (checkpointSetting >= 2)
                                {
                                    checkpointSetting -= 1;
                                    Game.instance.currentCheckpointNumber -= 1;
                                    Game.instance.currentCheckpointSubObjectives = 0;
                                    mainWindowRect.width = mainWindowRect.height = 0;
                                    Game.instance.RestartCheckpoint();
                                }
                            }
                            GUILayout.Label($"{checkpointSetting}/{checkpointCount}", noWrapTextFieldStyle, expandOptions);
                            if (GUILayout.Button("+", noWrapButtonStyle))
                            {
                                if (checkpointSetting < checkpointCount)
                                {
                                    checkpointSetting += 1;
                                    Game.instance.currentCheckpointNumber += 1;
                                    Game.instance.currentCheckpointSubObjectives = 0;
                                    mainWindowRect.width = mainWindowRect.height = 0;
                                    Game.instance.RestartCheckpoint();
                                }
                            }
                            if (GUILayout.Button(">>", noWrapButtonStyle))
                            {
                                checkpointSetting = Game.instance.currentCheckpointNumber = checkpointCount;
                                Game.instance.currentCheckpointSubObjectives = 0;
                                mainWindowRect.width = mainWindowRect.height = 0;
                                Game.instance.RestartCheckpoint();
                            }
                        }
                    }
                    disablePassTrigger = GUILayout.Toggle(disablePassTrigger, "Disable passtriggers", noWrapToggleStyle, expandOptions);
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
                                    whitelistStrings[i] = GUILayout.TextArea(whitelistStrings[i], noWrapListTextFieldStyle, expandOptions);
                                }
                                else
                                {
                                    blacklistStrings[i] = GUILayout.TextArea(blacklistStrings[i], noWrapListTextFieldStyle, expandOptions);
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

    [HarmonyPatch(typeof(NetGame), nameof(NetGame.OnConnect))]
    public static class PreOnConnect
    {
        [HarmonyPrefix]
        public static bool OnConnect(ref object connection)
        {
            if (!NetGame.isServer || (applyWhitelist && !whitelistStrings.Contains(connection.ToString())) || (applyBlacklist && blacklistStrings.Contains(connection.ToString())))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Fall))]
    public static class PreFall
    {
        [HarmonyPrefix]
        public static bool Fall(ref HumanBase humanBase, ref bool drown, ref bool fallAchievement)
        {
            Human human = humanBase as Human;
            if (makeHumanInvincible)
            {
                if (drown)
                {
                    StatsAndAchievements.IncreaseDrownCount(human);
                }
                else if (fallAchievement)
                {
                    StatsAndAchievements.IncreaseFallCount(human);
                }
            }
            return !makeHumanInvincible;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.OnSceneLoaded))]
    public static class PreOnSceneLoaded
    {
        [HarmonyPrefix]
        public static void OnSceneLoaded()
        {
            checkpointSetting = checkpointCount = 0;
            foreach (Checkpoint checkpoint in FindObjectsOfType<Checkpoint>())
            {
                if (checkpointCount < checkpoint.number)
                {
                    checkpointCount = checkpoint.number;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterCheckpoint))]
    public static class PreEnterCheckpoint
    {
        [HarmonyPrefix]
        public static bool EnterCheckpoint()
        {
            return !disableCheckpoints;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.RestartLevel))]
    public static class PreRestartLevel
    {
        [HarmonyPrefix]
        public static bool RestartLevel()
        {
            return !disableCheckpoints;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterPassZone))]
    public static class PreEnterPassZone
    {
        [HarmonyPrefix]
        public static bool EnterPassZone()
        {
            return !disablePassTrigger;
        }
    }

}
