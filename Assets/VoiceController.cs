using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class VoiceController : MonoBehaviour
{
    [SerializeField]
    private TextAsset intentsJson;

    [SerializeField]
    private string[] trigger_words = { "nova" };

    [SerializeField]
    private string[] affirm_words = { "yes", "confirm" };
    [SerializeField]
    private string[] cancel_words = { "no", "cancel" };

    private string[] confirm_words;

    [SerializeField]
    private Text status;

    [SerializeField]
    private Text confirmation;


    private KeywordRecognizer trigger_recognizer;
    private KeywordRecognizer confirm_recognizer;

    private DictationRecognizer m_DictationRecognizer;

    [SerializeField]
    private Text m_Recognitions;

    private IntentDetector intentDetector;
    private IntentExecutor executor;

    void Start()
    {

        intentDetector = new IntentDetector(intentsJson);
        int confirm_arr_length = affirm_words.Length + cancel_words.Length;
        confirm_words = new string[confirm_arr_length];
        affirm_words.CopyTo(confirm_words, 0);
        cancel_words.CopyTo(confirm_words, affirm_words.Length);

        ClearVoiceUI();

        trigger_recognizer = new KeywordRecognizer(trigger_words);
        trigger_recognizer.OnPhraseRecognized += GetDictation;
        trigger_recognizer.Start();

        confirm_recognizer = new KeywordRecognizer(confirm_words);
        confirm_recognizer.OnPhraseRecognized += GetConfirmation;

        m_DictationRecognizer = new DictationRecognizer();
        m_DictationRecognizer.AutoSilenceTimeoutSeconds = 2;

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.LogFormat("Dictation result: {0}", text);
            m_Recognitions.color = Color.green;
            m_Recognitions.fontSize = 18;
            m_Recognitions.fontStyle = FontStyle.Bold;
            m_Recognitions.text = text;
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            Debug.LogFormat("Dictation hypothesis: {0}", text);
            m_Recognitions.color = Color.yellow;
            m_Recognitions.fontSize = 16;
            m_Recognitions.fontStyle = FontStyle.Normal;

            m_Recognitions.text = text;
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);

            Debug.Log("Dictation Complete due to " + completionCause.ToString());

            executor = intentDetector.GetIntent(m_Recognitions.text);
            Debug.Log(executor);

            StartConfirm();

        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };
    }

    public void StartDictation()
    {
        // Shutdown the PhraseRecognitionSystem. This controls the KeywordRecognizers.
        if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            PhraseRecognitionSystem.Shutdown();
            trigger_recognizer.Stop();
        }

        StartCoroutine(StartDictationWhenPossible());
    }

    public void StartKeyword()
    {
       // Shutdown the PhraseRecognitionSystem. This controls the KeywordRecognizers.
       if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
       {
            PhraseRecognitionSystem.Shutdown();
            confirm_recognizer.Stop();
        }

         
        StartCoroutine(StartKeywordWhenPossible());
    }

    public void StartConfirm()
    {
        // Shutdown the PhraseRecognitionSystem. This controls the KeywordRecognizers.
        if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            PhraseRecognitionSystem.Shutdown();
            m_DictationRecognizer.Stop();
        }


        StartCoroutine(StartConfirmWhenPossible());
    }



    private IEnumerator StartDictationWhenPossible()
    {
        while (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }

        m_DictationRecognizer.Start();
    }

    private IEnumerator StartKeywordWhenPossible()
    {
        while (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }
        Debug.Log("Starting keyword");
        ClearVoiceUI();
        trigger_recognizer.Start();
    }

    private IEnumerator StartConfirmWhenPossible()
    {
        while (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }
        Debug.Log("Starting confirm");
        confirmation.text = "Confirm?";
        confirm_recognizer.Start();
    }


    private void ClearVoiceUI()
    {
        status.text = "Waiting...";
        status.color = Color.green;

        m_Recognitions.color = Color.yellow;
        m_Recognitions.fontSize = 16;
        m_Recognitions.fontStyle = FontStyle.Normal;

        m_Recognitions.text = "";

        confirmation.text = "";

    }

    private void GetDictation(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        builder.AppendFormat("\tTimestamp: {0}{1}", args.phraseStartTime, Environment.NewLine);
        builder.AppendFormat("\tDuration: {0} seconds{1}", args.phraseDuration.TotalSeconds, Environment.NewLine);
        Debug.Log(builder.ToString());

        Debug.Log("Starting dictation listner");
        status.color = Color.blue;
        status.text = "Listening...";

        StartDictation();
    }

    private void GetConfirmation(PhraseRecognizedEventArgs args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
        Debug.Log(builder.ToString());

        if(args.text == "confirm" || args.text == "yes")
        {
            Debug.Log("doing the action");
            executor.Execute();
        }
        else
        {
            Debug.Log("cancelling");
        }

        StartKeyword();
    }

}
