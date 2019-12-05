﻿using City2BIM.Alkis;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace City2BIM
{
    /// <remarks>
    /// The "HelloWorld" external command. The class must be Public.
    /// </remarks>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ReadALKIS : IExternalCommand
    {
        // The main Execute method (inherited from IExternalCommand) must be public
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = revit.Application.ActiveUIDocument;

            GeoRefSettings.SetInitialSettings(uiDoc.Document);

            AlkisReader alkis = new AlkisReader(uiDoc.Document);

            return Result.Succeeded;

        }
    }
}