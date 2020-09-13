using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EBRS
{
    /// <summary>
    /// SubWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SubWindow : Window
    {
        private static SubWindow openWindow = null;
        private string url = "";

        public SubWindow()
        {
            InitializeComponent();
            this.Loaded += SubWindow_OnLoaded;
        }

        private void SubWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (openWindow != null)
            {
                openWindow.Close();
            }
            openWindow = this;
        }

        private void txtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                webBrowser_.Navigate(txtUrl.Text);
        }

        private void wbSample_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            txtUrl.Text = e.Uri.OriginalString;
            url = e.Uri.OriginalString;
        }

        private void BrowseBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((webBrowser_ != null) && (webBrowser_.CanGoBack));
        }

        private void BrowseBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            webBrowser_.GoBack();
        }

        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((webBrowser_ != null) && (webBrowser_.CanGoForward));
        }

        private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            webBrowser_.GoForward();
        }

        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GoToPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            webBrowser_.Navigate(txtUrl.Text);
        }

        private void btn_saveThisPage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dynamic document = webBrowser_.Document;
                var htmlText = document.documentElement.InnerHtml;
                string text = (string)htmlText;
                string frontText = @"<link title=""Structured Descriptor Document (N3 format)"" href=""";
                string endText = @""" rel=""alternate"" type=""text/n3"">";
                int front = text.IndexOf(frontText) + frontText.Length;
                int rear = text.IndexOf(endText);
                text = text.Substring(front, rear - front);

                string link = text;
                string[] str = link.Split('/');
                // string[] str = url.Split('/');
                // string link = @"http://dbpedia.org/data/" + str[4] + ".n3";

                using (var client = new WebClient())
                {
                    client.DownloadFile(link, "input/" + str[str.Length - 1]);
                }
                this.Close();
            } catch (Exception exc) { }
        }
    }
}
