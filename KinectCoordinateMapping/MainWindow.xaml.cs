using Microsoft.Kinect;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WindowsInput.Native;
using WindowsInput;
using System.Text.RegularExpressions;


namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The mode
        /// </summary>
        CameraMode _mode = CameraMode.Color;
        /// <summary>
        /// The gesture
        /// </summary>
        static Gesture _gesture = new Gesture();

        /// <summary>
        /// The sensor
        /// </summary>
        KinectSensor _sensor;
        /// <summary>
        /// The bodies
        /// </summary>
        Skeleton[] _bodies = new Skeleton[6];
        /// <summary>
        /// The gesto
        /// </summary>
        GesturePartResult Gesto = GesturePartResult.None;

        /// <summary>
        /// The tecl
        /// </summary>
        InputSimulator Tecl = new InputSimulator();
        /// <summary>
        /// The distancia limite x
        /// </summary>
        float DistanciaLimiteX = 0.5F;
        /// <summary>
        /// The n presion tecla
        /// </summary>
        int NPresionTecla = 25;
        /// <summary>
        /// The TCL presionada
        /// </summary>
        VirtualKeyCode TclPresionada = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new Acciones();
        }


        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.KinectSensors.Where(s => s.Status == KinectStatus.Connected).FirstOrDefault();
            txtVecesTeclado.Text = Constants.N_VECES_TECLADO.ToString();
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

        /// <summary>
        /// Handles the AllFramesReady event of the Sensor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AllFramesReadyEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Unloaded event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_sensor != null)
            {
                _sensor.Stop();
            }
        }

        /// <summary>
        /// Handles the SkeletonFrameReady event of the Sensor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SkeletonFrameReadyEventArgs"/> instance containing the event data.</param>
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
                        var user = skeletons.Where(x => x.TrackingState != SkeletonTrackingState.NotTracked &&
                        (x.Joints[JointType.ShoulderCenter].Position.X >= -DistanciaLimiteX && x.Joints[JointType.ShoulderCenter].Position.X <= DistanciaLimiteX))
                        .OrderBy(x => x.Position.Z).FirstOrDefault();

                        if (user != null)
                        {
                            Gesto = _gesture.Update(user);
                            SetAccionReconocida(Gesto);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GestureRecognized event of the Gesture control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        static void Gesture_GestureRecognized(object sender, EventArgs e)
        {
            Console.WriteLine("Empezar reconocimiento");
        }


        /// <summary>
        /// Setear eventos de teclado -> presionando n veces las teclas
        /// </summary>
        /// <param name="gesto">The gesto.</param>
        public void SetAccionReconocida(GesturePartResult gesto)
        {
            switch (gesto)
            {
                case GesturePartResult.StartHandsUp:
                    txtAccion.Text = "Empezar";
                    Tecl.Keyboard.KeyPress(VirtualKeyCode.SPACE);
                    break;
                case GesturePartResult.MoveToRight:
                    txtAccion.Text = "Mover Derecha";
                    PresionarVariasVeces(VirtualKeyCode.RIGHT, NPresionTecla);
                    break;
                case GesturePartResult.MoveToLeft:
                    txtAccion.Text = "Mover Izquierda";
                    PresionarVariasVeces(VirtualKeyCode.LEFT, NPresionTecla);
                    break;
                default:
                    txtAccion.Text = "--";
                    break;
            }

        }

        // <summary>
        /// Setear eventos de teclado -> mantener presionado teclas hasta cambiar de posicion
        /// </summary>
        /// <param name="gesto">The gesto.</param>
        /*public void SetAccionReconocida(GesturePartResult gesto)
        {
            if (TclPresionada != 0)
            {
                Tecl.Keyboard.KeyUp(TclPresionada);
            }

            switch (gesto)
            {
                case GesturePartResult.StartHandsUp:
                    txtAccion.Text = "Empezar";
                    TclPresionada = VirtualKeyCode.SPACE;
                    Tecl.Keyboard.KeyPress(TclPresionada);
                    break;
                case GesturePartResult.MoveToRight:
                    txtAccion.Text = "Mover Derecha";
                    TclPresionada = VirtualKeyCode.RIGHT;
                    Tecl.Keyboard.KeyDown(TclPresionada);
                    break;
                case GesturePartResult.MoveToLeft:
                    txtAccion.Text = "Mover Izquierda";
                    TclPresionada = VirtualKeyCode.LEFT;
                    Tecl.Keyboard.KeyDown(TclPresionada);
                    break;
                default:
                    txtAccion.Text = "--";
                    Tecl.Keyboard.KeyUp(TclPresionada);
                    break;
            }

        }*/

        /// <summary>
        /// Obteners the valor numero veces teclado.
        /// </summary>
        private void ObtenerValorNumeroVecesTeclado()
        {
            int valor = 0;
            int.TryParse(txtVecesTeclado.Text, out valor);

            if (valor == 0)
            {
                NPresionTecla = Constants.N_VECES_TECLADO;
            }

            NPresionTecla = valor;
        }

        /// <summary>
        /// Presionars the varias veces.
        /// </summary>
        /// <param name="tecla">The tecla.</param>
        /// <param name="numVeces">The number veces.</param>
        private void PresionarVariasVeces(VirtualKeyCode tecla, int numVeces)
        {
            for (int i = 0; i < numVeces; i++)
            {
                Tecl.Keyboard.KeyPress(tecla);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnIniciarValores control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnIniciarValores_Click(object sender, RoutedEventArgs e)
        {
            _gesture = new Gesture();
            txtAccion.Text = string.Empty;
        }

        /// <summary>
        /// Clase Acciones
        /// </summary>
        public class Acciones
        {
            /// <summary>
            /// The iniciar valores command
            /// </summary>
            private ICommand iniciarValoresCommand;
            /// <summary>
            /// Gets the iniciar valores command.
            /// </summary>
            /// <value>
            /// The iniciar valores command.
            /// </value>
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

        /// <summary>
        /// Numbers the validation text box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> instance containing the event data.</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Handles the Click event of the ButtonNumeroTeclas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonNumeroTeclas_Click(object sender, RoutedEventArgs e)
        {
            ObtenerValorNumeroVecesTeclado();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    enum CameraMode
    {
        /// <summary>
        /// The color
        /// </summary>
        Color,
        /// <summary>
        /// The depth
        /// </summary>
        Depth
    }

    /// <summary>
    /// 
    /// </summary>
    public enum GesturePartResult
    {
        /// <summary>
        /// The start hands up
        /// </summary>
        StartHandsUp,
        /// <summary>
        /// The go to right
        /// </summary>
        GoToRight,
        /// <summary>
        /// The go to left
        /// </summary>
        GoToLeft,
        /// <summary>
        /// The move right hand
        /// </summary>
        MoveRightHand,
        /// <summary>
        /// The move left hand
        /// </summary>
        MoveLeftHand,
        /// <summary>
        /// The move to right
        /// </summary>
        MoveToRight,
        /// <summary>
        /// The move to left
        /// </summary>
        MoveToLeft,
        /// <summary>
        /// The none
        /// </summary>
        None
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class ActionCommand : ICommand
    {
        /// <summary>
        /// The action
        /// </summary>
        private readonly Action _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCommand"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public ActionCommand(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        public void Execute(object parameter)
        {
            _action();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns>
        ///   <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        /// </returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;
    }
}
