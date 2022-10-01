// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebView2WindowsFormsBrowser
{
    public partial class BrowserForm : Form
    {
        public BrowserForm()
        {
            InitializeComponent();
            AttachControlEventHandlers(webView2Control);
            HandleResize();
        }

        public async Task InitializeAsync()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "\\WebView2");

            try
            {
                CoreWebView2Environment.SetLoaderDllFolderPath("\\runtimes\\win-x64\\native");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.ToString());
            }

            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null,
                                                                                    filePath);

            await webView2Control.EnsureCoreWebView2Async(env);

            webView2Control.CoreWebView2.OpenDevToolsWindow();
            webView2Control.CoreWebView2.OpenTaskManagerWindow();

            //webView2Control.Source = new Uri("https://www.bing.com/");

            webView2Control.CoreWebView2.Navigate("http://www.bing.com/");
        }

        private void UpdateTitleWithEvent(string message)
        {
            string currentDocumentTitle = webView2Control?.CoreWebView2?.DocumentTitle ?? "Uninitialized";
            Text = currentDocumentTitle + " (" + message + ")";
        }

        #region Event Handlers

        private void WebView2Control_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            UpdateTitleWithEvent("NavigationStarting");
        }

        private void WebView2Control_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            UpdateTitleWithEvent("NavigationCompleted");
        }

        private void WebView2Control_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            txtUrl.Text = webView2Control.Source.AbsoluteUri;
        }

        private void WebView2Control_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"WebView2 creation failed with exception = {e.InitializationException}");
                UpdateTitleWithEvent("CoreWebView2InitializationCompleted failed");
                return;
            }

            webView2Control.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            webView2Control.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView2Control.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webView2Control.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;
            webView2Control.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            webView2Control.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
            webView2Control.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
            webView2Control.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            webView2Control.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webView2Control.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;
            UpdateTitleWithEvent("CoreWebView2InitializationCompleted succeeded");
        }

        private void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            UpdateTitleWithEvent("WebResourceResponseReceived");
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            UpdateTitleWithEvent("WebMessageReceived");
        }

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            UpdateTitleWithEvent("WebResourceRequested");
        }

        private void CoreWebView2_ProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            UpdateTitleWithEvent("ProcessFailed");
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            UpdateTitleWithEvent("NavigationCompleted");
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            UpdateTitleWithEvent("NavigationStarting");
        }

        private void AttachControlEventHandlers(Microsoft.Web.WebView2.WinForms.WebView2 control)
        {
            control.CoreWebView2InitializationCompleted += WebView2Control_CoreWebView2InitializationCompleted;
            control.NavigationStarting += WebView2Control_NavigationStarting;
            control.NavigationCompleted += WebView2Control_NavigationCompleted;
            control.SourceChanged += WebView2Control_SourceChanged;
            control.KeyDown += WebView2Control_KeyDown;
            control.KeyUp += WebView2Control_KeyUp;
        }

        private void WebView2Control_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateTitleWithEvent($"AcceleratorKeyUp key={e.KeyCode}");
            if (!acceleratorKeysEnabledToolStripMenuItem.Checked)
            {
                e.Handled = true;
            }
        }

        private void WebView2Control_KeyDown(object sender, KeyEventArgs e)
        {
            UpdateTitleWithEvent($"AcceleratorKeyDown key={e.KeyCode}");
            if (!acceleratorKeysEnabledToolStripMenuItem.Checked)
            {
                e.Handled = true;
            }
        }

        private void CoreWebView2_HistoryChanged(object sender, object e)
        {
            // No explicit check for webView2Control initialization because the events can only start
            // firing after the CoreWebView2 and its events exist for us to subscribe.
            btnBack.Enabled = webView2Control.CoreWebView2.CanGoBack;
            btnForward.Enabled = webView2Control.CoreWebView2.CanGoForward;
            UpdateTitleWithEvent("HistoryChanged");
        }

        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            txtUrl.Text = webView2Control.Source.AbsoluteUri;
            UpdateTitleWithEvent("SourceChanged");
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            Text = webView2Control.CoreWebView2.DocumentTitle;
            UpdateTitleWithEvent("DocumentTitleChanged");
        }

        #endregion

        #region UI event handlers

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            webView2Control.Reload();
        }

        private void BtnGo_Click(object sender, EventArgs e)
        {
            string rawUrl = txtUrl.Text;
            Uri uri = null;

            if (Uri.IsWellFormedUriString(rawUrl, UriKind.Absolute))
            {
                uri = new Uri(rawUrl);
            }
            else if (!rawUrl.Contains(" ") && rawUrl.Contains("."))
            {
                // An invalid URI contains a dot and no spaces, try tacking http:// on the front.
                uri = new Uri("http://" + rawUrl);
            }
            else
            {
                // Otherwise treat it as a web search.
                uri = new Uri("https://bing.com/search?q=" +
                    string.Join("+", Uri.EscapeDataString(rawUrl).Split(new string[] { "%20" }, StringSplitOptions.RemoveEmptyEntries)));
            }

            webView2Control.Source = uri;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            webView2Control.GoBack();
        }

        private void btnEvents_Click(object sender, EventArgs e)
        {
            new EventMonitor(webView2Control).Show(this);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            webView2Control.GoForward();
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            HandleResize();
        }

        private void xToolStripMenuItem05_Click(object sender, EventArgs e)
        {
            webView2Control.ZoomFactor = 0.5;
        }

        private void xToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            webView2Control.ZoomFactor = 1.0;
        }

        private void xToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            webView2Control.ZoomFactor = 2.0;
        }

        private void xToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Zoom factor: {webView2Control.ZoomFactor}", "WebView Zoom factor");
        }

        private void backgroundColorMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            Color backgroundColor = Color.FromName(menuItem.Text);
            webView2Control.DefaultBackgroundColor = backgroundColor;
        }

        private void allowExternalDropMenuItem_Click(object sender, EventArgs e)
        {
            webView2Control.AllowExternalDrop = allowExternalDropMenuItem.Checked;
        }

        #endregion

        private void HandleResize()
        {
            // Resize the webview
            webView2Control.Size = ClientSize - new System.Drawing.Size(webView2Control.Location);

            // Move the Events button
            btnEvents.Left = ClientSize.Width - btnEvents.Width;
            // Move the Go button
            btnGo.Left = btnEvents.Left - btnGo.Size.Width;

            // Resize the URL textbox
            txtUrl.Width = btnGo.Left - txtUrl.Left;
        }
    }
}
