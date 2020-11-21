using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiringHarnessDetect.Model
{
    public class User : ValidateModelBase
    {



        [Key]
        public string UserID { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public Authority Access { get; set; }

        public string LastLoginTime { get; set; }
    }

    public enum Authority
    {
        Operator = 1,
        Maintainer = 2,
        Admin = 3,
    }
}
