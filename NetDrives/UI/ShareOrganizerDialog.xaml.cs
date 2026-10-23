using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;
using Microsoft.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;

namespace Hardcodet.NetDrives.UI
{
  /// <summary>
  /// Interaction logic for ShareOrganizerDialog.xaml
  /// </summary>
  public partial class ShareOrganizerDialog : Window
  {
    /// <summary>
    /// The bound list.
    /// </summary>
    public ObservableCollection<IShareConfiguration> ShareList { get; set; }


    #region DraggedItem

    /// <summary>
    /// DraggedItem Dependency Property
    /// </summary>
    public static readonly DependencyProperty DraggedItemProperty =
        DependencyProperty.Register("DraggedItem", typeof(IShareConfiguration), typeof(ShareOrganizerDialog));

    /// <summary>
    /// Gets or sets the DraggedItem property.  This dependency property 
    /// indicates ....
    /// </summary>
    public IShareConfiguration DraggedItem
    {
      get { return (IShareConfiguration)GetValue(DraggedItemProperty); }
      set { SetValue(DraggedItemProperty, value); }
    }

    #endregion

        
    /// <summary>
    /// Inits the dialog with the currently registered shares.
    /// </summary>
    public ShareOrganizerDialog()
    {
      InitializeComponent();

      var shares = AppController.Instance.ShareManager.Store.ShareConfigurations.OrderBy(sc => sc.OrderIndex);
      var list = new ObservableCollection<IShareConfiguration>();
      shares.Do(list.Add);
      DataContext = ShareList = list;
    }


    /// <summary>
    /// Stores the updates share indices and closes the dialog.
    /// </summary>
    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      //update the share indices
      ShareList.Do(sh => sh.OrderIndex = ShareList.IndexOf(sh));
      DialogResult = true;
    }



    #region edit mode monitoring

    /// <summary>
    /// State flag which indicates whether the grid is in edit
    /// mode or not.
    /// </summary>
    public bool IsEditing { get; set; }

    private void OnBeginEdit(object sender, DataGridBeginningEditEventArgs e)
    {
      IsEditing = true;
      //in case we are in the middle of a drag/drop operation, cancel it...
      if (IsDragging) ResetDragDrop();
    }

    private void OnEndEdit(object sender, DataGridCellEditEndingEventArgs e)
    {
      IsEditing = false;
    }

    #endregion


    #region Drag and Drop Rows

    /// <summary>
    /// Keeps in mind whether
    /// </summary>
    public bool IsDragging { get; set; }

    /// <summary>
    /// Initiates a drag action if the grid is not in edit mode.
    /// </summary>
    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (IsEditing) return;

      var row = UIHelpers.TryFindFromPoint<DataGridRow>((UIElement) sender, e.GetPosition(shareGrid));
      if (row == null || row.IsEditing) return;

      //set flag that indicates we're capturing mouse movements
      IsDragging = true;
      DraggedItem = (IShareConfiguration) row.Item;
    }



    /// <summary>
    /// Completes a drag/drop operation.
    /// </summary>
    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      IsEditing.ToString().ToConsole("Is Editing: {0}");

      if (!IsDragging || IsEditing) return;

      //get the target item
      IShareConfiguration targetItem = (IShareConfiguration)shareGrid.SelectedItem;

      if (targetItem != null && !ReferenceEquals(DraggedItem, targetItem))
      {
        int sourceIndex = ShareList.IndexOf(DraggedItem);
        int targetIndex = ShareList.IndexOf(targetItem);
        ShareList.Move(sourceIndex, targetIndex);        

        //select the dropped item
        shareGrid.SelectedItem = DraggedItem;
      }

      //reset
      ResetDragDrop();
    }


    /// <summary>
    /// Closes the popup and resets the
    /// grid to read-enabled mode.
    /// </summary>
    private void ResetDragDrop()
    {
      IsDragging = false;
      popup1.IsOpen = false;
      shareGrid.IsReadOnly = false;      
    }


    /// <summary>
    /// Updates the popup's position in case of a drag/drop operation.
    /// </summary>
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (!IsDragging || e.LeftButton != MouseButtonState.Pressed) return;

      //display the popup if it hasn't been opened yet
      if (!popup1.IsOpen)
      {
        //switch to read-only mode
        shareGrid.IsReadOnly = true;

        //make sure the popup is visible
        popup1.IsOpen = true;
      }


      Size popupSize = new Size(popup1.ActualWidth, popup1.ActualHeight);
      popup1.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

      //make sure the row under the grid is being selected
      Point position = e.GetPosition(shareGrid);
      var row = UIHelpers.TryFindFromPoint<DataGridRow>(shareGrid, position);
      if(row != null) shareGrid.SelectedItem = row.Item;
    }

    #endregion

  }
}