using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Windows.Input;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using WobbrockLib;
using WobbrockLib.Extensions;

namespace ItemCompare
{
    public partial class MainWindow : SurfaceWindow
    {
        private static readonly int PADDLE_NORMAL = 0;
        private static readonly int PADDLE_ENLARGE = 1;
        private static readonly int PADDLE_SMALLER = -1;
        private static readonly int PADDLE_FROZEN = 2;
        private static readonly int SQUARE_SIDE=125;
        private static readonly int PADDLE_LENGTH=250;
        private static readonly int PADDLE_DISTANCE =75;
        private static readonly int CIRCLE_RADIOUS = 50;
        private static readonly int BALL_RADIOUS = 30;
        private static readonly int POWER_TIMEOUT = 10000;
        private static readonly int POWER_SPAWN_TIMEOUT = 5000;
        private static readonly int BORDER_X = 200;
        private static readonly int SCORE_LIFE = 100;

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private enum Powers { Enlarge, Freeze, Smaller, Tangible, Type, Wall, Full };
        private Dictionary<long, TagVisualization> dic = new Dictionary<long, TagVisualization>();
        private Dictionary<Shape, Recta[]> rectas = new Dictionary<Shape, Recta[]>();
        private Dictionary<Color, int> colorInt = new Dictionary<Color, int>() { { Colors.Red, 0 }, { Colors.Blue, 1 }, { Colors.Yellow, 2 }, { Colors.Green, 3 }, { Colors.White, 4 }, { Colors.Black, 5 }, { Colors.Transparent, 6 }, { Colors.WhiteSmoke, 7 },};
        private Dictionary<long, Color> tagColor = new Dictionary<long, Color>();
        private Dictionary<String,long> runningPowers = new Dictionary<String,long>();
        private const string itemDataFile = "ItemData.xml";
        private const double tableHorizontalMargin = 100;
        private readonly Items.ItemData itemData;
        private Polygon screen,p1, p2, pT1a, pT1b, pT2a, pT2b,wall1,wall2;
        private Ellipse e1, e2;
        private Ellipse ball;
        private int posLeft = 100;
        private int posTop = 100;
        private int margin=0;

        private const int MAX = 4;
        private const int TIME_TICK = 15;
        private const int STEP=10;
        private const int DIST = 5;
        private const int P1TAG=38;
        private const int T1A=24;
        private const int T2A=35;
        bool flagCorner = false;
        SynSocketListener socket;
        DispatcherTimer timer;

        private LinkedList<String> [] powerAvailables;
        private LinkedList<String> [] remainAdd;
        private LinkedList<String> [] remainRemove;
        private Point vector;
        private long [] needVibrate;
        private bool [] loser;
        private static bool[] moveTan;
        private int[] paddlePower;
        private int[] increment = { 0, 1, 5, 20 };
        private int[,] colors = {{ 1, 1, 2,3,3,2 },{ 3,1,2,1,2,2 },{ 2,3,1,1,2,2 },{ 1,3,2,1,2,2 },{ 1,1,2,3,1,2},{2,2,2,2,2,2}};
        private int colorBall = 5;
        private int lastPlayerTouch = 0;
        Label lab1 = new Label();
        Label lab2 = new Label();
        Label score1 = new Label();
        Label score2 = new Label();
        ////
        private SurfaceButton sbutton = null;
        private SurfaceButton sbutton2 = null;
        private long timePower1 = 0;
        private long timePower2 = 0;
        private double currTime = 0;
        /////
        bool player_1_Button_Touchdown;
        bool player_2_Button_Touchdown;
        double player_1_Button_X;
        double player_1_Button_Y;
        Stopwatch player_1_Press_Time;
        Stopwatch player_2_Press_Time;
        double player_2_Button_X;
        double player_2_Button_Y;

        private List<TimePointF> _Points_Player1;
        private List<TimePointF> _Points_Player2;
        private const int MinNoPoints = 5;
        private Recognizer _rec;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();
            InitializeComponent();
            // Plug our item data into our visualizations
            itemData = new Items.ItemData(itemDataFile);

            TagInfoText.Visibility = Visibility.Visible;

            //EventManager.RegisterClassHandler(typeof(MainWindow),
            //                          Mouse.MouseDownEvent,
            //                          new MouseButtonEventHandler(OnMouseClick));
            initialization();
            socket = new SynSocketListener();
            socket.rec += OnRec;
            Thread oThread = new Thread(new ThreadStart(socket.StartListening));
            oThread.Start();
            while (!oThread.IsAlive) ;
            Thread.Sleep(1);
        }
        void itemRegister(ItemVisualization item)
        {
            item.ItemData = itemData;
            item.Moved += OnMove;
            item.clicker += OnClick;
            item.setGlowVisibility(false);
        }
        void initialization()
        {
            itemRegister(Paddle1);
            itemRegister(Paddle2);
            itemRegister(T1a);
            itemRegister(T1b);
            itemRegister(T1c);
            itemRegister(T2a);
            itemRegister(T2b);
            itemRegister(T2c);

            Canv.Loaded += OnComparisonCanvasLoaded;
            vector = new Point(1, 0);
            initPlayerButtons();
            remainAdd = new LinkedList<String>[2];
            remainRemove = new LinkedList<String>[2];
            powerAvailables = new LinkedList<String>[2];
            needVibrate = new long[2];
            moveTan = new bool[2];
            loser = new bool[2];
            paddlePower = new int[2];
            needVibrate[0] = 0;
            needVibrate[1] = 0;
            loser[0] = false;
            loser[1] = false;
            moveTan[0] = false;
            moveTan[1] = false;
            paddlePower[0] = PADDLE_NORMAL;
            paddlePower[1] = PADDLE_NORMAL;
            remainAdd[0] = new LinkedList<string>();
            remainAdd[0].AddLast("smaller");
            remainAdd[0].AddLast("enlarge");
            remainAdd[0].AddLast("tangible");
            remainAdd[0].AddLast("wall");
            remainAdd[1] = new LinkedList<string>();
            remainAdd[1].AddLast("wall");
            remainAdd[1].AddLast("type");
            remainAdd[1].AddLast("tangible");

            remainRemove[0] = new LinkedList<string>();
            remainRemove[1] = new LinkedList<string>();
            powerAvailables[0] = new LinkedList<string>();
            powerAvailables[0].AddLast("wall");
            powerAvailables[0].AddLast("smaller");
            powerAvailables[0].AddLast("enlarge");
            powerAvailables[0].AddLast("tangible");

            powerAvailables[1] = new LinkedList<string>();
            powerAvailables[1].AddLast("wall");
            powerAvailables[1].AddLast("type");
            powerAvailables[1].AddLast("tangible");
        }

