using VMS.BLL;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VMS.UI
{
    public partial class HeritageSites : Page
    {
        private HeritageSiteConditions siteConditions = new HeritageSiteConditions();
        private HeritageSite selectedSite;

        public HeritageSites()
        {
            InitializeComponent();
            LoadSites();
            LoadMap();
        }

        private void LoadSites()
        {
            dgSites.ItemsSource = siteConditions.GetAllSites();
        }

        private void LoadMap()
        {
            try
            {
                // 1. Configure provider settings to prevent request blocking
                GMapProvider.UserAgent = "VMS_Application_v1.0";
                GMaps.Instance.Mode = AccessMode.ServerAndCache;

                // 2. Set map tile source provider to match the Dashboard
                heritageMap.MapProvider = GMapProviders.OpenStreetMap;

                // 3. Configure mouse interaction and layout controls
                heritageMap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
                heritageMap.CanDragMap = true;
                heritageMap.DragButton = System.Windows.Input.MouseButton.Left;
                heritageMap.ShowCenter = false; // Hides the default center crosshair

                // 4. Defer layout initialization until UI rendering completes
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    heritageMap.MinZoom = 6;
                    heritageMap.MaxZoom = 18;
                    heritageMap.Zoom = 7.5; // Optimal zoom level for Sri Lanka
                    heritageMap.Position = new PointLatLng(7.8731, 80.7718); // Center on Sri Lanka

                    // Load markers only after the map is fully prepared
                    LoadMarkers();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Map Loading Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMarkers()
        {
            heritageMap.Markers.Clear();
            List<HeritageSite> sites = siteConditions.GetAllSites();

            foreach (HeritageSite site in sites)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(site.latitude, site.longitude));

                // Define UI element properties for the marker point (matching your Dashboard style)
                Ellipse circle = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Fill = Brushes.Red,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    ToolTip = site.siteName // Displays location name on mouse hover
                };

                marker.Shape = circle;

                // Apply negative half-width/height offset to anchor marker exactly to coordinates
                marker.Offset = new Point(-7, -7);

                heritageMap.Markers.Add(marker);
            }
        }

        // --- BUTTON ACTIONS ---
        private void btnAddSite_Click(object sender, RoutedEventArgs e)
        {
            SiteForm form = new SiteForm();

            // 1. ShowDialog() stops the code here and waits for the popup to close
            if (form.ShowDialog() == true)
            {
                // 2. The popup closed successfully! Grab the data and send it to the database
                siteConditions.AddSite(form.Site);

                // 3. Refresh the table and the map instantly
                LoadSites();
                LoadMarkers();
                MessageBox.Show("Heritage Site successfully added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnUpdateSite_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSite == null)
            {
                MessageBox.Show("Please select a site from the table first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SiteForm form = new SiteForm(selectedSite);
            if (form.ShowDialog() == true)
            {
                siteConditions.UpdateSite(form.Site);
                LoadSites();
                LoadMarkers(); // Refresh map pins
                MessageBox.Show("Heritage Site successfully updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnDeleteSite_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSite == null)
            {
                MessageBox.Show("Please select a site from the table first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you absolutely sure you want to delete this heritage site?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                siteConditions.DeleteSite(selectedSite.Id);
                LoadSites();
                LoadMarkers(); // Refresh map pins
                MessageBox.Show("Heritage Site deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // --- DATAGRID SELECTION (INTERACTIVE MAP FEATURE) ---
        private void dgSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSite = dgSites.SelectedItem as HeritageSite;

            if (selectedSite != null && heritageMap != null)
            {
                // Instantly pan the map to the selected site and zoom in
                heritageMap.Position = new PointLatLng(selectedSite.latitude, selectedSite.longitude);
                heritageMap.Zoom = 12;
            }
        }

        // --- SEARCH BAR LOGIC ---
        private void txtSearchSite_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Grab the text and make it lowercase so the search isn't case-sensitive
            string keyword = txtSearchSite.Text.ToLower().Trim();

            // 1. Get the full list of sites from the database
            List<HeritageSite> allSites = siteConditions.GetAllSites();

            // 2. Filter the list in memory using LINQ
            var filteredSites = allSites.Where(site =>
                site.siteName.ToLower().Contains(keyword) ||
                site.province.ToLower().Contains(keyword)
            ).ToList();

            // 3. Instantly update the Table
            dgSites.ItemsSource = filteredSites;

            // 4. Instantly update the Map to only show pins that match the search!
            heritageMap.Markers.Clear();
            foreach (var site in filteredSites)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(site.latitude, site.longitude));

                Ellipse circle = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Fill = Brushes.Red,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    ToolTip = site.siteName
                };

                marker.Shape = circle;
                marker.Offset = new Point(-7, -7);
                heritageMap.Markers.Add(marker);
            }
        }
    }
}