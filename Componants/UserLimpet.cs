using DotNetNuke.Entities.Users;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_GDPR.Componants
{
    public class UserLimpet
    {
        private int _portalId; 
        private List<UserInfo> _removeList;
        UserLimpet(int portalId, int removeLimitDays)
        {
            _portalId = portalId;
            RemoveLimitDays = removeLimitDays;


        }
        /// <summary>
        /// Do calculation on all users and add into removeList is over RemoveLimitDays since last login.
        /// WARNING: SLOW --> Has been designed to be used from the scheulder.
        /// </summary>
        public void ProcessUsers()
        {
            var objCtrl = new NBrightBuyController();
            var list = objCtrl.GetDnnUsers(_portalId, "",0, 0, 0, 0);
            foreach (var u in list)
            {
                var userInfo = UserController.Instance.GetUser(_portalId, u.ItemID);
                if (userInfo.Membership.LastLoginDate < DateTime.Now.AddDays(RemoveLimitDays * -1))
                {
                    RemoveList.Add(userInfo);
                }
            }
        }
        public int RemoveLimitDays { get; set; }
        public List<UserInfo> RemoveList { get { return _removeList;} }
    }
}
