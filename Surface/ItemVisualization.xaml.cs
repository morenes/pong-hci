using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System;
using System.Windows.Media.Animation;

namespace ItemCompare
{
    public class RotateEventArgs : EventArgs
    {
        public RotateEventArgs(double angle)
        {
            this.angle = angle;
        }

        public double angle { get; protected set; }
    }
    /// <summary>
    /// Interaction logic for ItemVisualization.xaml
    /// </summary>
    public partial class ItemVisualization : TagVisualization
    {
        private const int P1TAG = 38;
        public event EventHandler clicker;
        //public event EventHandler rotate;

        private Items.ItemData itemData;
        public Items.ItemData ItemData
        {
            get { return itemData; }
            set { itemData = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemVisualization()
        {
            InitializeComponent();

            TagRemovedBehavior = TagRemovedBehavior.Wait;
            LostTagTimeout = double.PositiveInfinity;
        }

        public void SetItem(
            Items.ItemProperty[] properties,
            Items.Item item,
            TagVisualization tv)
        {
            // clear out any prior contents
            RowHost.Children.Clear();
            RowHost.RowDefinitions.Clear();

            // set up our row definitions
            for (int index = 0; index < properties.Length; ++index)
            {
                RowHost.RowDefinitions.Add(new RowDefinition());
            }

            // add our rows
            for (int index = 0; index < properties.Length; ++index)
            {
                InformationPanelRow row = new InformationPanelRow();
                Grid.SetRow(row, index);
                RowHost.Children.Add(row);              
                row.HeadingLabel.Text = properties[index].Name;                
                row.Cell.SetValue(item.Values[index], properties[index].PropertyType);
            }
            TagData tagValue = tv.VisualizedTag;
            
            BRed.Tag = tagValue;
            BWhite.Tag = tagValue;
            BYellow.Tag = tagValue;
            BBlue.Tag = tagValue;
            BGreen.Tag = tagValue;
            ItemNamePanel.Text = item.Name;
        }

        /// <summary>
        /// Determines whether this visualization matches the specified input device.
        /// </summary>
        /// <param name="inputDevice"></param>
        /// <returns></returns>
        public override bool Matches(InputDevice inputDevice)
        {
            // We match a given InputDevice if it's tag value is present
            // in our item data.

            if (itemData == null)
            {
                return false;
            }

            // Has to be a valid tag
            if (!IsTagValid(inputDevice.GetTagData()))
            {
                return false;
            }

            // Look for a match in our item data
            Items.Item matchingItem = itemData.Find((byte)inputDevice.GetTagData().Value);

            return matchingItem != null;
        }

        public void setGlowVisibility(bool glow)
        {
            if (glow) TagDownArea.Visibility = Visibility.Visible;
            else TagDownArea.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Refresh the item visualization properties when a tag is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnGotTag(RoutedEventArgs e)
        {
            TagVisualization tv = (TagVisualization)e.Source;
            TagData tag = tv.VisualizedTag;
            long tagValue = tag.Value;
            // Has to be a valid tag
            if (IsTagValid(tag))
            {
                // Look for a match in our item data
                Items.Item matchingItem = itemData.Find((byte)tag.Value);

                if (matchingItem != null)
                {
                    SetItem(itemData.Properties, matchingItem,tv);
                }
            }
            if (MainWindow.shouldShowTable(tagValue))
            {
                Storyboard sb = (Storyboard)this.FindResource("ShowPanelStoryboard");
                sb.Begin();
            }
        }
        protected override void OnLostTag(RoutedEventArgs e)
        {
            TagVisualization tv = (TagVisualization)e.Source;
            TagData tag = tv.VisualizedTag;
            long tagValue = tag.Value;
            if (InformationPanel.Visibility==Visibility.Visible)
            {
                Storyboard sb = (Storyboard)this.FindResource("HidePanelStoryboard");
                sb.Begin();
            }
        }
    
        protected override void OnMoved(RoutedEventArgs e)
        {
            TagVisualization tv = (TagVisualization)e.Source;
            double o = tv.Orientation;
            System.Diagnostics.Debug.WriteLine("ItemVi: Ahi van la orientacion");
            System.Diagnostics.Debug.WriteLine(o);
        }
        /// <summary>
        /// Helper method to validate tag data.
        /// </summary>
        /// <param name="tagData">the tag data to validate</param>
        /// <returns>true if this tag is valid for this app</returns>
        private static bool IsTagValid(TagData tagData)
        {
            return tagData.Schema == 0
                && tagData.Series == 0
                && tagData.ExtendedValue == 0
                && tagData.Value >= 0
                && tagData.Value < 256;
        }

        private void OnButtonPressed(object sender, RoutedEventArgs e)
        {
            //Button b = (Button)sender;
            if (clicker!=null) clicker(sender,e);
        }
    }
}
