using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string connectionString = "Server=localhost;Database=DBDiplom;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";
            DatabaseService databaseService = new DatabaseService(connectionString);

            while (true)
            {
                using (LoginForm loginForm = new LoginForm(databaseService))
                {
                    if (loginForm.ShowDialog() != DialogResult.OK)
                    {
                        break; // Выход из приложения
                    }

                    if (LoginForm.CurrentUserRole == "Teacher")
                    {
                        using (var teacherForm = new TeacherMainForm(databaseService))
                        {
                            teacherForm.ShowDialog();
                        }
                    }
                    else
                    {
                        using (var studentForm = new StudentCalculatorForm(databaseService))
                        {
                            studentForm.ShowDialog();
                        }
                    }
                }
            }
        }
    }
    
}
