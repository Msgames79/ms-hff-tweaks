using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
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
    public static bool editSlideAngle = false;
    public static string slideAngleString = "45";
    public static string oldSlideAngleString = "45";
    public static float slideAngle = 45f;
    public static int checkpointSetting = 0;
    public static int checkpointCount;
    public static int ssgMode = 0;
    public static bool autoGetUp = false;
    public static bool noIntervalJump = false;
    public static bool powerfulPF = false;
    public static List<string> ssgModeStrings = ["Default", "Always", "Disabled(Currently nothing)"];
    public static List<string> whitelistStrings = new List<string>(10);
    public static List<string> blacklistStrings = new List<string>(10);
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
    private static GUIStyle noWrapLabelStyle;
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
                if (App.state == AppSate.PlayLevel && checkpointCount == 0)
                {
                    PreOnSceneLoaded_Game.OnSceneLoaded();
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
            noWrapLabelStyle = new GUIStyle(GUI.skin.label)
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
                    editSlideAngle = GUILayout.Toggle(editSlideAngle, "Edit slide angle", noWrapToggleStyle, expandOptions);
                    if (editSlideAngle)
                    {
                        slideAngleString = GUILayout.TextField(slideAngleString, noWrapTextFieldStyle, expandOptions);
                        if (slideAngleString != oldSlideAngleString)
                        {
                            float.TryParse(slideAngleString, out slideAngle);
                            slideAngle = Mathf.Clamp(slideAngle, -1f, float.MaxValue);
                            oldSlideAngleString = slideAngleString;
                        }
                        else
                        {
                            GUILayout.Label($"{slideAngle}", noWrapLabelStyle, expandOptions);
                        }
                    }
                    if (GUILayout.Button($"SSG mode: {ssgModeStrings[ssgMode]}"))
                    {
                        ssgMode = (ssgMode + 1) % ssgModeStrings.Count;
                        mainWindowRect.width = mainWindowRect.height = 0;
                    }
                    autoGetUp = GUILayout.Toggle(autoGetUp, "Auto get-up", noWrapToggleStyle, expandOptions);
                    noIntervalJump = GUILayout.Toggle(noIntervalJump, "Jump without interval", noWrapToggleStyle, expandOptions);
                    powerfulPF = GUILayout.Toggle(powerfulPF, "Full force PF", noWrapToggleStyle, expandOptions);
                    break;
                }
            case 1524676901:
                {
                    syncVisibility = GUILayout.Toggle(syncVisibility, "Sync visibility", noWrapToggleStyle, expandOptions);
                    int loops = applyWhitelist ? whitelistStrings.Count : blacklistStrings.Count;
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
    public static class PreOnConnect_NetGame
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
    public static class PreFall_Game
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
    public static class PreOnSceneLoaded_Game
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
    public static class PreEnterCheckpoint_Game
    {
        [HarmonyPrefix]
        public static bool EnterCheckpoint()
        {
            return !disableCheckpoints;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.RestartLevel))]
    public static class PreRestartLevel_Game
    {
        [HarmonyPrefix]
        public static bool RestartLevel()
        {
            return !disableCheckpoints;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterPassZone))]
    public static class PreEnterPassZone_Game
    {
        [HarmonyPrefix]
        public static bool EnterPassZone()
        {
            return !disablePassTrigger;
        }
    }

    [HarmonyPatch(typeof(Human), nameof(Human.ProcessFall))]
    public static class PreProcessFall_Human
    {
        [HarmonyPrefix]
        public static bool ProcessFall(ref Human __instance)
        {
            __instance.PushGroundAngle();
            bool flag = false;
            if (__instance.groundAnglesSum / __instance.groundAngles.Length > (editSlideAngle ? slideAngle : 45f))
            {
                flag = true;
                __instance.slideTimer = 0f;
                __instance.onGround = false;
                __instance.state = HumanState.Slide;
            }
            else if (__instance.state == HumanState.Slide && __instance.groundAnglesSum / __instance.groundAngles.Length < 37f && __instance.ragdoll.partBall.rigidbody.velocity.y > -1f)
            {
                __instance.slideTimer += Time.fixedDeltaTime;
                if (__instance.slideTimer < 0.003f)
                {
                    __instance.onGround = false;
                }
            }

            if (!__instance.onGround && !flag)
            {
                if (__instance.fallTimer < 5f)
                {
                    __instance.fallTimer += Time.deltaTime;
                }

                if (__instance.state == HumanState.Climb)
                {
                    __instance.fallTimer = 0f;
                }

                if (__instance.fallTimer > 3f)
                {
                    __instance.state = HumanState.FreeFall;
                }
                else if (__instance.fallTimer > 1f)
                {
                    __instance.state = HumanState.Fall;
                }
            }
            else
            {
                __instance.fallTimer = 0f;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Human), nameof(Human.FixedUpdate))]
    public static class PreFixedUpdate_Human
    {
        [HarmonyPrefix]
        public static bool FixedUpdate(ref Human __instance)
        {
            if (__instance.thisFrameHit + __instance.lastFrameHit > 30f)
            {
                __instance.MakeUnconscious();
                __instance.ReleaseGrab(3f);
            }

            __instance.lastFrameHit = __instance.thisFrameHit;
            __instance.thisFrameHit = 0f;
            __instance.jumpDelay -= Time.fixedDeltaTime;
            __instance.groundDelay -= Time.fixedDeltaTime;
            if (!__instance.disableInput)
            {
                __instance.ProcessInput();
            }

            __instance.LimitFallSpeed();
            Quaternion quaternion = Quaternion.Euler(__instance.controls.targetPitchAngle, __instance.controls.targetYawAngle, 0f);
            __instance.targetDirection = quaternion * Vector3.forward;
            __instance.targetLiftDirection = Quaternion.Euler(Mathf.Clamp(__instance.controls.targetPitchAngle, -70f, 80f), __instance.controls.targetYawAngle, 0f) * Vector3.forward;
            if (NetGame.isClient || ReplayRecorder.isPlaying)
            {
                return false;
            }

            if (__instance.state == HumanState.Dead || __instance.state == HumanState.Unconscious || __instance.state == HumanState.Spawning)
            {
                __instance.controls.leftGrab = __instance.controls.rightGrab = false;
                __instance.controls.shootingFirework = false;
            }

            __instance.groundAngle = 90f;
            __instance.groundAngle = Mathf.Min(__instance.groundAngle, __instance.ragdoll.partBall.sensor.groundAngle);
            __instance.groundAngle = Mathf.Min(__instance.groundAngle, __instance.ragdoll.partLeftFoot.sensor.groundAngle);
            __instance.groundAngle = Mathf.Min(__instance.groundAngle, __instance.ragdoll.partRightFoot.sensor.groundAngle);
            bool flag = __instance.hasGrabbed;
            __instance.onGround = __instance.groundDelay <= 0f && __instance.groundManager.onGround;
            __instance.hasGrabbed = __instance.grabManager.hasGrabbed;
            __instance.ragdoll.partBall.sensor.groundAngle = __instance.ragdoll.partLeftFoot.sensor.groundAngle = __instance.ragdoll.partRightFoot.sensor.groundAngle = 90f;
            if (__instance.hasGrabbed && __instance.transform.position.y < __instance.grabStartPosition.y)
            {
                __instance.grabStartPosition = __instance.transform.position;
            }

            if (__instance.hasGrabbed && __instance.transform.position.y - __instance.grabStartPosition.y > 0.5f)
            {
                __instance.isClimbing = true;
            }
            else
            {
                __instance.isClimbing = false;
            }

            if (flag != __instance.hasGrabbed && __instance.hasGrabbed)
            {
                __instance.grabStartPosition = __instance.transform.position;
            }

            if (__instance.state == HumanState.Spawning)
            {
                __instance.spawning = true;
                if (__instance.onGround)
                {
                    __instance.MakeUnconscious();
                }
            }
            else
            {
                __instance.spawning = false;
            }

            __instance.ProcessUnconscious();
            if (__instance.state != HumanState.Dead && __instance.state != HumanState.Unconscious && __instance.state != HumanState.Spawning)
            {
                __instance.ProcessFall();
                if (__instance.onGround)
                {
                    if (__instance.controls.jump && __instance.jumpDelay <= 0f)
                    {
                        __instance.state = HumanState.Jump;
                        __instance.jump = true;
                        if (!noIntervalJump)
                        {
                            __instance.jumpDelay = 0.5f;
                        }
                        __instance.groundDelay = 0.2f;
                    }
                    else if (__instance.controls.walkSpeed > 0f)
                    {
                        __instance.state = HumanState.Walk;
                    }
                    else
                    {
                        __instance.state = HumanState.Idle;
                    }
                }
                else
                {
                    switch (ssgMode)
                    {
                        case 1:
                            {
                                __instance.state = HumanState.Climb;
                                break;
                            }
                        default:
                            {
                                if (__instance.ragdoll.partLeftHand.sensor.grabObject != null || __instance.ragdoll.partRightHand.sensor.grabObject != null)
                                {
                                    __instance.state = HumanState.Climb;
                                }
                                break;
                            }
                    }
                }
            }

            if (__instance.skipLimiting)
            {
                __instance.skipLimiting = false;
                return false;
            }

            for (int i = 0; i < __instance.rigidbodies.Length; i++)
            {
                Vector3 vector = __instance.velocities[i];
                Vector3 vector2 = __instance.rigidbodies[i].velocity;
                Vector3 vector3 = vector2 - vector;
                if (Vector3.Dot(vector, vector3) < 0f)
                {
                    Vector3 normalized = vector.normalized;
                    float magnitude = vector.magnitude;
                    float value = 0f - Vector3.Dot(normalized, vector3);
                    float num = Mathf.Clamp(value, 0f, magnitude);
                    vector3 += normalized * num;
                }

                float num2 = 1000f * Time.deltaTime;
                if (vector3.magnitude > num2)
                {
                    Vector3 vector4 = Vector3.ClampMagnitude(vector3, num2);
                    vector2 -= vector3 - vector4;
                    __instance.rigidbodies[i].velocity = vector2;
                }

                __instance.velocities[i] = vector2;
            }
            if (autoGetUp && __instance.state == HumanState.Spawning)
            {
                __instance.MakeUnconscious();
                CheatCodes.NotifyCheat("MHT AGU");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(LegMuscles), nameof(LegMuscles.ApplyJumpImpulses))]
    public static class PreApplyJumpImpulses_LegMuscles
    {
        [HarmonyPrefix]
        public static bool ApplyJumpImpulses(ref LegMuscles __instance)
        {
            float num = 1f;
            if (!powerfulPF)
            {
                for (int i = 0; i < __instance.human.groundManager.groundObjects.Count; i++)
                {
                    if (__instance.human.grabManager.IsGrabbed(__instance.human.groundManager.groundObjects[i]))
                    {
                        num = 0.75f;
                    }
                }
            }

            Vector3 vector = Vector3.up * __instance.upImpulse * num;
            Vector3 vector2 = __instance.human.controls.walkDirection.normalized * __instance.forwardImpulse * num;
            __instance.ragdoll.partHead.rigidbody.SafeAddForce(vector * 0.1f + vector2 * 0.1f, ForceMode.Impulse);
            __instance.ragdoll.partChest.rigidbody.SafeAddForce(vector * 0.1f + vector2 * 0.1f, ForceMode.Impulse);
            __instance.ragdoll.partWaist.rigidbody.SafeAddForce(vector * 0.1f + vector2 * 0.1f, ForceMode.Impulse);
            __instance.ragdoll.partHips.rigidbody.SafeAddForce(vector * 0.1f + vector2 * 0.1f, ForceMode.Impulse);
            __instance.ragdoll.partBall.rigidbody.SafeAddForce(vector * 0.1f + vector2 * 0.1f, ForceMode.Impulse);
            __instance.ragdoll.partLeftThigh.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partRightThigh.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partLeftLeg.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partRightLeg.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partLeftFoot.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partRightFoot.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partLeftArm.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partRightArm.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partLeftForearm.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.ragdoll.partRightForearm.rigidbody.SafeAddForce(vector * 0.05f + vector2 * 0.05f, ForceMode.Impulse);
            __instance.human.groundManager.DistributeForce(-vector / Time.fixedDeltaTime, __instance.ragdoll.partBall.rigidbody.position);
            return false;
        }
    }
}