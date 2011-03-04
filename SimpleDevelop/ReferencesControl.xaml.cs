using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Threading;

namespace SimpleDevelop
{
    /// <summary>
    /// Interaction logic for ReferencesControl.xaml
    /// </summary>
    public partial class ReferencesControl : UserControl
    {
        private BackgroundWorker _referencesLoader;
        private BackgroundWorker _updateFilterWorker;
        private bool _waitLongerToUpdateFilter;
        private ReferencesDataSet.ReferencesDataTable _references;
        private DataView _filteredReferences;
        private DataView _selectedReferences;

        public ReferencesControl()
        {
            InitializeComponent();

            _references = ((ReferencesDataSet)((ObjectDataProvider)Resources["ReferencesDataSet"]).Data).References;

            // Wow, this is the most annoying thing ever.
            _filteredReferences = (DataView)_referencesListBox.ItemsSource;
            _selectedReferences = (DataView)_selectedReferencesListBox.ItemsSource;

            _referencesLoader = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            _referencesLoader.DoWork += ReferencesLoaderDoWork;
            _referencesLoader.ProgressChanged += ReferencesLoaderProgressChanged;
            _referencesLoader.RunWorkerCompleted += ReferencesLoaderRunWorkerCompleted;
            _referencesLoader.RunWorkerAsync();

            _updateFilterWorker = new BackgroundWorker();

            _updateFilterWorker.DoWork += new DoWorkEventHandler(UpdateFilterWorkerDoWork);
            _updateFilterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(UpdateFilterWorkerRunWorkerCompleted);
        }

        private void UpdateFilterWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);
        }

        private void UpdateFilterWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_waitLongerToUpdateFilter)
            {
                _updateFilterWorker.RunWorkerAsync();
                _waitLongerToUpdateFilter = true;
            }
            else
            {
                string filter = _referenceFilterTextBox.Text;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    _filteredReferences.RowFilter = string.Format("Name like '%{0}%'", filter);
                }
                else
                {
                    _filteredReferences.RowFilter = null;
                }
            }
        }

        public event EventHandler<ReferenceAddedEventArgs> ReferenceAdded;

        public IEnumerable<string> SelectedReferences
        {
            get
            {
                var selectedFilePaths = from rowView in _selectedReferences.Cast<DataRowView>()
                                        let r = (ReferencesDataSet.ReferencesRow)rowView.Row
                                        select r.Path;

                return selectedFilePaths.ToList();
            }
        }

        private void ReferenceFilterTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_updateFilterWorker.IsBusy)
            {
                _updateFilterWorker.RunWorkerAsync();
            }
            else
            {
                _waitLongerToUpdateFilter = true;
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
            _references.AddReferencesRow(filePath, Path.GetFileName(filePath), false);
        }

        private void ReferencesLoaderRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var frameworkDirectory = e.Result as string;
            if (frameworkDirectory != null)
            {
                string pathOfMsCorLib = Path.Combine(frameworkDirectory, "mscorlib.dll");
                ReferencesDataSet.ReferencesRow row = _references.Where(r => r.Path == pathOfMsCorLib).SingleOrDefault();
                if (row != null)
                {
                    AddReference(row);
                }
            }
        }

        private void ReferencesListBoxMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedRowView = _referencesListBox.SelectedItem as DataRowView;
            if (selectedRowView == null)
            {
                return;
            }

            var selectedRow = (ReferencesDataSet.ReferencesRow)selectedRowView.Row;
            AddReference(selectedRow);
        }

        private void AddReference(ReferencesDataSet.ReferencesRow row)
        {
            if (row.Selected)
            {
                row.Selected = true;
                OnReferenceAdded(row.Path);
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
