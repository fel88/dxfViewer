using OpenTK.Mathematics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace dxfViewer
{
    /// <summary>
    /// Interaction logic for RibbonMenuWPF.xaml
    /// </summary>
    public partial class RibbonMenuWPF : System.Windows.Controls.UserControl
    {
        public RibbonMenuWPF()
        {
            InitializeComponent();
            RibbonWin.Loaded += RibbonMenuWPF_Loaded;
        }

        private void RibbonMenuWPF_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }
        }

        public Form1 Form => Form1.Form;







        private void fitAll()
        {

        }

        private void ImportImage_click(object sender, RoutedEventArgs e)
        {
            // Form.ImportImage();
        }

        private void SaveAsProject_Click(object sender, RoutedEventArgs e)
        {
            // Form.SaveAsProject();
        }

        private void OpenProject_click(object sender, RoutedEventArgs e)
        {
            Form.OpenDxf();
        }

        private void Delete_click(object sender, RoutedEventArgs e)
        {
            // Form.DeleteSelected();
        }

        private void Move_click(object sender, RoutedEventArgs e)
        {
            Form.Centrify();
        }

        private void Rect_click(object sender, RoutedEventArgs e)
        {
            // Form.CreateRect();
        }

        private void Undo_click(object sender, RoutedEventArgs e)
        {
            //Form.Undo();
        }

        private void Arrow_click(object sender, RoutedEventArgs e)
        {
            //  Form.CreateArrow();
        }

        private void Clear_click(object sender, RoutedEventArgs e)
        {
            Form.Clear();
        }

        private void Group_click(object sender, RoutedEventArgs e)
        {
            // Form.GroupSelected();
        }

        private void Ungroup_click(object sender, RoutedEventArgs e)
        {
            // Form.UnGroupSelected();
            //
        }

        private void ShowHideLayersPanel_click(object sender, RoutedEventArgs e)
        {
            //      Form.SwitchLayersPanelVisible();
        }

        private void Selection_click(object sender, RoutedEventArgs e)
        {
            //        Form.ResetTool();
        }

        private void GroupSettings_click(object sender, RoutedEventArgs e)
        {
            //          Form.GroupSettings();
        }

        private void FitAll_click(object sender, RoutedEventArgs e)
        {
            Form.FitAll();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetCamera_click(object sender, RoutedEventArgs e)
        {
            Form.ResetCamera();
        }

        private void RibbonButton_Click_1(object sender, RoutedEventArgs e)
        {
            Form.SwitchColorTheme();
        }

        private void RibbonButton_Click_2(object sender, RoutedEventArgs e)
        {
            Form.Settings();
        }
    }

}