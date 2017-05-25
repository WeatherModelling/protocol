using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

namespace GUI
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum State
        {
            TaskNotChosen,
            TaskChosen,
            TaskConfigured,
            Started,
            Completed
        }

        // current window state
        State state = State.TaskNotChosen;

        // application working directory
        public static readonly string WorkingDir = @"C:/Users/user/Desktop/protocol/workingDirectory/";

        // problem descriptions list
        private readonly List<Problem> problems;



        // object responsible for chart painting
        List<GNUPlotter> chartPainters;



        public MainWindow()
        {
            InitializeComponent();
            // Register available solver connectors
            SolverConnector.RegisterSolverConnectorType("local", typeof(LocalSolverConnector));

            // Populate problems list
            problems = Problem.ReadRirectory(WorkingDir + "/problems");
            ProblemsListBox.ItemsSource = problems;

        }

        // 
        private Problem currentProblem;
        internal Problem CurrentProblem
        {
            get => currentProblem;
            private set
            {
                if (currentProblem?.SolverConnector?.Connected ?? false)
                {
                    currentProblem.SolverConnector.Disconnect();
                }
                currentProblem = value;
                // Create appropriate solver
                state = State.TaskChosen;
            }
        }

        #region Dynamic system evolution
        Thread calculationThread;
        bool DoCalculations;
        // Current Calculation Speed
        public int LimitStepsPerSecond { get; private set; }

        const int maxFPS = 5;

        private void CalculationThread()
        {
            bool limitStepsPerFrame = LimitStepsPerSecond > 0;
            int maxStepsPerFrame=1;
            int desiredFPS = maxFPS;
            if (limitStepsPerFrame)
            {
                int maxStepsPerSecond = LimitStepsPerSecond;
                if (maxStepsPerSecond < maxFPS)
                {
                    desiredFPS = maxStepsPerSecond;
                    maxStepsPerFrame = 1;
                }
                else
                {
                    desiredFPS = maxFPS;
                    maxStepsPerFrame = maxStepsPerSecond / desiredFPS;
                }
            }


            DoCalculations = true;
            // оценка скорости счёта
            int lastTimeStepsPerFrame = 1; 
            while (DoCalculations)
            {
                // пытаемся увеличить количество шагов по времени так, 
                // чтобы они выполнялись за время 1/maxFPS
                
                // счётчик шагов и времени на этом кадре
                int timeStepsPerFrame = 0;
                DateTime frameStart = DateTime.Now;

                double framePeroid = 1.0 / desiredFPS;
                // за начальное количество шагов берём оценку для предыдущего шага
                int stepsEstimate = lastTimeStepsPerFrame;
                double period = 0;
                while (true)
                {
                    // надеемся, что вычисление займёт время framePeriod
                    CurrentProblem.SolverConnector.Evolve(stepsEstimate);
                    timeStepsPerFrame += stepsEstimate;
                    period = (DateTime.Now - frameStart).TotalSeconds;
                    // если сосчитали кадр до конца -- выходим
                    if (timeStepsPerFrame == maxStepsPerFrame)  
                    {
                        break;
                    }
                    // если закончилось время - выходим
                    if (period>=framePeroid)
                    {
                        break;
                    }
                    // увеличиваем количество шагов для следующего запуска evolve
                    stepsEstimate = stepsEstimate*5/4;
                    // проверяем, чтобы итоговое количество шагов не превысило ограничение
                    if (stepsEstimate+timeStepsPerFrame > maxStepsPerFrame)
                    {
                        // делаем оставшиеся на этом кадре шаги 
                        stepsEstimate = maxStepsPerFrame - timeStepsPerFrame;
                    }
                }
                // если после счёта осталось время - спим
                if (framePeroid > period)
                {
                    Thread.Sleep((int)(1000 * (framePeroid - period)));
                }

                lastTimeStepsPerFrame = timeStepsPerFrame;

                // забираем результаты и рисуем их
                string res = CurrentProblem.SolverConnector.GetResults();
                UpdateChartPainters(res);
            }
        }

        private void UpdateChartPainters(string res)
        {
            foreach (var chartPainter in chartPainters)
            {
                chartPainter.Update(res);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            LimitStepsPerSecond = (int)Math.Pow(10, StepsPerSecondSlider.Value);
            calculationThread = new Thread(CalculationThread);
            calculationThread.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            DoCalculations = false;
        }
        #endregion


        private void Select_Click(object sender, RoutedEventArgs e)
        {
            state = State.TaskConfigured;
            // initialize calculation
            CurrentProblem.SolverConnector.InitializeSolver();
            // calculation is completed right after initialization for stationary systems
            if (!CurrentProblem.SolverConnector.Dynamic)
            {
                state = State.Completed;
            }




            ChartsContainer.Children.Clear();
           
            var z = FactorizeCount(CurrentProblem.json["views"].Count());
            ChartsContainer.Rows = z.Item1;
            ChartsContainer.Columns = z.Item2;

            chartPainters = new List<GNUPlotter>();

            foreach (JObject view in CurrentProblem.json["views"])
            {
                GNUPlotter chartPainter = new GNUPlotter(
                    new GUI.GNUPlot.LineSeries(
                        view,
                        CurrentProblem.SolverConnector.OutputConfiguration
                    )
                );



                Image im = new Image();
                im.SetBinding(
                    Image.SourceProperty,
                    new Binding()
                    {
                        Path = new PropertyPath("Image"),
                        Mode= BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Source = chartPainter
                    }
                );
                im.DataContext = chartPainter;
                chartPainters.Add(chartPainter);
                ChartsContainer.Children.Add(im);
                
            }



            // get calculation results for stationary model or initial fields for dynamic model
            // paint result charts
            UpdateChartPainters(CurrentProblem.SolverConnector.GetResults());

            // switch to results tab
            ResultsTab.IsSelected = true;
            // enable dynamic model controls 
            DynamicCalculatorControls.Visibility =
                CurrentProblem.SolverConnector.Dynamic ? Visibility.Visible : Visibility.Hidden;
        }

        private Tuple<int,int> FactorizeCount(int count)
        {
            int rows, cols;
            switch (count)
            {
                case 1:
                    rows = 1;
                    cols = 1;
                    break;
                case 2:
                    rows = 1;
                    cols = 2;
                    break;
                case 3:
                    rows = 1;
                    cols = 3;
                    break;
                case 4:
                    rows = 2;
                    cols = 2;
                    break;
                case 5:
                    rows = 2;
                    cols = 3;
                    break;
                case 6:
                    rows = 2;
                    cols = 3;
                    break;
                default:
                    rows = (int)Math.Ceiling(Math.Sqrt(count));
                    cols = rows;
                    break;
            }
            return new Tuple<int, int>(rows, cols);
        }

        private void ProblemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Problem)ProblemsListBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите задачу из списка");
            }

            // initialize current problem
            CurrentProblem = (Problem)ProblemsListBox.SelectedValue;
            // connect to solver
            CurrentProblem.SolverConnector.Connect();
            // now ready to set initial values
            state = State.TaskChosen;

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CurrentProblem?.SolverConnector?.Disconnect();
        }
    }


}
