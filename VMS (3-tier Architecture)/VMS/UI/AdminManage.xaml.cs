using System;
using System.Windows;
using System.Windows.Controls;
using VMS.BLL;

namespace VMS.UI
{
    public partial class AdminManage : Page
    {
        private AdminManageConditions _adminLogic = new AdminManageConditions();
        private long _selectedAdminId = 0;
        private PendingRequest _selectedRequest = null;

        public AdminManage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDataGrids();
        }

        private void RefreshDataGrids()
        {
            try
            {
                dgAdmins.ItemsSource = _adminLogic.LoadStandardAdmins();
                dgRequests.ItemsSource = _adminLogic.LoadPendingRequests();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // UI TOGGLES
        // ==========================================
        private void dgAdmins_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAdmins.SelectedItem is AdminUser selectedAdmin)
            {
                // Clear the other grid's selection so the user knows which mode they are in
                dgRequests.SelectedItem = null;
                _selectedRequest = null;

                txtFormTitle.Text = "Edit Administrator";
                _selectedAdminId = selectedAdmin.AdminID;
                txtName.Text = selectedAdmin.Name;
                txtUsername.Text = selectedAdmin.Username;
                txtContact.Text = selectedAdmin.Contact.ToString();
                txtPassword.Text = selectedAdmin.Password;
                rbFemale.IsChecked = (selectedAdmin.Gender == "Female");
                rbMale.IsChecked = (selectedAdmin.Gender != "Female");

                // Show standard buttons, hide request buttons
                pnlStandardActions.Visibility = Visibility.Visible;
                pnlRequestActions.Visibility = Visibility.Collapsed;

                btnAdd.IsEnabled = false;
                btnUpdate.IsEnabled = true;
                btnDelete.IsEnabled = true;
            }
        }

        private void dgRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRequests.SelectedItem is PendingRequest req)
            {
                // Clear the other grid's selection
                dgAdmins.SelectedItem = null;
                _selectedAdminId = 0;

                txtFormTitle.Text = "Review Account Request";
                _selectedRequest = req;
                txtName.Text = req.FullName;
                txtUsername.Text = req.Username;
                txtContact.Text = req.Contact.ToString();
                txtPassword.Text = req.Password;
                rbFemale.IsChecked = (req.Gender == "Female");
                rbMale.IsChecked = (req.Gender != "Female");

                // Hide standard buttons, show request buttons
                pnlStandardActions.Visibility = Visibility.Collapsed;
                pnlRequestActions.Visibility = Visibility.Visible;
            }
        }

        // ==========================================
        // STANDARD ADMIN ACTIONS
        // ==========================================
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string gender = rbMale.IsChecked == true ? "Male" : "Female";
                if (_adminLogic.CreateNewAdmin(txtName.Text, gender, txtContact.Text, txtUsername.Text, txtPassword.Text))
                {
                    MessageBox.Show("Admin created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshDataGrids();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string gender = rbMale.IsChecked == true ? "Male" : "Female";
                if (_adminLogic.UpdateExistingAdmin(_selectedAdminId, txtName.Text, gender, txtContact.Text, txtUsername.Text, txtPassword.Text))
                {
                    MessageBox.Show("Admin updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshDataGrids();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete this administrator?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (_adminLogic.DeleteAdmin(_selectedAdminId))
                    {
                        MessageBox.Show("Admin deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshDataGrids();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==========================================
        // REQUEST ACTIONS
        // ==========================================
        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest != null)
            {
                try
                {
                    // Update the object just in case the Super Admin corrected a typo in the UI before approving
                    _selectedRequest.FullName = txtName.Text;
                    _selectedRequest.Username = txtUsername.Text;
                    _selectedRequest.Contact = Convert.ToInt64(txtContact.Text);
                    _selectedRequest.Password = txtPassword.Text;
                    _selectedRequest.Gender = rbMale.IsChecked == true ? "Male" : "Female";

                    if (_adminLogic.ApproveRequest(_selectedRequest))
                    {
                        MessageBox.Show("Request approved! They are now an Administrator.", "Approved", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshDataGrids();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest != null && MessageBox.Show("Are you sure you want to reject and delete this request?", "Confirm Reject", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (_adminLogic.RejectRequest(_selectedRequest.Id))
                    {
                        MessageBox.Show("Request rejected and removed.", "Rejected", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshDataGrids();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedAdminId = 0;
            _selectedRequest = null;

            txtFormTitle.Text = "Admin Details";
            txtName.Clear();
            txtUsername.Clear();
            txtContact.Clear();
            txtPassword.Clear();
            rbMale.IsChecked = true;

            // Reset to standard mode layout
            pnlStandardActions.Visibility = Visibility.Visible;
            pnlRequestActions.Visibility = Visibility.Collapsed;

            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;

            dgAdmins.SelectedItem = null;
            dgRequests.SelectedItem = null;
        }
    }
}