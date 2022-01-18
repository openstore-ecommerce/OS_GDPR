using DotNetNuke.Entities.Users;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_GDPR.Componants
{
    public class OrderLimpet
    {
        private int _portalId; 
        private List<OrderData> _orderMaskList;
        OrderLimpet(int portalId, int removeLimitDays)
        {
            _portalId = portalId;
            RemoveLimitDays = removeLimitDays;
            var _orderMaskList = new List<OrderData>();
        }
        /// <summary>
        /// Do calculation on all Ordeer and add into removeList is over RemoveLimitDays since creation date.
        /// Has been designed to be used from the scheulder.
        /// </summary>
        public void ProcessUsers()
        {
            var _orderMaskList = new List<OrderData>();
            var objCtrl = new NBrightBuyController();
            var filter = " and [XMLData].value('(genxml/createddate)[1]','DateTime') < DATEADD(DAY, -" + RemoveLimitDays + " ,GetDate()) ";
            var list = objCtrl.GetList(_portalId, -1, "ORDER", filter);
            foreach (var info in list)
            {
                var orderData = new OrderData(_portalId, info.ItemID);
                _orderMaskList.Add(orderData);
            }
            foreach (var o in _orderMaskList)
            {
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/firstname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/lastname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/telephone", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/email", "xxxxxxxxxxxx"); 
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/company", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/unit", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/street", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/city", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/textbox/postalcode", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/billaddress/dropdownlist/selectaddress", "xxxxxxxxxxxx");

                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/firstname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/lastname", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/telephone", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/email", "xxxxxxxxxxxx"); 
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/company", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/unit", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/street", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/city", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/textbox/postalcode", "xxxxxxxxxxxx");
                o.PurchaseInfo.SetXmlProperty("genxml/shipaddress/dropdownlist/selectaddress", "xxxxxxxxxxxx");

                o.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/textbox/cartemailaddress", "xxxxxxxxxxxx"); 
                o.PurchaseInfo.SetXmlProperty("genxml/extrainfo/genxml/textbox/taxnumber", "xxxxxxxxxxxx"); 
                o.PurchaseInfo.SetXmlProperty("genxml/clientdisplayname", "xxxxxxxxxxxx");

                o.Save();
            }

        }
        public int RemoveLimitDays { get; set; }
        public List<OrderData> OrderMaskList { get { return _orderMaskList;} }
    }
}
