using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace ImageRetrevial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataController dataController;
        IndexController indexController;

        double ScrollDistance = 0;
        List<ISearchResult> SearchResults;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
            dataController = new DataController("../../images/");
            indexController = new IndexController();

            //foreach (var file in moc.GetResults())
            //{
            //    Console.WriteLine(file.ImageName);
            //    Console.WriteLine(file.FileName);
            //    Console.WriteLine(file.Description);
            //}

        }
        //Add all your UI Setup here
        private void InitializeUI()
        {
            List<string> List = new List<string>();
            List.Add("Text");
            List.Add("Artist");
            List.Add("Name");
            List.Add("Description");
            List.Sort();
            SearchCombobox.ItemsSource = List;
            SearchCombobox.SelectedIndex = 0;



            MakeTextBoxSelectable(SearchTextBox);
        }

        //When clicked Select All Text
        protected void MakeTextBoxSelectable(TextBox Element)
        {
            Element.PreviewMouseLeftButtonDown += SelectivelyHandleMouseButton;
            Element.GotKeyboardFocus += SelectAllText;

            //Use to make all TextBox Selectable
            //EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseLeftButtonDownEvent,
            //   new MouseButtonEventHandler(SelectivelyHandleMouseButton), true);
            //EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
            //  new RoutedEventHandler(SelectAllText), true);
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO
            //Perform Search
            //Get Results ... return List<ISearchResult>
            //Display them
            SearchResults = dataController.GetResults().ToList();
            DisplaySearch();

        }


        public void DisplaySearch()
        {

            int i = -1;
            do {
                RowDefinition RowDef = new RowDefinition();
                RowDef.Height = new GridLength(200);
                ImageGrid.RowDefinitions.Add(RowDef);
                i++;
            } while (i< SearchResults.Count/3);


            if (SearchResults.Count > 15)
            {
                for (i = 0; i < 15; i++)     //load first 9 Results in SearchSpace
                {
                    CreateResultImageEntry(i);
                }
            }
            else
            {
                for (i = 0; i < SearchResults.Count; i++)     //load all Results in SearchSpace
                {
                    CreateResultImageEntry(i);
                }
            }


        }

        private void SearchSpace_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollDistance += e.VerticalChange;
            int RowIndex = (int)e.VerticalOffset / 200;

            if (ScrollDistance > 200)   //Scrolling down
            {
                ScrollDistance -= 200;
                for(int i=0; i<3; i++)  //Add new Lower Row of Images 
                {
                    int index = (RowIndex + 4) * 3 + i;
                    if (index < SearchResults.Count)
                        CreateResultImageEntry(index);

                }
                for (int i = 0; i < 3; i++)  //Remove Upper Row
                {
                    if (RowIndex -1 >= 0)
                    {
                        Image element = ImageGrid.Children.Cast<Image>().FirstOrDefault(e2 => Grid.GetColumn(e2) == i && Grid.GetRow(e2) == RowIndex -1);
                        ImageGrid.Children.Remove(element);

                    }
                }

            }
            if (ScrollDistance < 0)   //Scrolling up
            {
                ScrollDistance += 200;
                for (int i = 0; i < 3; i++)  //Add new Row of Images
                {
                    int index = (RowIndex ) * 3 + i;
                    if (index >= 0)
                        CreateResultImageEntry(index);

                }
                for (int i = 0; i < 3; i++)  //Remove Lower Row
                {
                    if (RowIndex + 5 < (int)SearchResults.Count/3)
                    {
                        Image element = ImageGrid.Children.Cast<Image>().FirstOrDefault(e2 => Grid.GetColumn(e2) == i && Grid.GetRow(e2) == RowIndex + 5);
                        ImageGrid.Children.Remove(element);
                    }
                }

            }
        }

        private void CreateResultImageEntry(int index)
        {
            Image Entry = new Image();
            string path = Environment.CurrentDirectory + SearchResults[index].RelativeURL;
            var uri = new Uri(path, UriKind.Absolute);
            var uriSource = new Uri(path);
            Entry.Source = new BitmapImage(uriSource);
            Entry.Tag = index;
            Entry.Margin = new Thickness(10);
            Entry.MouseLeftButtonDown += EnlargeImage;
            Grid.SetRow(Entry, (int)index / 3);
            Grid.SetColumn(Entry, index % 3);
            ImageGrid.Children.Add(Entry);
        }

        private void EnlargeImage(object sender, MouseButtonEventArgs e)
        {
            DetailView.Visibility = Visibility.Visible;
            Image Clicked = (Image)sender;
            DetailImage.Source = Clicked.Source;
            int SearchIndex = (int)Clicked.Tag;
            DisplayDetailInformation(SearchResults[SearchIndex]);
        }

        private void DisplayDetailInformation(ISearchResult SearchResult)
        {
            if (SearchResult.ImageName != null)
            {
                Grid Headerline = new Grid();
                DetailInformationSpace.Children.Add(Headerline);

                Label ImageName = new Label();
                ImageName.Content = "ImageName:\n" + SearchResult.ImageName;
                ImageName.FontSize = 18;
                Headerline.Children.Add(ImageName);

                Button FileOpener = new Button();
                FileOpener.Content = "Open FilePath";
                FileOpener.Tag = SearchResult.RelativeURL;
                FileOpener.Height = 25;
                FileOpener.Width = 100;
                FileOpener.Margin = new Thickness(80, -20, 0, 0);
                FileOpener.Click += OpenFilePath;
                Headerline.Children.Add(FileOpener);

            }

            if (SearchResult.Description != null)
            {
                TextBox Description = new TextBox();
                Description.Text = "Description:\n" + SearchResult.Description;
                Description.TextWrapping = TextWrapping.Wrap;
                Description.IsReadOnly = true;
                Description.Background = null;
                Description.BorderThickness = new Thickness(0);
                Description.Margin = new Thickness(2);
                Description.FontSize = 18;
                DetailInformationSpace.Children.Add(Description);
            }
        }

        private void CloseDetailView(object sender, RoutedEventArgs e)
        {
            DetailView.Visibility = Visibility.Hidden;
            DetailInformationSpace.Children.Clear();
        }

        private void OpenFilePath(object sender, RoutedEventArgs e)
        {
            string path = Environment.CurrentDirectory + ((Button)sender).Tag;
            var uri = new Uri(path, UriKind.Absolute);
            Console.WriteLine(uri);
            Process.Start("explorer.exe", @"/select," + uri);
        }

        private void SelectivelyHandleMouseButton(object sender, RoutedEventArgs e)
        {

            var textbox = (sender as TextBox);
            if (textbox != null && !textbox.IsKeyboardFocusWithin)
            {
                if (e.OriginalSource.GetType().Name == "TextBoxView")
                {
                    e.Handled = true;
                    textbox.Focus();
                }
            }
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    }
}
