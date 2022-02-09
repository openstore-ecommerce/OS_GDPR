using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using OS_GDPR.Componants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace OpenStore.Providers.OS_GDPR
{
    public static class LocalUtils
    {

        public static string SaveData(NBrightInfo ajaxInfo)
        {
            // update record with ajax data
            var portalId = DnnUtils.GetCurrentPortalSettings().PortalId;
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            info.SetXmlProperty("genxml/textbox/removelimitdays", ajaxInfo.GetXmlProperty("genxml/textbox/removelimitdays"));
            info.SetXmlProperty("genxml/textbox/email", ajaxInfo.GetXmlProperty("genxml/textbox/email"));
            info.SetXmlProperty("genxml/checkbox/active", ajaxInfo.GetXmlProperty("genxml/checkbox/active"));
            info.SetXmlProperty("genxml/checkbox/autoremoveusers", ajaxInfo.GetXmlProperty("genxml/checkbox/autoremoveusers"));
            objCtrl.Update(info);
            DataCache.ClearCache();
            return NBrightBuyUtils.RazorTemplRender("admin.cshtml", 0, "", info, "/DesktopModules/NBright/OS_GDPR", "config", "en-US", StoreSettings.Current.Settings());
        }
        public static string GetData(string razortemplate = "admin.cshtml")
        {
            var portalId = DnnUtils.GetCurrentPortalSettings().PortalId;
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            return NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, "/DesktopModules/NBright/OS_GDPR", "config", "en-US", StoreSettings.Current.Settings());
        }

        public static Boolean CheckRights()
        {
            if (UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) || UserController.Instance.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) || UserController.Instance.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                return true;
            }
            return false;
        }


        public static void ExecuteGDPR()
        {
            //get uploaded params
            ActionGDPR(DnnUtils.GetCurrentPortalSettings().PortalId);
        }
        public static void SchedulerActionGDPR(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            var strlastrun = info.GetXmlProperty("genxml/textbox/lastrun");
            if (Utils.IsDate(strlastrun,"en-US"))
            {
                var lastrun = Convert.ToDateTime(strlastrun);
                // run between 2am - 3am, once a day.
                if ((lastrun < DateTime.Now.Date && DateTime.Now > DateTime.Now.Date.AddHours(2)))
                {
                    ActionGDPR(portalId);
                }
            }
        }

        public static void ActionGDPR(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            var autoremoveusers = info.GetXmlPropertyBool("genxml/checkbox/autoremoveusers");
            var active = info.GetXmlPropertyBool("genxml/checkbox/active");
            var removelimitdays = info.GetXmlPropertyInt("genxml/textbox/removelimitdays");
            var email = info.GetXmlProperty("genxml/textbox/email"); // search for email, is a single user needs to be removed.
            if (email != "")
            {
                removelimitdays = 0; // searching for single user, may not reach limit.
                autoremoveusers = false;
            }

            if (active)
            {
                var userData = new UserLimpet(portalId, removelimitdays);
                userData.ProcessUsers(email);
                info.RemoveXmlNode("genxml/userlist");
                info.SetXmlProperty("genxml/userlist", "");
                foreach (var u in userData.RemoveList)
                {
                    var sRec = new NBrightInfo(true);
                    sRec.SetXmlProperty("genxml/firstname", u.FirstName);
                    sRec.SetXmlProperty("genxml/lastname", u.LastName);
                    sRec.SetXmlProperty("genxml/portalid", u.PortalID.ToString());
                    sRec.SetXmlProperty("genxml/userid", u.UserID.ToString());
                    sRec.SetXmlProperty("genxml/username", u.Username);
                    sRec.SetXmlProperty("genxml/email", u.Email);
                    sRec.SetXmlProperty("genxml/displayname", u.DisplayName);
                    sRec.SetXmlProperty("genxml/lastlogindate", u.Membership.LastLoginDate.ToString("O"), TypeCode.DateTime);                    

                    info.AddXmlNode(sRec.XMLData, "genxml", "genxml/userlist");
                    if (autoremoveusers && email == "") DeleteUser(portalId, u.UserID);

                }
                info.SetXmlProperty("genxml/textbox/lastrun", DateTime.Now.ToString("O"), TypeCode.DateTime);
                info.SetXmlProperty("genxml/textbox/email", "");
                objCtrl.Update(info);

                if (email == "") // if we are searching for user, do not process orders.
                {
                    var ordData = new OrderLimpet(portalId, removelimitdays);
                    ordData.ProcessOrders();
                }
            }

        }
        public static void DeleteAll()
        {
            var portalId = DnnUtils.GetCurrentPortalSettings().PortalId;
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            var userNodList = info.XMLDoc.SelectNodes("genxml/userlist/*");
            foreach(XmlNode nod in userNodList)
            {
                var sRec = new NBrightInfo();
                sRec.XMLData = nod.OuterXml;
                var userId = sRec.GetXmlPropertyInt("genxml/userid");
                UserOrders(portalId, userId);
                DeleteUser(portalId, userId);
            }
        }
        public static void Delete(NBrightInfo ajaxInfo)
        {
            var userid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
            if (userid > 0)
            {
                UserOrders(DnnUtils.GetCurrentPortalSettings().PortalId, userid);
                DeleteUser(DnnUtils.GetCurrentPortalSettings().PortalId, userid);
            }
        }

        public static void DeleteUser(int portalId, int userId)
        {
            var objCtrl = new NBrightBuyController();
            var userInfo = UserController.Instance.GetUserById(portalId, userId);
            if (userInfo != null)
            {
                UserController.DeleteUser(ref userInfo, false, false);
                UserController.RemoveUser(userInfo);

                // remove from data list
                var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
                var userNodList = info.XMLDoc.SelectNodes("genxml/userlist/*");
                var xmlList = new List<string>();
                foreach (XmlNode nod in userNodList) // process before removal, I think it's a "race condition/ By Ref" with the remove.
                {
                    xmlList.Add(nod.OuterXml);
                }
                info.RemoveXmlNode("genxml/userlist");
                info.SetXmlProperty("genxml/userlist", "");
                foreach (var strXml in xmlList)
                {
                    var sRec = new NBrightInfo();
                    sRec.XMLData = strXml;
                    var uId = sRec.GetXmlPropertyInt("genxml/userid");
                    if (uId != userId)
                    {
                        info.AddXmlNode(sRec.XMLData, "genxml", "genxml/userlist");
                    }
                }
                objCtrl.Update(info);

            }

            objCtrl.ExecSql("delete from NBrightBuy where TypeCode = 'CLIENT' and userid = " + userId);

        }
        public static void UserOrders(int portalId, int userId)
        {
            var orderUsers = DataContext.Instance().ExecuteQuery<int>(CommandType.Text, "select itemid from NBrightBuy where TypeCode = 'ORDER' and userid = " + userId + " ");
            foreach (var o in orderUsers)
            {
                var ordData = new OrderLimpet(portalId, 0);
                var orderData = new OrderData(portalId, o);
                ordData.MaskOrder(orderData);
            }
        }



    }
}
