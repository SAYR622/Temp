using System;
using System.Windows;
using VMS.BLL;

namespace VMS.UI
{
    public partial class RequestAccount : Window
    {
        // Instantiate the Business Logic Layer
        private AdminRequestConditions _adminLogic = new AdminRequestConditions();

        public RequestAccount()
        {
            InitializeComponent();
        }

        private void btnRequestAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Extract values without spaces in the beginning and the end
                string fullName = txtFullName.Text.Trim();
                string username = txtUsername.Text.Trim();
                string contact = txtContact.Text.Trim();
                string password = txtPassword.Password;
                string gender = rbMale.IsChecked == true ? "Male" : "Female";

                // 2. Pass everything to the BLL
                bool isSuccess = _adminLogic.ProcessAccountRequest(fullName, username, contact, gender, password);

                // 3. React to Success
                if (isSuccess)
                {
                    MessageBox.Show("Account successfully requested and stored in the database!", "System Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFormData();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to save the request. Please try again.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // 4. Catch Validation Errors from the BLL and display them
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearFormData()
        {
            txtFullName.Clear();
            txtUsername.Clear();
            txtContact.Clear();
            txtPassword.Clear();
            rbMale.IsChecked = true;
        }
    }
}