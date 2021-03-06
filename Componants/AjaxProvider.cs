using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;
using Nevoweb.DNN.NBrightBuy.Components.Products;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Xml;

namespace OpenStore.Providers.OS_GDPR
{
    public class AjaxProvider : AjaxInterface
    {
        public override string Ajaxkey { get; set; }

        public override string ProcessCommand(string paramCmd, HttpContext context, string editlang = "")
        {
            if (!LocalUtils.CheckRights())
            {
                return "Security Error.";
            }

            var ajaxInfo = NBrightBuyUtils.GetAjaxFields(context);
            var lang = NBrightBuyUtils.SetContextLangauge(ajaxInfo); // Ajax breaks context with DNN, so reset the context language to match the client.
            var objCtrl = new NBrightBuyController();

            var strOut = "OS_GDPR Ajax Error";

            // NOTE: The paramCmd MUST start with the plugin ref. in lowercase. (links ajax provider to cmd)
            switch (paramCmd)
            {
                case "os_gdpr_getdata":
                    strOut = LocalUtils.GetData("datafields.cshtml");
                    break;
                case "os_gdpr_deleterecord":
                    var infoDel = objCtrl.GetPluginSinglePageData("OS_GDPRDATA", "OS_GDPRDATA", "en-US");
                    objCtrl.Delete(infoDel.ItemID);
                    break;
                case "os_gdpr_savedata":
                    strOut = LocalUtils.SaveData(ajaxInfo);
                    break;
                case "os_gdpr_execute":
                    LocalUtils.SaveData(ajaxInfo);
                    LocalUtils.ExecuteGDPR();
                    strOut = "OK";
                    break;
                case "os_gdpr_delete":
                    LocalUtils.Delete(ajaxInfo);
                    strOut = "OK";
                    break;
                case "os_gdpr_deleteall":
                    LocalUtils.DeleteAll();
                    strOut = "OK";
                    break;
            }

            return strOut;

        }

        public override void Validate()
        {
        }


    }
}
