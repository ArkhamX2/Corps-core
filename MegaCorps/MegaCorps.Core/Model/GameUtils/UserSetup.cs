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
    }
}
