using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            Do(OriginalColorHelper);
        }
        public void OriginalColorMB() {
            DoMB(OriginalColorHelper);
        }
        protected void OriginalColorHelper() {
            textElement.color = originalColors[0];
            titleElement.color = originalColors[1];
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent("restore original text color", new Dictionary<string, object>());
        }


        /// <summary>
        /// First argument is a description of the text to be displayed.  This is logged if the wordEventReporter field is populated in the editor.
        /// 
        /// Second argument is the text to be displayed.  All elements in the textElements field will be updated.  This is logged in the "data" field under "displayed text" if the wordEventReporter field is populated in the editor.
        /// </summary>
        /// <param name="description">Description.</param>
        /// <param name="text">Text.</param>
        public void DisplayText(StackString description, StackString text) {
            Do(DisplayTextHelper, description, text);
        }
        public void DisplayTextMB(StackString description, StackString text) {
            DoMB(DisplayTextHelper, description, text);
        }
        protected void DisplayTextHelper(StackString description, StackString text) {
            if (OnText != null)
                OnText(text);

            textElement.text = text;
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            dataDict.Add("displayed text", text);
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent(description, dataDict);
        }

        public void DisplayTitle(StackString description, StackString text) {
            Do(DisplayTitleHelper, description, text);
        }
        public void DisplayTitleMB(StackString description, StackString text) {
            DoMB(DisplayTitleHelper, description, text);
        }
        protected void DisplayTitleHelper(StackString description, StackString text) {
            if (OnText != null)
                OnText(text);

            if (titleElement == null) {
                return;
            }

            titleElement.text = text;
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            dataDict.Add("displayed title", text);
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent(description, dataDict);
        }

        public void Display(StackString description, StackString title, StackString text) {
            Do(DisplayHelper, description, title, text);
        }
        public void DisplayMB(StackString description, StackString title, StackString text) {
            DoMB(DisplayHelper, description, title, text);
        }
        protected void DisplayHelper(StackString description, StackString title, StackString text) {
            if (OnText != null)
                OnText(text);

            if (titleElement == null) {
                return;
            }

            titleElement.text = title;
            textElement.text = text;
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            dataDict.Add("displayed title and text", text);
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent(description, dataDict);
        }


        /// <summary>
        /// Clears the text of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
        /// </summary>
        public void ClearText() {
            Do(ClearTextHelper);
        }
        public void ClearTextMB() {
            DoMB(ClearTextHelper);
        }
        protected void ClearTextHelper() {
            textElement.text = "";
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent("text display cleared", new Dictionary<string, object>());
        }

        public void ClearTitle() {
            Do(ClearTitleHelper);
        }
        public void ClearTitleMB() {
            DoMB(ClearTitleHelper);
        }
        protected void ClearTitleHelper() {
            titleElement.text = "";
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent("title display cleared", new Dictionary<string, object>());
        }

        public void Clear() {
            Do(ClearHelper);
        }
        public void ClearMB() {
            DoMB(ClearHelper);
        }
        protected void ClearHelper() {
            titleElement.text = "";
            textElement.text = "";
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent("title display cleared", new Dictionary<string, object>());
        }

        /// <summary>
        /// Changes the color of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
        /// </summary>
        /// <param name="newColor">New color.</param>
        public void ChangeColor(Color newColor) {
            Do(ChangeColorHelper, newColor);
        }
        public void ChangeColorMB(Color newColor) {
            DoMB(ChangeColorHelper, newColor);
        }
        protected void ChangeColorHelper(Color newColor) {
            textElement.color = newColor;
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            dataDict.Add("new color", newColor.ToString());
            if (scriptedEventReporter != null)
                scriptedEventReporter.ReportScriptedEvent("text color changed", dataDict);
        }

        /// <summary>
        /// Returns the current text being displayed on the first textElement.  Throws an error if there are no textElements.
        /// </summary>
        public async Task<StackString> CurrentText() {
            return await DoGet(CurrentTextHelper);
        }
        public StackString CurrentTextMB() {
            return DoGetMB(CurrentTextHelper);
        }
        public StackString CurrentTextHelper() {
            if (textElement == null)
                throw new UnityException("There aren't any text elements assigned to this TextDisplayer.");
            return textElement.text;
        }
    }

}