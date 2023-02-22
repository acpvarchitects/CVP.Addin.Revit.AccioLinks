using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using DataFormats = System.Windows.Forms.DataFormats;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AccioLinks
{
    public class LinkCheckboxes
    {
        private string _chkname;
        public string ChkName
        { get
            {return _chkname;}
          set
            {_chkname = value;}
        }

        public bool IsChecked { get; set; }
    }

    public partial class UserControl : Window
    {
        public ObservableCollection<LinkCheckboxes> LinkcheckboxesList { get; set; }
        private ExternalEvent m_ExEvent;
        private RequestHandler m_Handler;
        private string m_Linkoption;


        //private System.ComponentModel.IContainer components = null;

        //protected override void OnClosed(EventArgs e)
        //{
        //    if (_isClosed = true && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    m_ExEvent.Dispose();
        //    m_ExEvent = null;
        //    m_Handler = null;
        //    base.OnClosed(e);
        //}

        public UserControl(ExternalEvent exEvent, RequestHandler handler)
        {
            LinkcheckboxesList = new ObservableCollection<LinkCheckboxes>();
            InitializeComponent();
            m_ExEvent = exEvent;
            m_Handler = handler;
            DataContext = this;
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string txt = txtboxFolderPath.Text;
            System.Windows.Forms.OpenFileDialog fdial = new System.Windows.Forms.OpenFileDialog();
            {
                fdial.Filter = "Revit Files (*.rvt)|*.rvt";
                fdial.Multiselect = true;
                if (string.IsNullOrEmpty(txtboxFolderPath.Text))
                    fdial.InitialDirectory = "P:";
                else
                    fdial.InitialDirectory = txt;
            }
            DialogResult dr = fdial.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string[] filename = fdial.FileNames;
                foreach (string str in filename)
                {
                    LinkcheckboxesList.Add(new LinkCheckboxes() { ChkName = str, IsChecked = true } );
                }
            }
        }
 
        public void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fdial = new System.Windows.Forms.OpenFileDialog();
            {
            fdial.Filter = "Revit Files (*.rvt)|*.rvt";
            fdial.Multiselect = true;
            fdial.InitialDirectory = "P:";
             }
            DialogResult dr = fdial.ShowDialog();
            
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
            string[] filename = fdial.FileNames;
            foreach (string str in filename)
                {
                    LinkcheckboxesList.Add(new LinkCheckboxes() { ChkName = str, IsChecked = true });
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.CheckBox;
            var item = cb.DataContext;
            fileNames.SelectedItem = item;
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            foreach (var a in LinkcheckboxesList.ToList())
            {
                    if (a.IsChecked==true)
                {
                    LinkcheckboxesList.Remove(a);
                }
            };
            fileNames.Items.Refresh();
        }

        private void BtnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var a in LinkcheckboxesList)
            {
                a.IsChecked = true;
            }
            fileNames.Items.Refresh();
        }
        
        private void BtnUnheckAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var a in LinkcheckboxesList)
            {
                a.IsChecked = false;
            }
            fileNames.Items.Refresh();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddLinks_Click(object sender, RoutedEventArgs e)
        {
            if (btnOrigin.IsChecked == btnShared.IsChecked)
            {
                System.Windows.MessageBox.Show("Please select Link Positioning");
                return;
            }
            if (!fileNames.HasItems)
            {
                System.Windows.MessageBox.Show("Please select at least one Revit Link");
                return;
            }
            if (btnOrigin.IsChecked == true && fileNames.HasItems)
            {
                {
                    if (m_ExEvent != null)
                    {
                        m_Linkoption = "origin";
                        m_Handler.Linkoption = m_Linkoption;
                        m_Handler.Modelslist = LinkcheckboxesList;
                        m_ExEvent.Raise();
                        this.Close();
                    }
                    else
                    System.Windows.MessageBox.Show("external event handler is null");
                }
            }
            if (btnShared.IsChecked == true && fileNames.HasItems)
            {
                {
                    if (m_ExEvent != null)
                    {
                        m_Linkoption = "shared";
                        m_Handler.Linkoption = m_Linkoption;
                        m_Handler.Modelslist = LinkcheckboxesList;
                        m_ExEvent.Raise();
                        this.Close();
                    }

                    else
                        System.Windows.MessageBox.Show("external event handler is null");
                }
            }
        }

        private void OnFileDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string str in files)
                {
                    if (System.IO.Path.GetExtension(str) == ".rvt")
                    {
                        LinkcheckboxesList.Add(new LinkCheckboxes() { ChkName = str, IsChecked = true });
                    }
                }
                
            }
        }
    }
}