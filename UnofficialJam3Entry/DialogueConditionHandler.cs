using UnityEngine;

namespace UnofficialJam3Entry;

internal static class DialogueConditionHandler
{
    public static void Setup()
    {
        GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
    }

    private static void OnExitConversation()
    {
        if (UnofficialJam3Entry.NHAPI.GetCurrentStarSystem() != "Jam3")
        {
            return;
        }

        if (DialogueConditionManager.SharedInstance.GetConditionState("SecretRecordingKaboom"))
        {
            GameObject.Find("Gravelrock_Body/Sector/ExplosionRoot/Explosion")?.GetComponentInChildren<ExplosionController>()?.Play();
            GameObject.Find("Gravelrock_Body/Sector/ExplosionRoot/Explosion")?.GetComponentInChildren<OWAudioSource>()?.PlayOneShot(AudioType.ShipDamageShipExplosion);
            GameObject.Find("Gravelrock_Body/Sector/ExplosionRoot/SecretRecording")?.gameObject?.SetActive(false);
        }
    }
}
