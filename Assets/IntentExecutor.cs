using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
This class serves as the base for 
building variable intents to execute by VoiceController.
The IntentDetector will build a specific IntentExecutor child
when it matches a dictation with a specific intent and then pass
that IntentExecutor child to the VoiceController, which can then execute
the Execute() function.
*/
public interface IntentExecutor
{
    // Intentionally left empty
    // For now, there is nothing specific this needs to do
    // just make sure you implment it in your executor classes
}
