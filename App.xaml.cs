namespace BLEFinder
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

#pragma warning disable CS8765
        protected override Window CreateWindow(IActivationState activationState)
#pragma warning restore CS8765
        {
            int Width = 380;
            int Height = 764;
            var window = base.CreateWindow(activationState);
            window.Width = Width;
            window.Height = Height;
            window.MinimumWidth = Width;
            window.MinimumHeight = Height;
            //window.MaximumWidth = Width;
            //window.MaximumHeight = Height;
            return window;


        }
    }
}