        private void initPlayerButtons()
        {
            player_2_Press_Time = new Stopwatch();
            player_1_Press_Time = new Stopwatch();

            this.Player_1_Button.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"SmallSquareP1.jpg", UriKind.Relative))
            };

            this.Player_2_Button.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"SmallSquareP2.jpg", UriKind.Relative))
            };

            this.Player_1_Gesture_Input.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"BigSquareP1.jpg", UriKind.Relative))
            };

            this.Player_2_Gesture_Input.Fill = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"BigSquareP2.jpg", UriKind.Relative))
            };

            _rec = new Recognizer();
            _Points_Player1 = new List<TimePointF>(256);
            _Points_Player2 = new List<TimePointF>(256);


            this.Player_1_Button.PreviewTouchDown += playerButtonTouchDown;
            this.Player_1_Button.PreviewTouchMove += playerButtonMove;
            this.Player_1_Button.PreviewTouchUp += playerButtonTouchUp;
            this.Player_1_Button.TouchLeave += playerButtonTouchUp;
            this.Player_1_Button.LostTouchCapture += playerButtonTouchUp;

            this.Player_1_Gesture_Input.TouchDown += new_TouchDown;
            this.Player_1_Gesture_Input.TouchMove += new_TouchMove;
            this.Player_1_Gesture_Input.TouchUp += new_TouchUp;

            this.Player_2_Button.PreviewTouchDown += playerButtonTouchDown;
            this.Player_2_Button.PreviewTouchMove += playerButtonMove;
            this.Player_2_Button.PreviewTouchUp += playerButtonTouchUp;
            this.Player_2_Button.TouchLeave += playerButtonTouchUp;
            this.Player_2_Button.LostTouchCapture += playerButtonTouchUp;

            this.Player_2_Gesture_Input.TouchDown += new_TouchDown;
            this.Player_2_Gesture_Input.TouchMove += new_TouchMove;
            this.Player_2_Gesture_Input.TouchUp += new_TouchUp;

            loadGestures();
        }
        void loadGestures()
        {
            _rec.LoadGesture("Resources/EnlargeBackwards.xml");
            _rec.LoadGesture("Resources/Enlarge.xml");
            _rec.LoadGesture("Resources/Freeze.xml");
            _rec.LoadGesture("Resources/Back.xml");
            _rec.LoadGesture("Resources/Move Tangibles.xml");
            _rec.LoadGesture("Resources/Move TangiblesBackwards.xml");
            _rec.LoadGesture("Resources/Shrink.xml");
            _rec.LoadGesture("Resources/ShrinkBackwards.xml");
            _rec.LoadGesture("Resources/Wall.xml");
            _rec.LoadGesture("Resources/WallBackwards.xml");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        private void playerButtonTouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Rectangle senderRect = (Rectangle)sender;
            string name = senderRect.Name;
            if (name.Equals("Player_1_Button"))
            {
                player_1_Button_Touchdown = true;
                player_1_Button_X = e.GetTouchPoint(null).Position.X;
                player_1_Button_Y = e.GetTouchPoint(null).Position.Y;
                player_1_Press_Time.Reset();
                player_1_Press_Time.Start();
            }
            else
            {
                player_2_Button_Touchdown = true;
                player_2_Button_X = e.GetTouchPoint(null).Position.X;
                player_2_Button_Y = e.GetTouchPoint(null).Position.Y;
                player_2_Press_Time.Reset();
                player_2_Press_Time.Start();
            }
        }

        private void playerButtonMove(object sender, System.Windows.Input.TouchEventArgs e)
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            var width = screen.Width;
            Rectangle item = (Rectangle)sender;
            string name = item.Name;
            if (name.Equals("Player_1_Button"))
            {
                if (player_1_Button_Touchdown)
                {

                    // Calculate the current position of the object.
                    double deltaV = e.GetTouchPoint(null).Position.Y - player_1_Button_Y;
                    Debug.WriteLine("Old position: X:" + player_1_Button_X + " Y:" + player_1_Button_Y + " of player 1");
                    double deltaH = e.GetTouchPoint(null).Position.X - player_1_Button_X;
                    double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
                    double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

                    Debug.WriteLine("Touchpoint X: " + e.GetTouchPoint(null).Position.X + " " + "; Y: " + e.GetTouchPoint(null).Position.Y);

                    Debug.WriteLine("Old position: X:" + player_1_Button_X + " Y:" + player_1_Button_Y + " of player 1");
                    Debug.WriteLine("New position: X:" + newLeft + " Y:" + newTop + " of player 1");

                    // Set new position of object.ezezeze
                    this.Player_1_Button.SetValue(Canvas.TopProperty, newTop);
                    this.Player_1_Button.SetValue(Canvas.LeftProperty, Math.Min(newLeft, width / 2));
                    //this.Player_1_Button.SetValue(Canvas.LeftProperty, newLeft);

                    // Update position global variables.
                    player_1_Button_Y = e.GetTouchPoint(null).Position.Y;
                    player_1_Button_X = e.GetTouchPoint(null).Position.X;
                }
            }

            else
            {
                if (player_2_Button_Touchdown)
                {
                    // Calculate the current position of the object.
                    double deltaV = e.GetTouchPoint(null).Position.Y - player_2_Button_Y;
                    double deltaH = e.GetTouchPoint(null).Position.X - player_2_Button_X;
                    double newTop = deltaV + (double)item.GetValue(Canvas.TopProperty);
                    double newLeft = deltaH + (double)item.GetValue(Canvas.LeftProperty);

                    Debug.WriteLine("Old position: X:" + player_1_Button_X + " Y:" + player_1_Button_Y + "; of player 2");
                    Debug.WriteLine("New position: X:" + player_1_Button_X + " Y:" + player_1_Button_Y + "; of player 2");

                    // Set new position of object.
                    this.Player_2_Button.SetValue(Canvas.TopProperty, newTop);
                    this.Player_2_Button.SetValue(Canvas.LeftProperty, Math.Max(newLeft, width / 2));
                    //this.Player_2_Button.SetValue(Canvas.LeftProperty, newLeft);

                    // Update position global variables.
                    player_2_Button_Y = e.GetTouchPoint(null).Position.Y;
                    player_2_Button_X = e.GetTouchPoint(null).Position.X;
                }
            }

        }

        private void playerButtonTouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Rectangle item = (Rectangle)sender;
            string name = item.Name;
            if (name.Equals("Player_1_Button"))
            {
                player_1_Button_Touchdown = false;
                player_1_Button_X = -1;
                player_1_Button_Y = -1;
                player_1_Press_Time.Stop();
                if (player_1_Press_Time.ElapsedMilliseconds < 500)
                    showGestureInputPlayer1();
            }
            else
            {
                player_2_Button_Touchdown = false;
                player_2_Button_X = -1;
                player_2_Button_Y = -1;
                player_2_Press_Time.Stop();
                if (player_2_Press_Time.ElapsedMilliseconds < 500)
                    showGestureInputPlayer2();
            }
        }

        private void showGestureInputPlayer2()
        {
            enableGestures(Player_2_Button);
        }

        private void showGestureInputPlayer1()
        {
            enableGestures(Player_1_Button);
        }

        private void enableGestures(Rectangle button)
        {
            string name = button.Name;
            button.Height = 0;
            button.Width = 0;
            int size = 500;
            if (name.Equals("Player_1_Button"))
            {
                this.Player_1_Gesture_Input.Height = size;
                this.Player_1_Gesture_Input.Width = size;
                this.Player_1_Gesture_Input.SetValue(Canvas.TopProperty, button.GetValue(Canvas.TopProperty));
                this.Player_1_Gesture_Input.SetValue(Canvas.LeftProperty, button.GetValue(Canvas.LeftProperty));
            }
            else
            {
                this.Player_2_Gesture_Input.Height = size;
                this.Player_2_Gesture_Input.Width = size;
                this.Player_2_Gesture_Input.SetValue(Canvas.TopProperty, button.GetValue(Canvas.TopProperty));
                this.Player_2_Gesture_Input.SetValue(Canvas.LeftProperty, button.GetValue(Canvas.LeftProperty));

            }
        }

        private void disableGestures(Rectangle button)
        {
            string name = button.Name;
            button.Height = 0;
            button.Width = 0;

            if (name.Equals("Player_1_Gesture_Input"))
            {
                this.Player_1_Button.Height = 100;
                this.Player_1_Button.Width = 100;
            }
            else
            {
                this.Player_2_Button.Height = 100;
                this.Player_2_Button.Width = 100;
            }
        }

        #region GestureRecognition

        void new_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Rectangle senderRect = (Rectangle)sender;
            string name = senderRect.Name;
            if (name.Equals("Player_1_Gesture_Input"))
            {
                player_1_Button_Touchdown = true;
                _Points_Player1.Clear();
                _Points_Player1.Add(new TimePointF(e.GetTouchPoint(this).Position.X, e.GetTouchPoint(this).Position.Y, TimeEx.NowMs));
            }
            else
            {
                player_2_Button_Touchdown = true;
                _Points_Player2.Clear();
                _Points_Player2.Add(new TimePointF(e.GetTouchPoint(this).Position.X, e.GetTouchPoint(this).Position.Y, TimeEx.NowMs));
            }
        }

        void new_TouchMove(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Rectangle senderRect = (Rectangle)sender;
            string name = senderRect.Name;
            if (name.Equals("Player_1_Gesture_Input"))
            {
                if (player_1_Button_Touchdown)
                {
                    _Points_Player1.Add(new TimePointF(e.GetTouchPoint(this).Position.X, e.GetTouchPoint(this).Position.Y, TimeEx.NowMs));
                }
            }
            else
            {
                if (player_2_Button_Touchdown)
                {
                    _Points_Player2.Add(new TimePointF(e.GetTouchPoint(this).Position.X, e.GetTouchPoint(this).Position.Y, TimeEx.NowMs));
                }
            }
        }

        void new_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            Rectangle senderRect = (Rectangle)sender;
            string name = senderRect.Name;
            if (name.Equals("Player_1_Gesture_Input"))
            {
                if (player_1_Button_Touchdown)
                {
                    player_1_Button_Touchdown = false;

                    if (_Points_Player1.Count >= MinNoPoints)
                    {
                        if (_rec.NumGestures > 0) // not recording, so testing
                        {
                            RecognizeAndDisplayResults(((Rectangle)sender).Name); // Send the name of the object to identify the player
                        }
                    }
                }
            }
            else
            {
                if (player_2_Button_Touchdown)
                {
                    player_2_Button_Touchdown = false;

                    if (_Points_Player2.Count >= MinNoPoints)
                    {
                        if (_rec.NumGestures > 0) // not recording, so testing
                        {
                            RecognizeAndDisplayResults(((Rectangle)sender).Name); // Send the name of the object to identify the player
                        }
                    }
                }
            }
        }

        void RecognizeAndDisplayResults(string sender)
        {
            NBestList result;
            if (sender.Equals("Player_1_Gesture_Input"))
            {
                result = _rec.Recognize(_Points_Player1, false); // False to signal we are using Golden Section Search
                if (result.Name.Equals("Back"))
                    disableGestures(this.Player_1_Gesture_Input);
                else
                {
                    string name = result.Name.Split('>')[0];
                    userPower("0" + name.ToLower());
                    remainRemove[0].AddLast(name.ToLower());
                    disableGestures(this.Player_1_Gesture_Input);
                }
            }

            else
            {
                result = _rec.Recognize(_Points_Player2, false); // False to signal we are using Golden Section Search
                if (result.Name.Equals("Back"))
                    disableGestures(this.Player_2_Gesture_Input);
                else
                {
                    string name = result.Name.Split('>')[0];
                    userPower("1" + name.ToLower());
                    remainRemove[1].AddLast(name.ToLower());
                    disableGestures(this.Player_2_Gesture_Input);
                }
            }
        }

        #endregion

        private void OnRec(object sender, EventArgs e)
        {
            AppData res = ((TextEventArgs)e).ad;
            
            Console.WriteLine();
            string data=res.getText();
            int player = data[0]-'0';
            byte code=res.getOPCode();
            res.setText("OK");
            res.setOPCode(1);
            switch (code)
            {
                case AppData.REQUEST:
                    if(loser[player]){
                        res.setOPCode(AppData.LOSE);
                        loser[player] = false;
                    }else if (remainRemove[player].Count != 0)
                    {
                        String text = "";
                        foreach (String s in remainRemove[player])
                        {
                            if (text.Equals("")) text += s;
                            else text += ">" + s;
                        }
                        remainRemove[player].Clear();
                        res.setText(text);
                        res.setOPCode(AppData.POWER_REMOVE);
                    }else if (remainAdd[player].Count != 0)
                    {
                        String text = "";
                        foreach (String s in remainAdd[player])
                        {
                            if (text.Equals("")) text += s;
                            else text += ">" + s;
                        }
                        remainAdd[player].Clear();
                        res.setText(text);
                        res.setOPCode(AppData.POWER_ADD);
                    }
                    else if (needVibrate[player]!=0)
                    {
                        res.setOPCode(AppData.VIBR);
                        needVibrate[player] = 0;
                    }
                    else
                    {
                        res.setText("NOTHING");
                        res.setOPCode(0);
                    }
                    break;
                case AppData.POWER_ADD:
                    if (!userPower(data)){
                        res.setText("BAD");
                        res.setOPCode(0);
                    }
                    break;
                case AppData.SUR:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        losePoint(player);
                    }));
                    
                    break;
                case AppData.PLAY:
                    
                    String str = data.Substring(1);
                    if (str.Equals("lose"))
                    {
                        if (player == 0)
                        {
                            vector = new Point(1, 0.8);
                        }
                        else
                        {
                            vector = new Point(-1, -0.8);
                        }
                    }
                    pauseGame(false);
                    break;
                case AppData.PAUSE:
                    pauseGame(true);
                    break;
                case AppData.PTYPE:
                    changeType(data);
                    break;
            }
        }
        private bool userPower(String text)
        {
            int player = text[0]-'0';
            player = player % 2;
            text = text.Substring(1);
            if (powerAvailables[player].Contains(text)){
                if (text.Equals("enlarge")){
                    paddlePower[player] = PADDLE_ENLARGE;
                }else if (text.Equals("smaller"))
                {
                    paddlePower[(player+1)%2] = PADDLE_SMALLER;
                }
                else if (text.Equals("freeze"))
                {
                    paddlePower[(player + 1) % 2] = PADDLE_FROZEN;
                }
                else if (text.Equals("wall"))
                {
                    tagColor[1] = Colors.Brown;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (player == 0) wall1 = drawSquare(wall1, new Point(25, RootGrid.ActualHeight / 2), 50, (int)RootGrid.ActualHeight, 1,0);
                        else wall2 = drawSquare(wall2, new Point(RootGrid.ActualWidth-25, RootGrid.ActualHeight / 2), 50, (int)RootGrid.ActualHeight, 1,0);
                    }));
                }
                else if (text.Equals("tangible"))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        moveTan[player] = true;
                        setTangiblesGlow(player, true);
                    }));
                    pauseGame(true);
                }
                powerAvailables[player].Remove(text);
                runningPowers[player + text] = CurrentTimeMillis();
                System.Diagnostics.Debug.WriteLine(player + text);
                System.Diagnostics.Debug.WriteLine("time " + runningPowers[player + text]);
                return true;
            }
            return false;
        }
        private void removePower(String text)
        {
            int player = text[0] - '0';
            player = player % 2;
            text = text.Substring(1);
            if (text.Equals("smaller")||text.Equals("freeze")) paddlePower[(player + 1) % 2] = PADDLE_NORMAL;
            else if (text.Equals("enlarge")) paddlePower[player] = PADDLE_NORMAL;
            else if (text.Equals("wall")){
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (player==0) eraseShape(wall1);
                    else eraseShape(wall2);
                }));
            }else if (text.Equals("tangible"))
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    moveTan[player] = false;
                    setTangiblesGlow(player,false);
                }));
            }
        }

        private void setTangiblesGlow(int player,bool glow){
            if (player == 0)
            {
                T1a.setGlowVisibility(glow);
                T1b.setGlowVisibility(glow);
                T1c.setGlowVisibility(glow);
            }else
            {
                T2a.setGlowVisibility(glow);
                T2b.setGlowVisibility(glow);
                T2c.setGlowVisibility(glow);
            }
        }

        private void changeType(String type)
        {
            int player = type[0] - '0';
            type = type.Substring(1);
            int index = -1;
            Color color=Colors.Black;
            switch (type)
            {
                case "red":
                    index = 0;
                    color=Colors.Red;
                    break;
                case "blue":
                    index = 1;
                    color=Colors.Blue;
                    break;
                case "yellow":
                    index = 2;
                    color=Colors.Yellow;
                    break;
                case "green":
                    index = 3;
                    color=Colors.Green;
                    break;
                case "white":
                    index = 4;
                    color=Colors.White;
                    break;
                case "black":
                    index=5;
                    color=Colors.Black;
                    break;
            }
            colorBall = index;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SolidColorBrush brush = new SolidColorBrush();
                brush.Color = color;
                ball.Fill = brush;
            }));
        }

        private void increaseScore(Shape s)
        {
            long now = CurrentTimeMillis();
            long tag=(long)s.Tag;
            if (tag == P1TAG)
            {
                lastPlayerTouch = 0;
                if (now-needVibrate[0]<300)
                {
                    losePoint(0);
                }
                else needVibrate[0] = now;
            }
            else if (tag == P1TAG + 1)
            {
                lastPlayerTouch = 1;
                if (now-needVibrate[1]<300)
                {
                    losePoint(1);
                }
                else needVibrate[1] = now;
            }
            else if ((tag != 0)&&(tag!=1))
            {
                int col = colorInt[tagColor[tag]];
                int val = colors[colorBall, col];

                if (lastPlayerTouch == 0)
                {
                    int newValue = (int)lab2.Content + increment[val];
                    if (newValue >= SCORE_LIFE)
                        score2.Content = (int)score2.Content + 1;
                    lab2.Content = newValue % SCORE_LIFE;
                }
                else
                {
                    int newValue = (int)lab1.Content + increment[val];
                    if (newValue >= SCORE_LIFE)
                        score1.Content = (int)score1.Content + 1;
                    lab1.Content = newValue % SCORE_LIFE;
                }
            }
            else if (tag==0)
            {
                int left = (int)Canvas.GetLeft(ball);
                if (left < 10)
                {
                    losePoint(0);
                }
                if (left >= (RootGrid.ActualWidth - BALL_RADIOUS * 2))
                {
                    losePoint(1);
                }
            }
        }

        private void losePoint(int player)
        {
            pauseGame(true);
            if (player == 0)
            {
                posLeft = (int)PADDLE_DISTANCE + BALL_RADIOUS;
                posTop = (int)Paddle1.Center.Y;
                vector = new Point(1, 0.8);
                loser[0] = true;
                score2.Content = (int)score2.Content + 1;

            }
            else
            {
                posLeft = (int)RootGrid.ActualWidth-PADDLE_DISTANCE - BALL_RADIOUS*2;
                posTop = (int)Paddle2.Center.Y;
                vector = new Point(-1,-0.8);
                loser[1] = true;
                score1.Content = (int)score1.Content + 1;
            }
            Canvas.SetLeft(ball, posLeft);
            Canvas.SetTop(ball, posTop);
        }
        private void pauseGame(bool pause)
        {
            if (pause)
            {
                timer.Stop();
            }
            else
            {
                timer.Start();
            }
        }
        public static bool shouldShowTable(long tag)
        {
            if (tag == T1A || tag == T1A + 1 || tag == T1A + 2)
                return moveTan[0];
            else if (tag == T2A || tag == T2A + 1 || tag == T2A + 2)
                return moveTan[1];
            else return false;
        }
        private void OnMouseClick(Object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point p=e.GetPosition(RootGrid);
                if (p.X < RootGrid.ActualWidth / 2)
                {
                    TagVisualizerCanvas.SetCenter(Paddle1, p);
                    dic[P1TAG] = Paddle1;
                    drawByTag(P1TAG,0);
                }
                else
                {
                    TagVisualizerCanvas.SetCenter(Paddle2, p);
                    dic[P1TAG+1] = Paddle2;
                    drawByTag(P1TAG+1,0);
                }
            }
        }

        private DispatcherTimer loop()
        {
            // Create an Ellipse
            ball = new Ellipse();
            ball.Height = BALL_RADIOUS;
            ball.Width = BALL_RADIOUS;
            // Create a blue and a black Brush
            SolidColorBrush brush = new SolidColorBrush();
            brush.Color = Colors.Black;
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            // Set Ellipse's width and color
            ball.StrokeThickness = 4;
            ball.Stroke = blackBrush;
            // Fill rectangle with blue color
            ball.Fill = brush;

            // Add Ellipse to the Grid.
            Canv.Children.Add(ball);
            posLeft= PADDLE_DISTANCE+BALL_RADIOUS;
            posTop=(int)RootGrid.ActualHeight/2;
            Canvas.SetLeft(ball,posLeft);
            Canvas.SetTop(ball,posTop);

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, TIME_TICK);
            loser[0] = true;
            
            return timer;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (timer.IsEnabled)
            {
                currTime += 0.0015*TIME_TICK;
                if (currTime > 10)
                {
                    Random r = new Random();
                    int widthP1 = r.Next(BORDER_X, (int)RootGrid.ActualWidth / 2);
                    int heightP1 = r.Next(0, (int)RootGrid.ActualHeight);
                    int widthP2 = r.Next((int)RootGrid.ActualWidth / 2, (int)RootGrid.ActualWidth - BORDER_X);
                    int heightP2 = r.Next(0, (int)RootGrid.ActualHeight);

                    Point p1 = new Point(widthP1, heightP1);
                    Point p2 = new Point(widthP2, heightP2);
                    spawnPower(p1, p2);
                    currTime = 0;
                }
            }

            // check if there are overdue power-up pickups
            long actual = CurrentTimeMillis();

            if (sbutton != null)
            {
                if (actual - timePower1 > POWER_SPAWN_TIMEOUT)
                {
                    RootGrid.Children.Remove(sbutton);
                    sbutton = null;
                }
            }
            if (sbutton2 != null)
            {
                if (actual - timePower2 > POWER_SPAWN_TIMEOUT)
                {
                    RootGrid.Children.Remove(sbutton2);
                    sbutton2 = null;
                }
            }

            //Chech if there are overdue power-ups
            actual=CurrentTimeMillis();
            String[] removed = new String[runningPowers.Count];
            int i = 0;
            foreach (String s in runningPowers.Keys)
            {
                long time = runningPowers[s];
                if (actual - time > POWER_TIMEOUT)
                {
                    removed[i] = s;
                    i++;
                    removePower(s);
                }
            }
            for (int j = 0; j < i; j++) runningPowers.Remove(removed[j]);

            int left = (int)Canvas.GetLeft(ball);
            int top = (int)Canvas.GetTop(ball);
            double pLeft=vector.X/1.0;
            double pTop=vector.Y/1.0;
            posLeft += (int)Math.Round(STEP * pLeft);
            posTop += (int)Math.Round(STEP * pTop);

            margin--;
            foreach (Shape pol in rectas.Keys)
            {
                Recta[] array = rectas[pol];
                foreach (Recta r in array)
                {
                    if (r.B.Y == -1)
                    { //If it is a circle
                        Point p=collideWithCircle(r.A, (int)r.B.X);
                        if ((p.X != -100) || (p.Y != -100))
                            if ((margin <= 0) || margin == MAX) CollisionRotation(p,pol);
                    }
                    else
                    {
                        Point p = collideWithLine(r.A, r.B);
                        if ((flagCorner)&&((p.X != -100) || (p.Y != -100)))
                        { //if collide with a corner
                            if ((margin <= 0) || margin == MAX)
                            {
                                CollisionRotation(p,pol);
                            }
                        }
                        else if ((p.X != -100) || (p.Y != -100))
                        { //if collide with a line / side
                            Point lineVector;
                            if ((vector.Y > 0) && (vector.X > 0)) //Down-right
                                lineVector = subPoint(r.A, r.B);
                            else if ((vector.Y > 0) && (vector.X < 0)) //Down-left
                                lineVector = subPoint(r.A, r.B);
                            else if ((vector.Y < 0) && (vector.X > 0)) //Up-right
                                lineVector = subPoint(r.B, r.A);
                            else lineVector = subPoint(r.B, r.A); //Up-left

                            if ((margin <= 0) || margin == MAX) CollisionRotation(lineVector,pol);
                        }
                    }
                    flagCorner = false;
                }
            }
            Canvas.SetLeft(ball, posLeft);
            Canvas.SetTop(ball,posTop);
        }

        private void spawnPower(Point p1, Point p2)
        {
            Powers pow1 = getRandomPower(0);
            Powers pow2 = getRandomPower(1);

            String pathPow1 = powerToPath(pow1);
            String pathPow2 = powerToPath(pow2);

            if (pow1 != Powers.Full)
            {
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(pathPow1, UriKind.Relative));

                sbutton = new SurfaceButton();
                sbutton.Name = pow1.ToString();
                sbutton.HorizontalAlignment = HorizontalAlignment.Left;
                sbutton.VerticalAlignment = VerticalAlignment.Top;
                sbutton.Background = brush;
                sbutton.Width = 100;
                sbutton.Height = 100;
                sbutton.Margin = new Thickness(p1.X, p1.Y, 0, 0);

                sbutton.Click += new RoutedEventHandler(sbutton_Click);
                sbutton.TouchDown += new EventHandler<TouchEventArgs>(sbutton_TouchDown);
                RootGrid.Children.Add(sbutton);
                timePower1 = CurrentTimeMillis();
            }

            if (pow2 != Powers.Full)
            {
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(pathPow2, UriKind.Relative));

                sbutton2 = new SurfaceButton();
                sbutton2.Name = pow2.ToString();
                sbutton2.HorizontalAlignment = HorizontalAlignment.Left;
                sbutton2.VerticalAlignment = VerticalAlignment.Top;
                sbutton2.Background = brush;
                sbutton2.Width = 100;
                sbutton2.Height = 100;
                sbutton2.Margin = new Thickness(p2.X, p2.Y, 0, 0);

                sbutton2.Click += new RoutedEventHandler(sbutton_Click);
                sbutton2.TouchDown += new EventHandler<TouchEventArgs>(sbutton_TouchDown);
                RootGrid.Children.Add(sbutton2);
                timePower2 = CurrentTimeMillis();
            }
        }

        void sbutton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("in");
            SurfaceButton button = (SurfaceButton)sender;
            RootGrid.Children.Remove(button);

            if (button.Margin.Left < RootGrid.ActualWidth / 2)
            {
                Debug.WriteLine("left player: " + button.Name.ToLower());
                powerAvailables[0].AddLast(button.Name.ToLower());
                remainAdd[0].AddLast(button.Name.ToLower());
            }
            else
            {
                Debug.WriteLine("right player: " + button.Name.ToLower());
                powerAvailables[1].AddLast(button.Name.ToLower());
                remainAdd[1].AddLast(button.Name.ToLower());
            }
        }

        void sbutton_TouchDown(object sender, TouchEventArgs e)
        {
            Debug.WriteLine("in");
            SurfaceButton button = (SurfaceButton)sender;
            RootGrid.Children.Remove(button);
            if (button.Margin.Left < RootGrid.ActualWidth / 2)
            {
                Debug.WriteLine("left player: " + button.Name.ToLower());
                powerAvailables[0].AddLast(button.Name.ToLower());
                remainAdd[0].AddLast(button.Name.ToLower());
            }
            else
            {
                Debug.WriteLine("right player: " + button.Name.ToLower());
                powerAvailables[1].AddLast(button.Name.ToLower());
                remainAdd[1].AddLast(button.Name.ToLower());
            }
        }

        Powers getRandomPower(int playerNumber)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            if (powerAvailables[playerNumber].Count == 6)
                return Powers.Full;

            int num = rand.Next(6);
            String str = powerToString((Powers)num);

            while (powerAvailables[playerNumber].Contains(str))
            {
                num = rand.Next(6);
                str = powerToString((Powers)num);
            }
            return (Powers)num;
        }

        String powerToString(Powers power)
        {
            switch (power)
            {
                case Powers.Enlarge:
                    return "enlarge";
                case Powers.Freeze:
                    return "freeze";
                case Powers.Smaller:
                    return "smaller";
                case Powers.Tangible:
                    return "tangible";
                case Powers.Type:
                    return "type";
                case Powers.Wall:
                    return "wall";
                default:
                    return "";
            }
        }

        String powerToPath(Powers power)
        {
            switch (power)
            {
                case Powers.Enlarge:
                    return "Resources/power_enlarge.png";
                case Powers.Freeze:
                    return "Resources/power_freeze.png";
                case Powers.Smaller:
                    return "Resources/power_smaller.png";
                case Powers.Tangible:
                    return "Resources/power_tangible.png";
                case Powers.Type:
                    return "Resources/power_type.png";
                case Powers.Wall:
                    return "Resources/power_wall.png";
                default:
                    return "";
            }
        }

        private void CollisionRotation(Point lineVector,Shape shape)
        {
            increaseScore(shape);
            margin--;
            int dot = (int)(lineVector.X * vector.X + lineVector.Y * vector.Y);      //# dot product
            int det = (int)(lineVector.X * vector.Y - lineVector.Y * vector.X);      //# determinant
            double angle = Math.Atan2(det, dot);  //# atan2(y, x) or atan2(sin, cos)
            angle = angle * (180 / Math.PI);
            double angle1 = angle;

            if ((angle < 90) && (angle > 0)) angle = -1 * angle;
            else if (angle > 90) angle = 180 - angle;
            else if (angle < -90)
            {
                angle = -180 - angle;
            }
            else angle = -1 * angle;
            double angle2 = angle;
            angle = (angle * Math.PI) / 180;
            RotateVector(angle * 2);
        }
        private void RotateVector(double radians)
        {
            double x = vector.X * Math.Cos(radians) - vector.Y * Math.Sin(radians);
            double y = vector.X * Math.Sin(radians) + vector.Y * Math.Cos(radians);
            vector.X = x;
            vector.Y = y;
        }

        private void OnMove(object sender, EventArgs e)
        {
            TagVisualizerEventArgs t = (TagVisualizerEventArgs)e;
            TagVisualization tv = t.TagVisualization;
            long tag=tv.VisualizedTag.Value;
            dic[tag] = tv;

            System.Diagnostics.Debug.WriteLine("tag: " + tag);
            System.Diagnostics.Debug.WriteLine("e.x: " + tv.Center.X);
            System.Diagnostics.Debug.WriteLine("e.y: " + tv.Center.Y);
            if ((tag==P1TAG)||(tag==P1TAG+1)) drawByTag(tag,tv.Orientation);
            else if (((tag==T1A)||(tag==T1A+1)||(tag==T1A+2))&&(moveTan[0])) drawByTag(tag,tv.Orientation);
            else if (((tag == T2A) || (tag == T2A + 1) || (tag == T2A + 2)) && (moveTan[1])) drawByTag(tag, tv.Orientation);
        }

        private Point subPoint(Point a,Point b){
            return new Point(a.X-b.X,a.Y-b.Y);
        }
        private Point plusPoint(Point a,Point b){
            return new Point(a.X+b.X,a.Y+b.Y);
        }
        private Point numPerPoint(Point a,double num){
            return new Point(a.X*num,a.Y*num);
        }
        private Point ortoVector(Point a)
        {
            return new Point(-a.Y, a.X);
        }
        
        private Point collideWithLine(Point LineP1, Point LineP2)
        {
            int Radius = (int)ball.Width / 2;
            int x0 = (int)Canvas.GetLeft(ball)+Radius;
            int y0 = (int)Canvas.GetTop(ball)+Radius;
            double lx1=LineP1.X; 
            double ly1=LineP1.Y;
            double lx2=LineP2.X;
            double ly2=LineP2.Y;
            int DIS;
            if ((margin <= 0) || margin == MAX) DIS = DIST + Radius;
            else DIS = Radius;
            if (((Math.Abs(lx1 - x0) < DIS) && (Math.Abs(ly1 - y0) < DIS)) || ((Math.Abs(lx2 - x0) < DIS) && (Math.Abs(ly2 - y0) < DIS)))
            {
                Point res;
                if ((Math.Abs(lx1 - x0) < DIS) && (Math.Abs(ly1 - y0) < DIS))
                    res=ortoVector(subPoint(new Point(x0, y0), LineP1));
                else
                    res = ortoVector(subPoint(new Point(x0, y0),LineP2));
                
                if (margin <= 0) margin = MAX;
                flagCorner = true;
                return res;
            }
            ////////START
            double A1 = ly2 - ly1;
            double B1 = lx1 - lx2;
            double C1 = (ly2 - ly1) * lx1 + (lx1 - lx2) * ly1;
            double C2 = -B1 * x0 + A1 * y0;
            double det = A1 * A1 - -B1 * B1;
            double cx = 0;
            double cy = 0;
            if (det != 0)
            {
                cx = (float)((A1 * C1 - B1 * C2) / det);
                cy = (float)((A1 * C2 - -B1 * C1) / det);
            }
            else
            {
                cx = x0;
                cy = y0;
            }
            double distancex = (cx - x0) * (cx - x0);
            double distancey = (cy - y0) * (cy - y0);
            double calcdistance = Math.Sqrt(distancex + distancey);
            if (calcdistance < (Radius+10))
            {
                if (lx1 > lx2)
                {
                    double aux = lx1;
                    lx1 = lx2;
                    lx2 = aux;
                }
                if (ly1 > ly2)
                {
                    double aux = ly1;
                    ly1 = ly2;
                    ly2 = aux;
                }
                if ((cx <= lx2) && (cx >= lx1) && (cy <= ly2) && (cy >= ly1))
                {
                    if (margin <= 0) margin = MAX;
                    return new Point(cx, cy);
                }
                else return new Point(-100,-100);
            }
            else
            {
                return new Point(-100, -100);
            }
        }
        private Point collideWithCircle(Point c, int R2)
        {
            int R1=(int)ball.Width/2;
            int left = (int)Canvas.GetLeft(ball);
            int top = (int)Canvas.GetTop(ball);

            // Calculate difference between centres
	        int distX = (left+R1)-(int)c.X;
	        int distY = (top+R1)-(int)c.Y;
	        // Get distance with Pythagoras
            int squaredist = (distX * distX) + (distY * distY);
            bool res=squaredist <= (R1 + R2) * (R1 + R2);
            if (res)
            {
                if (margin <= 0) margin = MAX;
                return ortoVector(subPoint(new Point(left + R1, top + R1), c));
            }
            else return new Point(-100, -100);
        }

        private void drawByTag(long tag,double orientation)
        {
            switch (tag)
            {
                case T1A: pT1a=drawTriangle(pT1a,dic[tag].Center,tag,orientation); break;
                case T1A + 1: pT1b = drawSquare(pT1b, dic[tag].Center, SQUARE_SIDE, SQUARE_SIDE, tag, orientation); break;
                case T1A + 2: e1 = drawCircle(e1, dic[tag].Center, tag); break;
                case T2A: pT2a = drawTriangle(pT2a, dic[tag].Center, tag, orientation); break;
                case T2A + 1: pT2b = drawSquare(pT2b, dic[tag].Center, SQUARE_SIDE, SQUARE_SIDE, tag, orientation); break;
                case T2A + 2: e2 = drawCircle(e2, dic[tag].Center, tag); break;
                case P1TAG: p1 = drawSquare(p1, new Point(PADDLE_DISTANCE, dic[tag].Center.Y), BALL_RADIOUS, PADDLE_LENGTH, tag, orientation); break;
                case P1TAG + 1: p2 = drawSquare(p2, new Point(RootGrid.ActualWidth - PADDLE_DISTANCE, dic[tag].Center.Y), BALL_RADIOUS, PADDLE_LENGTH, tag, orientation); break;
            }
        }
        private void eraseShape(Shape s)
        {
            Canv.Children.Remove(s);
            rectas.Remove(s);
        }
        private Polygon drawTriangle(Polygon myPolygon,Point p,long tag,double orientation)
        {
            double x, y;
            x = p.X;
            y = p.Y;
            System.Windows.Point Point1 = new System.Windows.Point(x, y - SQUARE_SIDE / 2);
            System.Windows.Point Point2 = new System.Windows.Point(x - SQUARE_SIDE / 2, y + SQUARE_SIDE/2);
            System.Windows.Point Point3 = new System.Windows.Point(x + SQUARE_SIDE/2, y + SQUARE_SIDE/2);
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(Point1);
            myPointCollection.Add(Point2);
            myPointCollection.Add(Point3);
            Recta[] array={new Recta(Point1, Point2),new Recta(Point3, Point2),new Recta(Point1, Point3)};
            
            if (myPolygon == null)
            {
                myPolygon = new Polygon();
                myPolygon.Stroke = System.Windows.Media.Brushes.Black;
                myPolygon.StrokeThickness = 2;
                myPolygon.Tag = tag;
                if ((tag != P1TAG) && (tag != P1TAG + 1))
                {
                    RotateTransform rotateTransform1 = new RotateTransform(orientation, p.X, p.Y);
                    myPolygon.RenderTransform = rotateTransform1;
                }
                myPolygon.HorizontalAlignment = HorizontalAlignment.Left;
                myPolygon.VerticalAlignment = VerticalAlignment.Top;
                myPolygon.Points = myPointCollection;
                Canv.Children.Add(myPolygon);
            }
            else myPolygon.Points = myPointCollection;
            if (orientation!=0)
            {
                RotateTransform rotateTransform1 = new RotateTransform(orientation, p.X, p.Y);
                myPolygon.RenderTransform = rotateTransform1;
            }
            SolidColorBrush sc = new SolidColorBrush();
            sc.Color = tagColor[tag];
            myPolygon.Fill = sc;
            rectas[myPolygon] = array;
            return myPolygon;
        }
        private Polygon drawSquare(Polygon myPolygon, Point p, int w, int h, long tag, double orientation)
        {
            if (tag == P1TAG||tag==P1TAG+1){
                int i = (int)tag - P1TAG;
                if (paddlePower[i] == PADDLE_FROZEN) return myPolygon;
                else if (paddlePower[i] == PADDLE_ENLARGE) h =(int)(h*1.5);
                else if (paddlePower[i] == PADDLE_SMALLER) h = (int)(h / 1.5);
            }
            double x, y;
            x = p.X;
            y = p.Y;
            System.Windows.Point Point1 = new System.Windows.Point(x - w / 2, y - h / 2);
            System.Windows.Point Point2 = new System.Windows.Point(x + w / 2, y - h / 2);
            System.Windows.Point Point3 = new System.Windows.Point(x + w / 2, y + h / 2);
            System.Windows.Point Point4 = new System.Windows.Point(x - w / 2, y + h / 2);
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(Point1);
            myPointCollection.Add(Point2);
            myPointCollection.Add(Point3);
            myPointCollection.Add(Point4);
            if (myPolygon == null)
            {
                myPolygon = new Polygon();
                myPolygon.Tag = tag;
                myPolygon.Stroke = System.Windows.Media.Brushes.Black;
                myPolygon.StrokeThickness = 2;
                myPolygon.HorizontalAlignment = HorizontalAlignment.Left;
                myPolygon.VerticalAlignment = VerticalAlignment.Top;
                myPolygon.Points = myPointCollection;
            }
            else myPolygon.Points = myPointCollection;
            if (orientation != 0)
            {
                RotateTransform rotateTransform1 = new RotateTransform(orientation, p.X, p.Y);
                myPolygon.RenderTransform = rotateTransform1;
            }
            if ((!Canv.Children.Contains(myPolygon))&&(tag!=0)) Canv.Children.Add(myPolygon);
            SolidColorBrush sc = new SolidColorBrush();
            sc.Color = sc.Color = tagColor[tag];
            myPolygon.Fill = sc;
            Recta[] array = { new Recta(Point1, Point2), new Recta(Point2, Point3), new Recta(Point4, Point3), new Recta(Point4, Point1)};
            rectas[myPolygon] = array;
            return myPolygon;
        }
        private Ellipse drawCircle(Ellipse myEllipse, Point p, long tag)
        {
            double left = p.X - (CIRCLE_RADIOUS);
            double top = p.Y - (CIRCLE_RADIOUS);

            if (myEllipse == null)
            {
                myEllipse = new Ellipse();
                myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
                myEllipse.VerticalAlignment = VerticalAlignment.Top;
                myEllipse.Stroke = System.Windows.Media.Brushes.Black;
                
                myEllipse.Tag = tag;
                myEllipse.Width = CIRCLE_RADIOUS*2;
                myEllipse.Height = CIRCLE_RADIOUS*2;
                
                myEllipse.Margin = new Thickness(left, top, 0, 0);
                Canv.Children.Add(myEllipse);
            }
            else
            {
                left = p.X - (myEllipse.Width / 2);
                top = p.Y - (myEllipse.Height / 2);
                myEllipse.Margin = new Thickness(left, top, 0, 0);
            }
            SolidColorBrush sc = new SolidColorBrush();
            sc.Color = tagColor[tag];
            myEllipse.Fill = sc;
            double Rad = myEllipse.Width / 2;
            Recta[] array = {new Recta(new Point(left + Rad, top + Rad), new Point(Rad, -1))};
            rectas[myEllipse] = array;
            return myEllipse;
        }
        private void OnClick(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b.Tag != null)
            {
                TagData t = (TagData)b.Tag;
                long v = t.Value;
                tagColor[v] = ((SolidColorBrush)b.Background).Color;
                drawByTag(v,0);
            }
         }

        private void OnComparisonCanvasLoaded(object sender, RoutedEventArgs e)
        {
            tagColor[0] = Colors.Transparent;
            screen = drawSquare(screen, new Point(RootGrid.ActualWidth / 2, RootGrid.ActualHeight / 2), (int)RootGrid.ActualWidth, (int)RootGrid.ActualHeight,0,0);
            
            tagColor[P1TAG] = Colors.WhiteSmoke;
            tagColor[P1TAG+1] = Colors.WhiteSmoke;
            TagVisualizerCanvas.SetCenter(Paddle1, new Point(PADDLE_DISTANCE, RootGrid.ActualHeight / 2));
            dic[P1TAG] = Paddle1;
            drawByTag(P1TAG,0);
            TagVisualizerCanvas.SetCenter(Paddle2, new Point(RootGrid.ActualWidth-PADDLE_DISTANCE, RootGrid.ActualHeight/2));
            dic[P1TAG+1] = Paddle2;
            drawByTag(P1TAG+1,0);
            
            double ItemMargin = 300.0;
            double p1X = RootGrid.ActualWidth / 6;
            double p2X = RootGrid.ActualWidth-(RootGrid.ActualWidth / 6);
            double pY = 0.5 * RootGrid.ActualHeight;
            double t1X = RootGrid.ActualWidth / 2 - ItemMargin;
            double t2X = RootGrid.ActualWidth / 2 + ItemMargin;
            double taY = 0.25 * RootGrid.ActualHeight;
            double tbY = 0.5 * RootGrid.ActualHeight;
            double tcY = 0.75 * RootGrid.ActualHeight;

            TagVisualizerCanvas.SetCenter(Paddle1, new Point(p1X,pY));
            TagVisualizerCanvas.SetCenter(Paddle2, new Point(p2X, pY));
            TagVisualizerCanvas.SetCenter(T1a, new Point(t1X, taY));
            TagVisualizerCanvas.SetCenter(T1b, new Point(t1X, tbY));
            TagVisualizerCanvas.SetCenter(T1c, new Point(t1X, tcY));
            TagVisualizerCanvas.SetCenter(T2a, new Point(t2X, taY));
            TagVisualizerCanvas.SetCenter(T2b, new Point(t2X, tbY));
            TagVisualizerCanvas.SetCenter(T2c, new Point(t2X, tcY));

            dic[T1A] = T1a;
            dic[T1A+1] = T1b;
            dic[T1A+2] = T1c;
            dic[T2A] = T2a;
            dic[T2A + 1] = T2b;
            dic[T2A + 2] = T2c;
            for (int i = T1A; i <= T1A + 2; i++)
            {
                tagColor[i] = Colors.Black;
                drawByTag(i, 0);
            }
            for (int i = T2A; i <= T2A + 2; i++)
            {
                tagColor[i] = Colors.Black;
                drawByTag(i, 0);
            }
            SolidColorBrush labColor=new SolidColorBrush();
            labColor.Color=Colors.Yellow;
            SolidColorBrush scoreColor=new SolidColorBrush();
            scoreColor.Color=Colors.Red;
            int scoreW = 55;
            int labW=50;
            int height = 40;
            score1.Content = 0;
            score1.FontSize=28;
            score1.Foreground = labColor;
            score1.Background = scoreColor;
            score1.HorizontalAlignment = HorizontalAlignment.Left;
            score1.VerticalAlignment = VerticalAlignment.Top;
            score1.Margin = new Thickness(0,0, 0, 0);
            score1.Width = scoreW;
            score1.Height = height;
            score2.Content = 0;
            score2.FontSize = 30;
            score2.Foreground = labColor;
            score2.Background = scoreColor;
            score2.HorizontalAlignment = HorizontalAlignment.Left;
            score2.VerticalAlignment = VerticalAlignment.Top;
            score2.Width = scoreW;
            score2.Height = height;
            score2.Margin = new Thickness(RootGrid.ActualWidth - scoreW,0, 0, 0);
            Canv.Children.Add(score1);
            Canv.Children.Add(score2);

            lab1.Content = 0;
            lab1.FontSize = 25;
            lab1.Background = labColor;
            lab1.HorizontalAlignment = HorizontalAlignment.Left;
            lab1.VerticalAlignment = VerticalAlignment.Top;
            lab1.Width = labW;
            lab1.Height = height;
            lab1.Margin = new Thickness(scoreW, 0, 0, 0);
            lab2.Content = 0;
            lab2.FontSize = 25;
            lab2.Background = labColor;
            lab2.Width = labW;
            lab2.Height = height;
            lab2.Margin = new Thickness(RootGrid.ActualWidth - scoreW-labW,0, 0, 0);
            lab2.HorizontalAlignment = HorizontalAlignment.Left;
            lab2.VerticalAlignment = VerticalAlignment.Bottom;
            Canv.Children.Add(lab1);
            Canv.Children.Add(lab2);

            timer = loop(); 
        }

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        //FROM HERE TO BELOW - LIFECYCLE
        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        private void SurfaceWindow_TouchMove(object sender, TouchEventArgs e)
        {
            TouchPoint tp = e.GetTouchPoint(RootGrid);
            if (tp == null) return;
            Point p = tp.Position;
            if (p.X < BORDER_X)
            {
                TagVisualizerCanvas.SetCenter(Paddle1, p);
                dic[P1TAG] = Paddle1;
                drawByTag(P1TAG,0);
            }
            else if (p.X >RootGrid.ActualWidth-BORDER_X)
            {
                TagVisualizerCanvas.SetCenter(Paddle2, p);
                dic[P1TAG + 1] = Paddle2;
                drawByTag(P1TAG + 1,0);
            }
        }

        
    }

    internal class Recta
    {
        private Point a;
        private Point b;
        public Recta(Point a, Point b)
        {
            this.a = a;
            this.b = b;
        }
        public Point A
        {
            get { return a; }
        }
        public Point B
        {
            get { return b; }
        }
    }
}