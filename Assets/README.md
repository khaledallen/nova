# README.md

## Basic Structure

The primary game object that contains the voice controller is VoiceController.
This has the `VoiceController.cs` script attached to it, and excepts the following

* A JSON files indicating the various intents that can be triggered
* A UI Text field that shows the status of the voice controller (waiting or listening for command)
* A UI Text field that shows the currently heard dictation and displays the final assessment to the user
* A UI Text field that shows what action the controller will take, depending on the intent detected
* A UI Text field that prompts the user to confirm or deny the guessed command

Additionally, this game object also has fields for setting its trigger word ("Nova" by default), and 
the words that will be recognized as confirming a guessed command or denying it ("confirm, yes", and "no" respectively).
These can be set prior to compilation and allow you to choose how it interacts with the user.

In the default package, the VoiceController Game Object already contains the needed UI Text fields arranged 
in a way that presents the information clearly.

## Concepts

The VoiceController.cs creates three objects that interect with user speech, and one object that attempts to detect user intent.

### trigger_recognizer

The trigger recognizer is a Unity Speech Recognition keyword recognizer that is set to listen for the trigger word ("Nova" by default). Upon hearing
the keyword, it will stop listening, and start the dictation recognizer.

### m_DictationRecognizer

The dictation recognizer is a Unity Speech Recognition dictation recognizer. It listens for user speech and attempts to contruct
a phrase based on its understanding of English, adjusting backwards to ensure guesses are consistent with newly heard words.

If it hears a pause of sufficient length, it will stop listening and present a final guess as to the phrase that the user said.

If the user does not speak again, the dictation recognizer will end and do two things:

1. Pass the final phrase to the intent detector and set the active intent of the VoiceController based on teh response and,
2. hand control over to the confirmation flow.

### intentDetector

The intent detector is a very simply intention matching engine that initially parses the JSON file of intents into programmatic objects. These are stored for late matching against phrases provided by the dictation regonizer.

#### Intent Matching Process

When the dictation recognizer completes a recognition, it will call the `getIntent` function of the intentDetector. This tests all stored intents against the provided phrase. 

**In order to be considered a match, there is only a single criteria that must be true: ALL words in the intent `wordsGroups` property must appear in the phrase.**

Thus, the best way to design an intent is to create `wordGroups` that are as large as necessary to exclude unintended matches.

For example, to match the intention of navigating to the rover, the wordGroups can be thing like `["navigate rover", "navigate to rover", "find rover"]`. Something like `[navigate to the rover now]` would be unnecessarily restrictive, unless you specifically wanted the word `"now"` to appear in the phrase in order for a match to occur.

Keep in mind that the intent detector will stop at the first match, so even in the above example, the phrase "begin navigate to return to the rover now" would match the wordgroup "navigate rover" before it matched "navigate to the rover now".

*This is the area for the most improvement in speed and effectiveness.*

### confirm_recognizer

The confirm regonizer is a Unity Speech Recognition keyword recognizer that listens for the words in both the confirm_words and cancel_words properties. If it detects one of the confirm words, it will instantiate an object of the class specificed in the intent that was detected, and then exceute the named method (again from the intent) passing in the arguments specified in the intent JSON.

If it instead detected a word from the cancel list, it will do nothing.

In both cases, it then returns control to the trigger recognizer.

## Intent JSON

Create intents by adding them to the JSON, named `intentMap.json` in the example. It has the following format:

```
{
    "intents": [
        {
            "intentName": "navigateRover",
            "wordGroups": [
                "navigate rover",
                "find rover"
            ],
            "action": {
                "description": "Begin navigation to rover",
                "className": "Navigator",
                "method": "Navigate",
                "args": [
                    "rover"
                ]
            }
        },
        ...
    ]
}
```

The `intentName` is just to help identify the intent. The `wordsGroups` is an array of strings, each of which is a set of keywords that must appear together, and must all appear, in a phrase for the intent to match. In the example, the words "navigate" and "rover" must both appear in a phrase, or "find" and "rover" must both appear. The wordGroup "navigate find rover" would not match unless the phrase was something like, "navigate to find the rover", but both example wordGroups would match that phrase.

The `action` tells the controller what to do if it matches and intent. The `description` is displayed to the user to allow for confirmation. Make sure this is descriptive enough that the user understands what is about to happen. `className` must match exactly the class name of the exceutor class that will actually carry out the desired action. Likewise, `method` must match exactly the method name of the method within the class. `args` is an array of strings that will be passed to the method upon execution. Your executing class and method will need to handle arguments in the form of strings rather than programmatic objects, since that is all that can be passed.

## IntentExecutor

The `IntentExecutor` is an empty interface that the VoiceController uses to instantiate the classes that carry out the intended action.

## Creating an intent

There are two stages to creating an intent

### Create the intent information

First, add the intent information to the JSON file being used in the project, using the format above.

### Create the IntentExecutor

Second, build a class that will actually execute the desired action of the intent. In the example, `Navigator.cs` is a class that implements the (empty) `IntentExecutor` interface, and is called by both example intents. This class must have a name that exactly matches that in the intent JSON, as well as a method with a matching name.

Actual implmentation of the action is up to you. In the example, `Navigator` only generates logs, but it could load an existing game object and act on it.

IntentExecutors should be considered unique to a particular project, as they generally have to act on game objects that are assumed to exist. Since they are not inheriting from `MonoBehavior` these game objects cannot be set in the Unity interface.
