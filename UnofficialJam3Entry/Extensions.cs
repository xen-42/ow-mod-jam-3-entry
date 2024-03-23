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
}
