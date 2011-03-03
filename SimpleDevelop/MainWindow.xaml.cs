using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using SimpleDevelop.Core;
using Microsoft.Win32;
using System.IO;

namespace SimpleDevelop
{
    public partial class MainWindow : DXWindow
    {
        private TextEditor _textEditor;
        private BackgroundWorker _buildWorker;

        // In case code completion is ever implemented, the below will be
        // necessary.
        //private CompletionWindow _completionWindow;

        public MainWindow()
        {
            InitializeComponent();

            _textEditor = new TextEditor
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12f,
                Padding = new Thickness(12.0),
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".cs")
            };

            ResetText(_textEditor);

            _textEditor.TextArea.TextEntered += TextEditorTextAreaTextEntered;

            _codePanel.Content = _textEditor;

            _buildWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            _buildWorker.DoWork += BuildWorkerDoWork;
            _buildWorker.ProgressChanged += BuildWorkerProgressChanged;
            _buildWorker.RunWorkerCompleted += BuildWorkerRunWorkerCompleted;
        }

        private void TextEditorTextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // If it's ever decided to implement code completion, code LIKE
                // the below may serve as a starting point.

                //_completionWindow = new CompletionWindow(_textEditor.TextArea);

                //int index = _textEditor.SelectionStart - 1;
                //if (index > 0)
                //{
                //    int previousSpaceLocation = index;
                //    while (previousSpaceLocation > 0 && !char.IsWhiteSpace(_textEditor.Document.GetCharAt(previousSpaceLocation - 1)))
                //    {
                //        --previousSpaceLocation;
                //    }

                //    string token = _textEditor.Document.GetText(previousSpaceLocation, index - previousSpaceLocation);
                //}

                //IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
                //data.Add(new CompletionData("Test 1"));
                //data.Add(new CompletionData("Test 2"));
                //data.Add(new CompletionData("Test 3"));

                //_completionWindow.Closed += (obj, args) =>
                //{
                //    _completionWindow = null;
                //};

                //_completionWindow.Show();
            }
        }

        private void BuildWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var code = (string)e.Argument;

            var executor = new CSharpCodeExecutor();

            executor.BuildOutput += (obj, args) =>
            {
                _buildWorker.ReportProgress(0, args.Message);
            };

            executor.Execute(code);
        }

        private void BuildWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _outputTextBox.AppendText(e.UserState + Environment.NewLine);
        }

        private void BuildWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _buildRunItem.IsEnabled = true;
        }

        private void BuildRunItemItemClick(object sender, ItemClickEventArgs e)
        {
            _buildRunItem.IsEnabled = false;
            _outputTextBox.Clear();
            _buildWorker.RunWorkerAsync(_textEditor.Text);
        }

        private void NewItemItemClick(object sender, ItemClickEventArgs e)
        {
            string fileName;
            bool inputSupplied = SimpleInputWindow.GetInput("New File", "Please enter a file name.", out fileName);

            if (!inputSupplied)
            {
                return;
            }

            fileName = fileName.Trim();

            if (fileName.EndsWith(".cs"))
            {
                fileName = fileName.Substring(0, fileName.Length - 3);
            }

            _codePanel.Caption = fileName + ".cs";
            ResetText(_textEditor);
        }

        private void SaveAsItemItemClick(object sender, ItemClickEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Title = "Select a path to save the file to.",
                Filter = "C# files |*.cs"
            };

            bool pathSelected = saveDialog.ShowDialog() ?? false;

            if (pathSelected)
            {
                string filePath = saveDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                _codePanel.Caption = fileName;
                _textEditor.Save(filePath);
            }
        }

        private void OpenItemItemClick(object sender, ItemClickEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Select a C# file to open.",
                Filter = "C# files|*.cs"
            };

            bool fileSelected = openDialog.ShowDialog() ?? false;

            if (fileSelected)
            {
                string filePath = openDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                _codePanel.Caption = fileName;
                _textEditor.Load(filePath);
            }
        }

        private void ExitItemItemClick(object sender, ItemClickEventArgs e)
        {
            Close();
        }

        private static void ResetText(TextEditor textEditor)
        {
            textEditor.Text = Properties.Resources.DefaultCSharp;
        }
    }
}
