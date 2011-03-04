using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.IO;
using System.Threading;

using SimpleDevelop.Collections;

namespace SimpleDevelop
{
    /// <summary>
    /// Interaction logic for ReferencesControl.xaml
    /// </summary>
    public partial class ReferencesControl : UserControl
    {
        private BackgroundWorker _referencesLoader;
        private BackgroundWorker _filterUpdater;
        private bool _waitToUpdateFilter;
        private SortedObservableCollection<Reference> _references;
        private SortedObservableCollection<Reference> _selectedReferences;

        public ReferencesControl()
        {
            InitializeComponent();

            _referencesListBox.ItemsSource = _references = new SortedObservableCollection<Reference>();
            _selectedReferencesListBox.ItemsSource = _selectedReferences = new SortedObservableCollection<Reference>();

            _referencesLoader = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            _referencesLoader.DoWork += ReferencesLoaderDoWork;
            _referencesLoader.ProgressChanged += ReferencesLoaderProgressChanged;
            _referencesLoader.RunWorkerCompleted += ReferencesLoaderRunWorkerCompleted;
            _referencesLoader.RunWorkerAsync();

            _filterUpdater = new BackgroundWorker();

            _filterUpdater.DoWork += new DoWorkEventHandler(FilterUpdaterDoWork);
            _filterUpdater.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FilterUpdaterRunWorkerCompleted);
        }

        private void FilterUpdaterDoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);
        }

        void FilterUpdaterRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_waitToUpdateFilter)
            {
                _filterUpdater.RunWorkerAsync();
                _waitToUpdateFilter = false;
            }
            else
            {
                string filter = _referencesFilterTextBox.Text;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    _referencesListBox.ItemsSource = _references.Where(r => r.FileName.Contains(filter)).ToList();
                }
                else
                {
                    _referencesListBox.ItemsSource = _references;
                }
            }
        }

        public event EventHandler<ReferenceAddedEventArgs> ReferenceAdded;

        public IEnumerable<string> SelectedReferences
        {
            get
            {
                var selectedFilePaths = from s in _selectedReferences
                                        select s.FilePath;

                return selectedFilePaths.ToList();
            }
        }

        private void ReferencesFilterTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_filterUpdater.IsBusy)
            {
                _filterUpdater.RunWorkerAsync();
            }
            else
            {
                _waitToUpdateFilter = true;
            }
        }

        private void ReferencesLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            string windowsDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string frameworkDirectoryPath = Path.Combine(windowsDirectoryPath, "Microsoft.NET", "Framework");

            string[] directories = Directory.GetDirectories(frameworkDirectoryPath, "v*");
            if (directories.Length == 0)
            {
                return;
            }

            Array.Sort(directories, StringComparer.OrdinalIgnoreCase);
            Array.Reverse(directories);

            string frameworkDirectory = directories[0];
            e.Result = frameworkDirectory;
            IEnumerable<string> files = Directory.EnumerateFiles(frameworkDirectory, "*.dll");
            foreach (string file in files)
            {
                _referencesLoader.ReportProgress(0, file);
            }
        }

        private void ReferencesLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string filePath = (string)e.UserState;
            _references.Add(new Reference(filePath));
        }

        private void ReferencesLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var frameworkDirectory = e.Result as string;
            if (frameworkDirectory != null)
            {
                AddReference(new Reference(Path.Combine(frameworkDirectory, "mscorlib.dll")));
            }
        }

        private void ReferencesListBoxMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedReference = _referencesListBox.SelectedItem as Reference;
            if (selectedReference != null)
            {
                AddReference(selectedReference);
            }
        }

        private void AddReference(Reference reference)
        {
            if (!_selectedReferences.Contains(reference))
            {
                _selectedReferences.Add(reference);
                OnReferenceAdded(reference.FilePath);
            }
        }

        protected void OnReferenceAdded(string reference)
        {
            EventHandler<ReferenceAddedEventArgs> handler = ReferenceAdded;
            if (handler != null)
            {
                handler(this, new ReferenceAddedEventArgs(reference));
            }
        }
    }
}
