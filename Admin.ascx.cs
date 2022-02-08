// --- Copyright (c) notice NevoWeb ---
//  Copyright (c) 2014 SARL NevoWeb.  www.nevoweb.com. The MIT License (MIT).
// Author: D.C.Lee
// ------------------------------------------------------------------------
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ------------------------------------------------------------------------
// This copyright notice may NOT be removed, obscured or modified without written consent from the author.
// --- End copyright notice --- 

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI.WebControls;
using DataProvider = DotNetNuke.Data.DataProvider;

namespace OpenStore.Providers.OS_GDPR
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Admin : NBrightBuyAdminBase
    {

        #region Event Handlers


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // inject any pageheader we need
            var nbi = new NBrightInfo();
            nbi.Lang = Utils.GetCurrentCulture();
            nbi.PortalId = PortalId;

            var pageheaderTempl = NBrightBuyUtils.RazorTemplRender("admin_head.cshtml", 0, "", nbi, "/DesktopModules/NBright/OS_GDPR", "config", Utils.GetCurrentCulture(), StoreSettings.Current.Settings());
            PageIncludes.IncludeTextInHeader(Page, pageheaderTempl);

        }



        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    RazorPageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void RazorPageLoad()
        {
            if (NBrightBuyUtils.CheckRights())
            {
                var lit = new Literal();
                lit.Text = LocalUtils.GetData();
                phData.Controls.Add(lit);
            }

        }

        #endregion



    }

}
