using VMS.DAL;
using System;
using System.Windows;

namespace VMS.UI
{
    public partial class AccountSettings : Window
    {
        private string _username;
        private AccountSettingsDA _AccountSettingsDA = new AccountSettingsDA();

        // Pass the logged-in username into the window when it opens
        public AccountSettings(string loggedInUsername)
        {
            InitializeComponent();
            _username = loggedInUsername;
            LoadCurrentDetails();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // ---> 1. PASSWORD LENGTH CHECK <---
            // Checks if it's empty OR strictly less than 6 characters
            if (string.IsNullOrWhiteSpace(txtPassword.Text) || txtPassword.Text.Length < 6)
            {
                MessageBox.Show("Security Alert: Password must be at least 6 characters long!", "Invalid Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Stops the code here!
            }

            // ---> 2. MISSING INFO CHECK <---
            // Ensures they didn't accidentally delete their name or contact number
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtContact.Text))
            {
                MessageBox.Show("Please fill out all required fields.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Stops the code here!
            }

            // If it passes the checks above, it moves on to save the data:
            try
            {
                AdminAccountDTO updatedAccount = new AdminAccountDTO
                {
                    Name = txtName.Text,
                    Contact = Convert.ToInt64(txtContact.Text),
                    Password = txtPassword.Text,
                    Gender = cmbGender.SelectedIndex == 0 // Sets True if Male is selected
                };

                _AccountSettingsDA.UpdateAdminAccount(_username, updatedAccount);

                MessageBox.Show("Account updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Closes the window
            }
            catch (FormatException)
            {
                MessageBox.Show("Please ensure your contact number only contains digits.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating account: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCurrentDetails()
        {
            try
            {
                AdminAccountDTO account = _AccountSettingsDA.GetAdminAccount(_username);

                txtName.Text = account.Name;
                txtContact.Text = account.Contact.ToString();
                txtPassword.Text = account.Password;

                // Assuming True = Male (Index 0), False = Female (Index 1)
                cmbGender.SelectedIndex = account.Gender ? 0 : 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load account details: " + ex.Message);
            }
        }

        

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}