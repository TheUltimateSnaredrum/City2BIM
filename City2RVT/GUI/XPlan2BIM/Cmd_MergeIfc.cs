﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace City2RVT.GUI.XPlan2BIM
{
    /// <remarks>
    /// The "HelloWorld" external command. The class must be Public.
    /// </remarks>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Cmd_MergeIfc : IExternalCommand
    {
        //ExternalCommandData commandData;
        // The main Execute method (inherited from IExternalCommand) must be public
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;

            Prop_GeoRefSettings.SetInitialSettings(doc);

            var process = new XPlan2BIM.Merge_Ifc(revit);
            process.ShowDialog();

            return Result.Succeeded;
        }
    }
}