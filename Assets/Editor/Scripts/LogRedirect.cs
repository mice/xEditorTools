using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class LogRedirect
{
    private static string LOG_KEY = "UqeeLog:";
    private static string LOG_CS_FILE = "UqeeLog.cs";
#if UNITY_EDITOR
    [UnityEditor.Callbacks.OnOpenAsset(0)]
    static bool OnOpenAsset(int instanceID,int line)
    {
        if (UnityEditor.EditorWindow.focusedWindow!=null && !UnityEditor.EditorWindow.focusedWindow.titleContent.text.Equals("Console"))
            return false;
        var obj = UnityEditor.EditorUtility.InstanceIDToObject(instanceID);
        var stackTrace = GetStackTrace();
        if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains(LOG_KEY))
        {
            Match matches = Regex.Match(stackTrace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
            string pathline = "";
            while (matches.Success)
            {
                pathline = matches.Groups[1].Value;
                if (!pathline.Contains(LOG_CS_FILE))
                {
                    int splitIndex = pathline.LastIndexOf(":");
                    string path = pathline.Substring(0, splitIndex);
                    line = System.Convert.ToInt32(pathline.Substring(splitIndex + 1));
                    string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                    fullPath = fullPath + path;
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace("/", "\\"), line);
                    return true;
                }
                matches = matches.NextMatch();
            }
        }
        return false;
    }

    static string GetStackTrace()
    {
        var ConsoleWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        var fieldInfo = ConsoleWindowType.GetField("ms_ConsoleWindow",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
        var consoleWindowInstance = fieldInfo.GetValue(null);
        if (consoleWindowInstance != null)
        {
            if((object)UnityEditor.EditorWindow.focusedWindow == consoleWindowInstance)
            {
                var ListViewStatType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.ListViewState");
                fieldInfo = ConsoleWindowType.GetField("m_ListView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var listView = fieldInfo.GetValue(consoleWindowInstance);
                fieldInfo = ListViewStatType.GetField("row", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                int row = (int)fieldInfo.GetValue(listView);
                fieldInfo = ConsoleWindowType.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
                return activeText;
            }
        }
        return string.Empty;
    }
#endif

}
