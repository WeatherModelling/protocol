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
        GNUPlotter chartPainter;

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

        const int maxFPS = 20;

        private void CalculationThread()
        {
            DoCalculations = true;
            int lastTimeStepsPerFrame = 1; // an estimate
            while (DoCalculations)
            {
                int timeStepsPerFrame = 0;
                DateTime lastFrameTime = DateTime.Now;
                // надеемся, что вычисление займёт время, примерно равное времени отрисовки кадра
                // если это время не прошло, повторяем
                do
                {
                    CurrentProblem.SolverConnector.Evolve(lastTimeStepsPerFrame);
                    timeStepsPerFrame += lastTimeStepsPerFrame;
                } while ((DateTime.Now - lastFrameTime).TotalSeconds < 1.0 / maxFPS);
                // оцениваем скорость счёта
                double calculationTime = (DateTime.Now - lastFrameTime).TotalSeconds;
                double stepsPerSecond = timeStepsPerFrame / calculationTime;
                // даём новую оценку в шагах за кадр
                lastTimeStepsPerFrame = (int)(stepsPerSecond / maxFPS);
                if (lastTimeStepsPerFrame < 1)
                {
                    // we have to do at least one step
                    lastTimeStepsPerFrame = 1;
                }

                string res = CurrentProblem.SolverConnector.GetResults();
                chartPainter.Update(res);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
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
            if (CurrentProblem.SolverConnector.Dynamic)
            {
                state = State.Completed;
            }


            chartPainter = new GNUPlotter(
                new GUI.GNUPlot.LineSeries(
                    CurrentProblem.json["views"][0] as JObject,
                    CurrentProblem.SolverConnector.OutputConfiguration
                )
            );
            // bind charting model to view
            ChartsContainer.DataContext = chartPainter;


            // get calculation results for stationary model or initial fields for dynamic model
            // paint result charts
            chartPainter.Update(
                CurrentProblem.SolverConnector.GetResults()
            );

            // switch to results tab
            ResultsTab.IsSelected = true;
            // enable dynamic model controls 
            DynamicCalculatorControls.Visibility =
                CurrentProblem.SolverConnector.Dynamic ? Visibility.Visible : Visibility.Hidden;


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
