using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace UnofficialJam3Entry;

internal static class Extensions
{
    public static string GetPath(this Transform current)
    {
        if (current.parent == null) return current.name;
        return current.parent.GetPath() + "/" + current.name;
    }

    public static List<XmlNode> GetChildNodes(this XmlNode parentNode, string tagName)
    {
        return parentNode.ChildNodes.Cast<XmlNode>().Where(node => node.LocalName == tagName).ToList();
    }

    public static XmlNode GetChildNode(this XmlNode parentNode, string tagName)
    {
        return parentNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(node => node.LocalName == tagName);
    }

    public static Bounds GetBoundsOfSelfAndChildMeshes(this GameObject gameObject)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        var corners = renderers.SelectMany(renderer => GetMeshCorners(renderer, gameObject)).ToList();

        if (corners.Count == 0)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }
        else
        {
            var bounds = new Bounds(corners[0], Vector3.zero);
            corners.ForEach(bounds.Encapsulate);

            return bounds;
        }
    }

    private static Vector3[] GetMeshCorners(Renderer renderer, GameObject relativeTo)
    {
        var bounds = renderer.bounds;

        var localCorners = new Vector3[]
        {
                 bounds.min,
                 bounds.max,
                 new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                 new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                 new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                 new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                 new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                 new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
        };

        var globalCorners = localCorners.Select(renderer.transform.TransformPoint).ToArray();

        if (relativeTo == null) return globalCorners;

        return globalCorners.Select(relativeTo.transform.InverseTransformPoint).ToArray();
    }
}
