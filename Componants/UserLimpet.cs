using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using Nevoweb.DNN.NBrightBuy.Components;
using System;
using System.Collections.Generic;
using System.Data;
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
        public void ProcessUsers(string email)
        {
            _removeList = new List<UserInfo>();
            var filter = "SELECT userid FROM Users";
            if (email != "") filter = "SELECT userid FROM Users where email like '%" + email + "%'";
            var listUsers = DataContext.Instance().ExecuteQuery<int>(CommandType.Text, filter.Replace("@", "@@"));
            foreach (var uId in listUsers)
            {
                var userInfo = UserController.Instance.GetUser(_portalId, uId);
                if (userInfo != null && userInfo.Membership.LastLoginDate < DateTime.Now.AddDays(RemoveLimitDays * -1))
                {
                    if (!userInfo.IsSuperUser && !userInfo.IsInRole("Administrators") && !userInfo.IsInRole("Manager"))
                    {
                        _removeList.Add(userInfo);
                    }
                }
            }
        }

        public int RemoveLimitDays { get; set; }
        public List<UserInfo> RemoveList { get { return _removeList;} }
    }
}
