using Frosty.Core.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScalableEmitterEditorPlugin
{
    public class EmitterStackItemData : BaseViewModel
    {

        #region -- Fields --

        public object EmitterItemObj;
        public object EvaluatorObj;

        private FrostyPropertyGrid propertyGrid;
        private bool isEmitterRoot;
        private bool processorSelected;
        private bool evaluatorSelected;
        private string processorText;
        private string evaluatorText;
        private bool evaluatorVisible;

        #endregion

        #region -- Properties --

        public bool IsEmitterRoot
        {
            get
            {
                return isEmitterRoot;
            }
            set
            {
                if (isEmitterRoot != value)
                {
                    isEmitterRoot = value;
                    RaisePropertyChanged("IsEmitterRoot");
                }
            }
        }
        public bool ProcessorSelected
        {
            get
            {
                return processorSelected;
            }
            set
            {
                if (processorSelected != value)
                {
                    processorSelected = value;
                    RaisePropertyChanged("ProcessorSelected");
                }
            }
        }
        public bool EvaluatorSelected
        {
            get
            {
                return evaluatorSelected;
            }
            set
            {
                if (evaluatorSelected != value)
                {
                    evaluatorSelected = value;
                    RaisePropertyChanged("EvaluatorSelected");
                }
            }
        }
        public string ProcessorText
        {
            get
            {
                return processorText;
            }
            set
            {
                if (processorText != value)
                {
                    processorText = value;
                    RaisePropertyChanged("ProcessorText");
                }
            }
        }
        public string EvaluatorText
        {
            get
            {
                return evaluatorText;
            }
            set
            {
                if (evaluatorText != value)
                {
                    evaluatorText = value;
                    RaisePropertyChanged("EvaluatorText");
                }
            }
        }
        public bool EvaluatorVisible
        {
            get
            {
                return evaluatorVisible;
            }
            set
            {
                if (evaluatorVisible != value)
                {
                    evaluatorVisible = value;
                    RaisePropertyChanged("EvaluatorVisible");
                }
            }
        }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// Initializes an instance of the <see cref="EmitterStackItemData"/> class with a referenced object.
        /// </summary>
        /// <param name="obj">The processor or evaluator that this item represents</param>
        /// <param name="isRoot">Is this item the emitter base?</param>
        /// <param name="pg">The property grid to be updated</param>
        public EmitterStackItemData(dynamic obj, bool isRoot, FrostyPropertyGrid pg)
        {
            propertyGrid = pg;
            EmitterItemObj = obj;
            isEmitterRoot = isRoot;
            ProcessorSelected = false;
            EvaluatorSelected = false;
            ProcessorText = "Processor";
            EvaluatorText = "Evaluator";
            EvaluatorVisible = false;

            if (isEmitterRoot)
            {
                ProcessorText = "Emitter Base";
            }
            else
            {
                ProcessorText = CleanUpName(((dynamic)EmitterItemObj).__Id);
                if (Utils.DoesPropertyExist(EmitterItemObj, "Pre"))
                {
                    EvaluatorObj = ((dynamic)EmitterItemObj).Pre.Internal;
                    if (EvaluatorObj != null)
                    {
                        EvaluatorText = CleanUpName(((dynamic)EvaluatorObj).__Id);
                        EvaluatorText += $" ({ CleanUpName(((dynamic)EmitterItemObj).EvaluatorInput.ToString()) })";
                        EvaluatorVisible = true;
                    }
                }
            }
        }

        #endregion

        string CleanUpName(string name)
        {
            if (name.EndsWith("Data"))
                return name.Remove(name.Length - 4);
            else if (name.StartsWith("Ef"))
                return name.Remove(0, 2);
            return name;
        }

    }
}
