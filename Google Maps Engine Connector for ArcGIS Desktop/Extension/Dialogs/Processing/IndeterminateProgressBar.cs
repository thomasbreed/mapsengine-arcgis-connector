/*
Copyright 2014 Google Inc

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Processing
{
    public partial class IndeterminateProgressBar : Form
    {
        public IndeterminateProgressBar()
        {
            InitializeComponent();
        }
        
        public event EventHandler canceled;
        
        private bool complete = false;
        public void onComplete()
        {
            complete = true;
            this.Close();
        }

        public void setText(String text)
        {
            this.Text = text;
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!complete && (null != canceled))
                canceled(this, new EventArgs());

            this.Close();
        }

        /*
         * Utility class binding an action with how its described
         */
        internal class ProcessStep
        {
            // COM Object access forbidden in this action
            // must bubble up all ThreadAbortExceptions
            internal Action process
            {
                get;
                set;
            }
            internal String description
            {
                get;
                set;
            }
        }

        private List<ProcessStep> steps;

        internal void runProcesses(List<ProcessStep> steps)
        {
            this.steps = steps;
            
            this.ShowDialog();
            
            
        }

        private void IndeterminateProgressBar_Shown(object sender, EventArgs e)
        {
            if (steps != null)
            {
                Thread processThread = new Thread(delegate()
                {
                    try
                    {
                        foreach (ProcessStep step in steps)
                        {
                            this.BeginInvoke(new Action<String>((x) => { this.setText(x); }),
                                                new Object[] { step.description });

                            step.process();
                        }
                        this.BeginInvoke(new Action(() => { this.Close(); }), new Object[] { });
                    }
                    catch (ThreadAbortException ex) { }
                });
                EventHandler ev = (s, e1) => { processThread.Abort(); };
                this.canceled += ev;

                processThread.Start();
            }
        }

    }
}
