using System;
using System.Collections.Generic;
using System.Diagnostics;       // NEW: Required to print to the Output window
using System.Threading.Tasks;   // NEW: Required for async Tasks
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;     // Added for Color formatting
using CHSMS.UI;
using Npgsql;
using Supabase;                 // NEW: The modern Supabase SDK
using VMS.DAL;

namespace VMS.UI
{
    public partial class MainWindow : Window
    {
        private string _loggedInUsername;

        public MainWindow(string usernameFromLogin)
        {
            InitializeComponent();
            _loggedInUsername = usernameFromLogin;

            // ---> SUBSCRIBE TO THE CLOSING EVENT <---
            this.Closing += MainWindow_Closing;

            try
            {
                MainFrame.Navigate(new Dashboard());

                // ---> SET DASHBOARD AS ACTIVE ON STARTUP <---
                SetActiveSidebarButton(btnDashboard);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard: " + ex.Message);
            }

            LoadUserProfile();
        }

        // ---> SIDEBAR HIGHLIGHTING LOGIC <---
        private void SetActiveSidebarButton(Button activeButton)
        {
            // 1. Define the colors (Matching your dark sidebar theme)
            var inactiveBackground = new SolidColorBrush(Colors.Transparent);
            var inactiveForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")); // Slate Gray

            var activeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")); // Dark highlighted box
            var activeForeground = new SolidColorBrush(Colors.White);

            // 2. Reset ALL buttons to inactive (Safely checks if they exist first)
            if (btnDashboard != null) { btnDashboard.Background = inactiveBackground; btnDashboard.Foreground = inactiveForeground; }
            if (btnHeritageSites != null) { btnHeritageSites.Background = inactiveBackground; btnHeritageSites.Foreground = inactiveForeground; }
            if (btnVisitors != null) { btnVisitors.Background = inactiveBackground; btnVisitors.Foreground = inactiveForeground; }
            if (btnEmailCenter != null) { btnEmailCenter.Background = inactiveBackground; btnEmailCenter.Foreground = inactiveForeground; }
            if (btnAdminManage != null) { btnAdminManage.Background = inactiveBackground; btnAdminManage.Foreground = inactiveForeground; }

            // 3. Apply the active colors to the clicked button
            if (activeButton != null)
            {
                activeButton.Background = activeBackground;
                activeButton.Foreground = activeForeground;
            }
        }

        // ---> THE NEW CLOSING EVENT HANDLER <---
        // This fires if they click Logout OR if they click the "X" in the corner
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                LoginDA loginDa = new LoginDA();
                loginDa.UpdateUserStatus(_loggedInUsername, false);
            }
            catch
            {
                // Silently fail if the database connection drops right as they are closing the app
            }
        }

        // 📈 Dashboard Button Click Event
        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetActiveSidebarButton(sender as Button); // <--- Highlight Button
                MainFrame.Navigate(new Dashboard());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to Dashboard: " + ex.Message, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 🏛 Heritage Sites Button Click Event
        private void btnHeritageSites_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetActiveSidebarButton(sender as Button); // <--- Highlight Button
                MainFrame.Navigate(new HeritageSites());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to Heritage Sites: " + ex.Message, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 👥 Visitors Button Click Event
        private void btnVisitors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetActiveSidebarButton(sender as Button); // <--- Highlight Button
                MainFrame.Navigate(new Visitors());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to Visitors: " + ex.Message, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 📧 Email Center Button Click Event
        private void btnEmailCenter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetActiveSidebarButton(sender as Button); // <--- Highlight Button
                MainFrame.Navigate(new EmailCenter());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to Email Center: " + ex.Message, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 📊 Admin Manage Button Click Event
        private void btnAdminManage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetActiveSidebarButton(sender as Button); // <--- Highlight Button
                MainFrame.Navigate(new AdminManage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to Admin Manage: " + ex.Message, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ⚙ Settings Button Click Event
        private void btnAccountSettings_Click(object sender, RoutedEventArgs e)
        {
            // _loggedInUsername is the variable you already have saved in MainWindow!
            AccountSettings settingsWindow = new AccountSettings(_loggedInUsername);

            if (settingsWindow.ShowDialog() == true)
            {
                // Optional: If they changed their name, you can refresh the profile block here!
                LoadUserProfile();
            }
        }

        // 🚪 Logout Button Click Event
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login login = new Login();
                login.Show();

                // This will trigger MainWindow_Closing automatically!
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during logout: " + ex.Message, "Logout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = !NotificationPopup.IsOpen;

            if (NotificationPopup.IsOpen)
            {
                LoadNotifications();
            }
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }

        private void LoadUserProfile()
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT name, a_type FROM admins WHERE username = @user";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", _loggedInUsername);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtAdminName.Text = reader["name"].ToString();
                                string adminType = reader["a_type"] != DBNull.Value ? reader["a_type"].ToString().ToLower() : "";

                                if (adminType == "super")
                                {
                                    txtAdminRole.Text = "Super Administrator";
                                    txtAdminRole.Visibility = Visibility.Visible;

                                    btnNotifications.Visibility = Visibility.Visible;
                                    btnAdminManage.Visibility = Visibility.Visible;

                                    LoadNotifications();
                                }
                                else
                                {
                                    txtAdminRole.Text = "";
                                    txtAdminRole.Visibility = Visibility.Collapsed;

                                    btnNotifications.Visibility = Visibility.Collapsed;
                                    btnAdminManage.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                txtAdminName.Text = "Unknown User";
                txtAdminRole.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadNotifications()
        {
            List<AdminHeaderItem> requests = new List<AdminHeaderItem>();

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(DatabaseConfig.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT fullname, contact, created_at FROM admin_req ORDER BY created_at DESC";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DateTime createdAt = Convert.ToDateTime(reader["created_at"]);

                                requests.Add(new AdminHeaderItem
                                {
                                    FullName = reader["fullname"].ToString(),
                                    Contact = Convert.ToInt64(reader["contact"]),
                                    TimeReceived = createdAt.ToString("MMM dd, hh:mm tt")
                                });
                            }
                        }
                    }
                }

                listAdminRequests.ItemsSource = requests;
                txtNotificationCount.Text = requests.Count.ToString();
                NotificationBadge.Visibility = requests.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load notifications: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public class AdminHeaderItem
        {
            public string FullName { get; set; }
            public long Contact { get; set; }
            public string TimeReceived { get; set; }
        }
    }
}