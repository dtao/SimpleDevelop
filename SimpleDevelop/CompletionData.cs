using System;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace SimpleDevelop
{
    class CompletionData : ICompletionData
    {
        public CompletionData(string text)
        {
            Text = text;
        }

        public virtual ImageSource Image
        {
            get { return null; }
        }

        public virtual double Priority
        {
            get { return 1.0; }
        }

        public string Text { get; private set; }

        public virtual object Content
        {
            get { return Text; }
        }

        public object Description
        {
            get { return Text; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }
    }
}
