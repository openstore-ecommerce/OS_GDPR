using DotNetNuke.Entities.Portals;
using NBrightDNN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Providers.OS_GDPR
{
    public class DiscountCode : Nevoweb.DNN.NBrightBuy.Components.Interfaces.DiscountCodeInterface
    {
        public override NBrightInfo CalculateItemPercentDiscount(int portalId, int userId, NBrightInfo cartItemInfo, string discountcode)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo UpdatePercentUsage(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo CalculateVoucherAmount(int portalId, int userId, NBrightInfo cartInfo, string discountcode)
        {
            throw new NotImplementedException();
        }

        public override NBrightInfo UpdateVoucherAmount(int portalId, int userId, NBrightInfo purchaseInfo)
        {
            throw new NotImplementedException();
        }

        public override string ProviderKey { get; set; }
    }
}
