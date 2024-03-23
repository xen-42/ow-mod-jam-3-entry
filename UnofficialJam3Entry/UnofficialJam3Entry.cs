using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;

namespace UnofficialJam3Entry
{
    public class UnofficialJam3Entry : ModBehaviour
    {
        public static UnofficialJam3Entry Instance { get; private set; }
        public static IModHelper Helper { get; private set; }
        public static INewHorizons NHAPI { get; private set; }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(UnofficialJam3Entry)} is loaded!", MessageType.Success);

            Instance = this;
            Helper = this.ModHelper;

            NHAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NHAPI.LoadConfigs(this);

            DialogueConditionHandler.Setup();

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                if (NHAPI.GetCurrentStarSystem() == "Jam3")
                {
                    OnSystemLoaded();
                }
            };
        }

        private void OnSystemLoaded()
        {
            new GameObject(nameof(PartyHandler)).AddComponent<PartyHandler>();
        }

        public static void WriteDebug(string msg)
        {
#if DEBUG
            Helper.Console.WriteLine($"DEBUG - {msg}");
#endif
        }
    }
}