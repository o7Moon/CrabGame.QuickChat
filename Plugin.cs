using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;

namespace quickchat
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static Dictionary<KeyCode, string> binds;
        public static ConfigEntry<string> binds_entry;
        public static Plugin Instance;
        public static string dictToString<T, U>(Dictionary<T, U> dict){
            var outs = "";
            foreach (KeyValuePair<T, U> pair in dict) {
                outs += pair.Key.ToString();
                outs += ",";
                outs += pair.Value.ToString();
                outs += "|";
            }
            return outs;
        }
        public static Dictionary<T, U> stringToDict<T, U>(string s){
            var outd = new Dictionary<T, U>();
            foreach (var pairstring in s.Split("|")) {
                try {
                    var pair = pairstring.Split(",");
                    T key = (T) System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(pair[0]);
                    U value = (U) System.ComponentModel.TypeDescriptor.GetConverter(typeof(U)).ConvertFromString(pair[1]);
                    outd.Add(key, value);
                } catch (System.Exception e) {}
            }
            return outd;
        }
        public override void Load()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(typeof(Plugin));

            binds_entry = Config.Bind<string>(
                "Bindings",
                "Keybinds",
                dictToString(
                new Dictionary<KeyCode, string>{
                    [KeyCode.G] = "gg",
                    [KeyCode.H] = "average crab game chat message",
                    [KeyCode.J] = "gl",
                    [KeyCode.K] = "NO WAY"
                })
            );
            binds = stringToDict<KeyCode, string>(binds_entry.Value);

            SceneManager.sceneLoaded += (UnityAction<Scene,LoadSceneMode>) onSceneLoad;

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public void onSceneLoad(Scene s, LoadSceneMode m){
            Config.Reload();
            binds = stringToDict<KeyCode, string>(binds_entry.Value);
        }

        [HarmonyPatch(typeof(GameUI), nameof(GameUI.Update))]
        [HarmonyPostfix]
        public static void postGameUIUpdate(GameUI __instance) {
            // dont register keybinds when typing in chat
            if (ChatBox.Instance.transform.GetChild(0).GetChild(1).GetComponent<TMP_InputField>().isFocused) return;
            foreach (KeyValuePair<KeyCode, string> bind in binds)
            {
                if (Input.GetKeyDown(bind.Key)) {
                    ChatBox.Instance.SendMessage(bind.Value.Trim());
                }
            }
        }
    }
}