﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class IntentDetector
{
    public Intents intentsFromJson;
    // Start is called before the first frame update
    public IntentDetector(TextAsset jsonFile)
    {
       intentsFromJson = JsonUtility.FromJson<Intents>(jsonFile.text); 
       foreach (Intent intent in intentsFromJson.intents)
       {
          Debug.Log(intent.intentName); 
       }
    }

    public Intent GetIntent(string dictationResult)
    {
        Debug.Log(dictationResult);
        Intent activeIntent = null;
        foreach (Intent intent in intentsFromJson.intents)
        {
            var intentPhrases = intent.wordGroups;
            foreach (string phrase in intentPhrases)
            {
                var phraseArr = phrase.Split(' ');
                var dictationArr = dictationResult.Split(' ');
                if(isSubset(dictationArr, phraseArr))
                {
                    activeIntent = intent;
                    break;
                }
            }
            if(activeIntent != null) break;
        }
        Debug.Log("intent detector");
        Debug.Log(activeIntent);
        return activeIntent;
    }

    private bool isSubset(string[] firstArr, string[] secondArr)
    {
        return !secondArr.Except(firstArr).Any();
    }
}
