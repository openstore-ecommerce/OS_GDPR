using DotNetNuke.Entities.Portals;
using NBrightDNN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Providers.OS_GDPR
{
    public class Promo : Nevoweb.DNN.NBrightBuy.Components.Interfaces.PromoInterface
    {
        public override NBrightInfo CalculatePromotion(int portalId, NBrightInfo cartInfo)
        {
            throw new NotImplementedException();
        }

        public override string ProviderKey { get; set; }
    }
}
