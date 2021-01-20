/****
* "App.xaml.cs" - Interaction logic for App.xaml
* Copyright (C) 2021 Nicholas Logozzo
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, see <http://www.gnu.org/licenses/>.
****/

using Microsoft.Extensions.DependencyInjection;
using Nickvision.MVVM.Services;
using NickvisionTagger.ViewModels;
using NickvisionTagger.Views;
using System.Windows;

namespace NickvisionTagger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IContentDialogService, ContentDialogService>();
            serviceCollection.AddTransient<INotificationService, BalloonNotificationService>();
            serviceCollection.AddTransient<IIODialogService, IODialogService>();
            serviceCollection.AddTransient<IComboBoxDialogService, ComboBoxDialogService>();
            serviceCollection.AddTransient<MainWindowViewModel>();
            var provider = serviceCollection.BuildServiceProvider();
            var mainWindow = new MainWindow();
            mainWindow.DataContext = provider.GetRequiredService<MainWindowViewModel>();
            mainWindow.Show();
        }
    }
}
