﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Class to switch scenes and update state in persistent manager
public class SceneManagerScript : MonoBehaviour
{
    public int CurrentScene;
    private int practiceAttempts;
    private int trialAttempts;

    //Time vars for time spent in transition
    public DateTime startTime;


    public void Start()
    {
        CurrentScene = PersistentManager.Instance.CurrentScene;
        practiceAttempts = PersistentManager.Instance.numPracticeAttempts;
        trialAttempts = PersistentManager.Instance.numTestingAttempts;
    }


    // Set MazeType
    public void GetConstraints()
    {
        if (PersistentManager.Instance.egoCentric && !PersistentManager.Instance.alloCentric)
        {
            PersistentManager.Instance.MazeType = 0;
        }
        else if (!PersistentManager.Instance.egoCentric && PersistentManager.Instance.alloCentric)
        {
            PersistentManager.Instance.MazeType = 1;
        }
        else if (PersistentManager.Instance.egoCentric && PersistentManager.Instance.alloCentric)
        {
            PersistentManager.Instance.MazeType = 2;
        }
    }


    // Change scene and update PersistentManager
    public void ChangeScene(int scene)
    {
        // Update PersistentManager vars
        // Only update previous to the current if it is a maze scene - needed for double transition with timeout
        if (PersistentManager.Instance.CurrentScene == 1 || PersistentManager.Instance.CurrentScene == 2)
        {
            PersistentManager.Instance.PreviousScene = PersistentManager.Instance.CurrentScene;
        }
        PersistentManager.Instance.CurrentScene = scene;

        // Load scene
        SceneManager.LoadScene(scene);

        //Make sure audio is set back to false
        PersistentManager.Instance.hasAnswered = false;
        PersistentManager.Instance.audioWaitActive = false;
    }


    // Set timer and PersisistentManager vars for timeout tracking
    public void SetTimer()
    {
        // Check current time and add constraint given timeout onto it 
        System.DateTime now = System.DateTime.Now;
        double timeout = System.Convert.ToDouble(PersistentManager.Instance.timeOut);
        System.DateTime tmp = now.AddMinutes(timeout);

        // Update var for later checking
        PersistentManager.Instance.endTime = tmp;
    }


    // 0 Main Menu // 1 Ego // 2 Alo // 3 Maze Attempt // 4 Maze Complete // 5 Survey // 6 Time Out // 7 Begin (After Practice) // 8 Trial Begin (After Training) // 9 Trial Second Screen // 10 Break


    // Change scene from Main Menu
    public void ChangeFromMainMenu()
    {
        SetTimer();

        // Move to Ego Practice if Ego or Both
        if (PersistentManager.Instance.MazeType == 0 || PersistentManager.Instance.MazeType == 2)
        {
            ChangeScene(1);
        }
        // Move to Allo Practice otherwise
        else
        {
            ChangeScene(2);
        }

    }

    // Change scene from a Maze Scene (Ego or Allo)
    public void ChangeFromMazeScene()
    {

        // Boolean condiitions represented end maze status 
        bool attemptsReached = PersistentManager.Instance.currentAttempts == PersistentManager.Instance.numAttempts;
        bool practiceAttemptsReached = PersistentManager.Instance.currentAttempts == practiceAttempts;
        bool trialAttemptsReached = PersistentManager.Instance.currentAttempts == trialAttempts;
        bool perfectsReached = PersistentManager.Instance.perfectRuns == PersistentManager.Instance.numSuccessfulAttempts;

        // Check for practice, training, or testing.

        // Practice Phase
        if (PersistentManager.Instance.isPractice)
        {

            // Check for conditions
            if (practiceAttemptsReached)
            {

                // Generate new maze colors
                GameObject.Find("Main Camera").GetComponent<MazeBuilder>().GenerateSectorColors();

                // Switch to 
                if (PersistentManager.Instance.MazeType == 2 && PersistentManager.Instance.CurrentScene == 1)
                {
                    ChangeScene(4);
                } else
                {
                    ChangeScene(7);
                    PersistentManager.Instance.isPractice = false;
                    PersistentManager.Instance.isTraining = true;
                }
                
            }
            else
            {
                // Change to Maze Attempt - Conditions not met, set player to current maze start
                ChangeScene(3);
            }

        }
        // Training Phase
        else if (PersistentManager.Instance.isTraining)
        {
            // Check for conditions
            if (attemptsReached || perfectsReached)
            {
                ChangeScene(8);
                PersistentManager.Instance.isTraining = false;
                PersistentManager.Instance.isTesting = true;
            }
            else
            {
                // Change to Maze Attempt - Conditions not met, set player to current maze start
                ChangeScene(3);
                // Start timespan recording
                PersistentManager.Instance.startSpanTime = DateTime.Now;
            }
        }
        // Testing Phase
        else if (PersistentManager.Instance.isTesting)
        {
            // Check for conditions
            if (trialAttemptsReached)
            {
                // Generate new maze colors
                GameObject.Find("Main Camera").GetComponent<MazeBuilder>().GenerateSectorColors();

                ChangeScene(4);
                PersistentManager.Instance.isTesting = false;
                PersistentManager.Instance.isTraining = true;
            }
            else
            {
                // Change to Maze Attempt - Conditions not met, set player to current maze start
                ChangeScene(9);
            }
        }


    }



