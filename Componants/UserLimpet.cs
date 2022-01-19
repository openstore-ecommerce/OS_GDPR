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
        public UserLimpet(int portalId, int removeLimitDays)
        {
            _portalId = portalId;
            RemoveLimitDays = removeLimitDays;
            _removeList = new List<UserInfo>();
        }
        /// <summary>
        /// Do calculation on all users and add into removeList is over RemoveLimitDays since last login.
        /// WARNING: SLOW --> Has been designed to be used from the scheulder.
        /// </summary>
        public void ProcessUsers()
        {
            _removeList = new List<UserInfo>();
            var objCtrl = new NBrightBuyController();
            var listUsers = UserController.Instance.GetUsersBasicSearch(_portalId,-1,-1,"",true,"","");
            foreach (var userInfo in listUsers)
            {
                if (userInfo != null && userInfo.Membership.LastLoginDate < DateTime.Now.AddDays(RemoveLimitDays * -1))
                {
                    if (!userInfo.IsSuperUser && !userInfo.IsInRole("Administrators") && !userInfo.IsInRole("Manager"))
                    {
                        _removeList.Add(userInfo);
                    }
                }
            }
        }

        public void DeleteUser(int portalId, int userId)
        {

        }

        public int RemoveLimitDays { get; set; }
        public List<UserInfo> RemoveList { get { return _removeList;} }
    }
}
