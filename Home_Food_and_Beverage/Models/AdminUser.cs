using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home_Food_and_Beverage.Models
{
    internal class AdminUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string TwoFactorCode { get; set; }
        public string Name { get; set; }
    }
}
