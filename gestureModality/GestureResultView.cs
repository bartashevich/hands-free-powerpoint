﻿//------------------------------------------------------------------------------
// <copyright file="GestureResultView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Timers;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using mmisharp;
    using System.Collections.Generic;

    /// <summary>
    /// Stores discrete gesture results for the GestureDetector.
    /// Properties are stored/updated for display in the UI.
    /// </summary>
    public sealed class GestureResultView : INotifyPropertyChanged
    {
        /// <summary> Image to show when the 'detected' property is true for a tracked body </summary>
        private readonly ImageSource seatedImage = new BitmapImage(new Uri(@"Images\Seated.png", UriKind.Relative));

        /// <summary> Image to show when the 'detected' property is false for a tracked body </summary>
        private readonly ImageSource notSeatedImage = new BitmapImage(new Uri(@"Images\NotSeated.png", UriKind.Relative));

        /// <summary> Image to show when the body associated with the GestureResultView object is not being tracked </summary>
        private readonly ImageSource notTrackedImage = new BitmapImage(new Uri(@"Images\NotTracked.png", UriKind.Relative));

        /// <summary> Array of brush colors to use for a tracked body; array position corresponds to the body colors used in the KinectBodyView class </summary>
        private readonly Brush[] trackedColors = new Brush[] { Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

        /// <summary> Brush color to use as background in the UI </summary>
        private Brush bodyColor = Brushes.Gray;

        /// <summary> The body index (0-5) associated with the current gesture detector </summary>
        private int bodyIndex = 0;

        /// <summary> Current confidence value reported by the discrete gesture </summary>
        private float confidence = 0.0f;

        /// <summary> True, if the discrete gesture is currently being detected </summary>
        private bool detected = false;

        /// <summary> Image to display in UI which corresponds to tracking/detection state </summary>
        private ImageSource imageSource = null;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

        private LifeCycleEvents lce;
        private MmiCommunication mmic;
        private MainWindow main;
        private float MaxConfidence = 0;
        private Timer MaxConfidenceTimer;
        private Timer VolumeControlTimer;


        private bool slide_leftProgress_Left = false;
        private bool slide_rightProgress_Right = false;

        Dictionary<string, string[]> gestureDictionary = new Dictionary<string, string[]>()
        {
            {"slide_leftProgress_Left", new string[] {"Left","false","PREV_SLIDE"} },
            {"slide_rightProgress_Right", new string[] {"Right","false","NEXT_SLIDE" } },
            {"left_arm", new string[] {"Left","false","LEFT" } },
            {"right_arm", new string[] {"Right","false","RIGHT" } }
        };
        
        

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="bodyIndex">Body Index associated with the current gesture detector</param>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="detected">True, if the gesture is currently detected for the associated body</param>
        /// <param name="confidence">Confidence value for detection of the 'Seated' gesture</param>
        public GestureResultView(int bodyIndex, bool isTracked, bool detected, float confidence, LifeCycleEvents lce, MmiCommunication mmic, MainWindow main)
        {
            this.BodyIndex = bodyIndex;
            this.IsTracked = isTracked;
            this.Detected = detected;
            this.Confidence = confidence;
            this.ImageSource = this.notTrackedImage;
            this.lce = lce;
            this.mmic = mmic;
            this.main = main;

            //main.SetState("deactive");
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Gets the body index associated with the current gesture detector result 
        /// </summary>
        public int BodyIndex
        {
            get
            {
                return this.bodyIndex;
            }

            private set
            {
                if (this.bodyIndex != value)
                {
                    this.bodyIndex = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets the body color corresponding to the body index for the result
        /// </summary>
        public Brush BodyColor
        {
            get
            {
                return this.bodyColor;
            }

            private set
            {
                if (this.bodyColor != value)
                {
                    this.bodyColor = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked 
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                if (this.IsTracked != value)
                {
                    this.isTracked = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the discrete gesture has been detected
        /// </summary>
        public bool Detected 
        {
            get
            {
                return this.detected;
            }

            private set
            {
                if (this.detected != value)
                {
                    this.detected = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a float value which indicates the detector's confidence that the gesture is occurring for the associated body 
        /// </summary>
        public float Confidence
        {
            get
            {
                return this.confidence;
            }

            private set
            {
                if (this.confidence != value)
                {
                    this.confidence = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets an image for display in the UI which represents the current gesture result for the associated body 
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }

            private set
            {
                if (this.ImageSource != value)
                {
                    this.imageSource = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        private void SendCommand(String command)
        {
            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            string json = "{ \"recognized\":[\"" + command + "\"] }";

            var exNot = lce.ExtensionNotification("", "", 100, json);
            mmic.Send(exNot);
            Console.WriteLine(command);
        }

        private void ResetMaxConfidence(Object source, ElapsedEventArgs e)
        {
            MaxConfidence = 0;
        }

        private void EnableVolumeControl(Object source, ElapsedEventArgs e)
        {
            main.activeVolumeChange = true;
            SendCommand("status\",\"VOLUME_ACTIVE");
        }

        /// <summary>
        /// Updates the values associated with the discrete gesture detection result
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="isGestureDetected">True, if the discrete gesture is currently detected for the associated body</param>
        /// <param name="detectionConfidence">Confidence value for detection of the discrete gesture</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence, String gestureName)
        {
            this.IsTracked = isBodyTrackingIdValid;
            this.Confidence = 0.0f;

            if (!this.IsTracked)
            {
                this.ImageSource = this.notTrackedImage;
                this.Detected = false;
                this.BodyColor = Brushes.Gray;
            }
            else
            {
                this.Detected = isGestureDetected;
                this.BodyColor = this.trackedColors[this.BodyIndex];

                if (this.Detected)
                {
                    foreach (KeyValuePair<string, string[]> entry in gestureDictionary)
                    {
                        string key = entry.Key;
                        string[] value = entry.Value;

                        if(key == gestureName)
                        {
                            // GUI confidence
                            main.SetConfidence((MaxConfidence * 100).ToString("0.00"));

                            // only increase confidence at the GUI
                            if (MaxConfidence < detectionConfidence)
                            {
                                MaxConfidence = detectionConfidence;
                            }

                            if (detectionConfidence < 0.4)
                            {
                                value[1] = "false";

                                // stop previous timer
                                /*if (value[2] == "VOLUME" && VolumeControlTimer != null)
                                {
                                    VolumeControlTimer.Stop();
                                    VolumeControlTimer = null;
                                    SendCommand("status\",\"VOLUME_INACTIVE");
                                    main.activeVolumeChange = false;
                                }*/
                            }
                            else if (detectionConfidence > 0.7)
                            {
                                if (value[1] == "false")
                                {
                                    value[1] = "true";

                                    if (value[2] == "LEFT" || value[2] == "RIGHT")
                                    {
                                        if (main.helpWindowOpen || main.activeVolumeChange)
                                        {
                                            continue;
                                        }
                                        SendCommand("combo\",\"" + value[2]);
                                    }
                                    /*else if (value[2] == "VOLUME")
                                    {
                                        // stop previous timer
                                        if (VolumeControlTimer != null)
                                        {
                                            VolumeControlTimer.Stop();
                                        }

                                        // set the timer to activate volume control
                                        VolumeControlTimer = new Timer(main.activationTime * 1000);
                                        VolumeControlTimer.Elapsed += EnableVolumeControl;
                                        VolumeControlTimer.AutoReset = false;
                                        VolumeControlTimer.Enabled = true;

                                        SendCommand("status\",\"VOLUME_ACTIVATING");
                                        continue;
                                    }*/

                                    SendCommand("action\",\"" + value[2]);

                                    // stop previous timer
                                    if (MaxConfidenceTimer != null)
                                    {
                                        MaxConfidenceTimer.Stop();
                                    }

                                    // set the timer to deactivate the kinect circle
                                    MaxConfidenceTimer = new Timer(2 * 1000);
                                    MaxConfidenceTimer.Elapsed += ResetMaxConfidence;
                                    MaxConfidenceTimer.AutoReset = false;
                                    MaxConfidenceTimer.Enabled = true;
                                }
                            }
                        }
                    }

                    this.Confidence = detectionConfidence;
                }
                /*else
                {
                    if (main.KinectStatus)
                    {
                        SendCommand("status\",\"KINECT_INACTIVE");
                        main.KinectStatus = false;
                    }
                }*/
            }
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
