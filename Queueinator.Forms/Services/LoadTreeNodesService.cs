using Queueinator.Domain.RabbitMq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Queueinator.Forms.Services
{
    public class LoadTreeNodesService
    {
        public void LoadNode<T, TTree>(TreeNode parentNode,
            List<T> elements,
            Func<T, TreeNode, TTree> buildTree,
            Func<String, IEnumerable<T>, bool, String> buildText,
            Func<String, T, String> buildKey,
            ref Dictionary<String, TTree> refElements,
            Action<TreeNode, T, TTree> decorateLastNode) where T : INode
        {
            LoadNode(
                parentNode,
                elements,
                buildTree,
                buildText,
                buildKey,
                ref refElements,
                decorateLastNode,
                0);
        }

        private void LoadNode<T, TTree>(
            TreeNode parentNode,
            List<T> elementsToAdd,
            Func<T, TreeNode, TTree> buildTree,
            Func<String, IEnumerable<T>, bool, String> buildText,
            Func<String, T, String> buildKey,
            ref Dictionary<String, TTree> refElements,
            Action<TreeNode, T, TTree> decorateNode,
            int depth) where T : INode
        {
            if (!elementsToAdd.Any()) return;

            char[] elementNameSeparator = new[] { '-', '.' };

            var groupedElements = elementsToAdd.GroupBy(x => x.Name.TrimEnd(elementNameSeparator).Split(elementNameSeparator)[depth]);

            foreach (var item in groupedElements)
            {
                var lastNodeItems = item.Where(x => x.Name.TrimEnd(elementNameSeparator).Count(y => elementNameSeparator.Contains(y)) == depth);

                var newItems = item.Except(lastNodeItems).ToList();

                if (!newItems.Any())
                {
                    var text = buildText(item.Key, lastNodeItems, true);

                    var lastElement = lastNodeItems.First();

                    if (!refElements.ContainsKey(buildKey(item.Key, lastElement)))
                    {
                        var elementNode = AddNode(parentNode, buildKey(item.Key, lastElement), text);

                        decorateNode(elementNode, lastElement, default);

                        refElements.Add(buildKey(item.Key, lastElement), buildTree(lastElement, elementNode));
                    }
                    else
                    {
                        try
                        {
                            var oldTree = refElements[buildKey(item.Key, lastElement)];

                            var node = parentNode.Nodes.Find(buildKey(item.Key, lastElement), false)[0];
                            if (node.Text != text)
                                node.Text = text;
                            decorateNode(node, lastElement, oldTree);

                            refElements[buildKey(item.Key, lastElement)] = buildTree(lastElement, node);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
                else
                {
                    TreeNode currentNode;

                    var text = buildText(item.Key, item, false);

                    if (parentNode.Nodes.ContainsKey(item.Key))
                    {
                        currentNode = parentNode.Nodes.Find(item.Key, false)[0];

                        if (currentNode.Text != text)
                            currentNode.Text = text;
                    }
                    else
                    {
                        currentNode = AddNode(parentNode, item.Key, text);
                    }

                    LoadNode(currentNode, newItems, buildTree, buildText, buildKey, ref refElements, decorateNode, depth + 1);
                }
            }
        }

        private TreeNode AddNode(TreeNode parent, string name, string text)
        {
            var treeNode = new TreeNode()
            {
                Name = name,
                Text = text,
                ImageIndex = 0,
                SelectedImageIndex = 0
            };

            parent.Nodes.Add(treeNode);

            return treeNode;
        }
    }
}
