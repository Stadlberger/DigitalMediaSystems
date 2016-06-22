using System;
using System.Collections.Generic;
using System.IO;
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
using QueryImage;

namespace ImageRetrevial
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataController dataController;
        Dictionary<string, SimilarityFinder> simFinders;
        
        static Dictionary<string, string> SearchTermToQuery = new Dictionary<string, string>();

        double ScrollDistance = 0;
        List<ISearchResult> SearchResults;
        List<string> simMethods;

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();
            dataController = new DataController();
            simFinders = new Dictionary<string, SimilarityFinder>();
            simMethods = new List<string>() { "CM3x3", "CN3x3", "GLRLM3x3", "HOG", "LBP3x3" };

            foreach (string s in simMethods)
            {
                simFinders.Add(s, new SimilarityFinder(Config.Get().m_pathToCSV, s, dataController));
            }
        }
        //Add all your UI Setup here
        private void InitializeUI()
        {
            List<string> List = new List<string>();
            List.Add("Date");
            List.Add("Description");
            List.Add("Tags");
            List.Add("Name");
            List.Add("Artist");
            List.Sort();
            SearchCombobox.ItemsSource = List;
            SearchCombobox.SelectedIndex = 0;

            SearchTermToQuery.Add("Date", "date_taken");
            SearchTermToQuery.Add("Description", "description");
            SearchTermToQuery.Add("Tags", "tags");
            SearchTermToQuery.Add("Name", "title");
            SearchTermToQuery.Add("Artist", "username");


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
            ResetSearch();
            AddSearchTerm(null, null);
            QueryData[] querys = new QueryData[SearchTermsUI.Children.Count];
            for(int i=0; i<querys.Length; i++)
            {
                QueryData data = new QueryData();
                Object[] UIData = (Object[])((FrameworkElement)SearchTermsUI.Children[i]).Tag;
                data.m_fieldName = UIData[0].ToString();
                data.m_fieldValue = UIData[1].ToString();
                querys[i] = data;
            }

            SearchResults = dataController.RunQuery(querys).ToList();
            DisplaySearch();
        }

        public void DisplaySearch()
        {
            if (SearchResults.Count <= 0)
                return;

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

        private void ResetSearch()
        {
            if(SearchResults != null)
            SearchResults.Clear();
            ImageGrid.Children.Clear();

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
            string path = Config.Get().m_pathToImages + SearchResults[index].RelativeURI;
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
                FileOpener.Tag = SearchResult.RelativeURI;
                FileOpener.Height = 25;
                FileOpener.Width = 100;
                FileOpener.Margin = new Thickness(80, -20, 0, 0);
                FileOpener.Click += OpenFilePath;
                Headerline.Children.Add(FileOpener);

                ComboBox SimilarSearchCombo = new ComboBox();
                SimilarSearchCombo.ItemsSource = simMethods;
                SimilarSearchCombo.SelectedIndex = 0;
                SimilarSearchCombo.Margin = new Thickness(230, 40, 0, 0);
                SimilarSearchCombo.Width = 50;
                SimilarSearchCombo.Height = 25;
                Headerline.Children.Add(SimilarSearchCombo);

                Object[] objArr = new Object[2];
                objArr[0] = SimilarSearchCombo;
                objArr[1] = System.IO.Path.GetFileNameWithoutExtension(DetailImage.Source.ToString());

                Button SimilarSearch = new Button();
                SimilarSearch.Content = "Search Similar";
                SimilarSearch.Tag = objArr;
                SimilarSearch.Height = 25;
                SimilarSearch.Width = 100;
                SimilarSearch.Margin = new Thickness(80, 40, 0, 0);
                SimilarSearch.Click += SimilarSearch_Click;
                Headerline.Children.Add(SimilarSearch);

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

        private void SimilarSearch_Click(object sender, RoutedEventArgs e)
        {
            ResetSearch();
            RemoveAllQueryTerms();
            FrameworkElement fe = (FrameworkElement)sender;
            Object[] Data = (Object[])fe.Tag;
            ComboBox cb = (ComboBox)Data[0];
            string SearchMethod = cb.Text;
            string Id = (string)Data[1];
            ////Todo get search results
            //SearchResults = dataController.RunQuery(querys).ToList();
            SearchResults = simFinders[SearchMethod].getSimilarImages(Id, 15);

            CloseDetailView(null,null);
            DisplaySearch();
        }

        private void CloseDetailView(object sender, RoutedEventArgs e)
        {
            DetailView.Visibility = Visibility.Hidden;
            DetailInformationSpace.Children.Clear();
        }

        private void OpenFilePath(object sender, RoutedEventArgs e)
        {
            string path = Config.Get().m_pathToImages + ((Button)sender).Tag;
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

        private void AddSearchTerm(object sender, RoutedEventArgs e)
        {
            Object[] Data = new Object[] { SearchTermToQuery[SearchCombobox.Text], SearchTextBox.Text };

            Border b = new Border();
            b.CornerRadius = new CornerRadius(20);
            b.BorderBrush = new SolidColorBrush(Colors.DarkGray);
            b.Background = new SolidColorBrush(Colors.LightGray);
            b.BorderThickness = new Thickness(2);
            b.Margin = new Thickness(2, 0, 2, 0);
            b.VerticalAlignment = VerticalAlignment.Top;
            b.Tag = Data;

            StackPanel spanel = new StackPanel();
            spanel.Orientation = Orientation.Horizontal;

            Label Text = new Label();
            Text.Content = SearchCombobox.Text.Substring(0,2)+": "+ SearchTextBox.Text;
            Text.FontSize = 18;
            Text.Margin = new Thickness(5, 0, -5, 0);
            Text.VerticalContentAlignment = VerticalAlignment.Top;
            spanel.Children.Add(Text);

            Label Remove = new Label();
            Remove.Content = "X  ";
            Remove.FontSize = 18;
            Remove.Tag = Data;
            Remove.MouseLeftButtonDown += RemoveQueryTerm;
            spanel.Children.Add(Remove);

            b.Child = spanel;
            SearchTermsUI.Children.Add(b);


        }

        private void RemoveQueryTerm(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement ele = (FrameworkElement)sender;
            Object[] Data = (Object[])ele.Tag;
            for (int i = SearchTermsUI.Children.Count-1; i >= 0; i--)
            {
                if (((FrameworkElement)SearchTermsUI.Children[i]).Tag == Data)
                {
                    SearchTermsUI.Children.RemoveAt(i);
                }

            }

        }

        private void RemoveAllQueryTerms()
        {
            SearchTermsUI.Children.Clear();
        }

        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(null, null);
            }
        }
    }
}
