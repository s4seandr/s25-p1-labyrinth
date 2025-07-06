using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class NPCDecisionManager : MonoBehaviour
{
    public string currentAction = "";

    public IEnumerator GetDecisionFromLLM(string prompt, int retryCount = 1)
    {
        string json = $"{{\"model\":\"mistral\",\"prompt\":\"{prompt}\",\"stream\":false}}";
        //string json = $"{{\"model\":\"llama3\",\"prompt\":\"{prompt}\",\"stream\":false}}";
        //string json = $"{{\"model\":\"phi3\",\"prompt\":\"{prompt}\",\"stream\":false}}";

        using (UnityWebRequest www = new UnityWebRequest("http://localhost:11434/api/generate", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                string rawAction = ParseResponse(jsonResponse);
                Debug.Log("KI-Antwort: " + rawAction);

                string[] validActions = {
                "GehZuSpieler", "GehWeg", "KriecheZuSpieler", "KriecheWeg",
                "LaufeZuSpieler", "LaufeWeg", "WinkeDemSpieler"
            };

                string matchedAction = System.Array.Find(validActions, action => rawAction.Contains(action));
                if (!string.IsNullOrEmpty(matchedAction))
                {
                    currentAction = matchedAction.Trim();
                }
                else
                {
                    Debug.LogWarning("Ungültige Aktion vom LLM: " + rawAction);
                }
            }
            else
            {
                Debug.LogError("KI-Fehler: " + www.error);

                if (retryCount > 0)
                {
                    Debug.Log("Erneuter Versuch in 1 Sekunden...");
                    yield return new WaitForSeconds(1);
                    StartCoroutine(GetDecisionFromLLM(prompt, retryCount - 1));
                }
            }
        }
    }


    string ParseResponse(string json)
    {
        // Einfache JSON-Auswertung – bei Bedarf robuster machen
        int idx = json.IndexOf("\"response\":");
        if (idx >= 0)
        {
            int start = json.IndexOf(":", idx) + 1;
            int end = json.IndexOf(",", start);
            if (start > 0 && end > start)
            {
                string part = json.Substring(start, end - start).Trim().Trim('"');
                return part;
            }
        }
        return "";
    }
}
