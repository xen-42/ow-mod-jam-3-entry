using UnityEngine;

namespace UnofficialJam3Entry;

internal class PartyInvitationSignal : MonoBehaviour
{
    private string _requiredCondition = "KnowsItsGranitesBirthday";

    public void Start()
    {
        GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);

        OnDialogueConditionChanged(_requiredCondition, DialogueConditionManager.SharedInstance.GetConditionState(_requiredCondition));
    }

    public void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    private void OnDialogueConditionChanged(string conditionName, bool conditionState)
    {
        if (conditionName == _requiredCondition)
        {
            this.gameObject.SetActive(conditionState);
        }
    }
}
