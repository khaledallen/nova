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
    private string[] m_Keywords;
    [SerializeField]
    private Text status;

    private KeywordRecognizer m_Recognizer;
    private DictationRecognizer m_DictationRecognizer;
    [SerializeField]
    private Text m_Hypotheses;

    [SerializeField]
    private Text m_Recognitions;

    void Start()
    {
        ClearVoiceUI();
        m_Recognizer = new KeywordRecognizer(m_Keywords);
        m_Recognizer.OnPhraseRecognized += OnPhraseRecognized;
        m_Recognizer.Start();

        m_DictationRecognizer = new DictationRecognizer();
        m_DictationRecognizer.AutoSilenceTimeoutSeconds = 2;

        m_DictationRecognizer.DictationComplete += (cause) =>
        {
            Debug.Log("Dictation Complete due to "+ cause.ToString());
            StartKeyword();
        };

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
            m_Recognizer.Stop();
        }

        StartCoroutine(StartDictationWhenPossible());
    }

    public void StartKeyword()
    {
       // Shutdown the PhraseRecognitionSystem. This controls the KeywordRecognizers.
       if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
       {
            PhraseRecognitionSystem.Shutdown();
            m_DictationRecognizer.Stop();
        }

         
        StartCoroutine(StartKeywordWhenPossible());
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
        m_Recognizer.Start();
    }

    private void ClearVoiceUI()
    {
        status.text = "Waiting...";
        status.color = Color.green;

        m_Recognitions.color = Color.yellow;
        m_Recognitions.fontSize = 16;
        m_Recognitions.fontStyle = FontStyle.Normal;

        m_Recognitions.text = "";

    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
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

}
