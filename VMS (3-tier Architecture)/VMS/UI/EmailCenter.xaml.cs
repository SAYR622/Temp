using VMS.BLL;
using VMS.DAL;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VMS.DAL;

namespace CHSMS.UI
{
    public partial class EmailCenter : Page
    {
        private EmailCenterConditions _logic = new EmailCenterConditions();
        private List<VisitorModel> _currentVisitors = new List<VisitorModel>();

        public EmailCenter()
        {
            InitializeComponent();
            _ = LoadPageDataAsync();
        }

        private async System.Threading.Tasks.Task LoadPageDataAsync()
        {
            try
            {
                await _logic.InitializeAsync();

                // Load Sites Dropdown (Tab 1)
                SiteFilterComboBox.ItemsSource = await _logic.GetFilterSitesAsync();
                SiteFilterComboBox.SelectedIndex = 0;

                // Load Template Dropdown (Tab 2)
                var rawSites = await _logic.GetRawSitesAsync();
                TemplateSelector.ItemsSource = rawSites;
                if (rawSites.Count > 0) TemplateSelector.SelectedIndex = 0;

                // Load HTML Engines
                await CampaignWebView.EnsureCoreWebView2Async(null);
                await TemplateWebView.EnsureCoreWebView2Async(null);

                await LoadQuillEditorAsync(CampaignWebView, "Write your custom email here...");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // TAB 1: CAMPAIGNS
        // ==========================================
        private async void Filters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SiteFilterComboBox.SelectedValue == null) return;

            int siteId = (int)SiteFilterComboBox.SelectedValue;
            string time = (TimeFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            _currentVisitors = await _logic.GetFilteredVisitorsAsync(siteId, time);
            VisitorsTable.ItemsSource = _currentVisitors;
        }

        private void chkSelectAll_Checked(object sender, RoutedEventArgs e) => ToggleCheckboxes(true);
        private void chkSelectAll_Unchecked(object sender, RoutedEventArgs e) => ToggleCheckboxes(false);

        private void ToggleCheckboxes(bool check)
        {
            foreach (var v in _currentVisitors) v.IsSelected = check;
            VisitorsTable.Items.Refresh();
        }

        // SPA Navigation: Open Compose View
        private async void btnOpenCampaignEditor_Click(object sender, RoutedEventArgs e)
        {
            // 1. Switch the views
            VisitorSelectionView.Visibility = Visibility.Collapsed;
            CampaignComposeView.Visibility = Visibility.Visible;

            // 2. Force the browser to reload the editor immediately!
            await LoadQuillEditorAsync(CampaignWebView, "Write your custom email here...");
        }

        // SPA Navigation: Go Back
        private void btnCancelCampaign_Click(object sender, RoutedEventArgs e)
        {
            CampaignComposeView.Visibility = Visibility.Collapsed;
            VisitorSelectionView.Visibility = Visibility.Visible;
        }

        private async void btnSendCampaign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSendCampaign.Content = "⏳ Sending...";
                btnSendCampaign.IsEnabled = false;

                string html = await CampaignWebView.ExecuteScriptAsync("getEditorHtml();");
                html = System.Text.Json.JsonSerializer.Deserialize<string>(html);

                int sentCount = await _logic.SendCampaignAsync(_currentVisitors, txtCampaignSubject.Text, html);

                MessageBox.Show($"Successfully sent {sentCount} emails!", "Success");
                btnCancelCampaign_Click(null, null); // Go back to table view
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                btnSendCampaign.Content = "Send Emails";
                btnSendCampaign.IsEnabled = true;
            }
        }

