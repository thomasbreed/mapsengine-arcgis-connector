/*
Copyright 2013 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Settings
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.wblegal = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // wblegal
            // 
            this.wblegal.AllowNavigation = false;
            this.wblegal.AllowWebBrowserDrop = false;
            this.wblegal.CausesValidation = false;
            this.wblegal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wblegal.IsWebBrowserContextMenuEnabled = false;
            this.wblegal.Location = new System.Drawing.Point(0, 0);
            this.wblegal.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.wblegal.MinimumSize = new System.Drawing.Size(20, 20);
            this.wblegal.Name = "wblegal";
            this.wblegal.ScriptErrorsSuppressed = true;
            this.wblegal.ScrollBarsEnabled = false;
            this.wblegal.Size = new System.Drawing.Size(759, 462);
            this.wblegal.TabIndex = 1;
            this.wblegal.WebBrowserShortcutsEnabled = false;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(759, 462);
            this.Controls.Add(this.wblegal);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(775, 500);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(775, 500);
            this.Name = "About";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = global::com.google.mapsengine.connectors.arcgis.Properties.Settings.Default.dialogs_aboutTitle;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.About_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser wblegal;


    }
}