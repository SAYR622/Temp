using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using VMS.BLL;

namespace VMS.UI
{
    public partial class Dashboard : Page
    {
        private DashboardConditions _dashboardLogic = new DashboardConditions();

        public Dashboard()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDashboardData();
                LoadMap();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dashboard Loading Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDashboardData()
        {
            // 1. Set current date dynamically
            txtCurrentDate.Text = DateTime.Now.ToString("dddd, MMM dd, yyyy");

            // 2. Load Top Stats
            txtTotalSites.Text = _dashboardLogic.GetTotalSites().ToString();
            txtTotalVisitors.Text = _dashboardLogic.GetTotalVisitors().ToString("N0");

            // 3. Load Grids and Lists
            dgRecentActivities.ItemsSource = _dashboardLogic.GetRecentVisitors();
            icMapSites.ItemsSource = _dashboardLogic.GetTopMapSites();
            icActiveAdmins.ItemsSource = _dashboardLogic.GetActiveAdmins();
            icTopVisitedSites.ItemsSource = _dashboardLogic.GetTopVisitedSitesChart();
        }

        private void LoadMap()
        {
            GMapProvider.UserAgent = "VMS_Application_v1.0";
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            MyMap.MapProvider = GMapProviders.OpenStreetMap;

            MyMap.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            MyMap.CanDragMap = true;
            MyMap.DragButton = System.Windows.Input.MouseButton.Left;
            MyMap.ShowCenter = false;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MyMap.MinZoom = 6;
                MyMap.MaxZoom = 18;
                MyMap.Zoom = 7.5;
                MyMap.Position = new PointLatLng(7.8731, 80.7718);

                PlotMapMarkers();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void PlotMapMarkers()
        {
            MyMap.Markers.Clear();
            List<HeritageSite> sites = _dashboardLogic.GetAllSites();

            foreach (var site in sites)
            {
                GMapMarker marker = new GMapMarker(new PointLatLng(site.latitude, site.longitude));

                System.Windows.Shapes.Ellipse circle = new System.Windows.Shapes.Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = System.Windows.Media.Brushes.Red,
                    Stroke = System.Windows.Media.Brushes.White,
                    StrokeThickness = 2,
                    ToolTip = site.siteName
                };

                marker.Shape = circle;
                marker.Offset = new Point(-6, -6);

                MyMap.Markers.Add(marker);
            }
        }

        private void btnQuickAddSite_Click(object sender, RoutedEventArgs e)
        {
            SiteForm form = new SiteForm();

            if (form.ShowDialog() == true)
            {
                // Bring in the Heritage Sites logic to save to the database
                HeritageSiteConditions siteLogic = new HeritageSiteConditions();
                siteLogic.AddSite(form.Site);

                // Refresh the dashboard stats, tables, and map!
                LoadDashboardData();
                LoadMap();

                MessageBox.Show("New Heritage Site added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnQuickAddAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use the built-in NavigationService instead of MainFrame
                this.NavigationService.Navigate(new AdminManage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error navigating to Admin Manage: " + ex.Message, "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}