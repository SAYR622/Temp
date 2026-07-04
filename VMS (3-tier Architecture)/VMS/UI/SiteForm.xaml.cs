using VMS.BLL;
using System;
using System.Windows;

namespace VMS.UI
{
    public partial class SiteForm : Window
    {
        // We still need this so the main window can grab the final data
        public HeritageSite Site { get; private set; }

        // We temporarily hold the ID here because it doesn't have a text box on the UI.
        // (Note: If your Id is a string instead of an int, just change 'int' to 'string' below)
        private int _siteId = 0;

        // -------------------------------------------------------------
        // CONSTRUCTOR 1: Used when ADDING a new site (Blank Form)
        // -------------------------------------------------------------
        public SiteForm()
        {
            InitializeComponent();
            // We no longer create the Site object here. The form is just blank.
        }

        // -------------------------------------------------------------
        // CONSTRUCTOR 2: Used when UPDATING an existing site (Filled Form)
        // -------------------------------------------------------------
        public SiteForm(HeritageSite selectedSite)
        {
            InitializeComponent();

            this.Title = "Update Heritage Site";
            btnSave.Content = "✏ Update";

            // 1. Save the ID so we remember it when the user clicks Save
            _siteId = selectedSite.Id;

            // 2. Dump the data directly into the UI text boxes
            // We don't bother creating a confusing duplicate object anymore.
            txtSiteName.Text = selectedSite.siteName;
            txtProvince.Text = selectedSite.province;
            txtLatitude.Text = selectedSite.latitude.ToString();
            txtLongitude.Text = selectedSite.longitude.ToString();
        }

        // -------------------------------------------------------------
        // SAVE/ADD BUTTON LOGIC
        // -------------------------------------------------------------
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // NOW we finally create the object, right before we close the form.
                Site = new HeritageSite();

                // 1. Give it the ID we stored (it will be 0 if this is a brand new site)
                Site.Id = _siteId;

                // 2. Grab the standard text directly from the UI
                Site.siteName = txtSiteName.Text;
                Site.province = txtProvince.Text;

                // 3. Convert the coordinate fields from text into numbers
                Site.latitude = double.Parse(txtLatitude.Text);
                Site.longitude = double.Parse(txtLongitude.Text);

                // 4. Safely attempt to set DialogResult
                try
                {
                    this.DialogResult = true;
                }
                catch (InvalidOperationException)
                {
                    this.Close();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Please ensure Latitude and Longitude are valid numeric coordinates (e.g., 7.8731).", "Formatting Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}