using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tests.Models
{
    // На сервер значения передаются в одноименные свойства, а не поля.
    public class TreeViewNode_Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        
    }

    public class TreeViewNode: Node
    {
        // На сервер значения передаются в одноименные свойства, а не поля.
        public TreeViewNode_Group[] Groups { get; set; }
        public TreeViewNode(Node node)
        {
            Action = node.Action;
            Area = node.Area;
            Controller = node.Controller;
            Description = node.Description;
            HasChildren = node.HasChildren;
            Id = node.Id;
            Name = node.Name;
            ParentId = node.ParentId;
            ParentName = node.ParentName;
        }
        public TreeViewNode() { }
    }
}