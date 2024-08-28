using FluentPassFinder.ViewModels;
using System.Windows;
using System.Windows.Controls;
using WpfScreenHelper;
using System.Timers;

namespace FluentPassFinder.Views
{
    internal partial class SearchWindow
    {
        public SearchWindowViewModel ViewModel { get; }
        public static double HeaderSize = 40.0;

        private bool isClosing = false;
        private bool isOpening = false;

        public SearchWindow(SearchWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            HideSearchWindow();
        }

        [RelayCommand]
        public void HideSearchWindow(bool clearInputDirectly=false)
        {
            if (!isClosing && !isOpening)
            {
                isClosing = true;
                Hide();
                if(clearInputDirectly){
                    ClearInputs();
                }else{
                    ClearInputsAfterDelay(TimeSpan.FromSeconds(30));
                }

                isClosing = false;
            }
        }
        private void ClearInputsAfterDelay(TimeSpan delay)
        {
            var timer = new System.Threading.Timer(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    ClearInputs();
                });
            }, null, delay, Timeout.InfiniteTimeSpan);
        }

        private void ClearInputs()
        {
            ViewModel.SearchText = string.Empty;
            ViewModel.Entries.Clear();

            ViewModel.IsContextMenuOpen = false;
            ViewModel.SelectedEntry = null;
        }

        

        public void ShowSearchWindow(bool showOnPrimaryScreen)
        {
            if (ViewModel.IsAnyDatabaseOpen)
            {
                isOpening = true;
                SetCenteredWindowPosition(showOnPrimaryScreen);
                Show();

                Activate();
                SearchBox.Focus();

                isOpening = false;
            }
        }

        private void SetCenteredWindowPosition(bool showOnPrimaryScreen)
        {
            Screen screen = showOnPrimaryScreen ? Screen.PrimaryScreen : Screen.FromPoint(MouseHelper.MousePosition);
            var workingAreaCenter = GetCenterOfWorkingArea(this, screen);

            var topOffset = HeaderSize;

            Left = workingAreaCenter.X;
            Top = workingAreaCenter.Y - topOffset;
            Width = workingAreaCenter.Width;
            Height = workingAreaCenter.Height;
        }

        private static Rect GetCenterOfWorkingArea(Window window, Screen screen)
        {
            var x = screen.WorkingArea.X + (screen.WorkingArea.Width - window.Width) / 2.0;
            var y = screen.WorkingArea.Y + (screen.WorkingArea.Height - window.Height) / 2.0;
            return new Rect(x * screen.ScaleFactor, y * screen.ScaleFactor, window.Width * screen.ScaleFactor, window.Height * screen.ScaleFactor);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                listView.ScrollIntoView(listView.SelectedItem);
            }
        }
    }
}
