using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjectClock.UI
{
    public partial class FrmPasswordChange : Form
    {
        FrmTimeClock parent;
        string EmployeeID;
        public FrmPasswordChange(FrmTimeClock Parent, string ID)
        {
            InitializeComponent();
            parent = Parent;
            EmployeeID = ID;
            lblID.Text = ID;
        }
        
        private string sendDetails(string ID, string oldPassword, string newPassword, string newPasswordValid)
        {
            // מנסה להתחבר למסד הנתונים
            if (!parent.connect())
                return "החיבור לא הצליח";
            // שאילתה של שפת מסד הנתונים
            string insert = "declare \r\n@employee_code int, \r\n@answer varchar(100)\r\n\r\nselect @employee_code=(select code from Employees where ID=@ID)\r\n\r\n\t\tif not exists(select code from Passwords where password=@old_password and is_active = 1 and employee_code=@employee_code)\r\n\t\t\tbegin\r\n\t\t\t\tselect @answer='הוקש נתון שגוי'\t\r\n\t\t\tend\r\n\t\telse\r\n\t\t\tbegin\r\n\t\t\t\tif (@new_password = @new_password_valid and not exists (select Employee_code from Passwords where @new_password = password and is_active = 0 ))\r\n\t\t\t\t\tbegin\r\n\t\t\t\t\t\tupdate Passwords set is_active = 0 where employee_code=@employee_code\r\n\t\t\t\t\t\tinsert into Passwords values(@employee_code, @new_password ,getdate()+180, 1)\r\n\t\t\t\t\t\tselect @answer = 'הסיסמא שונתה בהצלחה'\r\n\t\t\t\t\tend\r\n\t\t\t\telse\r\n\t\t\t\t\tbegin\r\n\t\t\t\t\t\tselect @answer = 'נסה שוב'\r\n\t\t\t\t\tend\r\n\t\t\tend\r\n\t\r\n\t\r\n\tselect @answer";
            // יצחרת אובייקט הביצוע והוספת שאילתה והחיבור לאובייקט
            SqlCommand cmd = new SqlCommand(insert, parent.connection);
            // הוספת הפרמטרים
            cmd.Parameters.AddWithValue("@ID", ID);
            cmd.Parameters.AddWithValue("@old_password", oldPassword);
            cmd.Parameters.AddWithValue("@new_password", newPassword);
            cmd.Parameters.AddWithValue("@new_password_valid", newPasswordValid);
            //החזרת תשובה
            string answer = cmd.ExecuteScalar().ToString();
            parent.connection.Close();
            return answer;
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            string response = sendDetails(EmployeeID, txtOldPass.Text, txtNewPass.Text, txtNewPassValid.Text);
            MessageBox.Show(response);
            if (response == "הסיסמא שונתה בהצלחה")
            {
                Close();
            }
        }
    }
}
