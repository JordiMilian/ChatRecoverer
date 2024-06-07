using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using System.Reflection;

public class UiEditor_script : MonoBehaviour
{
    UIDocument miMenu;
    VisualElement root;
    List<VisualElement> messageContainers;
    List<VisualElement> messagesStyles;
    [SerializeField] txtReader reader;
    [SerializeField] bool triggerUpdateUI;
    [SerializeField] float minWidth;
    [SerializeField] float maxWidth;
    [SerializeField] int maxCharactersPerLine;
    [SerializeField] float heightPerLine;
    [SerializeField] float baseHeight;


    string JessContainerStyle = "JessContainerStyle";
    string LauraContainerStyle = "LauraContainerSyle";
    string JessMessageStyle = "JessMessageStyle";
    string LauraMessageStyle = "LauraMessageStyle";

    private void Awake()
    {
        miMenu = GetComponent<UIDocument>();
        root = miMenu.rootVisualElement;
        messageContainers = root.Query("Container").ToList();
        messagesStyles = root.Query("Messaje").ToList();
    }
    private void Update()
    {
        if(triggerUpdateUI)
        {
            updateUI();
            triggerUpdateUI = false;
        }
    }
    public void updateUI()
    {
        for (int i = 0; i < messageContainers.Count; i++)
        {
            txtReader.message thisMessage = reader.getMessage(reader.messageIndex + i);

            //Get all labels and set them
            Label TextLabel = messageContainers[i].Query<Label>("Text");
            Label DateLabel = messageContainers[i].Query<Label>("Date");
            Label TimeLabel = messageContainers[i].Query<Label>("Time");
            TextLabel.text = thisMessage.Text;
            DateLabel.text = thisMessage.Date;
            TimeLabel.text = thisMessage.Time;

            /*
            float ratioOfCharacters = (thisMessage.Text.Length * 1f) / (maxCharactersPerLine * 1f);
            int extraLines = Convert.ToInt32(ratioOfCharacters - 0.5f);
            int totalLines = thisMessage.LinesAmount + extraLines;
            //Debug.Log("Lines: "+ totalLines + " Characters: " + thisMessage.Text.Length + " Ratio: "+ ratioOfCharacters);
            messageContainers[i].style.height = (totalLines * heightPerLine) + baseHeight;
            */

            StartCoroutine(CheckIdealWidth(thisMessage,i,TextLabel));
            StartCoroutine(CheckIdealHeight(thisMessage,i,TextLabel));
            /*
            float normalizedCharacters = Mathf.InverseLerp(0, maxCharactersPerLine, thisMessage.Text.Length);
            float equivalentWidth = Mathf.Lerp(minWidth,maxWidth,normalizedCharacters);
            
            messagesStyles[i].style.width = equivalentWidth;
            */


            //Decide which style it is
            if (thisMessage.SenderString == "JESS")
            {
                SetMessage_JessStyle(i);
            }
            else
            {
                SetMessage_LauraStyle(i);
            }
        }
    }
    IEnumerator CheckIdealHeight(txtReader.message messageInfo, int index, Label txtLabel)
    {

        if (messageInfo.Height != 0) //if its already set, set it as what it is
        {
            messageContainers[index].style.height = messageInfo.Height;
            yield break;
        }
        messageContainers[index].style.height = 300;
        yield return null;
        Debug.Log("txtLabel height was: " + txtLabel.contentRect.height);

        messageInfo.Height = txtLabel.contentRect.height + 80;

        messageContainers[index].style.height = messageInfo.Height;
    }
    IEnumerator CheckIdealWidth(txtReader.message messageInfo,int index, Label txtLabel)
    {
        if (messageInfo.Width != 0) //if its already set, set it as what it is
        { 
            messagesStyles[index].style.width = messageInfo.Width;
            yield break; 
        }

        messagesStyles[index].style.width = 600;
        yield return null;
        Debug.Log("txtLabel width was: "+ txtLabel.contentRect.width);

        if(txtLabel.contentRect.width > maxWidth)
        {
            messageInfo.Width = maxWidth;
        }
        else if(txtLabel.contentRect.width < minWidth)
        {
            messageInfo.Width= minWidth;
        }
        else
        {
            messageInfo.Width = txtLabel.contentRect.width + 50;
        }
        messagesStyles[index].style.width = messageInfo.Width;
    }
    void SetMessage_LauraStyle(int containerIndex)
    {
        messageContainers[containerIndex].AddToClassList(LauraContainerStyle);
        messageContainers[containerIndex].RemoveFromClassList(JessContainerStyle);

        messagesStyles[containerIndex].AddToClassList(LauraMessageStyle);
        messagesStyles[containerIndex].RemoveFromClassList(JessMessageStyle);
    }
    void SetMessage_JessStyle(int containerIndex)
    {
        messageContainers[containerIndex].AddToClassList(JessContainerStyle);
        messageContainers[containerIndex].RemoveFromClassList(LauraContainerStyle);

        messagesStyles[containerIndex].AddToClassList(JessMessageStyle);
        messagesStyles[containerIndex].RemoveFromClassList(LauraMessageStyle);
    }
    int RoundDownFloat(float f)
    {
        return Mathf.RoundToInt(f+0.5f);
    }
}
