﻿using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Rock.Cms.Cached
{
    /// <summary>
    /// Information about a page that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class Page : Security.ISecured
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Page object
        /// </summary>
        private Page() { }

        public int Id { get; private set; }
        public string Layout { get; private set; }
        public int Order { get; private set; }
        public int OutputCacheDuration { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IncludeAdminFooter { get; private set; }

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; private set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the page.  This object will not include values.
        /// To get values associated with the current page instance, use the AttributeValues
        /// </summary>
        public List<Rock.Cms.Cached.Attribute> Attributes
        {
            get
            {
                List<Rock.Cms.Cached.Attribute> attributes = new List<Rock.Cms.Cached.Attribute>();

                foreach ( int id in AttributeIds )
                    attributes.Add( Attribute.Read( id ) );

                return attributes;
            }
        }

        public string LayoutPath { get; set; }

        private int? ParentPageId;
        public Page ParentPage
        {
            get
            {
                if ( ParentPageId != null && ParentPageId.Value != 0 )
                    return Page.Read( ParentPageId.Value );
                else
                    return null;
            }
        }

        private int? SiteId;
        public Site Site
        {
            get
            {
                if ( SiteId != null && SiteId.Value != 0 )
                    return Site.Read( SiteId.Value );
                else
                    return null;
            }
        }

        private List<int> pageIds = null;
        public List<Page> Pages
        {
            get
            {
                List<Page> pages = new List<Page>();

                if ( pageIds != null )
                {
                    foreach ( int id in pageIds )
                        pages.Add( Page.Read( id ) );
                }
                else
                {
                    pageIds = new List<int>();

                    Rock.Services.Cms.PageService pageService = new Rock.Services.Cms.PageService();
                    foreach ( Rock.Models.Cms.Page page in pageService.GetPagesByParentPageId( this.Id ) )
                    {
                        pageIds.Add( page.Id );
                        pages.Add( Page.Read( page ) );
                    }
                }

                return pages;
            }
        }

        private List<int> blockInstanceIds = null;
        public List<BlockInstance> BlockInstances
        {
            get
            {
                List<BlockInstance> blockInstances = new List<BlockInstance>();

                if ( blockInstanceIds != null )
                {
                    foreach ( int id in blockInstanceIds )
                        blockInstances.Add( BlockInstance.Read( id ) );
                }
                else
                {
                    blockInstanceIds = new List<int>();

                    // Load Layout Blocks
                    Rock.Services.Cms.BlockInstanceService blockInstanceService = new Rock.Services.Cms.BlockInstanceService();
                    foreach ( Rock.Models.Cms.BlockInstance blockInstance in blockInstanceService.GetBlockInstancesByLayout( this.Layout ) )
                    {
                        blockInstanceIds.Add( blockInstance.Id );
                        blockInstanceService.LoadAttributes( blockInstance );
                        blockInstances.Add( BlockInstance.Read( blockInstance ) );
                    }

                    // Load Page Blocks
                    foreach ( Rock.Models.Cms.BlockInstance blockInstance in blockInstanceService.GetBlockInstancesByPageId( this.Id ) )
                    {
                        blockInstanceIds.Add( blockInstance.Id );
                        blockInstanceService.LoadAttributes( blockInstance );
                        blockInstances.Add( BlockInstance.Read( blockInstance ) );
                    }

                }
                return blockInstances;
            }
        }

        public void SaveAttributeValues(int? personId)
        {
            Rock.Services.Cms.PageService pageService = new Services.Cms.PageService();
            Rock.Models.Cms.Page pageModel = pageService.GetPage( this.Id );
            if ( pageModel != null )
            {
                pageService.LoadAttributes( pageModel );

                foreach ( Rock.Models.Core.Attribute attribute in pageModel.Attributes )
                    pageService.SaveAttributeValue( pageModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

        #region SharedItemCaching

        /// <summary>
        /// Used to save an item to the current HTTPRequests items collection.  This is useful if multiple blocks
        /// on the same page will need access to the same object.  The first block can read the object and save
        /// it using this method for the other blocks to reference
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        public void SaveSharedItem( string key, object item )
        {
            string itemKey = string.Format("{0}:Item:{1}", Page.CacheKey( Id ), key);
            
            System.Collections.IDictionary items = HttpContext.Current.Items;
            if ( items.Contains( itemKey ) )
                items[itemKey] = item;
            else
                items.Add( itemKey, item );
        }

        /// <summary>
        /// Retrieves an item from the current HTTPRequest items collection.  This is useful to retrieve an object
        /// that was saved by a previous block on the same page.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetSharedItem( string key )
        {
            string itemKey = string.Format( "{0}:Item:{1}", Page.CacheKey( Id ), key );

            System.Collections.IDictionary items = HttpContext.Current.Items;
            if ( items.Contains( itemKey ) )
                return items[itemKey];

            return null;
        }

        #endregion

        #region HtmlLinks

        /// <summary>
        /// Adds a new CSS link that will be added to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="href">Path to css file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public void AddCSSLink( System.Web.UI.Page page, string href )
        {
            CmsPage.AddCSSLink( page, href );
        }

        public void AddCSSLink( System.Web.UI.Page page, string href, string mediaType )
        {
            CmsPage.AddCSSLink( page, href, mediaType );
        }

        /// <summary>
        /// Adds a new Html link that will be added to the page header prior to the page being rendered
        /// </summary>
        public void AddHtmlLink( System.Web.UI.Page page, HtmlLink htmlLink )
        {
            CmsPage.AddHtmlLink( page, htmlLink );
        }

        /// <summary>
        /// Adds a new script tag to the page header prior to the page being rendered
        /// </summary>
        /// <param name="page">Current System.Web.UI.Page</param>
        /// <param name="href">Path to script file.  Should be relative to layout template.  Will be resolved at runtime</param>
        public void AddScriptLink( System.Web.UI.Page page, string path )
        {
            CmsPage.AddScriptLink( page, path );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        public string BuildUrl( int pageId, Dictionary<string, string> parms )
        {
            return CmsPage.BuildUrl( new Rock.Helpers.PageReference( pageId, -1 ), parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageId">Page to link to</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public string BuildUrl( int pageId, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return CmsPage.BuildUrl( new Rock.Helpers.PageReference( pageId, -1 ), parms, queryString );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        public string BuildUrl( Rock.Helpers.PageReference pageRef, Dictionary<string, string> parms )
        {
            return CmsPage.BuildUrl( pageRef, parms, null );
        }

        /// <summary>
        /// Builds a URL from a page and parameters with support for routes
        /// </summary>
        /// <param name="pageRef">PageReference to use for the link</param>
        /// <param name="parms">Dictionary of parameters</param>
        /// <param name="queryString">Querystring to include paramters from</param>
        public string BuildUrl( Rock.Helpers.PageReference pageRef, Dictionary<string, string> parms, System.Collections.Specialized.NameValueCollection queryString )
        {
            return CmsPage.BuildUrl( pageRef, parms, queryString );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Formats the page url based on the selected theme and layout
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        public static string FormatPath( string theme, string layout )
        {
            return string.Format( "~/Themes/{0}/Layouts/{1}.aspx", theme, layout );
        }

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Page:{0}", id );
        }

        /// <summary>
        /// Adds Page model to cache, and returns cached object
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Page Read( Rock.Models.Cms.Page pageModel )
        {
            Page page = Page.CopyModel( pageModel );

            string cacheKey = Page.CacheKey( pageModel.Id );
            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, page, new CacheItemPolicy() );

            return page;
        }

        /// <summary>
        /// Returns Page object from cache.  If page does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Page Read( int id )
        {
            string cacheKey = Page.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Page page = cache[cacheKey] as Page;

            if ( page != null )
                return page;
            else
            {
                Rock.Services.Cms.PageService pageService = new Services.Cms.PageService();
                Rock.Models.Cms.Page pageModel = pageService.GetPage( id );
                if ( pageModel != null )
                {
                    pageService.LoadAttributes( pageModel );

                    page = Page.CopyModel( pageModel );
 
                    cache.Set( cacheKey, page, new CacheItemPolicy() );

                    return page;
                }
                else
                    return null;

            }
        }

        private static Page CopyModel( Rock.Models.Cms.Page pageModel )
        {
            Page page = new Page();
            page.Id = pageModel.Id;
            page.ParentPageId = pageModel.ParentPageId;
            page.SiteId = pageModel.SiteId;
            page.Layout = pageModel.Layout;
            page.Order = pageModel.Order;
            page.OutputCacheDuration = pageModel.OutputCacheDuration;
            page.Name = pageModel.Name;
            page.Description = pageModel.Description;
            page.AttributeValues = pageModel.AttributeValues;
            page.IncludeAdminFooter = pageModel.IncludeAdminFooter;

            foreach ( Rock.Models.Core.Attribute attribute in pageModel.Attributes )
            {
                page.AttributeIds.Add( attribute.Id );
                Attribute.Read( attribute );
            }

            page.AuthEntity = pageModel.AuthEntity;
            page.SupportedActions = pageModel.SupportedActions;

            return page;
        }

        /// <summary>
        /// Removes page from cache
        /// </summary>
        /// <param name="guid"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Page.CacheKey( id ) );
        }

        #endregion

        #region ISecure Implementation

        public string AuthEntity { get; set; }

        public Security.ISecured ParentAuthority
        {
            get { return this.ParentPage; }
        }

        public List<string> SupportedActions { get; set; }

        public virtual bool Authorized( string action, System.Web.Security.MembershipUser user )
        {
            return Rock.Cms.Security.Authorization.Authorized( this, action, user );
        }

        public bool DefaultAuthorization( string action )
        {
            return action == "View";
        }

        #endregion
    }
}