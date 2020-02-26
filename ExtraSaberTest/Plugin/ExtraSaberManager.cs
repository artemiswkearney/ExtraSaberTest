using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static BS_Utils.Utilities.ReflectionUtil;

namespace ExtraSaberTest
{
    public class ExtraSaberManager : MonoBehaviour
    {
        public static ExtraSaberManager instance;
        public static DiContainer ISMCCContainer;
        public static DiContainer colorManagerContainer;
        public struct SaberInfo
        {
            public Vector3 eulerRotation;
            public Vector3 offset;
            public Saber saber;
            public bool isMainSaber;
        }
        public Dictionary<string, SaberInfo> sabersByName;
        public void Awake()
        {
            instance = this;
            sabersByName = new Dictionary<string, SaberInfo>();
            sabersByName.Add(Plugin.playerController.leftSaber.name, new SaberInfo { eulerRotation = Vector3.zero, offset = Vector3.zero, saber = Plugin.playerController.leftSaber, isMainSaber = true });
            sabersByName.Add(Plugin.playerController.rightSaber.name, new SaberInfo { eulerRotation = Vector3.zero, offset = Vector3.zero, saber = Plugin.playerController.rightSaber, isMainSaber = true });

            // container heists!
            ISMCCContainer = GameObject.FindObjectsOfType<MonoInstallerBase>()
                .Select(installer => installer.GetProperty<DiContainer>("Container"))
                .Where(container => container != null && container.HasBinding<ISaberModelController>())
                .FirstOrDefault();
            if (ISMCCContainer == null) Logger.log.Error("Container heist 1 failed!");
            colorManagerContainer = GameObject.FindObjectsOfType<MonoInstallerBase>()
                .Select(installer => installer.GetProperty<DiContainer>("Container"))
                .Where(container => container != null && container.HasBinding<ColorManager>())
                .FirstOrDefault();
            if (colorManagerContainer == null) Logger.log.Error("Container heist 2 failed!");
        }
        public void Update()
        {
            foreach (var saberInfo in sabersByName.Values)
            {
                if (!saberInfo.isMainSaber)
                {
                    saberInfo.saber.ManualUpdate();
                }
            }
        }
        public void addSaber(string name, Vector3 eulerRotation, Vector3 offset, Saber.SaberType type, bool isLeftHand)
        {
            var oldSaber = isLeftHand ? Plugin.playerController.leftSaber : Plugin.playerController.rightSaber;
            /*
            var oldBSMC = (oldSaber
                .GetComponent<SaberModelContainer>()
                .GetField<ISaberModelController>("_saberModelController")
                as BasicSaberModelController);
            oldBSMC.gameObject.name = oldSaber.gameObject.name + " Model";
            */

            // disable the template saber so the new one will start out disabled
            oldSaber.gameObject.SetActive(false);

            var newSaber = GameObject.Instantiate(oldSaber.gameObject).GetComponent<Saber>();
            newSaber.name = name;

            // unfortunately, getting extra sabers working requires child sacrifice
            GameObject.Destroy(newSaber.GetComponentInChildren<SaberWeaponTrail>(true));

            // so what's actually going on here:
            // the saber's model is a separate GameObject that's a child of the saber, and gets instantiated by Zenject at runtime
            // instantiating the saber copies the model, which then SCREAMS AT YOU because its references weren't copied
            // so we kill the screaming malformed child, get Zenject to give us a new one, and pretend this never happened
            ISMCCContainer.InjectGameObject(newSaber.gameObject);

            var smc = newSaber.GetComponent<SaberModelContainer>();
            var bsmc = smc.GetField<ISaberModelController>("_saberModelController")
                as BasicSaberModelController;
            bsmc.gameObject.name = name + " Model";

            // it is now safe to turn on your old saber
            oldSaber.gameObject.SetActive(true);

            // set the saber's type
            var sto = newSaber.GetComponent<SaberTypeObject>();
            sto.SetField("_saberType", type);

            // not sure if these are actually necessary, but just in case
            newSaber.SetField("_saberType", sto);
            smc.SetField("_saberTypeObject", sto);

            // one could imagine a world in which the ColorManager is stored in a separate container from the saber model prefab
            // while this is not currently the case... best to be prepared
            if (!ISMCCContainer.HasBinding<ColorManager>()) colorManagerContainer.InjectGameObject(bsmc.gameObject);
            var trail = bsmc.GetField<SaberWeaponTrail>("_saberWeaponTrail");
            if (!ISMCCContainer.HasBinding<ColorManager>()) colorManagerContainer.InjectGameObject(trail.gameObject);

            // everything's set up; activate the new saber!
            newSaber.gameObject.SetActive(true);

            // record the info about how to rotate the new saber
            sabersByName.Add(name, new SaberInfo
            {
                eulerRotation = eulerRotation,
                offset = offset,
                saber = newSaber,
                isMainSaber = false,
            });
        }
        public void OnDestroy()
        {
            instance = null;
        }
    }
}