    // CHANGE FROM TRANSITIONS -------------------------------------------------------------------------------------------------------------------------------------------------



    // Change scene from Transition (Let's Begin)
    public void ChangeFromInitialBegin()
    {
        SetTimer();

        // Reset current attempts and perfect runs
        PersistentManager.Instance.currentAttempts = 0;
        PersistentManager.Instance.perfectRuns = 0;

        // Check previous scene
        if (PersistentManager.Instance.MazeType == 2)
        {

            // Switch to alternate scene
            if (PersistentManager.Instance.PreviousScene == 1)
            {
                ChangeScene(2);
            }
            else
            {
                ChangeScene(1);
            }

        }
        else
        {
            ChangeScene(PersistentManager.Instance.PreviousScene);
        }

    }



    // Change scene from Transition (Attempt)
    public void ChangeFromMazeAttempt()
    {
        // Only care for timeouts in training
        if (PersistentManager.Instance.isTraining)
        {
            // Get time spent in attempt transition screen
            TimeSpan transitionTime = DateTime.Now.Subtract(PersistentManager.Instance.startSpanTime);
            // Add it to the end time
            PersistentManager.Instance.endTime = PersistentManager.Instance.endTime.Add(transitionTime);
        }
        
        ChangeScene(PersistentManager.Instance.PreviousScene);
    }



    // Change scene from Transition (Trial Begin)
    public void ChangeFromTrialBegin()
    {
        SetTimer();

        // Reset current attempts and perfect runs
        PersistentManager.Instance.currentAttempts = 0;
        PersistentManager.Instance.perfectRuns = 0;

        ChangeScene(PersistentManager.Instance.PreviousScene);
        

    }



    // Change scene from Transition (Maze Complete)
    public void ChangeFromMazeComplete()
    {
        SetTimer();

        // Reset current attempts and perfect runs
        PersistentManager.Instance.currentAttempts = 0;
        PersistentManager.Instance.perfectRuns = 0;

        // Get current maze from list
        int currentIndex = PersistentManager.Instance.mazeArrayIndex;

        // Check previous scene
        if (PersistentManager.Instance.MazeType == 2)
        {

            if (currentIndex == (PersistentManager.Instance.mazeSize - 1) && PersistentManager.Instance.PreviousScene == 2)
            {
                // Change to end survey scene
                ChangeScene(5);
            }
            // Switch to alternate maze scene
            else if (PersistentManager.Instance.PreviousScene == 1)
            {
                ChangeScene(2);
            }
            else
            {
                // Switch to break scene every 5 mazes and not already on break scene
                if ((PersistentManager.Instance.mazeArrayIndex == 4 || PersistentManager.Instance.mazeArrayIndex == 9) && PersistentManager.Instance.CurrentScene != 10)
                {
                    ChangeScene(10);
                } else
                {
                    // Move to next maze and switch to alternate maze scene
                    currentIndex++;
                    PersistentManager.Instance.mazeArrayIndex = currentIndex;
                
                    ChangeScene(1);
                }
                

            }

        }
        else
        {

            // Switch to survey screen
            if (currentIndex == (PersistentManager.Instance.mazeSize - 1))
            {
                // Change to end survey scene
                ChangeScene(5);
            }
            // Switch to break scene every 5 mazes and not already on break scene
            else if ((PersistentManager.Instance.mazeArrayIndex == 4 || PersistentManager.Instance.mazeArrayIndex == 9) && PersistentManager.Instance.CurrentScene != 10)
            {
                ChangeScene(10);
            }
            // Change maze and switch back to maze scene
            else
            {
                currentIndex++;
                PersistentManager.Instance.mazeArrayIndex = currentIndex;
                ChangeScene(PersistentManager.Instance.PreviousScene);
            }


            
        }

    }



    // Change scene from Transition (Time Out)
    public void ChangeFromTimeOut()
    {
        SetTimer();

        // Reset current attempts and perfect runs
        PersistentManager.Instance.currentAttempts = 0;
        PersistentManager.Instance.perfectRuns = 0;

        // Change to test mode
        PersistentManager.Instance.isPractice = false;
        PersistentManager.Instance.isTraining = false;
        PersistentManager.Instance.isTesting = true;

        ChangeScene(8);

    }

}
