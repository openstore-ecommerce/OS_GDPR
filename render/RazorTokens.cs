using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using NBrightBuy.render;
using NBrightCore.common;
using NBrightCore.images;
using NBrightCore.providers;
using NBrightCore.render;
using NBrightDNN;
using NBrightDNN.render;
using OpenStore;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace NBrightBuy.OS_GDPR.render
{
    public class RazorTokens<T> : NBrightBuyRazorTokens<T>
    {

        public IEncodedString TestName(NBrightInfo info)
        {
            return new RawString("MY PLUGIN TOKEN");
        }


    }
}
