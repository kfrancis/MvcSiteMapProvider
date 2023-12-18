using System;
using System.Collections.Generic;

namespace MvcSiteMapProvider.Visitor
{
    /// <summary>
    /// Used to chain several <see cref="T:MvcSiteMapProvider.Visitor.ISiteMapNodeVisitor"/> instances in succession.
    /// The visitors will be processed in the same order as they are specified in the constructor.
    /// </summary>
    public class CompositeSiteMapNodeVisitor
        : ISiteMapNodeVisitor
    {
        protected readonly IEnumerable<ISiteMapNodeVisitor> siteMapNodeVisitors;

        public CompositeSiteMapNodeVisitor(params ISiteMapNodeVisitor[] siteMapNodeVisitors)
        {
            this.siteMapNodeVisitors = siteMapNodeVisitors ?? throw new ArgumentNullException(nameof(siteMapNodeVisitors));
        }

        public void Execute(ISiteMapNode node)
        {
            foreach (var visitor in siteMapNodeVisitors)
            {
                visitor.Execute(node);
            }
        }
    }
}