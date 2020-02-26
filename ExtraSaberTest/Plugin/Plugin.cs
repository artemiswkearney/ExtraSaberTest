using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using Harmony;
using System.Reflection;
using UnityEngine;
using System.Linq;
using BS_Utils.Utilities;

namespace ExtraSaberTest
{
    public class Plugin : IBeatSaberPlugin
    {
        public static PlayerController playerController;
        public void Init(IPALogger logger)
        {
            BS_Utils.Utilities.BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
            Logger.log = logger;
        }

        private void BSEvents_gameSceneLoaded()
        {
            playerController = GameObject.FindObjectsOfType<PlayerController>().FirstOrDefault();
            if (playerController == null)
            {
                Logger.log.Error("Could not find PlayerController!");
            }

            if (!(ExtraSaberManager.instance?.enabled ?? false))
            {
                ExtraSaberManager.instance = new GameObject("ExtraSaberManager").AddComponent<ExtraSaberManager>();
            }
            ExtraSaberManager.instance.addSaber("LeftMaul", Vector3.up * 180, Vector3.zero, Saber.SaberType.SaberB, true);
            ExtraSaberManager.instance.addSaber("RightMaul", Vector3.up * 180, Vector3.zero, Saber.SaberType.SaberA, false);
            BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("ExtraSaberTest");
        }

        public void OnApplicationStart()
        {
            var harmony = HarmonyInstance.Create("com.arti.BeatSaber.Plugin");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnApplicationQuit()
        {
            Logger.log.Debug("OnApplicationQuit");
        }

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }
    }
}
