using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace MMSPro.WebApp
{
    public class LeftMenu : System.Web.UI.Page
    {
        private static LogHelper _log = LogHelper.GetInstance();
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                try
                {

                    StringBuilder Menu1Builder = new StringBuilder();
                    StringBuilder Menu2Builder = new StringBuilder();
                    StringBuilder Menu3Builder = new StringBuilder();
                    StringBuilder Menu4Builder = new StringBuilder();
                    //StringBuilder Menu5Builder = new StringBuilder();
                    StringBuilder Menu6Builder = new StringBuilder();
                    StringBuilder Menu7Builder = new StringBuilder();
                    StringBuilder Menu8Builder = new StringBuilder();
                    //-------------------------------------------------
                    Menu1Builder.Append("<tr>");
                    Menu1Builder.Append("<td onClick=collapse('g_1') style='cursor:hand; background-image:url(images/menu_a.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    Menu1Builder.Append("Home");
                    Menu1Builder.Append("</td>");
                    Menu1Builder.Append("</tr>");
                    Menu1Builder.Append("<tr>");
                    Menu1Builder.Append("<td id='g_1'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    Menu1Builder.Append("<tbody>");
                    //-------------------------------------------------
                    Menu2Builder.Append("<tr>");
                    Menu2Builder.Append("<td onClick=collapse('g_2') style='cursor:hand; background-image:url(images/menu_b.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    Menu2Builder.Append("Customer");
                    Menu2Builder.Append("</td>");
                    Menu2Builder.Append("</tr>");
                    Menu2Builder.Append("<tr>");
                    Menu2Builder.Append("<td id='g_2'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    Menu2Builder.Append("<tbody>");
                    //-------------------------------------------------
                    Menu3Builder.Append("<tr>");
                    Menu3Builder.Append("<td onClick=collapse('g_3') style='cursor:hand; background-image:url(images/menu_c.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    Menu3Builder.Append("Produce");
                    Menu3Builder.Append("</td>");
                    Menu3Builder.Append("</tr>");
                    Menu3Builder.Append("<tr>");
                    Menu3Builder.Append("<td id='g_3'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    Menu3Builder.Append("<tbody>");
                    //-------------------------------------------------
                    Menu4Builder.Append("<tr>");
                    Menu4Builder.Append("<td onClick=collapse('g_4') style='cursor:hand; background-image:url(images/menu_e.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    Menu4Builder.Append("Resource");
                    Menu4Builder.Append("</td>");
                    Menu4Builder.Append("</tr>");
                    Menu4Builder.Append("<tr>");
                    Menu4Builder.Append("<td id='g_4'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    Menu4Builder.Append("<tbody>");
                    //-------------------------------------------------
                    //Menu5Builder.Append("<tr>");
                    //Menu5Builder.Append("<td onClick=collapse('g_5') style='cursor:hand; background-image:url(images/menu_d.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    //Menu5Builder.Append("Vendor");
                    //Menu5Builder.Append("</td>");
                    //Menu5Builder.Append("</tr>");
                    //Menu5Builder.Append("<tr>");
                    //Menu5Builder.Append("<td id='g_5'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    //Menu5Builder.Append("<tbody>");

                    //-------------------------------------------------
                    Menu6Builder.Append("<tr>");
                    Menu6Builder.Append("<td onClick=collapse('g_6') style='cursor:hand; background-image:url(images/menu_f.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    Menu6Builder.Append("Finance");
                    Menu6Builder.Append("</td>");
                    Menu6Builder.Append("</tr>");
                    Menu6Builder.Append("<tr>");
                    Menu6Builder.Append("<td id='g_6'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    Menu6Builder.Append("<tbody>");
                    //-------------------------------------------------
                    //-------------------------------------------------
                    //-------------------------------------------------
                    Menu8Builder.Append("<tr>");
                    Menu8Builder.Append("<td onClick=collapse('g_8') style='cursor:hand; background-image:url(images/menu_aa.gif); color:#fff;  padding-bottom:3px; height:36px; background-repeat:no-repeat;background-position:center' class='style3'>");
                    Menu8Builder.Append("System Manager");
                    Menu8Builder.Append("</td>");
                    Menu8Builder.Append("</tr>");
                    Menu8Builder.Append("<tr>");
                    Menu8Builder.Append("<td id='g_8'><table width='100%' align='center' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
                    Menu8Builder.Append("<tbody>");
                    //-------------------------------------------------

                    StringBuilder sb = new StringBuilder();
                    sb.Append("<table border='0' cellpadding='0' cellspacing='0' class='menuall'>");
                    sb.Append("<tr>");
                    sb.Append("<td><img src='images/菜单top.gif' alt='' /></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td>");
                    sb.Append("<a href='javascript:expandAll()' target='_self'><img src='images/菜单展开.gif' alt='展开菜单' onMouseOver=nereidFade(this,100,10,5) style='FILTER:alpha(opacity=50)' onMouseOut=nereidFade(this,50,10,5) /></a>&nbsp;<a href='javascript:collapseAll()' target='_self'><img src='images/菜单收起.gif' alt='收拢菜单' onMouseOver=nereidFade(this,100,10,5) style='FILTER:alpha(opacity=50)' onMouseOut=nereidFade(this,50,10,5) /></a></td>");
                    sb.Append("</tr>");
                    SPList list = SPContext.Current.Web.Lists["MSSLeftMenuList"];

                    foreach (SPListItem item in list.Items)
                    {
                        if (item["NodeType"] != null && item["NodeType"].ToString() != "")
                        {
                            switch (item["NodeType"].ToString())
                            {
                                case "我的桌面":
                                    Menu1Builder.Append("<tr>");
                                    Menu1Builder.Append("<td height='30' align='center' background='images/菜单选中.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                    Menu1Builder.Append("</tr>");
                                    break;
                                case "物资入库":
                                    Menu2Builder.Append("<tr>");
                                    Menu2Builder.Append("<td height='30' align='center' background='images/菜单选中.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                    Menu2Builder.Append("</tr>");
                                    break;
                                case "物资出库":
                                    Menu3Builder.Append("<tr>");
                                    Menu3Builder.Append("<td height='30' align='center' background='images/菜单选中.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                    Menu3Builder.Append("</tr>");
                                    break;
                                case "基础信息":
                                    Menu4Builder.Append("<tr>");
                                    Menu4Builder.Append("<td height='30' align='center' background='images/菜单选中.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                    Menu4Builder.Append("</tr>");
                                    break;
                                //case "Vendor":
                                //    Menu5Builder.Append("<tr>");
                                //    Menu5Builder.Append("<td height='30' align='center' background='images/left_bg01.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                //    Menu5Builder.Append("</tr>");
                                //    break;

                                case "Finance":
                                    Menu6Builder.Append("<tr>");
                                    Menu6Builder.Append("<td height='30' align='center' background='images/菜单选中.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                    Menu6Builder.Append("</tr>");
                                    break;
                                case "System Manager":
                                    Menu8Builder.Append("<tr>");
                                    Menu8Builder.Append("<td height='30' align='center' background='images/菜单选中.gif' style='cursor:hand'  onclick=javascript:parent.right.location.href='" + item["UrlValue"].ToString() + "';><table cellpadding='0' cellspacing='0' width='100%'><tr><td width='20'>&nbsp;</td><td>" + item.Title + "</td></tr></table></td>");
                                    Menu8Builder.Append("</tr>");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    Menu1Builder.Append("<tr><td height='5'></td></tr>");
                    Menu1Builder.Append("</tbody>");
                    Menu1Builder.Append("</table></td>");
                    Menu1Builder.Append("</tr>");
                    //-------------------------------------------------
                    Menu2Builder.Append("<tr><td height='5'></td></tr>");
                    Menu2Builder.Append("</tbody>");
                    Menu2Builder.Append("</table></td>");
                    Menu2Builder.Append("</tr>");
                    //-------------------------------------------------
                    Menu3Builder.Append("<tr><td height='5'></td></tr>");
                    Menu3Builder.Append("</tbody>");
                    Menu3Builder.Append("</table></td>");
                    Menu3Builder.Append("</tr>");
                    //-------------------------------------------------
                    Menu4Builder.Append("<tr><td height='5'></td></tr>");
                    Menu4Builder.Append("</tbody>");
                    Menu4Builder.Append("</table></td>");
                    Menu4Builder.Append("</tr>");
                    //-------------------------------------------------
                    //Menu5Builder.Append("<tr><td height='5'></td></tr>");
                    //Menu5Builder.Append("</tbody>");
                    //Menu5Builder.Append("</table></td>");
                    //Menu5Builder.Append("</tr>");

                    //-------------------------------------------------
                    Menu6Builder.Append("<tr><td height='5'></td></tr>");
                    Menu6Builder.Append("</tbody>");
                    Menu6Builder.Append("</table></td>");
                    Menu6Builder.Append("</tr>");
                    //-------------------------------------------------
                    //-------------------------------------------------
                    Menu8Builder.Append("<tr><td height='5'></td></tr>");
                    Menu8Builder.Append("</tbody>");
                    Menu8Builder.Append("</table></td>");
                    Menu8Builder.Append("</tr>");
                    //-------------------------------------------------

                    sb.Append(Menu1Builder.ToString());
                    sb.Append(Menu2Builder.ToString());
                    sb.Append(Menu3Builder.ToString());
                    sb.Append(Menu4Builder.ToString());
                    ////sb.Append(Menu5Builder.ToString());

                    sb.Append(Menu6Builder.ToString());
                    sb.Append(Menu8Builder.ToString());

                    sb.Append("<tr>");
                    sb.Append("<td><img src='images/菜单底部.gif' alt='' /></td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");

                    Literal liRender = (Literal)this.FindControl("liRender");
                    liRender.Text = sb.ToString();

                }
                catch (Exception ex)
                {
                    _log.Log(LogSeverity.Error, ex, "LeftMenu异常001", string.Empty);
                }
            }
        }
    }
}
