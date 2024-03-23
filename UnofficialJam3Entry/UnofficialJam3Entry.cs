using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace UnofficialJam3Entry
{
    public class UnofficialJam3Entry : ModBehaviour
    {
        private INewHorizons _newHorizons;

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(UnofficialJam3Entry)} is loaded!", MessageType.Success);

            _newHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            _newHorizons.LoadConfigs(this);

            DialogueConditionHandler.Setup();

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                if (_newHorizons.GetCurrentStarSystem() == "Jam3")
                {
                    OnSystemLoaded();
                }
            };
        }

        private void OnSystemLoaded()
        {

        }
    }
}