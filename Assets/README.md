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

