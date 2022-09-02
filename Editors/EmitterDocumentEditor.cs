using Frosty.Core;
using Frosty.Core.Controls;
using FrostySdk.Ebx;
using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Frosty.Controls;
using System.Windows.Media;

namespace ScalableEmitterEditorPlugin
{
    [TemplatePart(Name = PART_EmitterStackPanel, Type = typeof(FrostyDockablePanel))]
    [TemplatePart(Name = PART_EmitterStack, Type = typeof(ItemsControl))]
    public class EmitterDocumentEditor : FrostyAssetEditor
    {

        #region -- Part Names --

        private const string PART_AssetPropertyGrid = "PART_AssetPropertyGrid";
        private const string PART_EmitterStackPanel = "PART_EmitterStackPanel";
        private const string PART_EmitterStack = "PART_EmitterStack";

        #endregion

        #region -- Parts --

        private FrostyPropertyGrid pgAsset;
        private FrostyDockablePanel emitterStackPanel;
        private ItemsControl emitterStack;

        #endregion

        public ObservableCollection<EmitterStackItemData> EmitterStackItems { get; set; }

        #region -- Constructors --

        static EmitterDocumentEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EmitterDocumentEditor), new FrameworkPropertyMetadata(typeof(EmitterDocumentEditor)));
        }

        public EmitterDocumentEditor()
            : base(null)
        {
            EmitterStackItems = new ObservableCollection<EmitterStackItemData>();
        }

        public EmitterDocumentEditor(ILogger inLogger)
            : base(inLogger)
        {
            EmitterStackItems = new ObservableCollection<EmitterStackItemData>();
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            pgAsset = GetTemplateChild(PART_AssetPropertyGrid) as FrostyPropertyGrid;
            pgAsset.OnModified += PgAsset_OnModified;
            emitterStackPanel = GetTemplateChild(PART_EmitterStackPanel) as FrostyDockablePanel;
            emitterStackPanel.MouseLeftButtonDown += EmitterStackPanel_MouseLeftButtonDown;
            emitterStack = GetTemplateChild(PART_EmitterStack) as ItemsControl;
            emitterStack.Loaded += EmitterStack_Loaded;
            emitterStack.MouseLeftButtonDown += EmitterStack_MouseLeftButtonDown;
            emitterStack.ItemsSource = EmitterStackItems;
        }

        void GetEmitterProcessors(dynamic obj)
        {
            dynamic proc = obj;
            EmitterStackItems.Clear();

            // add emitter base
            EmitterStackItems.Add(new EmitterStackItemData(proc, true, pgAsset));
            proc = proc.RootProcessor.Internal;

            // add root processor
            if (proc != null)
            {
                EmitterStackItems.Add(new EmitterStackItemData(proc, false, pgAsset));
                proc = proc.NextProcessor.Internal;
                while (proc != null)
                {
                    EmitterStackItems.Add(new EmitterStackItemData(proc, false, pgAsset));
                    proc = proc.NextProcessor.Internal;
                }
            }
        }

        #region -- Control Events --

        private void PgAsset_OnModified(object sender, ItemModifiedEventArgs e)
        {
            EmitterStackItems.Clear();
            dynamic obj = asset.RootObject;

            if (activeQualityLevel[0])
            {
                GetEmitterProcessors(obj.TemplateDataLow.Internal);
            }
            else if (activeQualityLevel[1])
            {
                GetEmitterProcessors(obj.TemplateDataMedium.Internal);
            }
            else if (activeQualityLevel[2])
            {
                GetEmitterProcessors(obj.TemplateDataHigh.Internal);
            }
            else if (activeQualityLevel[3])
            {
                GetEmitterProcessors(obj.TemplateDataUltra.Internal);
            }
        }

        private void EmitterStackPanel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject visualHit = VisualTreeHelper.HitTest(emitterStackPanel, e.GetPosition(emitterStackPanel)).VisualHit;
            while (visualHit != emitterStack && visualHit != null)
            {
                visualHit = VisualTreeHelper.GetParent(visualHit);
            }

            if (visualHit == emitterStack)
            {
                pgAsset.Object = 0;
                pgAsset.Object = null;
                //logger.Log("Clicked emitter stack");

                for (int i = 0; i < emitterStack.Items.Count; i++)
                {
                    // go down the visual tree into the UniformGrid
                    UIElement stackItemParent = (UIElement)emitterStack.ItemContainerGenerator.ContainerFromIndex(i);
                    for (int k = 0; k < 4; k++)
                    {
                        stackItemParent = VisualTreeHelper.GetChild(stackItemParent, 0) as UIElement;
                    }

                    // go down the visual tree into the processor block
                    UIElement proc = stackItemParent;
                    for (int k = 0; k < 2; k++)
                    {
                        proc = VisualTreeHelper.GetChild(proc, 1 - k) as UIElement;
                    }

                    // go down the visual tree into the evaluator block
                    UIElement eval = stackItemParent;
                    for (int k = 0; k < 2; k++)
                    {
                        eval = VisualTreeHelper.GetChild(eval, 0) as UIElement;
                    }

                    if (proc != null)
                    {
                        if (proc.IsMouseOver)
                        {
                            EmitterStackItems[i].ProcessorSelected = true;
                            pgAsset.SetClass(EmitterStackItems[i].EmitterItemObj);
                            //logger.Log("Processor selected");
                        }
                        else
                        {
                            EmitterStackItems[i].ProcessorSelected = false;
                            //logger.Log("Processor deselected");
                        }
                    }
                    if (eval != null)
                    {
                        if (eval.IsMouseOver)
                        {
                            EmitterStackItems[i].EvaluatorSelected = true;
                            pgAsset.SetClass(EmitterStackItems[i].EvaluatorObj);
                            //logger.Log("Evaluator selected");
                        }
                        else
                        {
                            EmitterStackItems[i].EvaluatorSelected = false;
                            //logger.Log("Evaluator deselected");
                        }
                    }
                }
            }
            else
            {
                // stop the property grid from being cleared if there isn't a stack being displayed
                if (EmitterStackItems.Count > 0)
                {
                    foreach (EmitterStackItemData item in EmitterStackItems)
                    {
                        item.ProcessorSelected = false;
                        item.EvaluatorSelected = false;
                    }
                    pgAsset.Object = 0;
                    pgAsset.Object = null;
                }
            }
        }

        private void EmitterStack_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void EmitterStack_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ///logger.Log("Clicked emitter stack");
            ///for (int i = 0; i < emitterStack.Items.Count; i++)
            ///{
            ///    //ContentPresenter c = (ContentPresenter)emitterStack.ItemContainerGenerator.ContainerFromItem(emitterStack.Items[i]);
            ///    //Border proc = c.ContentTemplate.FindName("PART_ProcessorBox", c) as Border;
            ///    Border proc = ItemsControlHelpers.findElementInItemsControlItemAtIndex<Border>(emitterStack, i, "PART_ProcessorBox") as Border;
            ///    Border eval = ItemsControlHelpers.findElementInItemsControlItemAtIndex<Border>(emitterStack, i, "PART_EvaluatorBox") as Border;
            ///    //Border eval = c.ContentTemplate.FindName("PART_EvaluatorBox", c) as Border;
            ///
            ///    if (proc != null)
            ///    {
            ///        if (proc.IsMouseOver)
            ///        {
            ///            EmitterStackItems[i].ProcessorSelected = true;
            ///            logger.Log("Processor selected");
            ///        }
            ///        else
            ///        {
            ///            EmitterStackItems[i].ProcessorSelected = false;
            ///            logger.Log("Processor deselected");
            ///        }
            ///    }
            ///    if (eval != null)
            ///    {
            ///        if (eval.IsMouseOver)
            ///        {
            ///            EmitterStackItems[i].EvaluatorSelected = true;
            ///            logger.Log("Evaluator selected");
            ///        }
            ///        else
            ///        {
            ///            EmitterStackItems[i].EvaluatorSelected = false;
            ///            logger.Log("Evaluator deselected");
            ///        }
            ///    }
            ///
            ///}
            ///EmitterStackItem stackItem = emitterStack.InputHitTest(e.GetPosition(emitterStack)) as EmitterStackItem;
            ///if (stackItem != null)
            ///{
            ///    emitterStack.Items.Contains(stackItem);
            ///    logger.Log("passed hit test");
            ///}
        }

        #endregion

        #region -- Toolbar --

        bool[] activeQualityLevel = { false, false, false, false };

        public override List<ToolbarItem> RegisterToolbarItems()
        {
            return new List<ToolbarItem>()
            {
                new ToolbarItem("Show All", "Display all quality levels",        "", new RelayCommand((object state) => { ShowAllButton_Click(this, new RoutedEventArgs()); })),
                new ToolbarItem("Low",      "Display emitter in Low quality",    "", new RelayCommand((object state) => { LowButton_Click(this,     new RoutedEventArgs()); })),
                new ToolbarItem("Medium",   "Display emitter in Medium quality", "", new RelayCommand((object state) => { MediumButton_Click(this,  new RoutedEventArgs()); })),
                new ToolbarItem("High",     "Display emitter in High quality",   "", new RelayCommand((object state) => { HighButton_Click(this,    new RoutedEventArgs()); })),
                new ToolbarItem("Ultra",    "Display emitter in Ultra quality",  "", new RelayCommand((object state) => { UltraButton_Click(this,   new RoutedEventArgs()); })),
            };
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            pgAsset.Object = asset.RootObject;
            EmitterStackItems.Clear();
            activeQualityLevel = new bool[] { false, false, false, false };
        }

        private void LowButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic obj = asset.RootObject;
            pgAsset.Object = obj.TemplateDataLow.Internal;
            GetEmitterProcessors(pgAsset.Object);
            activeQualityLevel = new bool[]{ true, false, false, false };
        }

        private void MediumButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic obj = asset.RootObject;
            pgAsset.Object = obj.TemplateDataMedium.Internal;
            GetEmitterProcessors(pgAsset.Object);
            activeQualityLevel = new bool[] { false, true, false, false };
        }

        private void HighButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic obj = asset.RootObject;
            pgAsset.Object = obj.TemplateDataHigh.Internal;
            GetEmitterProcessors(pgAsset.Object);
            activeQualityLevel = new bool[] { false, false, true, false };
        }

        private void UltraButton_Click(object sender, RoutedEventArgs e)
        {
            dynamic obj = asset.RootObject;
            pgAsset.Object = obj.TemplateDataUltra.Internal;
            GetEmitterProcessors(pgAsset.Object);
            activeQualityLevel = new bool[] { false, false, false, true };
        }

        #endregion

    }
}
