using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace MMSPro.WebApp
{
    class WSSRightHelper
    {
        /// <summary>
        /// 判断组是否存在于某个网站
        /// </summary>
        /// <param name="web"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public bool IsExistGroup(SPWeb web, string groupName)
        {
            try
            {
                foreach (SPGroup groupList in web.SiteGroups)//判断组是否存在
                {
                    if (groupList.ToString().ToLower() == groupName.ToLower())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 向指定网站添加组
        /// </summary>
        /// <param name="web"></param>
        /// <param name="groupName"></param>
        /// <param name="member"></param>
        /// <param name="spuser"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool AddGroup(SPWeb web, string groupName, SPMember member, SPUser spuser, string description)
        {
            try
            {
                if (!IsExistGroup(web, groupName))
                {
                    web.SiteGroups.Add(groupName, member, spuser, description);//新建组
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// 判断用户是否存在于某个组
        /// </summary>
        /// <param name="web"></param>
        /// <param name="userName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public bool IsExistUser(SPWeb web, string userName, string groupName)
        {
            try
            {
                foreach (SPUser userlist in web.SiteGroups[groupName].Users)//判断指定组是否存在用户
                {
                    if (userlist.ToString().ToLower() == userName.ToLower())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 向指定组添加用户
        /// </summary>
        /// <param name="web"></param>
        /// <param name="loginName"></param>
        /// <param name="groupName"></param>
        /// <param name="email"></param>
        /// <param name="cnname"></param>
        /// <param name="notes"></param>
        /// <returns></returns>
        public bool AddUserToGroup(SPWeb web, string loginName, string groupName, string email, string cnname, string notes)
        {
            try
            {
                if (!IsExistUser(web, loginName, groupName))
                {
                    web.AllowUnsafeUpdates = true;
                    web.SiteGroups[groupName].AddUser(loginName, email, cnname, notes);//新建用户
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool AddGroupToRoles(SPWeb web, string groupName, string[] roles)
        {
            try
            {
                string[] _roles = roles;
                int rolemun = _roles.Length;

                if (IsExistGroup(web, groupName))
                {
                    //改变站点继承权
                    if (!web.HasUniqueRoleDefinitions)
                    {
                        web.RoleDefinitions.BreakInheritance(true, true);//复制父站点角色定义并且保持权限
                    }

                    //站点继承权改变后重新设置状态
                    web.AllowUnsafeUpdates = true;

                    //组权限分配与定义(New)
                    SPRoleDefinitionCollection roleDefinitions = web.RoleDefinitions;
                    SPRoleAssignmentCollection roleAssignments = web.RoleAssignments;
                    SPMember memCrossSiteGroup = web.SiteGroups[groupName];
                    SPPrincipal myssp = (SPPrincipal)memCrossSiteGroup;
                    SPRoleAssignment myroles = new SPRoleAssignment(myssp);
                    SPRoleDefinitionBindingCollection roleDefBindings = myroles.RoleDefinitionBindings;
                    if (rolemun > 0)
                    {
                        for (int i = 0; i < rolemun; i++)
                        {
                            roleDefBindings.Add(roleDefinitions[_roles[i]]);
                        }
                    }
                    roleAssignments.Add(myroles);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
