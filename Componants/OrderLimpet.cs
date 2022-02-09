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
        public OrderLimpet(int portalId, int removeLimitDays)
        {
            _portalId = portalId;
            RemoveLimitDays = removeLimitDays;
            _orderMaskList = new List<OrderData>();
        }
        /// <summary>
        /// Do calculation on all Ordeer and add into removeList if over RemoveLimitDays since creation date.
        /// Has been designed to be used from the scheulder.
        /// </summary>
        public void ProcessOrders()
        {
            if (RemoveLimitDays > 90) RemoveLimitDays = 90;
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
                MaskOrder(o);
            }
        }
        public void MaskOrder(OrderData o)
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

        public int RemoveLimitDays { get; set; }
        public List<OrderData> OrderMaskList { get { return _orderMaskList;} }
    }
}
