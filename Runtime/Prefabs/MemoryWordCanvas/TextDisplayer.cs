﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
namespace UnityEPL {

    public class TextDisplayer : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        /// <summary>
        /// Subscribe to this event to be notified of changes in the displayed text.
        /// 
        /// Single string argument is the new text which is being displayed.
        /// </summary>
        public delegate void TextDisplayed(string text);
        public static TextDisplayed OnText;

        /// <summary>
        /// Drag a scripted event reporter here to have this monobehavior automatically report when text is displayed or cleared.
        /// </summary>
        public ScriptedEventReporter scriptedEventReporter = null;

        /// <summary>
        /// These text elements will all be updated when this monobehaviors public methods are used.
        /// </summary>
        public UnityEngine.UI.Text textElement;
        public UnityEngine.UI.Text titleElement;

        private Color[] originalColors;

        protected void Start() {
            originalColors = new Color[2];
            originalColors[0] = textElement.color;
            originalColors[1] = titleElement.color;
        }

        /// <summary>
        /// Returns the color of the assigned text elements to whatever they were when this monobehavior initialized (usually scene load).
        /// </summary>
        public void OriginalColor() {
            DoMB(OriginalColorHelper);
        }
        public void OriginalColorTS() {
            Do(OriginalColorHelper);
        }     
        protected void OriginalColorHelper() {
            textElement.color = originalColors[0];
            titleElement.color = originalColors[1];
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB("restore original text color", new());
        }


        /// <summary>
        /// First argument is a description of the text to be displayed.  This is logged if the wordEventReporter field is populated in the editor.
        /// 
        /// Second argument is the text to be displayed.  All elements in the textElements field will be updated.  This is logged in the "data" field under "displayed text" if the wordEventReporter field is populated in the editor.
        /// </summary>
        /// <param name="description">Description.</param>
        /// <param name="text">Text.</param>
        public void DisplayText(string description, string text) {
            DoMB(DisplayTextHelper, description.ToNativeText(), text.ToNativeText());
        }
        public void DisplayTextTS(string description, string text) {
            Do(DisplayTextHelper, description.ToNativeText(), text.ToNativeText());
        }
        protected void DisplayTextHelper(NativeText description, NativeText text) {
            var displayedText = text.ToString();
            if (OnText != null)
                OnText(displayedText);

            textElement.text = displayedText;
            Dictionary<string, object> dataDict = new() {
                { "displayed text", displayedText },
            };
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB(description.ToString(), dataDict);

            description.Dispose();
            text.Dispose();
        }

        public void DisplayTitle(string description, string title) {
            DoMB(DisplayTitleHelper, description.ToNativeText(), title.ToNativeText());
        }
        public void DisplayTitleTS(string description, string title) {
            Do(DisplayTitleHelper, description.ToNativeText(), title.ToNativeText());
        }
        protected void DisplayTitleHelper(NativeText description, NativeText title) {
            var displayedTitle = title.ToString();
            if (OnText != null)
                OnText(displayedTitle);

            if (titleElement == null) {
                return;
            }

            titleElement.text = displayedTitle;
            Dictionary<string, object> dataDict = new() {
                { "displayed title", displayedTitle },
            };
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB(description.ToString(), dataDict);

            description.Dispose();
            title.Dispose();
        }

        public void Display(string description, string title, string text) {
            DoMB(DisplayHelper, description.ToNativeText(), title.ToNativeText(), text.ToNativeText());
        }
        public void DisplayTS(string description, string title, string text) {
            Do(DisplayHelper, description.ToNativeText(), title.ToNativeText(), text.ToNativeText());
        }
        protected void DisplayHelper(NativeText description, NativeText title, NativeText text) {
            var displayedTitle = title.ToString();
            var displayedText = text.ToString();
            if (OnText != null) {
                OnText(title.ToString());
                OnText(text.ToString());
            }
                
            if (titleElement == null || textElement == null) {
                return;
            }

            titleElement.text = displayedTitle;
            textElement.text = displayedText;
            Dictionary<string, object> dataDict = new() {
                { "displayed title", displayedTitle },
                { "displayed text", displayedText },
            };
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB(description.ToString(), dataDict);

            description.Dispose();
            title.Dispose();
            text.Dispose();
        }


        /// <summary>
        /// Clears the text of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
        /// </summary>
        public void ClearText() {
            DoMB(ClearTextHelper);
        }
        public void ClearTextTS() {
            Do(ClearTextHelper);
        }
        protected void ClearTextHelper() {
            textElement.text = "";
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB("text display cleared", new());
        }
       
        public void ClearTitle() {
            DoMB(ClearTitleHelper);
        }
        public void ClearTitleTS() {
            Do(ClearTitleHelper);
        }
        protected void ClearTitleHelper() {
            titleElement.text = "";
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB("title display cleared", new());
        }

        public void Clear() {
            DoMB(ClearHelper);
        }
        public void ClearTS() {
            Do(ClearHelper);
        }
        protected void ClearHelper() {
            titleElement.text = "";
            textElement.text = "";
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB("title display cleared", new());
        }

        /// <summary>
        /// Changes the color of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
        /// </summary>
        /// <param name="newColor">New color.</param>
        public void ChangeColor(Color newColor) {
            DoMB(ChangeColorHelper, newColor);
        }
        public void ChangeColorTS(Color newColor) {
            Do(ChangeColorHelper, newColor);
        }
        protected void ChangeColorHelper(Color newColor) {
            textElement.color = newColor;
            Dictionary<string, object> dataDict = new();
            dataDict.Add("new color", newColor.ToString());
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEventMB("text color changed", dataDict);
        }

        /// <summary>
        /// Returns the current text being displayed on the first textElement.  Throws an error if there are no textElements.
        /// </summary>
        public string CurrentText() {
            var text = DoGetMB(CurrentTextHelper);
            var ret = text.ToString();
            text.Dispose();
            return ret;
        }
        public async Task<string> CurrentTextTS() {
            var text = await DoGet(CurrentTextHelper);
            var ret = text.ToString();
            text.Dispose();
            return ret;
        }
        protected NativeText CurrentTextHelper() {
            if (textElement == null)
                throw new UnityException("There aren't any text elements assigned to this TextDisplayer.");
            return textElement.text.ToNativeText();
        }

        public Task PressAnyKey(string description, string displayText) {
            return DoWaitForMB(PressAnyKeyHelper, description.ToNativeText(), displayText.ToNativeText());
        }
        public Task PressAnyKeyTS(string description, string displayText) {
            return DoWaitFor(PressAnyKeyHelper, description.ToNativeText(), displayText.ToNativeText());
        }
        protected async Task PressAnyKeyHelper(NativeText description, NativeText displayText) {
            _ = manager.hostPC.SendStateMsg(HostPC.StateMsg.WAITING);
            DisplayText($"{description.ToString()} (press any key prompt)", displayText.ToString());
            await InputManager.Instance.GetKey();
            ClearText();
            description.Dispose();
            displayText.Dispose();
        }
    }

}