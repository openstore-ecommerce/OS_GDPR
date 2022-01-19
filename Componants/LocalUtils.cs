using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using OS_GDPR.Componants;
using System;
using System.Collections.Generic;
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

        public static string GetData(string lang, string razortemplate = "admin.cshtml")
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_GDPRDATA", "OS_GDPRDATA", lang);
            return NBrightBuyUtils.RazorTemplRender(razortemplate, 0, "", info, "/DesktopModules/NBright/OS_GDPR", "config", lang, StoreSettings.Current.Settings());
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

        public static void ActionGDPR(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_GDPRDATA", "OS_GDPRDATA", "en-US");
            var autoremoveusers = info.GetXmlPropertyBool("genxml/checkbox/autoremoveusers");
            var active = info.GetXmlPropertyBool("genxml/checkbox/active");
            var removelimitdays = info.GetXmlPropertyInt("genxml/textbox/removelimitdays");

            if (active)
            {
                var userData = new UserLimpet(portalId, removelimitdays);
                userData.ProcessUsers();
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
                    info.AddXmlNode(sRec.XMLData, "genxml", "genxml/userlist");
                    if (autoremoveusers) userData.DeleteUser(portalId, u.UserID);

                }
                info.SetXmlProperty("genxml/textbox/lastrun", DateTime.Now.ToString("O"), TypeCode.DateTime);
                objCtrl.Update(info);

                var ordData = new OrderLimpet(portalId, removelimitdays);
                ordData.ProcessOrders();
            }

        }

    }
    }
