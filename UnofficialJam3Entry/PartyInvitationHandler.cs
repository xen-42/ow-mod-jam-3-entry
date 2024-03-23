using Epic.OnlineServices.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace UnofficialJam3Entry;

internal class PartyInvitationHandler : MonoBehaviour
{
    // For blinking while they go to the party
    private PlayerCameraEffectController _playerCameraEffectController;

    private Dictionary<string, GameObject> _invitationIDs = new();
    private HashSet<string> _uniqueIDs = new();

    private int _inviteCount = 0;
    private int _gravelrockInviteCount = 0;

    private PartyPlacementHandler _placementHandler;

    public void Start()
    {
        _placementHandler = this.GetComponent<PartyPlacementHandler>();

        // CharacterDialogueTrees are late initialized
        UnofficialJam3Entry.Helper.Events.Unity.FireInNUpdates(() =>
        {
            // Find all potential partygoers
            foreach (var dialogue in GameObject.FindObjectsOfType<CharacterDialogueTree>())
            {
                try
                {
                    dialogue.VerifyInitialized();

                    var nomai = dialogue.GetComponentInParent<SolanumAnimController>()?.transform;
                    var traveler = dialogue.GetComponentInParent<TravelerController>()?.transform;
                    var hearthian = dialogue.GetComponentInParent<CharacterAnimController>()?.transform;
                    var other = dialogue.GetComponentInParent<FacePlayerWhenTalking>()?.transform;

                    var parent = nomai ?? traveler ?? hearthian ?? other;

                    // Don't invite Granite to their own party
                    if (dialogue.transform.GetPath().Contains("Gravelrock_Body/Sector/Granite")) continue;

                    if (parent)
                    {
                        Invite(parent.gameObject, dialogue);

                        UnofficialJam3Entry.WriteDebug($"Successfully invited {dialogue._characterName} to the party!");
                    }
                    else
                    {
                        UnofficialJam3Entry.WriteDebug($"Couldn't invite {dialogue?._characterName ?? "NO NAME"} at {dialogue.transform.GetPath()} to the party since they didn't have a good parent object.");
                    }
                }
                catch (Exception ex)
                {
                    UnofficialJam3Entry.WriteDebug($"Couldn't invite {dialogue?._characterName ?? "NO NAME"} at {dialogue.transform.GetPath()} to the party - {ex}");
                }
            }
        }, 3);

        _playerCameraEffectController = GameObject.FindObjectOfType<PlayerCameraEffectController>();
        GlobalMessenger.AddListener("ExitConversation", OnExitConversation);
        GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    public void OnDestroy()
    {
        GlobalMessenger.RemoveListener("ExitConversation", OnExitConversation);
        GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    private string _queuedConditionID;

    private void OnDialogueConditionChanged(string conditionName, bool conditionState)
    {
        UnofficialJam3Entry.WriteDebug($"CONDITION {conditionName}");
        if (conditionState && _invitationIDs.Keys.Contains(conditionName))
        {
            UnofficialJam3Entry.WriteDebug($"Queuing up invitation for {conditionName}");
            _queuedConditionID = conditionName;
        }
    }

    private void OnExitConversation()
    {
        if (!string.IsNullOrEmpty(_queuedConditionID) && _invitationIDs.TryGetValue(_queuedConditionID, out var characterObj))
        {
            UnofficialJam3Entry.WriteDebug($"Moving {_queuedConditionID} to the party!");

            if (characterObj.GetAttachedOWRigidbody().name == "Gravelrock_Body")
            {
                _gravelrockInviteCount++;
            }
            _inviteCount++;

            var hasAllFriends = _gravelrockInviteCount >= 4;
            var hasBonusFriends = _inviteCount > _gravelrockInviteCount;

            DialogueConditionManager.SharedInstance.SetConditionState("HasSomePartyGoers", hasAllFriends && !hasBonusFriends);
            DialogueConditionManager.SharedInstance.SetConditionState("HasManyPartyGoers", hasAllFriends && hasBonusFriends);

            StartCoroutine(Coroutine(characterObj));
            _queuedConditionID = null;
        }
    }

    private IEnumerator Coroutine(GameObject characterObj)
    {
        OWInput.ChangeInputMode(InputMode.None);
        Locator.GetPauseCommandListener().AddPauseCommandLock();

        _playerCameraEffectController.CloseEyes(0.7f);
        yield return new WaitForSeconds(0.7f);

        // Eyes closed: swap character state
        _placementHandler.PlacePartygoer(characterObj);

        yield return new WaitForSeconds(0.3f);

        // Open eyes
        _playerCameraEffectController.OpenEyes(0.7f);

        OWInput.ChangeInputMode(InputMode.Character);
        Locator.GetPauseCommandListener().RemovePauseCommandLock();
    }

    private void Invite(GameObject root, CharacterDialogueTree dialogue)
    {
        var uniqueName = dialogue._characterName;

        // Append a number if two people have the same name
        // I imagine there will be Ernesto duplicates and stuff
        var i = 2;
        while (_uniqueIDs.Contains(uniqueName))
        {
            uniqueName = $"{dialogue._characterName} {i++}";
        }

        var uniqueCondition = $"{uniqueName}WasInvited";

        // Now we adjust their dialogue
        // Probably just always have the option to invite them?
        var existingText = dialogue._xmlCharacterDialogueAsset.text;

        var existingDialogueDoc = new XmlDocument();
        existingDialogueDoc.LoadXml(existingText);
        var existingDialogueTree = existingDialogueDoc.DocumentElement.SelectSingleNode("//DialogueTree");

        // Add the invitation option to each node that has options
        var wasInvited = false;
        foreach (XmlNode existingDialogueNode in existingDialogueTree.GetChildNodes("DialogueNode"))
        {
            var options = existingDialogueNode.GetChildNode("DialogueOptionsList");
            if (options != null)
            {
                AddToDialogueOptionsList(options, uniqueCondition);

                wasInvited = true;
            }
        }

        if (!wasInvited)
        {
            // Was not able to insert an invitation option
            // Add to all nodes
            foreach (XmlNode existingDialogueNode in existingDialogueTree.GetChildNodes("DialogueNode"))
            {
                var options = existingDialogueDoc.CreateElement("DialogueOptionsList");
                existingDialogueNode.AppendChild(options);
                AddToDialogueOptionsList(options, uniqueCondition);
                wasInvited = true;
            }
        }

        if (!wasInvited)
        {
            throw new Exception("Couldn't add invitation to dialogue at all");
        }

        // Add the dialogue node where they accept
        var inviteNode = existingDialogueDoc.CreateElement("DialogueNode");
        inviteNode.InnerText = "<Name>GranitePartyInvite</Name>" +
            "<Dialogue><Page>I'd love to! I'll go right over!</Page></Dialogue>" +
            $"<SetCondition>{uniqueCondition}</SetCondition>";

        existingDialogueTree.AppendChild(inviteNode);

        var name = dialogue._xmlCharacterDialogueAsset.name;
        // The symbols are all wrong
        var text = existingDialogueDoc.OuterXml.Replace("&lt;", "<").Replace("&gt;", ">");

        //UnofficialJam3Entry.WriteDebug(name);
        //UnofficialJam3Entry.WriteDebug(text);

        // Create a new dialogue node through NH so that it runs all the proper stuff on it, we just want the final dialogue asset
        var tempGameObject = new GameObject("TEMP");
        var (editedDialogue, _) = UnofficialJam3Entry.NHAPI.CreateDialogueFromXML(name, text, "{ }", tempGameObject);

        // Replace the xml
        dialogue.SetTextXml(editedDialogue._xmlCharacterDialogueAsset);

        // Clean up the temp asset
        GameObject.Destroy(tempGameObject);

        var signal = UnofficialJam3Entry.NHAPI.SpawnSignal(UnofficialJam3Entry.Instance, root, "ToolProbeFlight_LP", uniqueName, "Comms Network", detectionRadius: 0);
        signal.transform.parent = root.transform;
        signal.transform.localPosition = Vector3.zero;
        signal.gameObject.AddComponent<PartyInvitationSignal>();

        _uniqueIDs.Add(uniqueName);
        _invitationIDs[uniqueCondition] = root;
    }

    private void AddToDialogueOptionsList(XmlNode options, string uniqueCondition)
    {
        var newOption = options.OwnerDocument.CreateElement("DialogueOption");
        newOption.InnerText = $"<RequiredCondition>KnowsItsGranitesBirthday</RequiredCondition>" +
            $"<CancelledCondition>{uniqueCondition}</CancelledCondition>" +
            "<Text>Want to go to Granite's birthday party?</Text>" +
            "<DialogueTarget>GranitePartyInvite</DialogueTarget>";
        var firstNode = options.GetChildNode("DialogueOption");
        if (firstNode == null)
        {
            options.AppendChild(newOption);
        }
        else
        {
            options.InsertBefore(newOption, firstNode);
        }
    }
}
