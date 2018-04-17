﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Lite;
using System.IO;

namespace Emgu.TF.Lite.Models
{
    
    public class SmartReply : DownloadableModels
    {
        private String _inputLayer;
        private String _outputLayer;

        public SmartReply(            
            String[] modelFiles = null,
            String downloadUrl = "https://github.com/emgucv/models/raw/master/smartreply_1.0_2017_11_01/",
            String inputLayer = "input", 
            String outputLayer = "output")
            : base(
                modelFiles ?? new string[] { "smartreply.tflite", "backoff_response.txt" },
                downloadUrl)
        {
            _inputLayer = inputLayer;
            _outputLayer = outputLayer;
        }

        public void Init(
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null)
        {
            int retry = 1;
            Download(
                retry,
                onDownloadProgressChanged,
                (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                {
                    /*
                    byte[] model = File.ReadAllBytes(GetLocalFileName(_modelFiles[0]));

                    Buffer modelBuffer = Buffer.FromString(model);

                    using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                        ImportGraphDef(modelBuffer, options, status);
                    */
                    if (onDownloadFileCompleted != null)
                    {
                        onDownloadFileCompleted(sender, e);
                    }
                });
        }
        

        public String[] Labels
        {
            get
            {
                return File.ReadAllLines(GetLocalFileName(_modelFiles[1]));
            }
        }

        /*
        public float[] Recognize(Tensor image)
        {
            Session inceptionSession = new Session(this);
            Tensor[] finalTensor = inceptionSession.Run(new Output[] { this[_inputLayer] }, new Tensor[] { image },
                new Output[] { this[_outputLayer] });
            float[] probability = finalTensor[0].GetData(false) as float[];
            return probability;
        }*/
    }
}