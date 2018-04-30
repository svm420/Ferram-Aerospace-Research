using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FerramAerospaceResearch.FARGUI.FAREditorGUI
{
    class StabilityDerivativeExportFileElement
    {
    }

    class StructElement : StabilityDerivativeExportFileElement
    {
    }

    class StringElement : StabilityDerivativeExportFileElement
    {
    }

    class ScalarElement : StabilityDerivativeExportFileElement
    {
    }

    class StabilityDerivativeExportFile
    {
        private StructElement cellelement;

        public StabilityDerivativeExportFile()
        {
        }

        static public string TextFilePath
        {
            get
            {
            string path = KSPUtil.ApplicationRootPath;
            path += "GameData/FerramAerospaceResearch/Plugins/PluginData/";
            path += "sdexport.txt";
            return path;
            }
        }

        static public bool Export(Simulation.StabilityDerivExportOutput output)
        {
            StabilityDerivativeExportFile body = new StabilityDerivativeExportFile();
            Debug.Log("[Rodhern] FAR: Pretend to export data to file (" + TextFilePath + ").");
            return true;
        }
    }
}
