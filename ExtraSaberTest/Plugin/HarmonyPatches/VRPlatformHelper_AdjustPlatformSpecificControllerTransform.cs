using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtraSaberTest.HarmonyPatches
{
    [HarmonyPatch(typeof(VRPlatformHelper), "AdjustPlatformSpecificControllerTransform")]
    public class VRPlatformHelper_AdjustPlatformSpecificControllerTransform
    {
        public static void Postfix(ref Transform transform)
        {
            // look up if this is a saber by name (and if so, how to adjust it)
            ExtraSaberManager.SaberInfo info = default;
            if (!(ExtraSaberManager.instance?.sabersByName?.TryGetValue(transform.name, out info) ?? false)) return;
            //Logger.log.Info("Transforming saber: " + transform.name);
            transform.Translate(info.offset);
            transform.Rotate(info.eulerRotation);
        }
    }
}
