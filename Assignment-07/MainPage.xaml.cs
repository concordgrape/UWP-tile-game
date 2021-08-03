/*
* FILE : MainPage.xaml.cs
* PROJECT : PROG2121 - Assignment 07
* PROGRAMMER : Sky Roth
* FIRST VERSION: 2020 - 01 - 16
* DESCRIPTION : This program will be a 4x4 puzzle game, it will perform the game logic, keep a scoreboard, and ensure the settings
*       are saved when the program is suspended.
*/






using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using System.Diagnostics;
using System.Timers;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.ApplicationModel.Activation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Assignment_07
{
    public sealed partial class MainPage : Page
    {
        private static Dictionary<int, int[]> solvable = new Dictionary<int, int[]>();
        private static int totalClicks = 0;
        private static DispatcherTimer dt = new DispatcherTimer();

        private static int increment = 0;
        private static int minuteIncrement = 0;
        private static int totalPlayers = 0;
        private static string playerName = "Player";

        private static List<int> solve = new List<int>();
        private static Grid updated = new Grid();

        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;





        /*
            * FUNCTION : MainPage
            *
            * DESCRIPTION : This will act as the constructor for the main page, it will init the solvable patterns and boxes
            * PARAMETERS : void
            * RETURNS : void
        */
        public MainPage()
        {
            this.InitializeComponent();

            //this will also stop the app from launching in full screen mode
            // ApplicationView.PreferredLaunchViewSize = new Size(790, 570);

            //set the handlers for when the program is suspended or resuming
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;

            //initalize the puzzle and boxes
            Init_Solvable();
            InitBoxes();
        }








        /*
            * FUNCTION : Init_Solvable
            *
            * DESCRIPTION : This will possible puzzles that will display to the user, these are all solvable
            * PARAMETERS : void
            * RETURNS : void
        */
        private static void Init_Solvable()
        {
            //only add the puzzles to the dictionary if its empty
            if (solvable.Count == 0)
            {
                //this will clear the solve list every time and add itself to the dictionary
                solve = new List<int> { 15, 8, 7, 14, 6, 0, 3, 13, 1, 5, 10, 4, 9, 2, 11, 12 };
                solvable.Add(1, solve.ToArray());

                solve = new List<int> { 7, 5, 8, 2, 9, 1, 14, 15, 13, 6, 3, 10, 12, 11, 0, 4 };
                solvable.Add(2, solve.ToArray());

                solve = new List<int> { 1, 7, 8, 3, 5, 4, 10, 0, 9, 6, 15, 2, 13, 12, 11, 14 };
                solvable.Add(3, solve.ToArray());

                solve = new List<int> { 2, 0, 8, 4, 1, 3, 9, 12, 7, 5, 15, 11, 13, 6, 14, 10 };
                solvable.Add(4, solve.ToArray());

                solve = new List<int> { 5, 1, 10, 3, 2, 9, 6, 4, 13, 0, 7, 8, 14, 11, 12, 15 };
                solvable.Add(5, solve.ToArray()); 
            }
        }





        /*
            * FUNCTION : InitBoxes
            *
            * DESCRIPTION : This function will initalize the boxes when the program is first opened
            *
            * PARAMETERS : void
            * RETURNS : void
        */
        private void InitBoxes()
        {
            Random rand = new Random();

            //choose a random puzzle
            int index = rand.Next(solvable.Count + 1);
            int counter = 0;

            if (index == 0)
            {
                index++;
            }

            //determine which puzzle to show first
            int[] puzz = solvable[index];

            //set the puzzle in the grid
            foreach (Button btn in puzzle_game.Children)
            {
                if (counter >= puzz.Length)
                { break; }

                if (puzz[counter] == 0)
                {
                    int row = Grid.GetRow(this.btn_E);
                    int col = Grid.GetColumn(this.btn_E);

                    Grid.SetColumn(btn_E, Grid.GetColumn(btn));
                    Grid.SetRow(btn_E, Grid.GetRow(btn));

                    Grid.SetColumn(btn, col);
                    Grid.SetRow(btn, row);
                    btn.Content = puzz[++counter].ToString();
                    counter++;
                    continue;
                }
                btn.Content = puzz[counter].ToString();
                counter++;
            }
            //this button will be the "invisible" button, nothing will happen if its clicked
            btn_E.Content = "";

        }






        /*
            * FUNCTION : CheckWinner
            *
            * DESCRIPTION : This function will check if the user won and if the boxes are in the correct locations
            *
            * PARAMETERS : void
            * RETURNS : void
        */
        private void CheckWinner()
        {
            int match = 0;

             foreach (Button btn in puzzle_game.Children)
            {
                int row = Grid.GetRow(btn);
                int col = Grid.GetColumn(btn);

                //c will hold the correct value of the cell in  the grid
                int c = ((row * 4) + 1) + (col + 1) - 1;

                //compare the correct value to the current cell that is currently in the grid
                if (btn.Content.ToString() == c.ToString())
                {
                    match++;
                }
            }

            if (totalPlayers < 10)
            {
                if (match == 15)
                {
                    totalPlayers++;
                    dt.Stop();
                    scoreBoard.Text += "\n" + totalPlayers.ToString() + ". " + playerName + "\t\t" + totalClicksLbl.Text + "\t" + timerLbl.Text;
                    totalClicks = 0;
                    winnerPromptLbl.Visibility = Visibility.Visible;
                }
            }

        }








        /*
            * FUNCTION : box_Click
            *
            * DESCRIPTION : This function will be called whenever someone clicks one of the puzzle buttons
            * PARAMETERS : 
            *   object sender   :   Reference to the control that called it
            *   EventArgs e     :   Contains the event data
            * RETURNS : void
        */
        private void box_Click(object sender, RoutedEventArgs e)
        {
            //check if the timer is null, create it if it is
            if (dt == null)
            {
                dt = new DispatcherTimer();
            }

            //check if the timer is enabled before continuing
            if (increment == 0 && !dt.IsEnabled)
            {
                dt.Interval = TimeSpan.FromSeconds(1);
                dt.Tick += dtTicker;
                dt.Start();
            }

            //get the sender (determine which button was clicked as all of the buttons are sent to this function)
            var btn = sender as Button;

            //if the blank box is clicked, don't increase the click counter and return
            if (btn.Name == btn_E.Name)
            { return; }

            var row = Grid.GetRow(btn);
            var column = Grid.GetColumn(btn);

            var empty_row = Grid.GetRow(btn_E);
            var empty_column = Grid.GetColumn(btn_E);

            if ((row + 1 == empty_row || row - 1 == empty_row) && column == empty_column)
            {
                Grid.SetRow(btn, empty_row);
                Grid.SetRow(btn_E, row);

            }
            else if ((column + 1 == empty_column || column - 1 == empty_column) && row == empty_row)
            {
                Grid.SetColumn(btn, empty_column);
                Grid.SetColumn(btn_E, column);
            }
           
            //when the box is clicked, check if the user won
            CheckWinner();

            totalClicks += 1;
            totalClicksLbl.Text = totalClicks.ToString();
        }









        /*
            * FUNCTION : shuffleBtn_Click
            *
            * DESCRIPTION : This will shuffle the board and pause the timer
            * PARAMETERS : 
            *   object sender         :   Reference to the control that called it
            *   RoutedEventArgs e     :   Contains the event data
            * RETURNS : void
        */
        private void shuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            //hide the continue or restart prompt
            ReturnPromptLbl.Visibility = Visibility.Collapsed;
            Continue.Visibility = Visibility.Collapsed;
            Restart.Visibility = Visibility.Collapsed;

            //hide the winning message
            winnerPromptLbl.Visibility = Visibility.Collapsed;

            //reset the increment and total clicks (will restart the labels as well)
            increment = 0;
            totalClicks = 0;
            dt = null;
            Random rand = new Random();

            //Update the labels
            totalClicksLbl.Text = "0";
            timerLbl.Text = "0:00";

            //choose a random puzzle
            int index = rand.Next(solvable.Count + 1);
            int counter = 0;

            if (index == 0)
            {
                index++;
            }

            int[] puzz = solvable[index];

            foreach (Button btn in puzzle_game.Children)
            {
                if (counter >= puzz.Length)
                { break; }

                if (puzz[counter] == 0)
                {
                    int row = Grid.GetRow(btn_E);
                    int col = Grid.GetColumn(btn_E);

                    Grid.SetColumn(btn_E, Grid.GetColumn(btn));
                    Grid.SetRow(btn_E, Grid.GetRow(btn));

                    Grid.SetColumn(btn, col);
                    Grid.SetRow(btn, row);

                    btn.Content = puzz[++counter].ToString();
                    counter++;
                    continue;
                }
                btn.Content = puzz[counter].ToString();
                counter++;
            }
            btn_E.Content = "";
        }

















        /*
            * FUNCTION : dtTicker
            *
            * DESCRIPTION : This will increase the timer that is used for the games
            * PARAMETERS : 
            *   object sender   :   Reference to the control that called it
            *   object e        :   Reference to the event handler
            * RETURNS : void
        */
        private void dtTicker(object sender, object e)
        {
            increment++;

            //determine how to display the time depending on the second and minute values
            if (increment < 10)
            {
                timerLbl.Text = minuteIncrement.ToString() + ":0" + increment.ToString();
                return;
            }
            else if (increment >= 60)
            {
                increment = 0;
                minuteIncrement++;
            }

            timerLbl.Text = minuteIncrement.ToString() + ":" + increment.ToString();
        }









        /*
            * FUNCTION : nameBtn_Click
            *
            * DESCRIPTION : This will the name of the user, this will be used for the scoreboard when they won
            * PARAMETERS : 
            *   object sender         :   Reference to the control that called it
            *   RoutedEventArgs e     :   Contains the event data with a routed event
            * RETURNS : void
        */
        private void nameBtn_Click(object sender, RoutedEventArgs e)
        {
            //if the name is empty when the user won, display the default name from above ("Player")
            if (enterNameTxtBox.Text != "")
            {
                if (enterNameTxtBox.Text.Length >= 5)
                {
                    playerName = enterNameTxtBox.Text.Substring(0, 5) + "...";
                }
                else
                {
                    playerName = enterNameTxtBox.Text;
                }
                playerNamePromptTxt.Text = "Name set!";
            }
            else
            {
                playerNamePromptTxt.Text = "Please enter a name!";
            }
        }









        /*
            * FUNCTION : Current_Resuming
            *
            * DESCRIPTION : This will be called when the program is resumed from a suspended state
            * PARAMETERS : 
            *   object sender   :   Reference to the control that called it
            *   object e        :   Reference to the event handler
            * RETURNS : void
        */
        private void Current_Resuming(object sender, object e)
        {
            Int32.TryParse(localSettings.Values["increment"].ToString(), out increment);
            Int32.TryParse(localSettings.Values["minuteIncrement"].ToString(), out minuteIncrement);

            this.Name = localSettings.Values["leaderboard"].ToString();

            Int32.TryParse(localSettings.Values["totalPlayers"].ToString(), out totalPlayers);

            //display the message and buttons asking if the user would like to start over
            ReturnPromptLbl.Visibility = Visibility.Visible;
            Continue.Visibility = Visibility.Visible;
            Restart.Visibility = Visibility.Visible;

            //restart the timer so that 'increment' and 'minuteIncrement' can be called (this will start the timer with the valu
            dt.Stop();
            dt = null;

            if (dt == null)
            {
                dt = new DispatcherTimer();
            }

            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += dtTicker;
            dt.Start();
        }








        /*
            * FUNCTION : Current_Suspending
            *
            * DESCRIPTION : This will be called when the program is called from being suspended
            * PARAMETERS : 
            *   object sender                :   Reference to the control that called it
            *   SuspendingEventArgs e        :   Provides data for an app suspension
            * RETURNS : void
        */
        private void Current_Suspending(object sender, SuspendingEventArgs e)
        {
            localSettings = ApplicationData.Current.LocalSettings;

            localSettings.Values["increment"] = increment;
            localSettings.Values["minuteIncrement"] = minuteIncrement;
            localSettings.Values["leaderboard"] = this.Name;

            localSettings.Values["totalPlayers"] = totalPlayers.ToString();
        }








        /*
            * FUNCTION : Continue_Click
            *
            * DESCRIPTION : This button will be displayed when the user returns from a suspended state, if it's clicked then the prompt and buttons will disappear
            * PARAMETERS : 
            *   object sender                :   Reference to the control that called it
            *   RoutedEventArgs e            :   Reference to the event handler that it's associated with
            * RETURNS : void
        */
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            ReturnPromptLbl.Visibility = Visibility.Collapsed;
            Continue.Visibility = Visibility.Collapsed;
            Restart.Visibility = Visibility.Collapsed;
        }

    }
}
