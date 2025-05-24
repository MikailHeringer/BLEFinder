using Microsoft.Maui.Controls;
using Shiny.BluetoothLE;
using System;
using System.Reactive.Linq;
using BLEFinder;
using System.Diagnostics;
using BLEFinder.Classes;

namespace BLEFinder
{
    public partial class MainPage : ContentPage
    {

        public MainPage(IBleManager bleManager)
        {
            InitializeComponent();
            BleScanner.bleManager = bleManager;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Mapa");
        }

    }
}