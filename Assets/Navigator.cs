using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigator : IntentExecutor
{
    public void Navigate(string target)
    {
        Debug.Log("Executing navigate with " + target);
    }
    public void Execute() {
        Debug.Log("Executing the navigator action");
    }
}