using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEditor;
using UnityEngine;

public class txtReader : MonoBehaviour
{
    [SerializeField] TextAsset textAsset;
    [SerializeField] bool RestartMessagesList;
    [SerializeField] bool ReadJsonTrigger;
    [Header("UI")]
    [SerializeField] UiEditor_script UI_Script;
    [SerializeField] bool updateUI;
    [SerializeField] bool nextMessage;
    [SerializeField] bool previusMessage;
    [Header("Debugger")]
    [Range(0,546399)]
    [SerializeField] public int messageIndex;
    [SerializeField] int messageLineCount;
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] TextMeshProUGUI dateMeshPro;
    [SerializeField] TextMeshProUGUI timeMeshPro;
    [SerializeField] TextMeshProUGUI senderMeshPro;
    [SerializeField] int LinesPerFrame = 1000;
    [Header("Searcher")]
    [SerializeField] string wordToSearch;
    [SerializeField] int searchLinesPerFrame;
    [SerializeField] bool startSearchTrigger;
    [SerializeField] List<int> LastSearchResults = new List<int>();
    [Header("Json")]
    [SerializeField] JsonCreator jsonCreator;
    [SerializeField] bool writeJsonTrigger;
    [SerializeField] string jsonPath;
   
   [SerializeField] bool isReadingComplete;
    [Serializable]
    public class message
    {
        public string Text;
        public string SenderString;
        public string Date;
        public string Time;
        public int Count;
        public int LinesAmount;
        public float Width;
        public float Height;
        public message(string tempText,string date, string time, string sender, int currentCount, int linesAmount = 1)
        {
            Text = tempText;
            Date = date;
            Time = time;
            SenderString = sender;
            Count = currentCount;
            LinesAmount = linesAmount;
        }
    }
    List<message> messagesList = new List<message>();
    [SerializeField] ArrayHolder messagesHolder;
    IEnumerator CountLines(string path)
    {
        messagesList.Clear();
        StreamReader reader = new StreamReader(path);

        float startingTime = Time.time;

        int linesCountedThisBulk = 0;
        int TotalLinesCounted = 1;
        int messagesRegistered = 0;

        string currentLine = reader.ReadLine();
        while (currentLine != null)
        {
            int possibleOffset = GetPosibleOffset(currentLine);

            if (CheckIfDate(currentLine, possibleOffset)) //If its a DATE, store the info into a new message
            {
                string[] LineInfo = ExtractInfo(currentLine, possibleOffset);
                createNewMessage(LineInfo[3], LineInfo[0], LineInfo[1], LineInfo[2], TotalLinesCounted);
                messagesRegistered++;
            }
            else //if its not a DATE, add the message to the previus message stored and count one more line
            {
                messagesList[messagesRegistered-1].Text += "\r\n" + currentLine;
                messagesList[messagesRegistered - 1].LinesAmount++;
            }

            //Bulk management
            linesCountedThisBulk++;
            TotalLinesCounted++;
            Debug.Log( " Lines:" + TotalLinesCounted + " Messages: "+ messagesRegistered) ;
            if (linesCountedThisBulk >= LinesPerFrame) { linesCountedThisBulk = 0; yield return null; }

            //Go to next line
            currentLine = reader.ReadLine();
        }

        //Counting completed
        Debug.Log("Counting took: " + (Time.time - startingTime).ToString());
        isReadingComplete = true;
        messagesHolder.isFilled = true;
    }
    
    void createNewMessage(string text, string date, string time, string senderString, int currentCount) 
    {
        message newMessage = new message(text,date,time,senderString, currentCount);
        messagesList.Add(newMessage);
    }
    private void Start()
    {
        messagesList = jsonCreator.getCompleteArray().ToList();
    }
    private void Update()
    {
        if(isReadingComplete)
        {
            messageLineCount = messagesList[messageIndex].Count;
            textMeshPro.text = messagesList[messageIndex].Text;
            dateMeshPro.text = "DATE: " + messagesList[messageIndex].Date;
            timeMeshPro.text = "TIME: " + messagesList[messageIndex].Time;
            senderMeshPro.text = "SENDER: " + messagesList[messageIndex].SenderString;

        }

        if (startSearchTrigger)
        {
            StartCoroutine(SearchWord(wordToSearch));
            startSearchTrigger = false;
        }

        if(writeJsonTrigger)
        {
            jsonCreator.WriteJson(jsonPath, messagesList.ToArray());
            writeJsonTrigger = false;
        }
        if(RestartMessagesList)
        {
            string temppath = AssetDatabase.GetAssetPath(textAsset);
            StartCoroutine(CountLines(temppath));

            RestartMessagesList = false;
        }
        if(ReadJsonTrigger)
        {
            messagesList = jsonCreator.getCompleteArray().ToList();
            ReadJsonTrigger = false;
        }
        if(updateUI)
        {
            UI_Script.updateUI();
            updateUI = false;
        }
        if(nextMessage)
        {
            messageIndex++;
            UI_Script.updateUI();
            nextMessage = false;
        }
        if(previusMessage)
        {
            messageIndex--;
            UI_Script.updateUI();
            previusMessage = false;
        }
    }
    int GetPosibleOffset(string currentLine)
    {
        if (currentLine.Length < 3) return 0;
        if (Char.IsNumber(currentLine[0]) && currentLine[1] == '/') { return 0; }
        else if (Char.IsNumber(currentLine[0]) && currentLine[2] == '/') { return 1; }
        return 0;

    }
    string[] ExtractInfo(string line, int offset)
    {
        string tempDate = ""; //0
        string tempTime = ""; //1
        string tempSender = ""; //2
        string tempText = ""; //3

        int analisedChars = 0;
        for (int j = 0; j < 9 + offset; j++)
        {
            tempDate += line[0 + j];
        }
        analisedChars += 9 + offset;

        for (int k = 0; k < 9; k++)
        {
            tempTime += line[analisedChars + k];
        }
        analisedChars += 12;

        for (int l = 1; l < 20; l++)
        {
            if (line[analisedChars + l] == ']') { analisedChars += l + 2; break; }

            tempSender += line[analisedChars + l];
        }
        for (int m = analisedChars; m < line.Length; m++)
        {
            tempText += line[m];
        }
        
        return new string[] {tempDate,tempTime,tempSender,tempText};
    }
    bool CheckIfDate(string line, int offset) //Start from the second number
    {
        if(line.Length < 8) return false;

        if (Char.IsNumber(line[offset])
            && line[offset + 1] == '/'
            && Char.IsNumber(line[offset + 2])
            && Char.IsNumber(line[offset + 3])
            && line[offset + 4] == '/'
            && Char.IsNumber(line[offset + 5])
            && Char.IsNumber(line[offset + 6])
            )
        {
            return true;
        }
        return false;
    }
    bool CheckIfNewDay(string line, int startingIndex)
    {
        if (Char.IsNumber(line[startingIndex])
            && line[startingIndex + 1] == ' '
            && line[startingIndex + 2] == 'd'
            && line[startingIndex + 3] == 'e'
            && line[startingIndex + 4] == ' '
            )
        {
            return true;
        }
        return false;
    }
    public message getMessage(int index)
    {
        return messagesList[index];
    }
    IEnumerator SearchWord(string word)
    {
        int messagesSearched = 0;
        int totalMessages = messagesList.Count;
        int wordLenght = word.Length;
        float startingTime = Time.time;

        LastSearchResults.Clear();

        for (int i = 0; i < totalMessages; i+= searchLinesPerFrame)
        {
            for (int j = i; j < i + searchLinesPerFrame && j< totalMessages; j++)
            {
                Debug.Log("Messages searched: " + messagesSearched + "/" + totalMessages);
                messagesSearched++;
                if (messagesList[j].Text.Length < wordLenght) { continue; }
                else if (messagesList[j].Text.Contains(word)) { LastSearchResults.Add(j);continue; }
            }
            yield return null;
        }
        Debug.Log("Search completed - TIME: " + (Time.time - startingTime).ToString());
    }
}
