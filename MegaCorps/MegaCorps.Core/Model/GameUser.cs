using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model
{

    /// <summary>
    /// Класс пользователя
    /// </summary>
    public class GameUser
    {
        public int ID { get; set; }
        public int Score { get; set; }
        public GameUser(int id)
        {
            ID=id;
            Score=1;
        }
    }
}
