using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.GameUtils
{
    /// <summary>
    /// Класс, формирующий набор игроков
    /// </summary>
    public static  class UserSetup
    {
        public static List<Player> CreateUserList(int count){
            var UserList= new List<Player>();
            for(int i = 0; i < count; i++)
            {
                UserList.Add(new Player(i));
            }
            return UserList;
        }

        internal static List<Player> CreateUserList(List<string> usernameList)
        {
            var UserList = new List<Player>();
            for (int i = 0; i < usernameList.Count(); i++)
            {
                UserList.Add(new Player(i, usernameList[i]));
            }
            return UserList;
        }
    }
}
