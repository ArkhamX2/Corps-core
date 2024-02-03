﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corps.Core.Model.GameUtils
{
    public static  class UserSetup
    {
        public static List<GameUser> CreateUserList(int count){
            var UserList= new List<GameUser>();
            for(int i = 0; i < count; i++)
            {
                GameUser user = new GameUser(i);
                if (i==0)
                {
                    user.NextId=i+1;
                    user.PrevId=count-1;
                }
                else if(i==count-1) 
                {
                    user.NextId=0;
                    user.PrevId=i-1;
                }
                else
                {
                    user.NextId=i+1;
                    user.PrevId=i-1;
                }
                UserList.Add(user);
            }
            return UserList;
        }       
    }
}
