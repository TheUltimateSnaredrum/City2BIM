﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Forms;

using City2BIM.Alkis;
using City2BIM.Geometry;
using System.Xml.Linq;
using City2BIM;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace City2RVT.GUI
{
    /// <summary>
    /// Interaction logic for CityGML_settings.xaml
    /// </summary>
    public partial class Wpf_NAS_settings : Window
    {
        ExternalCommandData commandData;
        //copy from CityGML_settings, may enhance later for server request and/or code translation

        //private string[] codelistTypes = new string[] { "AdV (Arbeitsgemeinschaft der Vermessungsverwaltungen der Länder der BRD)", 
        //                                                "SIG3D (Special Interest Group 3D)" };

        //public static System.Collections.IList SelectedLayer { get => SelectedLayer; set => SelectedLayer = value; }
        //public static string[] SelectedLayer { get => SelectedLayer; set => SelectedLayer = value; }

        //public static string SelectedLayer { get => SelectedLayer; set => SelectedLayer = value; }

        public Wpf_NAS_settings(ExternalCommandData cData)
        {
            commandData = cData;

            UIApplication app = commandData.Application;
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            InitializeComponent();

            //tb_lat.Text = GeoRefSettings.WgsCoord[0].ToString();
            //tb_lon.Text = GeoRefSettings.WgsCoord[1].ToString();
            //tb_extent.Text = City2BIM_prop.Extent.ToString();
            tb_file.Text = Prop_NAS_settings.FileUrl;
            //tb_server.Text = NAS2BIM_prop.ServerUrl;

            //if (NAS2BIM_prop.IsServerRequest)
            //    rb_server.IsChecked = true;
            //else
            //    rb_file.IsChecked = true;

            //AlkisCategoryListbox.SelectedItem = SelectedLayer;

            if (Prop_NAS_settings.IsGeodeticSystem)
                rb_YXZ.IsChecked = true;
            else
                rb_XYZ.IsChecked = true;

            //foreach (var item in codelistTypes)
            //{
            //    cb_Codelist.Items.Add(item);
            //}

            //if no Terrain is added before, the functionality is not given
            //ATTENTION, TO DO: if Terrain is present but not imported via DTM2BIM this terrain will now not be considered
            //Workaround for later dev: FilterElementCollector for Terrains and maybe user-defined selection on which terrain alkis data should be draped
            if (Prop_Revit.TerrainId == null)
            {
                check_drapeBldgs.IsEnabled = false;
                check_drapeParcels.IsEnabled = false;
                check_drapeUsage.IsEnabled = false;
            }
        }

        public System.Collections.IList ListBox
        {
            get { return AlkisCategoryListbox.SelectedItems; }
        }

        private void Bt_browse_Click(object sender, RoutedEventArgs e)
        {
            Reader.FileDialog imp = new Reader.FileDialog();
            tb_file.Text = imp.ImportPath(Reader.FileDialog.Data.ALKIS);

        }

        //private void bt_editURL_Click(object sender, RoutedEventArgs e)
        //{
        //    tb_server.IsEnabled = true;
        //}

        private void Bt_apply_Click(object sender, RoutedEventArgs e)
        {
            //Prop_NAS_settings.selectedLayer = AlkisCategoryListbox.SelectedItems;

            //Prop_NAS_settings.selectedLayer = AlkisCategoryListbox.SelectedItems;

            //List<string> selectedLayer = new List<string>();
            //Prop_NAS_settings.selectedLayer = new List<string>();

            //ListBox = AlkisCategoryListbox.SelectedItems;

            GUI.Prop_NAS_settings.SelectedLayer = AlkisCategoryListbox.SelectedItems;
            //System.Windows.Forms.MessageBox.Show(Prop_NAS_settings.SelectedLayer.Count.ToString());

            //int i = 0;
            //foreach (var sl in AlkisCategoryListbox.SelectedItems)
            //{
            //    GUI.Prop_NAS_settings.SelectedLayer = (AlkisCategoryListbox.SelectedItems.ToString());
            //    i++;
            //}

            //Prop_NAS_settings.SelectedLayer = "hi";


            //int i = 0;
            //foreach (var sl in AlkisCategoryListbox.SelectedItems)
            //{
            //    Prop_NAS_settings.selectedLayer = (AlkisCategoryListbox.SelectedItem.ToString());
            //    i++;
            //}

            //foreach (var x in layers)
            //{
            //    System.Windows.Forms.MessageBox.Show(x.ToString());
            //}

            //read server / file url
            Prop_NAS_settings.FileUrl = tb_file.Text;
            //NAS2BIM_prop.ServerUrl = tb_server.Text;

            //set bool whether server or not
            //if ((bool)rb_server.IsChecked)
            //{
            //    NAS2BIM_prop.IsServerRequest = true;
            //    NAS2BIM_prop.IsGeodeticSystem = true;
            //}
            //else
            //    NAS2BIM_prop.IsServerRequest = false;

            //set center coordinates for request
            //if ((bool)rb_site.IsChecked)
            //{
            //    NAS2BIM_prop.ServerCoord[0] = GeoRefSettings.WgsCoord[1];
            //    NAS2BIM_prop.ServerCoord[1] = GeoRefSettings.WgsCoord[0];
            //}
            //else
            //{
            //    bool vLat = double.TryParse(tb_lat.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat);
            //    bool vLon = double.TryParse(tb_lon.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon);

            //    if (vLat && vLon)
            //    {
            //        NAS2BIM_prop.ServerCoord[1] = lat;
            //        NAS2BIM_prop.ServerCoord[0] = lon;
            //    }
            //}

            //set extent for request
            //bool ext = double.TryParse(tb_extent.Text, out var extSize);
            //if (ext)
            //     NAS2BIM_prop.Extent = extSize;

            //set coordinates order for file import
            if ((bool)rb_YXZ.IsChecked)
                Prop_NAS_settings.IsGeodeticSystem = true;
            else
                Prop_NAS_settings.IsGeodeticSystem = false;

            //if (check_applyCode.IsChecked == false)
            //    NAS2BIM_prop.CodelistName = Codelist.none;
            //else
            //{
            //    var item = cb_Codelist.SelectedItem;

            //    if (item.ToString().Equals("AdV (Arbeitsgemeinschaft der Vermessungsverwaltungen der Länder der BRD)"))
            //        NAS2BIM_prop.CodelistName = Codelist.adv;
            //    else if (item.ToString().Equals("SIG3D (Special Interest Group 3D)"))
            //        NAS2BIM_prop.CodelistName = Codelist.sig3d;
            //    else
            //        NAS2BIM_prop.CodelistName = Codelist.none;
            //}

            //if (check_saveResponse.IsChecked == true)
            //    NAS2BIM_prop.SaveServerResponse = true;
            //else
            //    NAS2BIM_prop.SaveServerResponse = false;

            if (check_drapeBldgs.IsChecked == true)
                Prop_NAS_settings.DrapeBldgsOnTopo = true;

            if (check_drapeParcels.IsChecked == true)
                Prop_NAS_settings.DrapeParcelsOnTopo = true;

            if (check_drapeUsage.IsChecked == true)
                Prop_NAS_settings.DrapeUsageOnTopo = true;

            this.Close();
        }

        private void Tb_file_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIApplication app = commandData.Application;
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            string alkisXmlPath = tb_file.Text;
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            XmlDocument xmlDoc = new XmlDocument();

            using (XmlReader reader = XmlReader.Create(alkisXmlPath, readerSettings))
            {
                xmlDoc.Load(reader);
                xmlDoc.Load(alkisXmlPath);
            }

            // Namespacemanager for used namespaces, e.g. in XPlanung GML or ALKIS XML files
            var XmlNsmgr = new Builder.Revit_Semantic(doc);
            XmlNamespaceManager nsmgr = XmlNsmgr.GetNamespaces(xmlDoc);

            List<string> xPlanObjectList = new List<string>();
            XmlNodeList allXPlanObjects = xmlDoc.SelectNodes("//gml:featureMember", nsmgr);


            foreach (XmlNode x in allXPlanObjects)
            {
                if (x.FirstChild.SelectSingleNode(".//gml:exterior", nsmgr) != null)
                {
                    if (xPlanObjectList.Contains(x.FirstChild.Name) == false)
                    {
                        xPlanObjectList.Add(x.FirstChild.Name);
                    }
                }
            }

            xPlanObjectList.Sort();

            int ix = 0;
            foreach (string item in xPlanObjectList)
            {
                AlkisCategoryListbox.Items.Add(xPlanObjectList[ix]);
                ix++;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (alkisRadioButton.IsChecked == true)
            {
                AlkisCategoryListbox.SelectAll();
            }
            else
            {
                AlkisCategoryListbox.UnselectAll();

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            AlkisCategoryListbox.UnselectAll();
            alkisRadioButton.IsChecked = false;
        }

        private void AlkisCategoryListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlkisCategoryListbox.SelectedItems.Count < AlkisCategoryListbox.Items.Count)
            {
                alkisRadioButton.IsChecked = false;
            }            
        }

        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }


        //private void rb_custom_Checked(object sender, RoutedEventArgs e)
        //{
        //    tb_lat.IsEnabled = true;
        //    tb_lon.IsEnabled = true;
        //}

        //private void bt_saveResponse_Click(object sender, RoutedEventArgs e)
        //{
        //    using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
        //    {
        //        //Log.Information("Start of changing directory. Dialogue opened.");

        //        fbd.RootFolder = Environment.SpecialFolder.Desktop;
        //        fbd.Description = "Select folder for CityGML file";

        //        fbd.ShowNewFolderButton = true;

        //        var result = fbd.ShowDialog();

        //        if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
        //        {
        //           NAS2BIM_prop.PathResponse = fbd.SelectedPath;
        //        }
        //    }
        //}
    }
}
