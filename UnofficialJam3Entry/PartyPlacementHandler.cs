using System;
using UnityEngine;

namespace UnofficialJam3Entry;

internal class PartyPlacementHandler : MonoBehaviour
{
    private Vector3[] _rootPlacementAreas = new Vector3[]
    {          
        new Vector3(100.1656f, -18.40758f, -6.820961f),
        new Vector3(99.33324f, -24.4555f, 2.872986f),
        new Vector3(97.25172f, -28.8805f, -3.560364f),
        new Vector3(105.5628f, -7.296196f, -14.75704f),
        new Vector3(102.0077f, -22.73014f, 9.733749f),
        new Vector3(97.40989f, -27.29577f, -7.292572f),
        new Vector3(97.20736f, -30.40901f, -15.26901f)
    };

    private Transform _rootSector, _campfire;

    public void Start()
    {
        _rootSector = GameObject.Find("Gravelrock_Body/Sector").transform;
        _campfire = GameObject.Find("Gravelrock_Body/Sector/Campfire").transform;
    }

    private (Vector3 position, Vector3 normal) GetPossiblePosition()
    {
        // 5 attempts to get a spot
        for (int i = 0; i < 5; i++)
        {
            // Centers of cylinders we can place them in
            var center = _rootPlacementAreas[UnityEngine.Random.Range(0, _rootPlacementAreas.Length)];
            var pos = UnityEngine.Random.insideUnitCircle * 3f;
            var up = center.normalized;
            var pos3D = (Quaternion.FromToRotation(Vector3.up, up) * new Vector3(pos.x, pos.y, 0)) + center;

            // Raycast to find the exact spot on the ground
            int layerMask = OWLayerMask.interactMask | OWLayerMask.physicalMask; 

            // In world space
            var origin = _rootSector.TransformPoint(pos3D + up * 5f);
            var direction = _rootSector.TransformDirection(-up);

            if (Physics.Raycast(origin, direction, out var hitInfo, 100f, layerMask))
            {
                if (hitInfo.collider.name == "BatchedMeshColliders_0")
                {
                    var newPos = hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point);
                    var norm = hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.normal);

                    // In local space
                    return (newPos, norm);
                }
            }
        }

        throw new Exception("Failed to find a place");
    }

    public void PlacePartygoer(GameObject partygoer)
    {
        try
        {
            var (pos, norm) = GetPossiblePosition();
            var up = pos.normalized;
            var localRotation = Quaternion.LookRotation(_campfire.localPosition - pos, up).eulerAngles;

            // Ernesto spawns in the ground
            if (partygoer.transform.Find("Beast_Anglerfish") != null)
            {
                pos += up * 1.5f;
            }

            //var bounds = partygoer.GetBoundsOfSelfAndChildMeshes();
            //var yOffset = bounds.center.y - bounds.extents.y;

            // Spawn using NH to handle sector type stuff
            var newPartygoer = UnofficialJam3Entry.NHAPI.SpawnObject(_rootSector.parent.gameObject, _rootSector.GetComponent<Sector>(), partygoer.transform.GetPath(), 
                pos, localRotation, partygoer.transform.localScale.x, false);

            UnofficialJam3Entry.Helper.Events.Unity.FireOnNextUpdate(() => FixStuff(newPartygoer));

            partygoer.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            UnofficialJam3Entry.WriteDebug($"Failed to move {partygoer.name} to the party! {e}");
            partygoer.gameObject.SetActive(false);
            DialogueConditionManager.SharedInstance.SetConditionState("FailedToPlacePartygoer", true);
        }
    }

    private void FixStuff(GameObject newPartygoer)
    {
        // NH is turning the colliders off sometimes for some reason?
        foreach (var collider in newPartygoer.GetComponentsInChildren<OWCollider>(true))
        {
            collider.enabled = true;
            collider._collider.enabled = true;
        }

        foreach (var shape in newPartygoer.GetComponentsInChildren<Shape>(true))
        {
            shape.enabled = true;
        }

        // FacePlayerWhenTalking is also getting goofy
        if (newPartygoer.GetComponent<FacePlayerWhenTalking>() is FacePlayerWhenTalking facePlayerWhenTalking)
        {
            facePlayerWhenTalking._origLocalRotation = facePlayerWhenTalking.transform.localRotation;
            facePlayerWhenTalking._targetLocalRotation = facePlayerWhenTalking.transform.localRotation;
        }
    }
}
