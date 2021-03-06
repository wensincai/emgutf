﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.TF;
using Emgu.TF.Models;
using System.Diagnostics;

namespace InceptionObjectRecognition
{
    public partial class MainForm : Form
    {
        private Inception inceptionGraph;

        public MainForm()
        {
            InitializeComponent();

            TfInvoke.CheckLibraryLoaded();
            messageLabel.Text = String.Empty;

            DisableUI();

            
            inceptionGraph = new Inception();
            inceptionGraph.OnDownloadProgressChanged += OnDownloadProgressChangedEventHandler;
            inceptionGraph.OnDownloadCompleted += onDownloadCompleted;

            //Use the following code for the full inception model
            inceptionGraph.Init();

            //Uncomment the following code to use a retrained model to recognize followers, downloaded from the internet
            //inceptionGraph.Init(new string[] {"optimized_graph.pb", "output_labels.txt"}, "https://github.com/emgucv/models/raw/master/inception_flower_retrain/", "Mul", "final_result");
        }

        public void OnDownloadProgressChangedEventHandler(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            String msg = String.Format("Downloading models, please wait... {0} of {1} bytes ({2}%) downloaded.", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    messageLabel.Text = msg;

                }));
            }
            else
            {
                messageLabel.Text = msg;

            }
        }

        public void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            EnableUI();
            Recognize("space_shuttle.jpg");
        }

        public void DisableUI()
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    openFileButton.Enabled = false;
                }));
            }
            else
            {
                openFileButton.Enabled = false;
            }
        }

        public void EnableUI()
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    openFileButton.Enabled = true;                    
                }));
            }
            else
            {
                openFileButton.Enabled = true;
            }

        }

        public void Recognize(String fileName)
        {            
            Tensor imageTensor = ImageIO.ReadTensorFromImageFile(fileName, 224, 224, 128.0f, 1.0f / 128.0f);

            Stopwatch sw = Stopwatch.StartNew();
            float[] probability = inceptionGraph.Recognize(imageTensor);
            sw.Stop();

            String resStr = String.Empty;
            if (probability != null)
            {
                String[] labels = inceptionGraph.Labels;
                float maxVal = 0;
                int maxIdx = 0;
                for (int i = 0; i < probability.Length; i++)
                {
                    if (probability[i] > maxVal)
                    {
                        maxVal = probability[i];
                        maxIdx = i;
                    }
                }
                resStr = String.Format("Object is {0} with {1}% probability. Recognition done in {2} milliseconds.", labels[maxIdx], maxVal * 100, sw.ElapsedMilliseconds);
            }

            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    fileNameTextBox.Text = fileName;
                    pictureBox.ImageLocation = fileName;
                    messageLabel.Text = resStr;
                }));
            }
            else
            {
                fileNameTextBox.Text = fileName;
                pictureBox.ImageLocation = fileName;
                messageLabel.Text = resStr;
            }

        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                String fileName = ofd.FileName;
                Recognize(fileName);
            }
        }
    }
}