        // ==========================================
        // TAB 2: AUTOMATED TEMPLATES
        // ==========================================
        private async void TemplateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateSelector.SelectedItem is SiteModel site)
            {
                var template = await _logic.GetTemplateForSiteAsync(site.Id.ToString(), site.siteName);
                txtTemplateSubject.Text = template.Subject;
                await LoadQuillEditorAsync(TemplateWebView, "Template Body...", template.HtmlBody);
            }
        }

        private async void btnSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSaveTemplate.Content = "⏳...";
                string html = await TemplateWebView.ExecuteScriptAsync("getEditorHtml();");
                html = System.Text.Json.JsonSerializer.Deserialize<string>(html);

                await _logic.SaveTemplateAsync(TemplateSelector.SelectedValue.ToString(), txtTemplateSubject.Text, html);
                MessageBox.Show("Template Saved!", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                btnSaveTemplate.Content = "💾 Save";
            }
        }

        // --- IMAGE HANDLING ---
        private async void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Images|*.png;*.jpeg;*.jpg" };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(dialog.FileName);
                    byte[] bytes = System.IO.File.ReadAllBytes(dialog.FileName);

                    string url = await _logic.UploadImageAsync(bytes, fileName);
                    await InjectImageIntoQuillAsync(TemplateWebView, url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Upload failed: {ex.Message}");
                }
            }
        }

        // SPA Navigation: Open Library
        private async void btnOpenLibrary_Click(object sender, RoutedEventArgs e)
        {
            ImageListBox.ItemsSource = await _logic.GetImagesAsync();
            TemplateEditorView.Visibility = Visibility.Collapsed;
            ImageLibraryView.Visibility = Visibility.Visible;
        }

        // SPA Navigation: Close Library
        private void btnCancelLibrary_Click(object sender, RoutedEventArgs e)
        {
            ImageLibraryView.Visibility = Visibility.Collapsed;
            TemplateEditorView.Visibility = Visibility.Visible;
        }

        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageListBox.SelectedItem is Supabase.Storage.FileObject file)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_logic.GetImageUrl(file.Name));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                PreviewImage.Source = bitmap;
            }
        }

        private async void btnInsertImage_Click(object sender, RoutedEventArgs e)
        {
            if (ImageListBox.SelectedItem is Supabase.Storage.FileObject file)
            {
                // Added await and the Async version of the helper method!
                await InjectImageIntoQuillAsync(TemplateWebView, _logic.GetImageUrl(file.Name));
                btnCancelLibrary_Click(null, null); // Return to editor
            }
        }

        // ==========================================
        // SHARED UI HELPER METHODS
        // ==========================================
        private async Task LoadQuillEditorAsync(Microsoft.Web.WebView2.Wpf.WebView2 webView, string placeholder, string existingHtml = "")
        {
            // THE FIX: This forces the code to wait until the browser engine is 100% awake and ready
            await webView.EnsureCoreWebView2Async(null);

            string htmlContent = $@"
        <!DOCTYPE html><html><head>
        <link href='https://cdn.quilljs.com/1.3.6/quill.snow.css' rel='stylesheet'>
        <style>body, html {{ margin: 0; padding: 0; height: 100%; font-family: sans-serif; }} #editor {{ height: calc(100vh - 42px); }}</style>
        </head><body><div id='editor'>{existingHtml}</div>
        <script src='https://cdn.quilljs.com/1.3.6/quill.js'></script>
        <script>
            var quill = new Quill('#editor', {{ theme: 'snow', placeholder: '{placeholder}' }});
            function getEditorHtml() {{ return quill.root.innerHTML; }}
        </script></body></html>";

            webView.NavigateToString(htmlContent);
        }

        private async Task InjectImageIntoQuillAsync(Microsoft.Web.WebView2.Wpf.WebView2 webView, string imageUrl)
        {
            // THE FIX: Safety check before injecting Javascript
            await webView.EnsureCoreWebView2Async(null);

            string js = $@"
        var range = quill.getSelection(true);
        var index = range ? range.index : quill.getLength();
        quill.insertEmbed(index, 'image', '{imageUrl}');
        quill.setSelection(index + 1);";

            await webView.ExecuteScriptAsync(js);
        }
    }
}