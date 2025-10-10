using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcSiteMapProvider.Web.Html.Models;

/// <summary>
///     SiteMapNodeModel
/// </summary>
public class SiteMapNodeModel
    : ISortable
{
    private readonly bool _drillDownToCurrent;

    private readonly ISiteMapNode _node;
    private readonly bool _startingNodeInChildLevel;
    private SiteMapNodeModelList? _ancestors;
    private SiteMapNodeModelList? _children;
    private SiteMapNodeModelList? _descendants;
    private int _maxDepth;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SiteMapNodeModel" /> class.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="sourceMetadata">The source metadata provided by the HtmlHelper.</param>
    public SiteMapNodeModel(ISiteMapNode node, IDictionary<string, object?> sourceMetadata)
        : this(node, sourceMetadata, int.MaxValue, true, false, node.SiteMap.VisibilityAffectsDescendants)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SiteMapNodeModel" /> class.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="sourceMetadata">The source metadata provided by the HtmlHelper.</param>
    /// <param name="maxDepth">The max depth.</param>
    /// <param name="drillDownToCurrent">Should the model exceed the maxDepth to reach the current node</param>
    /// <param name="startingNodeInChildLevel">Renders startingNode in child level if set to <c>true</c>.</param>
    public SiteMapNodeModel(ISiteMapNode node, IDictionary<string, object?> sourceMetadata, int maxDepth,
        bool drillDownToCurrent, bool startingNodeInChildLevel, bool visibilityAffectsDescendants)
    {
        if (maxDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth));
        }

        _node = node ?? throw new ArgumentNullException(nameof(node));
        _maxDepth = maxDepth;
        _startingNodeInChildLevel = startingNodeInChildLevel;
        _drillDownToCurrent = drillDownToCurrent;
        SourceMetadata = sourceMetadata ?? throw new ArgumentNullException(nameof(sourceMetadata));

        Key = node.Key;
        Area = node.Area;
        Controller = node.Controller;
        Action = node.Action;
        Title = node.Title;
        Description = node.Description;
        TargetFrame = node.TargetFrame;
        ImageUrl = node.ImageUrl;
        Url = node.Url;
        CanonicalUrl = node.CanonicalUrl;
        MetaRobotsContent = node.GetMetaRobotsContentString();
        IsCurrentNode = node.Equals(node.SiteMap.CurrentNode);
        IsInCurrentPath = node.IsInCurrentPath();
        IsRootNode = node.Equals(node.SiteMap.RootNode);
        IsClickable = node.Clickable;
        VisibilityAffectsDescendants = visibilityAffectsDescendants;
        RouteValues = node.RouteValues;
        Attributes = node.Attributes;
        Order = node.Order;
    }

    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    /// <value>The key.</value>
    public string Key { get; protected set; }

    /// <summary>
    ///     Gets or sets the area.
    /// </summary>
    /// <value>The area.</value>
    public string Area { get; protected set; }

    /// <summary>
    ///     Gets or sets the controller.
    /// </summary>
    /// <value>The controller.</value>
    public string Controller { get; protected set; }

    /// <summary>
    ///     Gets or sets the action.
    /// </summary>
    /// <value>The action.</value>
    public string Action { get; protected set; }

    /// <summary>
    ///     Gets or sets the URL.
    /// </summary>
    /// <value>The URL.</value>
    public string Url { get; protected set; }

    /// <summary>
    ///     Gets or sets the canonical URL.
    /// </summary>
    /// <value>The canonical URL.</value>
    public string CanonicalUrl { get; protected set; }

    /// <summary>
    ///     Gets or sets the content value of the meta robots tag.
    /// </summary>
    /// <value>The content value of the meta robots tag.</value>
    public string MetaRobotsContent { get; protected set; }

    /// <summary>
    ///     Gets or sets the title.
    /// </summary>
    /// <value>The title.</value>
    public string Title { get; protected set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    public string Description { get; protected set; }

    /// <summary>
    ///     Gets or sets the target frame.
    /// </summary>
    /// <value>The target frame.</value>
    public string TargetFrame { get; protected set; }

    /// <summary>
    ///     Gets or sets the image URL.
    /// </summary>
    /// <value>The image URL.</value>
    public string ImageUrl { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is current node.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is current node; otherwise, <c>false</c>.
    /// </value>
    public bool IsCurrentNode { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is in current path.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is in current path; otherwise, <c>false</c>.
    /// </value>
    public bool IsInCurrentPath { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is root node.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is root node; otherwise, <c>false</c>.
    /// </value>
    public bool IsRootNode { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is clickable.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is clickable; otherwise, <c>false</c>.
    /// </value>
    public bool IsClickable { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the visibility property of the current node
    ///     will affect the descendant nodes.
    /// </summary>
    /// <value>
    ///     <c>true</c> if visibility should affect descendants; otherwise, <c>false</c>.
    /// </value>
    public bool VisibilityAffectsDescendants { get; protected set; }

    /// <summary>
    ///     Gets or sets the route values.
    /// </summary>
    /// <value>The route values.</value>
    public IDictionary<string, object> RouteValues { get; protected set; }

    /// <summary>
    ///     Gets or sets the meta attributes.
    /// </summary>
    /// <value>The meta attributes.</value>
    public IDictionary<string, object> Attributes { get; protected set; }

    /// <summary>
    ///     Gets or sets the source metadata generated by the HtmlHelper.
    /// </summary>
    /// <value>The source metadata.</value>
    public IDictionary<string, object?> SourceMetadata { get; protected set; }

    /// <summary>
    ///     Gets the children.
    /// </summary>
    public SiteMapNodeModelList Children
    {
        get
        {
            if (_children == null)
            {
                _children = [];
                if (ReachedMaximalNodelevel(_maxDepth, _node, _drillDownToCurrent) && _node.HasChildNodes)
                {
                    var sortedNodes = SortSiteMapNodes(_node.ChildNodes);

                    if (VisibilityAffectsDescendants)
                    {
                        foreach (var child in sortedNodes)
                        {
                            if (child.IsAccessibleToUser() && child.IsVisible(SourceMetadata) && _maxDepth > 0)
                            {
                                _children.Add(new SiteMapNodeModel(child, SourceMetadata, _maxDepth - 1,
                                    _drillDownToCurrent, false, VisibilityAffectsDescendants));
                            }
                        }
                    }
                    else
                    {
                        foreach (var child in sortedNodes)
                        {
                            if (!child.IsAccessibleToUser())
                            {
                                continue;
                            }

                            if (child.IsVisible(SourceMetadata) && _maxDepth > 0)
                            {
                                _children.Add(new SiteMapNodeModel(child, SourceMetadata, _maxDepth - 1,
                                    _drillDownToCurrent, false, VisibilityAffectsDescendants));
                            }
                            else if
                                (_maxDepth >
                                 1) //maxDepth should be greater than 1 to be allowed to descent another level
                            {
                                var nearestVisibleDescendants = new List<SiteMapNodeModel>();
                                FindNearestVisibleDescendants(child, _maxDepth - 1, ref nearestVisibleDescendants);
                                var sortedDescendants = SortSiteMapNodes(nearestVisibleDescendants);
                                _children.AddRange(sortedDescendants);
                            }
                        }
                    }
                }
            }

            if (!_startingNodeInChildLevel)
            {
                return _children;
            }

            // Return children and reset the children collection to avoid returning the same children again
            var childrenRes = _children;
            _children = [];
            _maxDepth = 0;
            return childrenRes;
        }
    }

    /// <summary>
    ///     Gets the parent
    /// </summary>
    public SiteMapNodeModel? Parent
    {
        get => _node.ParentNode == null
            ? null
            : new SiteMapNodeModel(_node.ParentNode, SourceMetadata,
                _maxDepth == int.MaxValue ? int.MaxValue : _maxDepth + 1, _drillDownToCurrent,
                _startingNodeInChildLevel, VisibilityAffectsDescendants);
    }

    /// <summary>
    ///     Gets the descendants.
    /// </summary>
    public SiteMapNodeModelList Descendants
    {
        get
        {
            if (_descendants != null)
            {
                return _descendants;
            }

            _descendants = [];
            GetDescendants(this);
            return _descendants;
        }
    }

    /// <summary>
    ///     Gets the ancestors.
    /// </summary>
    public SiteMapNodeModelList Ancestors
    {
        get
        {
            if (_ancestors != null)
            {
                return _ancestors;
            }

            _ancestors = [];
            GetAncestors(this);
            return _ancestors;
        }
    }

    /// <summary>
    ///     Gets the order of the node relative to its sibling nodes.
    /// </summary>
    public int Order { get; protected set; }

    private void FindNearestVisibleDescendants(ISiteMapNode node, int maxDepth,
        ref List<SiteMapNodeModel> nearestVisibleDescendants)
    {
        foreach (var child in node.ChildNodes)
        {
            if (!child.IsAccessibleToUser())
            {
                continue;
            }

            if (child.IsVisible(SourceMetadata))
            {
                nearestVisibleDescendants.Add(new SiteMapNodeModel(child, SourceMetadata, maxDepth - 1,
                    _drillDownToCurrent, false, VisibilityAffectsDescendants));
            }
            else if (maxDepth > 1) //maxDepth should be greater than 1 to be allowed to descent another level
            {
                FindNearestVisibleDescendants(child, maxDepth - 1, ref nearestVisibleDescendants);
            }
        }
    }

    private static IEnumerable<T> SortSiteMapNodes<T>(IList<T> nodesToSort) where T : ISortable
    {
        if (nodesToSort.Any(x => x.Order != 0))
        {
            return nodesToSort.OrderBy(x => x.Order);
        }

        return nodesToSort;
    }

    /// <summary>
    ///     Test if the maximal node level has not been reached
    /// </summary>
    /// <param name="maxDepth">The normal max depth.</param>
    /// <param name="node">The starting node</param>
    /// <param name="drillDownToCurrent">Should the model exceed the maxDepth to reach the current node</param>
    /// <returns></returns>
    private static bool ReachedMaximalNodelevel(int maxDepth, ISiteMapNode node, bool drillDownToCurrent)
    {
        if (maxDepth > 0)
        {
            return true;
        }

        if (!drillDownToCurrent)
        {
            return false;
        }

        if (node.IsInCurrentPath())
        {
            return true;
        }

        if (node.ParentNode?.Equals(node.SiteMap.CurrentNode) ?? false)
        {
            return true;
        }

        if (node.ParentNode?.ChildNodes == null)
        {
            return false;
        }

        foreach (var sibling in node.ParentNode.ChildNodes)
        {
            if (sibling.IsInCurrentPath())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Retrieve all descendants
    /// </summary>
    /// <param name="siteMapNodeModel">the node</param>
    /// <returns></returns>
    private void GetDescendants(SiteMapNodeModel siteMapNodeModel)
    {
        foreach (var child in SortSiteMapNodes(siteMapNodeModel.Children))
        {
            _descendants?.Add(child);
            GetDescendants(child);
        }
    }

    /// <summary>
    ///     Retrieve all ancestors
    /// </summary>
    /// <param name="siteMapNodeModel">the node</param>
    /// <returns></returns>
    private void GetAncestors(SiteMapNodeModel siteMapNodeModel)
    {
        while (true)
        {
            if (siteMapNodeModel.Parent == null)
            {
                return;
            }

            _ancestors?.Add(siteMapNodeModel.Parent);
            siteMapNodeModel = siteMapNodeModel.Parent;
        }
    }
}
