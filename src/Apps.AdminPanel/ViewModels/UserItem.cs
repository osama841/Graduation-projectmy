using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.AdminPanel.ViewModels
{
  public  class UserItem
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string DeviceKey { get; set; } // هذا هو الحقل الجديد (البصمة)
        public string Status { get; set; } // نشط / غير نشط
        public string StatusColor { get; set; } // لون الحالة (Green/Red)
    }

}
