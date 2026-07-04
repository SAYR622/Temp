using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
// REMOVED LiveCharts imports!
using VMS.BLL;

namespace VMS.UI
{
    public partial class Visitors : Page
    {
        private VisitorConditions _visitorLogic = new VisitorConditions();
        private HeritageSiteConditions _siteLogic = new HeritageSiteConditions();

        public Visitors()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadSitesDropdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void LoadSitesDropdown()
        {
            var sites = _siteLogic.GetAllSites();
            sites.Insert(0, new HeritageSite { Id = 0, siteName = "All Heritage Sites" });

            cmbSites.ItemsSource = sites;
            cmbSites.SelectedIndex = 0;
        }

        private void cmbSites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSites.SelectedItem is HeritageSite selectedSite)
            {
                RefreshData(selectedSite.Id);
            }
        }

        private void RefreshData(int siteId)
        {
            // 1. Update the DataGrid
            VisitorsDataGrid.ItemsSource = _visitorLogic.GetVisitorList(siteId);

            // 2. Prep the Data for ScottPlot
            var stats = _visitorLogic.GetVisitorStatsByDate(siteId);

            string[] labels = stats.Keys.ToArray();
            double[] values = stats.Values.Select(v => (double)v).ToArray();

            // 3. Draw the Chart!
            VisitorChart.Plot.Clear();

            // Add the bars
            VisitorChart.Plot.Add.Bars(values);

            // 4. Setup Custom Text Labels for the X-Axis
            ScottPlot.TickGenerators.NumericManual tickGen = new();

            for (int i = 0; i < labels.Length; i++)
            {
                tickGen.AddMajor(i, labels[i]);
            }

            VisitorChart.Plot.Axes.Bottom.TickGenerator = tickGen;

            // 5. Fix the Overlapping and Cut-off text
            // Rotate the text 45 degrees
            VisitorChart.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;

            // Give the bottom axis more room so the angled text doesn't get chopped off
            VisitorChart.Plot.Axes.Bottom.MinimumSize = 80;

            // Pin the text to the tick mark by its left edge so it hangs down below the line
            VisitorChart.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleLeft;

            // FORCE THE PLOT TO FILL THE BOX:
            // Remove all internal padding so the bars go to the edge
            VisitorChart.Plot.Layout.Fixed(new ScottPlot.PixelPadding(25, 25, 35, 20));

            // Make sure the axes are set to "Auto" so they scale to the fill
            VisitorChart.Plot.Axes.AutoScale();

            VisitorChart.Refresh();

            // 6. Tell the UI to draw it!
            VisitorChart.Refresh();
        }
    }
}