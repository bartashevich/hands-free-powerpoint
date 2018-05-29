//---------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <Description>
// This program tracks up to 6 people simultaneously.
// If a person is tracked, the associated gesture detector will determine if that person is seated or not.
// If any of the 6 positions are not in use, the corresponding gesture detector(s) will be paused
// and the 'Not Tracked' image will be displayed in the UI.
// </Description>
//----------------------------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Kinect;
    using mmisharp;
    using System.Windows.Media;
    using System.Timers;
    using System.Linq;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// Interaction logic for the MainWindow
    /// </summary>
    /// 

    public class MouseCoo
    {
        public double x;
        public double y;

        public MouseCoo(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private void OnActiveEnded(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Kinect stopped receving.");
            main.SetState("deactive");
        }

        private void Button_Help(object sender, RoutedEventArgs e)
        {
            HelpWindow window = new HelpWindow();
            window.Show();
        }

        private void button_k_on(object sender, RoutedEventArgs e)
        {
            SendCommand("status\",\"KINECT_ACTIVE");
            Console.WriteLine("Test KINECT_ACTIVE");
        }

        private void button_k_off(object sender, RoutedEventArgs e)
        {
            SendCommand("status\",\"KINECT_INACTIVE");
            Console.WriteLine("Test KINECT_INACTIVE");
        }

        private void button_m_on(object sender, RoutedEventArgs e)
        {
            SendCommand("status\",\"MOUSE_ACTIVE");
            Console.WriteLine("Test MOUSE_ACTIVE");
        }

        private void button_m_ack(object sender, RoutedEventArgs e)
        {
            SendCommand("status\",\"MOUSE_ACTIVATING");
            Console.WriteLine("Test MOUSE_ACTIVATING");
        }

        private void button_m_off(object sender, RoutedEventArgs e)
        {
            SendCommand("status\",\"MOUSE_INACTIVE");
            Console.WriteLine("Test MOUSE_INACTIVE");
        }

        private void button_right(object sender, RoutedEventArgs e)
        {
            SendCommand("combo\",\"RIGHT");
            System.Threading.Thread.Sleep(1000);
            SendCommand("action\",\"RIGHT");
            Console.WriteLine("Test RIGHT");
        }

        private void button_left(object sender, RoutedEventArgs e)
        {
            SendCommand("combo\",\"LEFT");
            System.Threading.Thread.Sleep(1000);
            SendCommand("action\",\"LEFT");
            Console.WriteLine("Test LEFT");
        }

        private void button_next_slide(object sender, RoutedEventArgs e)
        {
            SendCommand("action\",\"NEXT_SLIDE");
            Console.WriteLine("Test NEXT_SLIDE");
        }

        private void button_prev_slide(object sender, RoutedEventArgs e)
        {
            SendCommand("action\",\"PREV_SLIDE");
            Console.WriteLine("Test PREV_SLIDE");
        }

        private void button_clap(object sender, RoutedEventArgs e)
        {
            SendCommand("action\",\"CLAP");
            Console.WriteLine("Test CLAP");
        }

        private void ActivateMouse(Object source, ElapsedEventArgs e)
        {
            // GUI status update
            SetGUIMouseState("active");

            mouseTimer.Stop();
            mouseTimer = null;
            mouseState = "active";
            Console.WriteLine(mouseState);

            // activate laser pointer
            ActivatePointer("pointer");
        }

        private void DeactivateMouse(Object source, ElapsedEventArgs e)
        {
            // GUI status update
            SetGUIMouseState("inactive");

            mouseTimer.Stop();
            mouseTimer = null;
            mouseState = "inactive";
            Console.WriteLine(mouseState);

            // disable pointer
            ActivatePointer("deactivate");

            // remote person from mouse
            if(mouseActivePerson != 0)
            {
                mouseActivePerson = 0;
            }
        }

        private void OnGestureEnd(Object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                gesture_name.Content = "Gesture: -";
            });
        }

        private void ResetConfidence(Object source, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                confidence_percentage.Content = "Max confidence: 0.00 %";
            });
        }

        public void SetGUIMouseState(string state)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case "active":
                        mouse_status.Fill = Brushes.Green;
                        break;
                    case "pending":
                        mouse_status.Fill = Brushes.Yellow;
                        break;
                    case "inactive":
                        mouse_status.Fill = Brushes.Gray;
                        break;
                    default:
                        mouse_status.Fill = Brushes.Gray;
                        break;
                }
            });
        }

        public void SetState(string state)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case "active":
                        // show to the user that kinect is active
                        circle.Fill = Brushes.Green;

                        // stop previous timer
                        if (kinectTimer != null)
                        {
                            kinectTimer.Stop();
                        }

                        // set the timer to deactivate the kinect circle
                        kinectTimer = new Timer(2 * 1000);
                        kinectTimer.Elapsed += OnActiveEnded;
                        kinectTimer.AutoReset = false;
                        kinectTimer.Enabled = true;

                        break;
                    case "deactive":
                        circle.Fill = Brushes.Gray;
                        break;
                    default:
                        circle.Fill = Brushes.Gray;
                        break;
                }
            });
        }

        public void ActivatePointer(string action)
        {
            switch (action)
            {
                case "pointer":
                    if(pointerType != "pointer")
                    {
                        SendCommand("LASER_POINTER");
                        pointerType = "pointer";
                    }
                    break;
                case "pen":
                    if (pointerType != "pen")
                    {
                        SendCommand("LASER_PEN");
                        pointerType = "pen";
                    }
                    else
                    {
                        SendCommand("LASER_POINTER");
                        pointerType = "pointer";
                    }
                    break;
                case "deactivate":
                    if (pointerType == "pointer")
                    {
                        SendCommand("LASER_POINTER");
                        pointerType = "none";
                    }
                    else if (pointerType == "pen")
                    {
                        SendCommand("LASER_PEN");
                        pointerType = "none";
                    }
                    break;
            }
        }

        public void MouseActions(string action)
        {
            switch (action)
            {
                case "activating":
                    // check existing timer, case exist ignore
                    if (mouseTimer == null)
                    {
                        // set the timer to activate the mouse
                        mouseTimer = new Timer(2 * 1000);
                        mouseTimer.Elapsed += ActivateMouse;
                        mouseTimer.AutoReset = false;
                        mouseTimer.Enabled = true;

                        // GUI status update
                        SetGUIMouseState("pending");

                        mouseState = "activating";
                        Console.WriteLine(mouseState);
                    }
                    break;
                case "deactivating":
                    if (mouseTimer != null)
                    {
                        // GUI status update
                        SetGUIMouseState("inactive");

                        mouseTimer.Stop();
                        mouseTimer = null;
                        mouseState = "inactive";
                        Console.WriteLine(mouseState);
                    }
                    break;
                case "inactive":
                    // check existing timer, case exist ignore
                    if (mouseTimer == null)
                    {
                        // set the timer to deactivate the mouse
                        mouseTimer = new Timer(2 * 1000);
                        mouseTimer.Elapsed += DeactivateMouse;
                        mouseTimer.AutoReset = false;
                        mouseTimer.Enabled = true;

                        // GUI status update
                        SetGUIMouseState("pending");

                        mouseState = "inactivating";
                        Console.WriteLine(mouseState);
                    }
                    break;
                case "continue":
                    // GUI status update
                    SetGUIMouseState("active");

                    if (mouseTimer != null)
                    {
                        mouseTimer.Stop();
                    }
                    mouseTimer = null;
                    mouseState = "active";
                    Console.WriteLine(mouseState);
                    break;
            }
        }

        public void SetConfidence(string confidence)
        {
            this.Dispatcher.Invoke(() =>
            {
                confidence_percentage.Content = "Max confidence: " + confidence + " %";
            });

            // stop previous timer
            if (confidenceTimer != null)
            {
                confidenceTimer.Stop();
            }

            // set the timer to deactivate the kinect circle
            confidenceTimer = new Timer(2 * 1000);
            confidenceTimer.Elapsed += ResetConfidence;
            confidenceTimer.AutoReset = false;
            confidenceTimer.Enabled = true;
        }

        public void SetGesture(string gesture)
        {
            if (gestureTimer != null)
            {
                gestureTimer.Stop();
            }

            gestureTimer = new Timer(1 * 1000);
            gestureTimer.Elapsed += OnGestureEnd;
            gestureTimer.AutoReset = false;
            gestureTimer.Enabled = true;

            this.Dispatcher.Invoke(() =>
            {
                gesture_name.Content = "Gesture: " + gesture;
            });
        }

        /*private double Smoothing(double value, int count, string axis)
        {
            if(axis == "x")
            {
                if(count == x_count)
                {
                    double return_val = x_value / count;
                    x_value = 0;
                    x_count = 0;

                    return return_val;
                }
                else
                {
                    x_value += value;
                    x_count++;
                }
            }
            else if (axis == "y")
            {
                if (count == y_count)
                {
                    double return_val = y_value / count;
                    y_value = 0;
                    y_count = 0;

                    return return_val;
                }
                else
                {
                    y_value += value;
                    y_count++;
                }
            }

            return -1;
        }*/

        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;
        
        /// <summary> Array for the bodies (Kinect will track up to 6 people simultaneously) </summary>
        private Body[] bodies = null;

        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary> Current status text to display </summary>
        private string statusText = null;

        /// <summary> KinectBodyView object which handles drawing the Kinect bodies to a View box in the UI </summary>
        private KinectBodyView kinectBodyView = null;
        
        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        private MainWindow main;
        private Timer kinectTimer;
        private Timer gestureTimer;
        private Timer confidenceTimer;
        private string mouseState = "inactive";


        //mouse vars
        private Timer mouseTimer;
        private bool mouseClick;
        private int mousePosY = 0;
        private bool activateMarker = false;
        private double sholderPosX = 0, sholderPosY = 0;
        private string pointerType = "none";

        private List<MouseCoo> mousePosition = new List<MouseCoo>();
        private List<string> handStatusLog = new List<string>();
        private int mouseActivePerson = 0;


        /// <summary>
        /// Initializes a new instance of the MainWindow class
        /// </summary>
        /// 

        private string handSmoothing(string state)
        {
            int listSize = handStatusLog.Count;

            if (listSize >= 10)
            {
                handStatusLog.RemoveAt(0);
            }
            else
            {
                listSize++;
            }

            if (state != "Unknown")
            {
                handStatusLog.Add(state);
            }
            else
            {
                listSize--;
            }

            if (listSize <= 0)
            {
                return "Unknown";
            }

            var groupsWithCounts = from s in handStatusLog
                                   group s by s into g
                                   select new
                                   {
                                       Item = g.Key,
                                       Count = g.Count()
                                   };

            var groupsSorted = groupsWithCounts.OrderByDescending(g => g.Count);
            string mostFrequest = groupsSorted.First().Item;

            return mostFrequest;
        }

        private MouseCoo getMouseCoordinates(double x, double y)
        {
            int listSize = mousePosition.Count;

            if (listSize >= 10)
            {
                mousePosition.RemoveAt(0);
            }
            else
            {
                listSize++;
            }

            MouseCoo mouse = new MouseCoo(x, y);

            mousePosition.Add(mouse);

            double mouse_x = 0, mouse_y = 0, avg_mouse_x = 0, avg_mouse_y = 0;
            int real_count = 0;

            // get average
            for (int i = 0; i < listSize; i++)
            {
                avg_mouse_x += mousePosition[i].x;
                avg_mouse_y += mousePosition[i].y;
            }

            avg_mouse_x /= listSize;
            avg_mouse_y /= listSize;

            /*double max_radius = 0.03;

            // delete outside of the media
            for (int i = 0; i < listSize; i++)
            {
                if(avg_mouse_x + max_radius > mousePosition[i].x && avg_mouse_x - max_radius < mousePosition[i].x &&
                    avg_mouse_y + max_radius > mousePosition[i].y && avg_mouse_y - max_radius < mousePosition[i].y)
                {
                    mouse_x += mousePosition[i].x;
                    mouse_y += mousePosition[i].y;
                    real_count++;
                }
            }

            mouse_x /= real_count;
            mouse_y /= real_count;
            */
            return new MouseCoo(avg_mouse_x, avg_mouse_y);
        }

        public MainWindow()
        {
            main = this;

            /*double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;

            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;


            //this.Top = 698;
            //this.Left = 1111;
            this.Top = screenHeight - 70;
            this.Left = screenWidth - 255;
            this.Topmost = true;
            this.Show();*/

            //init LifeCycleEvents..
            lce = new LifeCycleEvents("TOUCH", "FUSION", "touch-1", "touch", "command");
            mmic = new MmiCommunication("localhost", 9876, "User1", "TOUCH");  //CHANGED To user1
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            // only one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();
            
            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // set the BodyFramedArrived event notifier
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            this.kinectBodyView = new KinectBodyView(this.kinectSensor);

            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // initialize the MainWindow
            this.InitializeComponent();

            // set our data context objects for display in UI
            this.DataContext = this;
            //this.kinectBodyViewbox.DataContext = this.kinectBodyView;

            //circle.Fill = Brushes.Green;

            // create a gesture detector for each body (6 bodies => 6 detectors) and create content controls to display results in the UI
            int col0Row = 0;
            int col1Row = 0;
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;

            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f, lce, mmic, main);
                GestureDetector detector = new GestureDetector(this.kinectSensor, result, main);
                this.gestureDetectorList.Add(detector);

                // split gesture results across the first two columns of the content grid
                ContentControl contentControl = new ContentControl();
                contentControl.Content = this.gestureDetectorList[i].GestureResultView;

                if (i % 2 == 0)
                {
                    // Gesture results for bodies: 0, 2, 4
                    Grid.SetColumn(contentControl, 0);
                    Grid.SetRow(contentControl, col0Row);
                    ++col0Row;
                }
                else
                {
                    // Gesture results for bodies: 1, 3, 5
                    Grid.SetColumn(contentControl, 1);
                    Grid.SetRow(contentControl, col1Row);
                    ++col1Row;
                }

                //this.contentGrid.Children.Add(contentControl);
            }
        }

        private void SendCommand(String command)
        {
            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            string json = "{ \"recognized\":[\"" + command + "\"] }";

            Console.WriteLine(json);

            var exNot = lce.ExtensionNotification("-1", "-1", 1.0f, json);
            //var exNot = lce.ExtensionNotification("", "", 100, json);
            mmic.Send(exNot);
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null)
            {
                // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                foreach (GestureDetector detector in this.gestureDetectorList)
                {
                    detector.Dispose();
                }

                this.gestureDetectorList.Clear();
                this.gestureDetectorList = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.IsAvailableChanged -= this.Sensor_IsAvailableChanged;
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the event when the sensor becomes unavailable (e.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }

        private bool BetweenInterval(double value1, double value2, double variance)
        {
            if(value1 < value2 + variance && value1 > value2 - variance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor and updates the associated gesture detector object for each body
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            main.SetState("active");

            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);

                    foreach (var body in bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                int body_id = (int) body.TrackingId;

                                //Console.WriteLine("Current body: {0}", mouseActivePerson);
                                //Console.WriteLine("Active person: {0}", body_id);

                                // there is active person
                                if (mouseActivePerson != 0)
                                {
                                    // active person is not this body_id
                                    if(mouseActivePerson != body_id)
                                    {
                                        continue;
                                    }
                                }

                                // get arm joints
                                Joint rightShoulder = body.Joints[JointType.ShoulderRight];
                                //Joint rightElbow = body.Joints[JointType.ElbowRight];
                                Joint rightWrist = body.Joints[JointType.WristRight];

                                // get joints X positions
                                double x_shoulder = rightShoulder.Position.X;
                                //double x_elbow = rightElbow.Position.X;
                                double x_wrist = rightWrist.Position.X;

                                // get joints Y positions
                                double y_shoulder = rightShoulder.Position.Y;
                                //double y_elbow = rightElbow.Position.Y;
                                double y_wrist = rightWrist.Position.Y;

                                // get joints Z positions
                                double z_shoulder = rightShoulder.Position.Z;
                                //double z_elbow = rightElbow.Position.Z;
                                double z_wrist = rightWrist.Position.Z;

                                //Console.WriteLine("Wrist z: {0}", z_wrist);
                                //Console.WriteLine("Shoulder z: {0}", z_shoulder);

                                double mouseActivationLimit = 0.10;
                                double mouseActiveLimit = 0.20;
                                double minArmLenght = 0.40;
                                
                                if (mouseState == "inactive" || mouseState == "activating")
                                {
                                    if(mouseState == "inactive")
                                    {
                                        if (mouseActivePerson != 0)
                                        {
                                            mouseActivePerson = 0;
                                        }
                                    }
                                    // mouse activation if wrist and shoulder are aligned
                                    if (BetweenInterval(x_wrist, x_shoulder, mouseActivationLimit) && BetweenInterval(y_wrist, y_shoulder, mouseActivationLimit) && z_shoulder > z_wrist + minArmLenght)
                                    {
                                        MouseActions("activating");

                                        if(mouseActivePerson != body_id)
                                        {
                                            mouseActivePerson = body_id;
                                        }
                                    }
                                    else
                                    {
                                        MouseActions("deactivating");

                                        if (mouseActivePerson != 0)
                                        {
                                            mouseActivePerson = 0;
                                        }
                                    }

                                    // reset shoulder
                                    if (sholderPosX != 0 || sholderPosY != 0)
                                    {
                                        sholderPosX = 0;
                                        sholderPosY = 0;
                                    }
                                }
                                else if(mouseState == "active" || mouseState == "inactivating")
                                {
                                    if(mouseActivePerson != body_id)
                                    {
                                        mouseActivePerson = body_id;
                                    }
                                    // first time activated mouse
                                    if(sholderPosX == 0 && sholderPosY == 0)
                                    {
                                        sholderPosX = x_wrist;
                                        sholderPosY = y_wrist;
                                    }
                                    else
                                    {
                                        x_shoulder = sholderPosX;
                                        y_shoulder = sholderPosY;
                                    }

                                    // deactive mouse if out of range
                                    if (!BetweenInterval(x_wrist, x_shoulder, mouseActiveLimit) || !BetweenInterval(y_wrist, y_shoulder, mouseActiveLimit) || z_shoulder <= z_wrist + minArmLenght)
                                    {
                                        MouseActions("inactive");
                                    }
                                    else
                                    {
                                        MouseActions("continue");

                                        String rightHandState = handSmoothing(body.HandRightState.ToString());
                                        //Console.WriteLine(rightHandState);

                                        double final_wrist_x = -(x_shoulder - mouseActiveLimit) + x_wrist;
                                        double final_wrist_y = -(y_shoulder - mouseActiveLimit) + y_wrist;

                                        MouseCoo smoothed_mouse = getMouseCoordinates(final_wrist_x, final_wrist_y);

                                        double smoothed_x = smoothed_mouse.x;
                                        double smoothed_y = smoothed_mouse.y;

                                        if (smoothed_x != -1 && smoothed_y != -1)
                                        {
                                            int mouse_x = (int)(smoothed_x * 800 / 0.4);
                                            int mouse_y = (int)(600 - smoothed_y * 600 / 0.4);

                                            if (rightHandState == "Closed")
                                            {
                                                activateMarker = false;
                                                mouseClick = true;
                                                if(mousePosY == 0)
                                                {
                                                    mousePosY = mouse_y;
                                                }
                                                else
                                                {
                                                    mouse_y = mousePosY;
                                                }
                                            }
                                            else if (rightHandState == "Open")
                                            {
                                                if(mousePosY != 0)
                                                {
                                                    mouse_y = mousePosY;
                                                }
                                                mouseClick = false;
                                                mousePosY = 0;
                                                activateMarker = false;
                                            }
                                            else if (rightHandState == "Lasso")
                                            {
                                                if (!activateMarker)
                                                {
                                                    ActivatePointer("pen");
                                                    activateMarker = true;
                                                }
                                            }

                                            KinectMouseController.KinectMouseMethods.SendMouseInput(mouse_x, mouse_y, 800, 600, mouseClick);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                // visualize the new body data
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }
                }
            }
        }
    }
}
