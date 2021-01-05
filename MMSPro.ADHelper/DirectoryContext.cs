/*------------------------------------------------------------------------------
 * 单元名称：DirectoryContext.cs
 * 单元描述：ADContext
 * 创建人： 李涛 
 * 创建日期： 2010-05-04
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Reflection;
using System.Collections;

namespace MMSPro.ADHelper.DirectoryServices
{

    /// <summary>
    /// ADContext
    /// </summary>
    public partial class DirectoryContext : IDisposable{

        #region Constructors

        /// <summary>
        /// InitContext
        /// </summary>
        /// <param name="ldap">LDAP Address</param>
        public DirectoryContext(string ldap){
            SearchRoot = new DirectoryEntry(ldap);
        }

        /// <summary>
        /// InitContext
        /// </summary>
        /// <param name="ldap">LDAP Address</param>
        /// <param name="username">name of user</param>
        /// <param name="password">password of user</param>
        public DirectoryContext(string ldap,string username,string password) {
            SearchRoot = new DirectoryEntry(ldap,username,password);
        }

        /// <summary>
        /// InitContext
        /// </summary>
        /// <param name="searchRoot">searchRoot</param>
        public DirectoryContext(DirectoryEntry searchRoot) {
            SearchRoot = searchRoot;
        }

        /// <summary>
        /// InitContext
        /// </summary>
        /// <param name="searchRoot">searchRoot</param>
        /// <param name="searchScope">searchScope</param>
        public DirectoryContext(DirectoryEntry searchRoot, SearchScope searchScope)
            : this(searchRoot) {
            this.searchScope = searchScope;
        }

        #endregion

        #region Members

        /// <summary>
        /// Default Search Range:All in AD
        /// </summary>
        private SearchScope searchScope = SearchScope.Subtree;
        
        /// <summary>
        /// SubChanges Chache
        /// </summary>
        private Dictionary<DirectoryEntity, DirectoryChangeInfo> _changes = new Dictionary<DirectoryEntity,DirectoryChangeInfo>();

        #endregion

        #region Properties

        /// <summary>
        /// SearchRoot
        /// </summary>
        public DirectoryEntry SearchRoot { get; set; }

        /// <summary>
        /// SearchScope
        /// </summary>
        public SearchScope SearchScope {
            get { return searchScope; }
            set { searchScope = value; }
        }

        /// <summary>
        /// All users in Directory
        /// </summary>
        public List<DirectoryUser> Users {
            get {
                return GetEntities<DirectoryUser>();
            }
        }

        /// <summary>
        /// All Groups in Directory
        /// </summary>
        public List<DirectoryGroup> Groups {
            get {
                return GetEntities<DirectoryGroup>();
            }
        }

        /// <summary>
        /// All computers in Directory
        /// </summary>
        public List<DirectoryComputer> Computers {
            get {
                return GetEntities<DirectoryComputer>();
            }
        }

        /// <summary>
        /// All OUs in Directory
        /// </summary>
        public List<DirectoryOrganizationalUnit> OrganizationalUnits {
            get {
                return GetEntities<DirectoryOrganizationalUnit>();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert a Entity(changes with submit)
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOnSubmit(DirectoryEntity entity) {
            InsertOnSubmit(this.SearchRoot, entity);
        }

        /// <summary>
        /// Insert a Entity(changes with submit)
        /// </summary>
        /// <param name="parent">父</param>
        /// <param name="entity"></param>
        public void InsertOnSubmit(DirectoryEntity parent, DirectoryEntity entity) {
            InsertOnSubmit(parent.DirectoryEntry, entity);
        }

        /// <summary>
        /// Insert a Entity(changes with submit)
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="entity"></param>
        public void InsertOnSubmit(DirectoryEntry parent, DirectoryEntity entity) {
            if (!_changes.ContainsKey(entity)) {
                DirectoryChangeInfo info = new DirectoryChangeInfo {
                    ChangeType = ChangeType.Insert,
                    Entity = entity,
                    Parent = parent
                };
                _changes.Add(entity, info);
            }
        }

        /// <summary>
        /// Delete a Entity(changes with submit)
        /// </summary>
        /// <param name="entity"></param>
        public void DeleteOnSubmit(DirectoryEntity entity) {
            if (!_changes.ContainsKey(entity)) {
                DirectoryChangeInfo info = new DirectoryChangeInfo {
                    ChangeType = ChangeType.Delete,
                    Entity = entity
                };
                _changes.Add(entity, info);
            }
        }

        /// <summary>
        /// submit changes
        /// </summary>
        public void SubmitChanges() {
            foreach (DirectoryChangeInfo changeInfo in _changes.Values) {
                DirectoryEntry de = null;
                switch (changeInfo.ChangeType) {
                    case ChangeType.Update:
                        de = changeInfo.Entity.DirectoryEntry;
                        foreach (string property in changeInfo.Properties) {
                            PropertyInfo pi = changeInfo.Entity.GetType().GetProperty(property);
                            foreach (object customAttribute in pi.GetCustomAttributes(true)) {
                                DirectoryAttributeAttribute attribute = customAttribute as DirectoryAttributeAttribute;
                                if (null != attribute && !attribute.ReadOnly) {
                                    object value = pi.GetValue(changeInfo.Entity, null);
                                    de.Properties[attribute.Attribute].Value = value;
                                    break;
                                }
                            }
                        }
                        break;
                    case ChangeType.Insert:
                        string schema = GetEntitySchemaClassType(changeInfo.Entity.GetType());
                        de = changeInfo.Parent.Children.Add(schema + "=" + changeInfo.Entity.Name, GetEntitySchemaClassName(changeInfo.Entity.GetType()));
                        changeInfo.Entity.DirectoryEntry = de;
                        foreach (PropertyInfo property in changeInfo.Entity.GetType().GetProperties()) {
                            foreach (object customAttribute in property.GetCustomAttributes(true)) {
                                DirectoryAttributeAttribute attribute = customAttribute as DirectoryAttributeAttribute;
                                if (null != attribute && !attribute.ReadOnly) {
                                    object value = property.GetValue(changeInfo.Entity, null);
                                    if (null != value)
                                        de.Properties[attribute.Attribute].Value = value;
                                }
                            }
                        }

                        foreach (PropertyInfo property in changeInfo.Entity.GetType().GetProperties()) {
                            foreach (object customAttribute in property.GetCustomAttributes(true)) {
                                DirectoryAttributeAttribute attribute = customAttribute as DirectoryAttributeAttribute;
                                if (null != attribute) {
                                    object value = de.Properties[attribute.Attribute].Value;
                                    property.SetValue(changeInfo.Entity, Convertor.ChangeType(de.Properties[attribute.Attribute].Value, property.PropertyType), null);
                                }
                            }
                        }
                        changeInfo.Entity.ParentGuid = de.Parent.Guid;
                        changeInfo.Entity.ParentPath = de.Parent.Path;
                        changeInfo.Entity.Path = de.Path;
                        changeInfo.Entity.DirectoryEntry = de;
                        changeInfo.Entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(entity_PropertyChanged);
                        break;
                    case ChangeType.Delete:
                        de = changeInfo.Entity.DirectoryEntry;
                        de.DeleteTree();
                        break;
                }
                de.CommitChanges();
                de.Close();
            }
            _changes.Clear();
        }

        private DirectoryEntry QueryEntry(string dn) {
            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);
            searcher.PageSize = int.MaxValue;
            searcher.Filter = "(&(distinguishedName=" + dn + "))";
            searcher.SearchScope =  SearchScope.Subtree;
            SearchResult result = searcher.FindOne();
            return result.GetDirectoryEntry();
        }

        #region GetEntities

        /// <summary>
        /// Get Active Directory Entities
        /// </summary>
        /// <returns>Active Directory Entities</returns>
        public List<T> GetEntities<T>() where T : IDirectoryEntity {

            string schema = GetEntitySchemaClassName<T>();

            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);

            searcher.PageSize = int.MaxValue;
            if (schema != null && schema != string.Empty)
                searcher.Filter = "(&(objectClass=" + schema + "))";
            searcher.SearchScope = searchScope;

            SearchResultCollection results = searcher.FindAll();

            List<T> list = new List<T>();

            foreach (SearchResult result in results) {
                try {
                    DirectoryEntry de = result.GetDirectoryEntry();
                    if (de.SchemaClassName == schema)
                        list.Add(GetEntity<T>(de));
                } catch { }
            }
            return list;
        }

        /// <summary>
        /// Get Active Directory Entities
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="root">Search Range</param>
        /// <returns>Active Directory Entities</returns>
        public List<T> GetEntities<T>(DirectoryEntry root) where T : IDirectoryEntity {

            string schema = GetEntitySchemaClassName<T>();

            DirectorySearcher searcher = new DirectorySearcher(root);

            searcher.PageSize = int.MaxValue;
            if (schema != null && schema != string.Empty)
                searcher.Filter = "(&(objectClass=" + schema + "))";
            searcher.SearchScope = searchScope;

            SearchResultCollection results = searcher.FindAll();

            List<T> entities = new List<T>();

            foreach (SearchResult result in results) {
                try {
                    DirectoryEntry de = result.GetDirectoryEntry();
                    if (de.SchemaClassName == schema)
                        entities.Add(GetEntity<T>(de));
                } catch { }
            }
            return entities;
        }

        /// <summary>
        /// Get Active Directory Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root">Root Entry</param>
        /// <param name="searchScope">Search Type</param>
        /// <returns></returns>
        static public List<T> GetEntities<T>(DirectoryEntry root, SearchScope searchScope) where T : IDirectoryEntity {
            using (DirectoryContext ctx = new DirectoryContext(root, searchScope)) {
                return ctx.GetEntities<T>();
            }
        }

        /// <summary>
        /// Get Active Directory Entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">filter condition</param>
        /// <returns></returns>
        public List<T> GetEntities<T>(string filter) where T : IDirectoryEntity {
            string schema = GetEntitySchemaClassName<T>();

            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);

            searcher.PageSize = int.MaxValue;
            if (schema != null && schema != string.Empty)
                searcher.Filter = "(&(objectClass=" + schema + ")(" + filter + "))";
            else
                searcher.Filter = "(&(" + filter + "))";
            searcher.SearchScope = SearchScope.Subtree;

            SearchResultCollection results = searcher.FindAll();
            List<T> list = new List<T>();

            foreach (SearchResult result in results) {
                try {
                    DirectoryEntry de = result.GetDirectoryEntry();
                    if (de.SchemaClassName == schema)
                        list.Add(GetEntity<T>(de));
                } catch { }
            }
            return list;

        }

        internal List<T> GetEntities<T>(string[] dNs) where T : IDirectoryEntity {
            List<T> entities = new List<T>();
            if (dNs != null) {
                foreach (string dn in dNs) {
                    entities.Add(GetEntity<T>(dn));
                }
            }
            return entities;
        }

        #endregion

        #region GetEntity

        /// <summary>
        /// Get Active Directory Entitiy
        /// </summary>
        /// <typeparam name="T">EntityType/typeparam>
        /// <param name="de">DirectoryEntry</param>
        /// <returns>DirectoryEntry</returns>
        public T GetEntity<T>(DirectoryEntry de) where T : IDirectoryEntity {
            try {
                T entityObject = (T)Activator.CreateInstance(typeof(T));
                foreach (PropertyInfo property in typeof(T).GetProperties()) {
                    foreach (object customAttribute in property.GetCustomAttributes(true)) {
                        DirectoryAttributeAttribute attribute = customAttribute as DirectoryAttributeAttribute;
                        if (null != attribute) {
                            object value = de.Properties[attribute.Attribute].Value;
                            property.SetValue(entityObject, Convertor.ChangeType(de.Properties[attribute.Attribute].Value, property.PropertyType), null);
                        }
                    }
                }
                DirectoryEntity entity = entityObject as DirectoryEntity;
                entity.ParentGuid = de.Parent.Guid;
                entity.ParentPath = de.Parent.Path;
                entity.Path = de.Path;
                entity.DirectoryEntry = de;
                entity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(entity_PropertyChanged);
                return entityObject;
            } catch {
                return default(T);
            }
        }

        /// <summary>
        /// Get DirectoryEntities
        /// </summary>
        /// <param name="dn">DistinguishedName</param>
        /// <returns>DirectoryEntities</returns>
        public T GetEntity<T>(string dn) where T : IDirectoryEntity {

            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);
            searcher.PageSize = int.MaxValue;
            searcher.Filter = "(&(distinguishedName=" + dn + "))";
            searcher.SearchScope = SearchScope.Subtree;
            SearchResultCollection results = searcher.FindAll();

            foreach (SearchResult result in results) {
                try {
                    return GetEntity<T>( result.GetDirectoryEntry());
                } catch { }
            }
            return default(T);
        }

        /// <summary>
        /// Get Active Directory Entitiy
        /// </summary>
        /// <typeparam name="T">EntityType</typeparam>
        /// <param name="guid">GUID</param>
        /// <returns>Directory Entitiy</returns>
        public T GetEntity<T>(Guid guid) where T : IDirectoryEntity {
            string schema = GetEntitySchemaClassName<T>();
            DirectorySearcher searcher = new DirectorySearcher(SearchRoot);
            searcher.PageSize = int.MaxValue;
            if (schema != null && schema != string.Empty)
                searcher.Filter = "(&(objectClass=" + schema + "))";
            searcher.SearchScope = SearchScope.Subtree;
            SearchResultCollection results = searcher.FindAll();

            foreach (SearchResult result in results) {
                try {
                    Guid id = (Guid)Convertor.ChangeType(result.Properties["objectGuid"][0], typeof(Guid));
                    if (id == guid)
                        return GetEntity<T>(result.GetDirectoryEntry());
                } catch { }
            }
            return default(T);
        }

        #endregion

        #endregion

        #region Events

        void entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            DirectoryEntity entity = sender as DirectoryEntity;
            if (!_changes.ContainsKey(entity)) {
                _changes.Add(entity, new DirectoryChangeInfo {
                    Entity = entity,
                    ChangeType = ChangeType.Update,
                    Properties = new List<string>()
                });
            }
            if (!_changes[entity].Properties.Contains(e.PropertyName)) {
                _changes[entity].Properties.Add(e.PropertyName);
            }
        }

        void entities_EntityDeletedEvent(DirectoryEntity sender) {
            this.DeleteOnSubmit(sender);
        }

        void entities_EntityAddedEvent(DirectoryEntry parent,DirectoryEntity sender) {
            this.InsertOnSubmit(parent, sender);
        }

        #endregion

        #region Static methods

        /// <summary>
        /// 获得活动目录实体的架构类型名
        /// </summary>
        /// <typeparam name="T">活动目录实体类型</typeparam>
        /// <returns>架构类型名</returns>
        static protected string GetEntitySchemaClassName<T>() where T : IDirectoryEntity {
            foreach (object attribute in typeof(T).GetCustomAttributes(false)) {
                DirectorySchemaAttribute schemaAttribute = attribute as DirectorySchemaAttribute;
                if (schemaAttribute != null) {
                    return schemaAttribute.Schema;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得活动目录实体的架构类型名
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns>架构类型名</returns>
        static protected internal string GetEntitySchemaClassName(Type type) {
            foreach (object attribute in type.GetCustomAttributes(false)) {
                DirectorySchemaAttribute schemaAttribute = attribute as DirectorySchemaAttribute;
                if (schemaAttribute != null) {
                    return schemaAttribute.Schema;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得活动目录实体的架构类型名
        /// </summary>
        /// <param name="type">实体类型</param>
        /// <returns></returns>
        static protected internal string GetEntitySchemaClassType(Type type) {
            foreach (object attribute in type.GetCustomAttributes(false)) {
                DirectorySchemaAttribute schemaAttribute = attribute as DirectorySchemaAttribute;
                if (schemaAttribute != null) {
                    return schemaAttribute.Type;
                }
            }
            return null;
        }


        #endregion

        #region IDisposable Members

        /// <summary>
        /// 释放实体对象
        /// </summary>
        public void Dispose() {
            if (this.SearchRoot != null)
                this.SearchRoot.Close();
        }

        #endregion
    }

    class DirectoryChangeInfo {

        public DirectoryChangeInfo() {
            Properties = new List<string>();
        }

        public DirectoryEntity Entity { get; set; }
        public ChangeType ChangeType { get; set; }
        public List<string> Properties { get; set; }
        public DirectoryEntry Parent { get; set; }

        public override bool Equals(object obj) {
            if (obj is DirectoryEntity)
                return (obj as DirectoryEntity).DistinguishedName == Entity.DistinguishedName;
            else
                return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }

    internal enum ChangeType{
        Update,
        Insert,
        Delete,
    }
}
