using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace MMSPro.WebApp
{
    class JSDialogAid
    {
        public static string GetJSForDialog(string controlClientID, string relativeUrl)
        {
            string domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function OpenDialogSelectUser()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:300px;dialogHeight:450px');");
            sb.Append("document.getElementById('" + controlClientID + "').value=\"" + domainAbbreviate +"\\\\\"+uuu;");
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString();
        }
        public static string GetJSForDialog(string ClientID_M, string ClientID_W, string ClientID_MA, string relativeUrl)
        {
            string domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function OpenDialogSelectUser()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:300px;dialogHeight:450px');");
            sb.Append("document.getElementById('" + ClientID_M + "').value=\"" + domainAbbreviate + "\\\\\"+uuu;");
            sb.Append("}");

            sb.Append("function OpenDialogSelectUserBackupOne()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:300px;dialogHeight:450px');");
            sb.Append("document.getElementById('" + ClientID_W + "').value=\"" + domainAbbreviate + "\\\\\"+uuu;");
            sb.Append("}");

            sb.Append("function OpenDialogSelectUserBackupTwo()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:300px;dialogHeight:450px');");
            sb.Append("document.getElementById('" + ClientID_MA + "').value=\"" + domainAbbreviate + "\\\\\"+uuu;");
            sb.Append("}");

            sb.Append("</script>");
            return sb.ToString();
        }
        public static string GetDialogInfo(string controlClientID,string txtid, string txtMid, string txtFid, string relativeUrl)
        {
            
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function SelectMaterial()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:800px;dialogHeight:600px');");
            sb.Append("var str=uuu.split('|');");
            sb.Append("var strA=str[0];");
            sb.Append("var strB=str[1];");
            sb.Append("var strC=str[2];");
            sb.Append("var strD=str[3];");
            sb.Append("document.getElementById('" + controlClientID + "').value=strA;");
           
            sb.Append("document.getElementById('" + txtid + "').value=strB;");
            sb.Append("document.getElementById('" + txtMid + "').value=strC;");
            sb.Append("document.getElementById('" + txtFid + "').value=strD;");
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString();
        }

        public static string GetDialogInfo(string txtmaterial, string txtcommitId,string relativeUrl)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function SelectCommitMaterial()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:800px;dialogHeight:600px');");
            sb.Append("var str=uuu.split('|');");
            sb.Append("var strA=str[0];");
            sb.Append("var strB=str[1];");
            //sb.Append("var strC=str[2];");
            //sb.Append("var strD=str[3];");
            sb.Append("document.getElementById('" + txtmaterial + "').value=strA;");

            sb.Append("document.getElementById('" + txtcommitId + "').value=strB;");
            //sb.Append("document.getElementById('" + txtMid + "').value=strC;");
            //sb.Append("document.getElementById('" + txtFid + "').value=strD;");
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString();
        }



        public static string GetMaterialInfo(string txtmaterial, string txtcommitId,string txtType,string relativeUrl)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function SelectMaterial()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:800px;dialogHeight:600px');");
            sb.Append("var str=uuu.split('|');");
            sb.Append("var strA=str[0];");
            sb.Append("var strB=str[1];");
            sb.Append("var strC=str[2];");
            //sb.Append("var strD=str[3];");
            sb.Append("document.getElementById('" + txtmaterial + "').value=strA;");

            sb.Append("document.getElementById('" + txtcommitId + "').value=strB;");
            sb.Append("document.getElementById('" + txtType + "').value=strC;");
            //sb.Append("document.getElementById('" + txtFid + "').value=strD;");
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString();
        }


        public static string UploadData(int storageInId, string relativeUrl)
        {
            string domainAbbreviate = ConfigurationManager.AppSettings["mmsDomainAbbreviate"].ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function OpenDialogUpload()");
            sb.Append("{");
            sb.Append("var s = new Object();");
            sb.Append("s.name = 'aaa';");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','s','0','dialogWidth:500px;dialogHeight:350px');");
            sb.Append("if(uuu.type=='')");
            sb.Append("{");
            sb.Append("document.getElementById('ctl00_PlaceHolderMain_tbarbusiness_RightRptControls_btnRefresh_ImageOfButton').click()");
            sb.Append("}");
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString();
        }






        public static string GetMat(string controlClientID, string txtid, string relativeUrl)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function SelectMat()");
            sb.Append("{");
            sb.Append("var uuu=window.showModalDialog('" + relativeUrl + "','0','dialogWidth:800px;dialogHeight:600px');");
            sb.Append("var str=uuu.split('|');");
            sb.Append("var strA=str[0];");
            sb.Append("var strB=str[1];");
            sb.Append("var strC=str[2];");
            sb.Append("document.getElementById('" + controlClientID + "').value=strA+' | '+strB;");
            sb.Append("document.getElementById('" + txtid + "').value=strC;");
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString();
        }


        public static string GetUsersJS(string controlid)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<script type=\"text/javascript\">");
            sb.Append("function OpenSelectUsersDialog()");
            sb.Append("{");
            sb.Append("var users=window.showModalDialog('SelectUsers.aspx','0','dialogWidth:300px;dialogHeight:450px');");
            sb.Append(string.Format("document.getElementById('{0}').value=users;",controlid));
            sb.Append("}");
            sb.Append("</script>");
            return sb.ToString(); 
        }

        //******************此JS适用于带有CustomValidator的客户端验证JS脚本*****************       
        public static string GetVerifyDDLJSForVld(string value,string errorinfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("function vldDDL(source, args)");
            sb.AppendLine("{");
            sb.AppendLine("if(args.IsValid)");
            sb.AppendLine("{");
            sb.AppendLine("var el =document.getElementsByTagName(\"select\");");
            sb.AppendLine("for(i=0;i<el.length;i++)");
            sb.AppendLine("{");
            sb.AppendLine("for(j=0;j<el[i].length;j++)");
            sb.AppendLine("{");
            sb.AppendLine("if(el[i][j].selected == true)");
            sb.AppendLine("{");
            sb.AppendLine(string.Format("if(el[i][j].text=='{0}')", value));
            sb.AppendLine("{");
            sb.AppendLine(string.Format("alert('{0}');",errorinfo));
            sb.AppendLine("args.IsValid=false;return;");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            //sb.AppendLine("return true;}");
            sb.AppendLine("}");
            //sb.AppendLine("else return false;");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            return sb.ToString();

        }

        public static string GetVerifyDDLJSForBtn(string value,string errorinfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("function VerifyDDL()");
            sb.AppendLine("{");            
            sb.AppendLine("var el =document.getElementsByTagName(\"select\");");
            sb.AppendLine("for(i=0;i<el.length;i++)");
            sb.AppendLine("{");
            sb.AppendLine("for(j=0;j<el[i].length;j++)");
            sb.AppendLine("{");
            sb.AppendLine("if(el[i][j].selected == true)");
            sb.AppendLine("{");
            sb.AppendLine(string.Format("if(el[i][j].text=='{0}')", value));
            sb.AppendLine("{");
            sb.AppendLine(string.Format("alert('{0}');",errorinfo));
            sb.AppendLine("return false;");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("return true;");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            return sb.ToString();

        }

        public static string GetVerifyBtnJS()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script type=\"text/javascript\">");
            sb.AppendLine("function VerifyBtn()");
            sb.AppendLine("{");
            sb.AppendLine("var el =document.getElementsByTagName(\"input\");");
            sb.AppendLine("var count = 0;");
            sb.AppendLine("for(i=0;i<el.length;i++)");
            sb.AppendLine("{");
            sb.AppendLine("if(el[i].type == 'submit')");
            sb.AppendLine("count++;");
            sb.AppendLine("}");
            sb.AppendLine("if(count != 1)");
            sb.AppendLine("{");
            sb.AppendLine("alert('您尚未处理完质检后的物资，不能结束该任务！');");
            sb.AppendLine("return false");
            sb.AppendLine("}");
            sb.AppendLine("else return true");
            sb.AppendLine("}");
            sb.AppendLine("</script>");
            return sb.ToString();
        }
    }
}
