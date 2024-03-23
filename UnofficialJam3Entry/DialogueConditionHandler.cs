using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        if (DialogueConditionManager.SharedInstance.GetConditionState("SecretRecordingKaboom"))
        {
            GameObject.Find("Gravelrock_Body/Sector/SecretRecording/Explosion").GetComponent<ExplosionController>().Play();
        }
    }
}
