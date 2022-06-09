using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
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
            if (info == null)
            {
                info = new NBrightInfo();
                info.TypeCode = "OS_GDPRDATA";
                info.GUIDKey = "OS_GDPRDATA";
                info.ItemID = -1;
                info.PortalId = portalId;
                info.XMLData = ajaxInfo.XMLData;
            }
            else
            {
                info.SetXmlProperty("genxml/textbox", "");
                info.SetXmlProperty("genxml/textbox/removelimitdays", ajaxInfo.GetXmlProperty("genxml/textbox/removelimitdays"));
                info.SetXmlProperty("genxml/textbox/orderlimitdays", ajaxInfo.GetXmlProperty("genxml/textbox/orderlimitdays"));
                info.SetXmlProperty("genxml/textbox/email", ajaxInfo.GetXmlProperty("genxml/textbox/email"));
                info.SetXmlProperty("genxml/checkbox/active", ajaxInfo.GetXmlProperty("genxml/checkbox/active"));
                info.SetXmlProperty("genxml/checkbox/autoremoveusers", ajaxInfo.GetXmlProperty("genxml/checkbox/autoremoveusers"));
                info.SetXmlProperty("genxml/checkbox/debugmode", ajaxInfo.GetXmlProperty("genxml/checkbox/debugmode"));
                var editlang = ajaxInfo.GetXmlProperty("genxml/hidden/editlang");
                var lastrun = ajaxInfo.GetXmlProperty("genxml/textbox/lastrun");
                if (Utils.IsDate(lastrun, editlang))
                {
                    var cultureInfo = new CultureInfo(editlang, true);
                    var lastrundate = Convert.ToDateTime(lastrun, cultureInfo);
                    lastrundate = lastrundate.Date.AddMinutes(130); // Make it 2:10 in the morning so scheudler runs if testing.
                    info.SetXmlProperty("genxml/textbox/lastrun", lastrundate.ToString("O"), TypeCode.DateTime);
                }
            }
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
            LogSystem("OS_GDPR - Scheduler: " + DateTime.Now.ToString());
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            if (info != null)
            {
                var strlastrun = info.GetXmlProperty("genxml/textbox/lastrun");
                var debugmode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
                if (Utils.IsDate(strlastrun))
                {
                    var lastrun = Convert.ToDateTime(strlastrun);
                    // run between 2am - 3am, once a day.
                    if ((lastrun < DateTime.Now.Date && DateTime.Now > DateTime.Now.Date.AddHours(2)))
                    {
                        LogSystem("OS_GDPR --------------- START Scheduler ---------------");
                        ActionGDPR(portalId);
                    }
                    else
                    {
                        LogSystem("OS_GDPR - Scheduler: Invalid Date Time (2am - 3am required)");
                    }
                }
                else
                {
                    LogSystem("OS_GDPR - Scheduler: Invalid Last Run Date.");
                }
            }
            else
            {
                LogSystem("OS_GDPR - NO DATA SETTINGS");
            }
        }

        public static void ActionGDPR(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            var autoremoveusers = info.GetXmlPropertyBool("genxml/checkbox/autoremoveusers");
            var debugmode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
            var active = info.GetXmlPropertyBool("genxml/checkbox/active");
            var removelimitdays = info.GetXmlPropertyInt("genxml/textbox/removelimitdays");
            var orderlimitdays = info.GetXmlPropertyInt("genxml/textbox/orderlimitdays");
            var email = info.GetXmlProperty("genxml/textbox/email"); // search for email, is a single user needs to be removed.
            if (removelimitdays > 0)
            {
                if (email != "")
                {
                    removelimitdays = 0; // searching for single user, may not reach limit.
                    autoremoveusers = false;
                }
                if (removelimitdays >= 0)
                {

                    LogSystem("OS_GDPR --------------- START ---------------");
                    LogSystem("OS_GDPR - active: " + active);
                    LogSystem("OS_GDPR - removelimitdays: " + removelimitdays);
                    LogSystem("OS_GDPR - debugmode: " + debugmode);
                    LogSystem("OS_GDPR - autoremoveusers: " + autoremoveusers);

                    if (active)
                    {
                        var removeList = new List<UserInfo>();
                        // only process 1000 records to help stop timeout.
                        var filter = "SELECT TOP 500 ";
                        filter += " Us.userid ";
                        filter += " FROM {databaseOwner}[{objectQualifier}Users] as Us ";
                        filter += " inner join {databaseOwner}[{objectQualifier}aspnet_Users] as U on U.UserName = Us.Username ";
                        filter += " inner join {databaseOwner}[{objectQualifier}aspnet_Membership] as M on M.UserId = U.UserId ";
                        filter += " where M.LastLoginDate < DATEADD(DAY, -" + removelimitdays + ",GetDate()) ";
                        if (email != "") filter += "and Us.email like '%" + email + "%'";
                        var listUsers = DataContext.Instance().ExecuteQuery<int>(CommandType.Text, filter.Replace("@", "@@"));
                        foreach (var uId in listUsers)
                        {
                            var userInfo = UserController.Instance.GetUser(portalId, uId);
                            if (userInfo != null && (userInfo.Membership.LastLoginDate < DateTime.Now.AddDays(removelimitdays * -1)))
                            {
                                if (!userInfo.IsSuperUser && !userInfo.IsInRole("Administrators") && !userInfo.IsInRole("Manager"))
                                {
                                    removeList.Add(userInfo);
                                }
                            }
                        }

                        info.RemoveXmlNode("genxml/userlist");
                        info.SetXmlProperty("genxml/userlist", "");
                        foreach (var u in removeList)
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
                            if (autoremoveusers && email == "") DeleteUser(portalId, u.UserID, debugmode);

                            LogSystem("OS_GDPR - RemoveList Add: " + u.UserID);

                        }
                        info.SetXmlProperty("genxml/textbox/lastrun", DateTime.Now.ToString("O"), TypeCode.DateTime);
                        info.SetXmlProperty("genxml/textbox/email", "");
                        objCtrl.Update(info);

                        if (email == "") // if we are searching for user, do not process orders.
                        {
                            // Auto remove any order detail, without a user, over the retention time. 
                            if (orderlimitdays < 60) orderlimitdays = 60;
                            var _orderMaskList = new List<OrderData>();
                            filter = " and [XMLData].value('(genxml/createddate)[1]','DateTime') < DATEADD(DAY, -" + orderlimitdays + " ,GetDate()) ";
                            var list = objCtrl.GetList(portalId, -1, "ORDER", filter, "", 500);
                            LogSystem("OS_GDPR - Order orderlimitdays: " + orderlimitdays);
                            LogSystem("OS_GDPR - Order List: " + list.Count);
                            foreach (var info2 in list)
                            {
                                // We only want to mask clients without a user.  User Orders will be masked when the user is deleted.
                                var orderData = new OrderData(portalId, info2.ItemID);
                                var userInfo = UserController.Instance.GetUserById(portalId, orderData.UserId);
                                if (userInfo == null)
                                {
                                    if (orderData.UserId > 0) DeleteDataRecords(orderData.UserId);
                                    _orderMaskList.Add(orderData);
                                }
                            }
                            LogSystem("OS_GDPR - Order Mask List (without user): " + _orderMaskList.Count);
                            foreach (var o in _orderMaskList)
                            {
                                MaskOrder(o, debugmode);
                            }

                        }
                    }
                    LogSystem("OS_GDPR --------------- END ---------------");
                }
            }

            LogSystemClear(30);
        }
        private static void DeleteDataRecords(int userid)
        {
            var objCtrl = new NBrightBuyController();

            var cmdStr = "select ItemId from {databaseOwner}[{objectQualifier}NBrightBuy] where TypeCode = 'CLIENT' and userid = " + userid.ToString() + " ";
            var list1 = DataContext.Instance().ExecuteQuery<int>(CommandType.Text, cmdStr);
            foreach (var itemId in list1)
            {
                objCtrl.Delete(itemId);
            }
            var cmdStr2 = "select ItemId from {databaseOwner}[{objectQualifier}NBrightBuy] where TypeCode = 'USERDATA' and userid = " + userid.ToString() + " ";
            var list2 = DataContext.Instance().ExecuteQuery<int>(CommandType.Text, cmdStr2);
            foreach (var itemId in list2)
            {
                objCtrl.Delete(itemId);
            }


            // This MIGHT be a better solution. (Using "DataContext.Instance().Execute") 
            //DataContext.Instance().Execute(CommandType.Text, "delete from {databaseOwner}[{objectQualifier}NBrightBuy] where TypeCode = 'CLIENT' and userid = " + userid);
            //DataContext.Instance().Execute(CommandType.Text, "delete from {databaseOwner}[{objectQualifier}NBrightBuy] where TypeCode = 'USERDATA' and userid = " + userid);

        }
        public static void DeleteAll()
        {
            LogSystem("OS_GDPR - Delete ALL ");
            var portalId = DnnUtils.GetCurrentPortalSettings().PortalId;
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetByGuidKey(portalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
            var debugmode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
            var userNodList = info.XMLDoc.SelectNodes("genxml/userlist/*");
            foreach(XmlNode nod in userNodList)
            {
                var sRec = new NBrightInfo();
                sRec.XMLData = nod.OuterXml;
                var userId = sRec.GetXmlPropertyInt("genxml/userid");
                DeleteUser(portalId, userId, debugmode);
            }
        }
        public static void Delete(NBrightInfo ajaxInfo)
        {
            var userid = ajaxInfo.GetXmlPropertyInt("genxml/hidden/userid");
            if (userid > 0)
            {
                var objCtrl = new NBrightBuyController();
                var info = objCtrl.GetByGuidKey(DnnUtils.GetCurrentPortalSettings().PortalId, -1, "OS_GDPRDATA", "OS_GDPRDATA");
                var debugmode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
                DeleteUser(DnnUtils.GetCurrentPortalSettings().PortalId, userid, debugmode);
            }
        }

        public static void DeleteUser(int portalId, int userId, bool debugmode)
        {
            var objCtrl = new NBrightBuyController();
            var userInfo = UserController.Instance.GetUserById(portalId, userId);
            if (userInfo != null)
            {
                LogSystem("OS_GDPR - Delete User: " + userInfo.UserID);
                if (!debugmode)
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

                    DeleteDataRecords(userId);

                }
                UserOrders(portalId, userId, debugmode);
            }
        }
        public static void UserOrders(int portalId, int userId, bool debugmode)
        {
            LogSystem("OS_GDPR - Mask Orders UserId: " + userId);
            var orderUsers = DataContext.Instance().ExecuteQuery<int>(CommandType.Text, "select itemid from {databaseOwner}[{objectQualifier}NBrightBuy] where TypeCode = 'ORDER' and userid = " + userId + " ");
            LogSystem("OS_GDPR - Mask Orders Count: " + orderUsers.Count());
            foreach (var o in orderUsers)
            {
                var orderData = new OrderData(portalId, o);
                MaskOrder(orderData, debugmode);
            }
        }

        public static void MaskOrder(OrderData o, bool debugmode)
        {
            if (!debugmode)
            {
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/firstname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/lastname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/telephone", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/email", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/company", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/unit", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/street", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/city", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/genxml/textbox/postalcode", "xxxxxxxxxxxx");
                o.PurchaseInfo.RemoveXmlNode("genxml/billaddress/genxml/dropdownlist");


                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/firstname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/lastname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/telephone", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/email", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/company", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/unit", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/street", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/city", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/genxml/textbox/postalcode", "xxxxxxxxxxxx");
                o.PurchaseInfo.RemoveXmlNode("genxml/shipaddress/genxml/dropdownlist");

                o.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/textbox/taxnumber", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/clientdisplayname", "xxxxxxxxxxxx");

                o.Save();
            }
            LogSystem("OS_GDPR - Mask Order: " + o.OrderNumber);

        }

        public static void LogSystem(string message)
        {
            var mappath = HostingEnvironment.MapPath("/Portals/_default/OS_GDPRLogs");
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            AppendToLog(mappath, "system", message);
        }
        public static void LogSystemClear(int daysToKeep)
        {
            var mappath = HostingEnvironment.MapPath("/Portals/_default/OS_GDPRLogs");
            if (!Directory.Exists(mappath)) Directory.CreateDirectory(mappath);
            DirectoryInfo di = new DirectoryInfo(mappath);
            foreach (FileInfo file in di.GetFiles())
            {
                if (file.CreationTime < DateTime.Now.AddDays(daysToKeep * -1)) file.Delete();
            }
        }
        private static void AppendToLog(string logMapPathFolder, string logName, string logMessage)
        {
            var dstring = DateTime.Now.ToString("yyyy-MM-dd");
            var logfilename = logMapPathFolder.TrimEnd('\\') + "\\" + dstring + "_" + Path.GetFileNameWithoutExtension(logName) + ".txt";
            if (!File.Exists(logfilename))
            {
                SaveFile(logfilename, "START" + Environment.NewLine);
            }
            using (StreamWriter w = File.AppendText(logfilename))
            {
                w.WriteLine($"{DateTime.Now.ToString("d/MM/yyyy HH:mm:ss") + " :  " + logMessage}");
            }

        }
        public static void SaveFile(string fullFileName, string data)
        {
            var encoding = new UTF8Encoding();
            var buffer = encoding.GetBytes(data);
            SaveFile(fullFileName, buffer);
        }
        public static void SaveFile(string fullFileName, byte[] buffer)
        {
            if (File.Exists(fullFileName))
            {
                File.SetAttributes(fullFileName, FileAttributes.Normal);
            }
            FileStream fs = null;
            try
            {
                fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write);
                fs.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                var ms = ex.ToString();
                // ignore, stop eror here, not important if locked.
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                }
            }
        }



    }
}
