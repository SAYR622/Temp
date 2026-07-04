using VMS.BLL;
using VMS.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static MaterialDesignThemes.Wpf.Theme;


namespace VMS
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private LoginConditions _loginConditions = new();

        public Login()
        {
            InitializeComponent();
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        [Obsolete]
        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 2. Pass the UI text to the BLL to do the heavy lifting
                bool isSuccess = _loginConditions.Login(txtUsername.Text, txtPassword.Password);

                // 3. React to the true/false result
                if (isSuccess)
                {

                    // THE FIX: Pass the text from the username box into the new MainWindow!
                    MainWindow dashboard = new MainWindow(txtUsername.Text);

                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                // 4. This catches the BLL exceptions (like "password must be 6 characters") 
                // OR any database connection errors from the DAL
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            RequestAccount create = new RequestAccount();
            create.Show();
        }

        private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If we press Down while in the Username box -> go to Password
            if (e.Key == Key.Down && sender == txtUsername)
            {
                txtPassword.Focus();
                e.Handled = true; // Tell WPF "I handled this key press, don't do anything else"
            }
            // If we press Up while in the Password box -> go to Username
            else if (e.Key == Key.Up && sender == txtPassword)
            {
                txtUsername.Focus();
                e.Handled = true;
            }
        }
    }
}