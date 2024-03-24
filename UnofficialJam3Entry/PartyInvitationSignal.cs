using UnityEngine;

namespace UnofficialJam3Entry;

internal class PartyInvitationSignal : MonoBehaviour
{
    private string _requiredCondition = "KnowsItsGranitesBirthday";
    public string disableCondition;
    private bool _hasStarted;

    public void Start()
    {
        GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);

        // Wait for the condition
        gameObject.SetActive(false);
    }

    public void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    private void OnDialogueConditionChanged(string conditionName, bool conditionState)
    {
        if (conditionName == _requiredCondition && conditionState)
        {
            _hasStarted = true;
        }

        // Only start actively checking after everything is enabled
        // This way we keep the Gravelrock signals in the list until theyre ready to go
        if (_hasStarted)
        {
            CheckShouldBeActive();
        }
    }

    private void CheckShouldBeActive()
    {
        // Set active if they've talked to Granite, also only have X active at once
        //var hasRoom = PartyInvitationHandler.ActiveSignals.Count < 20 || PartyInvitationHandler.ActiveSignals.Contains(gameObject);
        var hasRoom = true; // TODO, make this an option you can turn on, will see how performance is when other mods are out
        var hasCondition = DialogueConditionManager.SharedInstance.GetConditionState(_requiredCondition);
        var active = hasRoom && hasCondition;

        if (active && !PartyInvitationHandler.ActiveSignals.Contains(gameObject))
        {
            PartyInvitationHandler.ActiveSignals.Add(gameObject);
        }
        else if (!active && PartyInvitationHandler.ActiveSignals.Contains(gameObject))
        {
            PartyInvitationHandler.ActiveSignals.Remove(gameObject);
        }

        gameObject.SetActive(active);

        // Disable them when they are invited
        if (!string.IsNullOrEmpty(disableCondition))
        {
            if (DialogueConditionManager.SharedInstance.GetConditionState(disableCondition))
            {
                if (PartyInvitationHandler.ActiveSignals.Contains(gameObject))
                {
                    PartyInvitationHandler.ActiveSignals.Remove(gameObject);
                }

                GameObject.Destroy(gameObject);
                return;
            }

        }
    }
}
