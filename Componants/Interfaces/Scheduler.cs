using DotNetNuke.Entities.Portals;
using NBrightDNN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Providers.OS_GDPR
{
    public class Scheduler : Nevoweb.DNN.NBrightBuy.Components.Interfaces.SchedulerInterface
    {
        public override string DoWork(int portalId)
        {

            LocalUtils.SchedulerActionGDPR(portalId);
            return "GDPR Action";
        }
    }
}
