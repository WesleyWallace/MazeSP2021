﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AlloAudio : MonoBehaviour
{
    //Audio source
    private AudioSource Source;
    //Current sound playing
    private AudioClip Clip;

    //List of AudioClips
    List<AudioClip> Clips = new List<AudioClip>();

    //See if audio is disabled
    private bool disabledAudio = PersistentManager.Instance.disableAudio;

    private int randomNum;

    int counter = 0;

    private int WaitDelay = 2;
    public int MinMoves = 3;
    public int MaxMoves = 5;
    public float timeBetweenQues = 25;

    private int randomFileIndex;

    private int audioFileNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!disabledAudio)
        {
            randomNum = UnityEngine.Random.Range(MinMoves, MaxMoves);

            Source = GetComponent<AudioSource>();
            Clips = Resources.LoadAll<AudioClip>("AlloAudio").ToList();

            GetAudioAnswers();
        }
    }

    // Plays an audio clip based on the current index
    void Play(int index)
    {
        Clip = Clips[index];
        audioFileNumber = int.Parse(Clip.name.Split('_')[1]);
        Source.clip = Clip;
        Source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (PersistentManager.Instance.mazeArrayIndex >= 5 && PersistentManager.Instance.mazeArrayIndex < 10 && !PersistentManager.Instance.isTesting)
        {

            if (timeBetweenQues > 0)
            {
                timeBetweenQues -= Time.deltaTime;
            }
            else
            {
                randomFileIndex = (int)UnityEngine.Random.Range(0, Clips.Count - 1);
                Play(randomFileIndex);
                StartCoroutine(Wait((int)Clips[randomFileIndex].length));
                randomNum = (int)UnityEngine.Random.Range(MinMoves, MaxMoves);
                timeBetweenQues = randomNum;
            }

            if (PersistentManager.Instance.audioWaitActive && !disabledAudio && !PersistentManager.Instance.hasAnswered)
            {
                if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.N))
                {

                    PersistentManager.Instance.hasAnswered = true;
                    string rightAnswer = PersistentManager.Instance.alloAudioAnswers[audioFileNumber - 1];

                    if (Input.GetKeyDown(KeyCode.Y))
                    {
                        PersistentManager.Instance.audioAnswer = "y";
                        if (!rightAnswer.Equals("yes"))
                        {
                            PersistentManager.Instance.totalErrors += 1;
                        }
                        // Add Audio Answer to csv
                        if (PersistentManager.Instance.CurrentScene == 1)
                        {
                            gameObject.GetComponent<InfoTracker>().SetDirection("NULL", "Ego", "y");
                        }
                        else if (PersistentManager.Instance.CurrentScene == 2)
                        {
                            GameObject.Find("Cube").GetComponent<InfoTracker>().SetDirection("NULL", "Allo", "y");
                        }
                        else
                        {
                            Debug.Log("Couldn't write audio answer.");
                        }

                    }

                    else if (Input.GetKeyDown(KeyCode.N))
                    {
                        PersistentManager.Instance.audioAnswer = "n";
                        if (!rightAnswer.Equals("no"))
                        {
                            PersistentManager.Instance.totalErrors += 1;
                        }
                        // Add Audio Answer to csv
                        if (PersistentManager.Instance.CurrentScene == 1)
                        {
                            gameObject.GetComponent<InfoTracker>().SetDirection("NULL", "Ego", "n");
                        }
                        else if (PersistentManager.Instance.CurrentScene == 2)
                        {
                            GameObject.Find("Cube").GetComponent<InfoTracker>().SetDirection("NULL", "Allo", "n");
                        }
                        else
                        {
                            Debug.Log("Couldn't write audio answer.");
                        }

                    }

                }

            }
        }
    }

    // Coroutine to wait for another audio cue to play
    IEnumerator Wait(int time)
    {

        PersistentManager.Instance.audioWaitActive = true;
        yield return new WaitForSeconds(time + WaitDelay);

        if (!PersistentManager.Instance.hasAnswered)
        {
            PersistentManager.Instance.totalErrors += 1;
        }
        else if (PersistentManager.Instance.hasAnswered)
        {
            PersistentManager.Instance.hasAnswered = false;
            PersistentManager.Instance.audioAnswer = "";
        }

        PersistentManager.Instance.audioWaitActive = false;

    }


    public string GetAudioCheck()
    {

        if (PersistentManager.Instance.mazeArrayIndex >= 10)
        {
            return "0";
        }
        else if (PersistentManager.Instance.audioWaitActive)
        {
            Clip = Clips[randomFileIndex];
            string name = Clip.name;
            name = name.Split('_', '.')[1];

            return name;
        }
        else
        {
            return "0";
        }

    }

    // Reads in the answer key to the audio cues
    private void GetAudioAnswers()
    {
        try
        {
            TextAsset SourceFile = (TextAsset)Resources.Load("AudioAnswers/AlloAnswers", typeof(TextAsset));
            string text = SourceFile.text;
            string[] lines = text.Split('\n');

            for (int i = 0; i < lines.Length; i++)  
            {
                PersistentManager.Instance.alloAudioAnswers.Add(lines[i].Split('\t')[1].Trim());
            }
        }
        catch (Exception e) 
        {
            Debug.LogException(e, this);
        }

    }

}
