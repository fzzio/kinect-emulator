using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;


namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CameraMode _mode = CameraMode.Color;
        static Gesture _gesture = new Gesture();

        KinectSensor _sensor;
        Skeleton[] _bodies = new Skeleton[6];
        GesturePartResult Gesto = GesturePartResult.None;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new Acciones();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();

            if (_sensor != null)
            {
                _sensor.ColorStream.Enable();
                _sensor.DepthStream.Enable();
                _sensor.SkeletonStream.Enable();
                _sensor.SkeletonFrameReady += Sensor_SkeletonFrameReady;

                _gesture.GestureRecognized += Gesture_GestureRecognized;

                _sensor.AllFramesReady += Sensor_AllFramesReady;

                _sensor.Start();
            }
        }

        void Sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // Color
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame != null)
                {
                    if (_mode == CameraMode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    frame.CopySkeletonDataTo(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // COORDINATE MAPPING
                            foreach (Joint joint in body.Joints)
                            {
                                // 3D coordinates in meters
                                SkeletonPoint skeletonPoint = joint.Position;

                                // 2D coordinates in pixels
                                Point point = new Point();

                                if (_mode == CameraMode.Color)
                                {
                                    // Skeleton-to-Color mapping
                                    ColorImagePoint colorPoint = _sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPoint, ColorImageFormat.RgbResolution640x480Fps30);

                                    point.X = colorPoint.X;
                                    point.Y = colorPoint.Y;
                                }
                                else if (_mode == CameraMode.Depth) // Remember to change the Image and Canvas size to 320x240.
                                {
                                    // Skeleton-to-Depth mapping
                                    DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution320x240Fps30);

                                    point.X = depthPoint.X;
                                    point.Y = depthPoint.Y;
                                }

                                // DRAWING...
                                Ellipse ellipse = new Ellipse
                                {
                                    Fill = Brushes.LightBlue,
                                    Width = 20,
                                    Height = 20
                                };

                                Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                                Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                                canvas.Children.Add(ellipse);
                            }
                        }
                    }
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }
        }

        void Sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];

                    frame.CopySkeletonDataTo(skeletons);

                    if (skeletons.Length > 0)
                    {
                        var user = skeletons.Where(u => u.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();

                        if (user != null)
                        {
                            Gesto = _gesture.Update(user);
                            SetAccionReconocida(Gesto);
                        }
                    }
                }
            }
        }

        static void Gesture_GestureRecognized(object sender, EventArgs e)
        {
            Console.WriteLine("Empezar reconocimiento");
        }


        public void SetAccionReconocida(GesturePartResult gesto)
        {
            switch (gesto)
            {
                case GesturePartResult.StartHandsUp:
                    txtAccion.Text = "Empezar";
                    SendKeys.SendWait("{ENTER}");
                    SendKeys.Flush();
                    break;
                case GesturePartResult.GoToRight:
                    txtAccion.Text = "Mover Derecha";
                    SendKeys.SendWait("^{RIGHT 18}");
                    SendKeys.Flush();
                    break;
                case GesturePartResult.GoToLeft:
                    txtAccion.Text = "Mover Izquierda";
                    SendKeys.SendWait("^{LEFT 18}");
                    SendKeys.Flush();
                    break;
                default:
                    break;
            }
        }

        private void btnIniciarValores_Click(object sender, RoutedEventArgs e)
        {
            _gesture = new Gesture();
            txtAccion.Text = string.Empty;
        }

        public class Acciones
        {
            private ICommand iniciarValoresCommand;
            public ICommand IniciarValoresCommand
            {
                get
                {
                    return iniciarValoresCommand
                        ?? (iniciarValoresCommand = new ActionCommand(() =>
                        {
                            _gesture = new Gesture();
                        }));
                }
            }
        }

    }

    enum CameraMode
    {
        Color,
        Depth
    }

    public enum GesturePartResult
    {
        StartHandsUp,
        GoToRight,
        GoToLeft,
        MoveRightHand,
        MoveLeftHand,
        MoveToRight,
        MoveToLeft,
        None
    }

    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
