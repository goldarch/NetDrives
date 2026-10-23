using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;
using Hardcodet.NetDrives.ViewModel.Connections;
using Hardcodet.Util.Dependencies;
using Infragistics.Windows.Ribbon;
using Microsoft.Windows.Controls;

namespace Hardcodet.NetDrives.UI
{
  /// <summary>
  /// Interaction logic for Main.xaml
  /// </summary>
  public partial class Main : XamRibbonWindow
  {
    private readonly DependencyNode dependency;

    /// <summary>
    /// Inits the window and sets the <see cref="AppController"/>
    /// singleton instance as the root data context.
    /// </summary>
    public Main()
    {
      InitializeComponent();
      AppController controller = AppController.Instance;
      DataContext = controller;

      //create dependency in order to trigger grid refresh as soon as the order index
      //of a configuration changes
      var nodeSettings = DependencyNodeSettings.Create(() => controller.ShareManager.Store.ShareConfigurations);
      nodeSettings.ObserveCollectionItemProperties = true;
      nodeSettings.SubItemPropertyFilter = new[] {"OrderIndex"};
      nodeSettings.ChangeHandler = (s, e) => RefreshSortDescriptions();
      dependency = DependencyNode.Create(nodeSettings);
      
      //initially sort shares
      RefreshSortDescriptions();

      //use cached window settings to restore size/location
      var settings = Settings.Default;
      if (settings.MainWindowLeft != -1)
      {
        Left = settings.MainWindowLeft;
        Top = settings.MainWindowTop;
        Width = settings.MainWindowWidth;
        Height = settings.MainWindowHeight;
        if (settings.StartMaximized) WindowState = WindowState.Maximized;
      }
    }


    /// <summary>
    /// Refreshes the grids sort if the underlying collection is modified.
    /// This is a hack, but there seems to be no simple solution
    /// to get notified about changes of the underlying
    /// order index...
    /// </summary>
    private void RefreshSortDescriptions()
    {
      var view = CollectionViewSource.GetDefaultView(shareGrid.Items);
      using (view.DeferRefresh())
      {
        view.SortDescriptions.Clear();
        view.SortDescriptions.Add(new SortDescription("Configuration.OrderIndex", ListSortDirection.Ascending));
      }

      view.Refresh();
    }


    private void expander1_Collapsed(object sender, RoutedEventArgs e)
    {
      layoutRoot.RowDefinitions[2].Height = GridLength.Auto;
    }


    /// <summary>
    /// Attempts to open and browse a given network location.
    /// </summary>
    private void OnDoubleClick(object sender, MouseButtonEventArgs e)
    {
      DependencyObject source = (DependencyObject) e.OriginalSource;
      var row = UIHelpers.TryFindParent<DataGridRow>(source);

      //the user did not click on a row
      if (row == null) return;

      var share = AppController.Instance.ActiveShare;
      if (share != null)
      {
        bool status = share.IsConnected || ConnectionUtil.Connect(share) == true;
        if (status) ConnectionUtil.BrowseShare(share);
      }

      e.Handled = true;
    }



    /// <summary>
    /// Closes the observer for the share's order index.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
      dependency.Dispose();
      base.OnClosing(e);
    }



    /// <summary>
    /// Stores the windows size, location, and window state.
    /// Invoked if the size changes, and upon closing.
    /// </summary>
    /// <remarks>Only caching on close is not enough - if the
    /// window is maximized / minimized, the data is lost.</remarks>
    private void CacheWindowLocation(object sender, EventArgs e)
    {
      var settings = Settings.Default;

      if (WindowState == WindowState.Normal && Top > -10)
      {
        settings.MainWindowTop = Top;
        settings.MainWindowLeft = Left;
        settings.MainWindowHeight = Height;
        settings.MainWindowWidth = Width;
      }

      settings.StartMaximized = WindowState == WindowState.Maximized;
    }
  }
}