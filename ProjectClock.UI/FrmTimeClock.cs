using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ProjectClock.UI
{
    public partial class FrmTimeClock : Form
    {
        // נתיב חיבור למסד נתונים
        string ConnectionString = "data source=localhost\\SQLEXPRESS; initial catalog=KA_Programing; Integrated Security=true;";
        // משתנה אשר מחזיק את הקשר למסד הנתונים
        public SqlConnection connection;
        public FrmTimeClock()
        {
            InitializeComponent();
            // חיבור בין הנתיב למשתנה
            connection = new SqlConnection(ConnectionString);
        }
        
        //function - checks if id number is valid
        private bool checkID(string ID)
        {
            return true;
        }
        //function - data base connection check
        public bool connect()
        {
            try
            {
                connection.Open();
                //MessageBox.Show("החיבור הצליח");
                return true;
            }
            catch (SqlException ex)
            {
                //MessageBox.Show(ex.Message);
                return false;
            }
        }
        //function - פונקציה השולחת פרטי משתמש למסד נתונים ומקבלת תשובה
        private string sendDetails(string ID, string Password)
        {
            // מנסה להתחבר למסד הנתונים
            if (!connect())
                return "החיבור לא הצליח";
            // שאילתה של שפת מסד הנתונים
            string insert = "declare @answer varchar(100), @employee_code int\r\n\r\nselect @employee_code=(select code from Employees where ID=@ID)\r\nif @employee_code is null\r\n\tbegin\r\n\t\tselect @answer='הוקש נתון שגוי'\t\r\n\tend\r\nelse\r\n\tbegin\r\n\t\tif not exists(select code from Passwords where password=@password and is_active = 1 and employee_code=@employee_code)\r\n\t\t\tbegin\r\n\t\t\t\tselect @answer='הוקש נתון שגוי'\t\r\n\t\t\tend\r\n\t\telse\r\n\t\t\tbegin\r\n\t\t\t\tif not exists(select code from Passwords where password=@password and is_active = 1 and employee_code=@employee_code and expiry > getdate())\r\n\t\t\t\t\tbegin\r\n\t\t\t\t\t\tselect @answer='פג תוקף הסיסמא - אנא הזן סיסמא חדשה'\r\n\t\t\t\t\tend\r\n\t\t\t\telse\r\n\t\t\t\t\tbegin\r\n\t\t\t\t\t\tif not exists (select * from Times where employee_code=@employee_code and exit_time is null)\r\n\t\t\t\t\t\t\tbegin\r\n\t\t\t\t\t\t\t\tinsert into Times values(@employee_code, getdate(),null)\r\n\t\t\t\t\t\t\t\tselect @answer=' שעת כניסה ' + FORMAT (getdate(), 'dd/MM/yyyy - HH:mm:ss ')\r\n\t\t\t\t\t\t\tend\r\n\t\t\t\t\t\telse\r\n\t\t\t\t\t\t\tbegin\r\n\t\t\t\t\t\t\t\tupdate Times set exit_time = getdate() where employee_code=@employee_code and exit_time is null\r\n\t\t\t\t\t\t\t\tselect @answer=' שעת יציאה ' + FORMAT (getdate(), 'dd/MM/yyyy - HH:mm:ss ')\r\n\t\t\t\t\t\t\tend\r\n\t\t\t\t\tend\r\n\t\t\tend\r\n\tend\r\nselect @answer";
            // יצחרת אובייקט הביצוע והוספת שאילתה והחיבור לאובייקט
            SqlCommand cmd = new SqlCommand(insert, connection);
            // הוספת הפרמטרים
            cmd.Parameters.AddWithValue("@ID", ID);

            cmd.Parameters.Add("@password", SqlDbType.VarChar, 10);
            cmd.Parameters["@password"].Value = Password;
            //החזרת תשובה
            string answer = cmd.ExecuteScalar().ToString();
            connection.Close();
            return answer;
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                if (txtID.Text == "" || txtPassword.Text == string.Empty)
                {
                    MessageBox.Show("נא למלא את כל השדות", "מספר זהות וסיסמא", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                MessageBox.Show(sendDetails(txtID.Text, txtPassword.Text));
            }

        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string EmployeeID = txtID.Text;
            if (EmployeeID == string.Empty)
            {
                EmployeeID = Microsoft.VisualBasic.Interaction.InputBox("נא הכנס מספר זהות", "החלפת סיסמא");
            }
            FrmPasswordChange frmPasswordChange = new FrmPasswordChange(this, EmployeeID);
            frmPasswordChange.Show();
        }
    }
}
