using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using CVP.Addin.Revit.BIMOMatic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace AccioLinks
{
    public class 
        AddPanelAccio : IExternalApplication
    {
        // class instance
        internal static AddPanelAccio thisApp = null;

        // ModelessForm instance
        private UserControl m_MyForm;
        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_MyForm != null && m_MyForm.IsVisible)
            {
                m_MyForm.Close();
                m_MyForm = null;
            }
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("Accio Links");

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("cmdAccioLinks", "Accio Links", thisAssemblyPath, "AccioLinks.Accio");
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Load multi links by multiple shared coordinates or other link options.";

            Uri uriImage = new Uri("C:/Users/m.zeno/source/repos/CVP.Addin.Revit.AccioLink/AccioLinks/Resources/Accio32.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;

            return Result.Succeeded;
        }

        public void ShowForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            if (m_MyForm == null) //|| m_MyForm.Closed -= m_MyForm.Closed)
            {
                // A new handler to handle request posting by the dialog
                RequestHandler handler = new RequestHandler();

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_MyForm = new UserControl(exEvent, handler);
                m_MyForm.Show();
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Accio : IExternalCommand
    {
        public Document doc;
        public Autodesk.Revit.ApplicationServices.Application RevitApp;
        ExternalEvent exEvent;
        //The main Execute method(inherited from IExternalCommand) must be public
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            RequestHandler handler = new RequestHandler();
            exEvent = ExternalEvent.Create(handler);
            UserControl m_MyForm = new UserControl(exEvent, handler);
            m_MyForm.ShowDialog();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }

    public class RequestHandler : IExternalEventHandler
    {
        //// A trivial delegate, but handy
        private delegate void CreateRevitLink(Document revitDoc, ModelPath mp, RevitLinkOptions rlo);

        public ObservableCollection<LinkCheckboxes> Modelslist { get; set; }
        public string Linkoption { get; set; }

        public void Execute(UIApplication uiapp)
        {
            uiapp.DialogBoxShowing += HandleDialogBoxShowing;

            if (Linkoption == "origin")
            { LinkMultipleModels(uiapp, "Link Models by Origin", LinkMultipleModelsByOrigin); }

            if (Linkoption == "shared")
            { LinkMultipleModels(uiapp, "Link by Shared Coordinates", LinkBySharedCoordinates); }

            uiapp.DialogBoxShowing -= HandleDialogBoxShowing;
        }

        public String GetName()
        {
            return "Accio Links!";
        }

        /// <summary>
        ///   The top method of the event handler.
        /// </summary>
        private void LinkMultipleModels(UIApplication uiapp, string text, CreateRevitLink operation)
        {
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document revitDoc = uiapp.ActiveUIDocument.Document;

            using (Transaction tr = new Transaction(revitDoc))
                try
                {
                    if (tr.Start(text) == TransactionStatus.Started)
                    {
                        ViewFamilyType viewFamilyType3D = new FilteredElementCollector(revitDoc).OfClass(typeof(ViewFamilyType))
                            .Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>(x => ViewFamily.ThreeDimensional == x.ViewFamily);
                        View3D temporary3Dview = View3D.CreateIsometric(uiapp.ActiveUIDocument.Document, viewFamilyType3D.Id);

                        FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
                        IList<Element> links = collector.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkInstance)).ToElements();


                        foreach (var a in Modelslist)
                        {
                            FileInfo filePath = new FileInfo(a.ChkName);
                            // debug ***********
                            // MessageBox.Show("filePath.FullName.ToString() = " + filePath.FullName.ToString());
                            // debug ***********
                            ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath.FullName.ToString());
                            RevitLinkOptions rlo = new RevitLinkOptions(false);
                            operation(revitDoc, mp, rlo);
                        }

                        revitDoc.Delete(temporary3Dview.Id);
                        tr.Commit();

                        CoordinatorOMatic coordinatorOMatic = new CoordinatorOMatic(uiapp.Application);
                        coordinatorOMatic.SyncAndRelinquish(revitDoc, "");

                        TaskDialog.Show("ACCIO!", "Models have been linked!");
                    }
                }
                catch (Exception ex)
                {
                    text = ex.Message;
                }
        }

        void LinkMultipleModelsByOrigin(Document revitDoc, ModelPath mp, RevitLinkOptions rlo)
        {
            var linkType = RevitLinkType.Create(revitDoc, mp, rlo);
            var instance = RevitLinkInstance.Create(revitDoc, linkType.ElementId, ImportPlacement.Origin);
        }

        void LinkBySharedCoordinates(Document revitDoc, ModelPath mp, RevitLinkOptions rlo)
        {
            var linkType = RevitLinkType.Create(revitDoc, mp, rlo);
            var instance = RevitLinkInstance.Create(revitDoc, linkType.ElementId, ImportPlacement.Origin);

            FilteredElementCollector collector = new FilteredElementCollector(revitDoc);

            IList<Element> links = collector.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkInstance)).ToElements();

            foreach (Element e in links)
            {
                String s = String.Empty;

                RevitLinkInstance link = e as RevitLinkInstance;

                Document linkDoc = link.GetLinkDocument();
                ProjectLocation currentLocation = linkDoc.ActiveProjectLocation;
                ProjectLocationSet linkLocations = linkDoc.ProjectLocations;
                List<ProjectLocation> projLoc = new List<ProjectLocation>();

                foreach (ProjectLocation pl in linkLocations)
                {
                    //if (pl == currentLocation)
                    //{
                    //    {
                    //        TaskDialog.Show("Warning!", "The shared location " + pl.Name + " has not been linked being Current in the link model");
                    //    }
                    //}
                    //else
                    if (pl.Name != "Internal")
                    {
                        var linkInstance = RevitLinkInstance.Create(revitDoc, linkType.ElementId);
                        Document docum = linkInstance.GetLinkDocument();
                        Parameter parameter = linkInstance.LookupParameter("Name");

                        XYZ surveyPoint = new XYZ();
                        surveyPoint = BasePoint.GetSurveyPoint(docum).Position;

                        XYZ pbp = new XYZ();
                        pbp = BasePoint.GetProjectBasePoint(docum).Position;

                        ProjectPosition pp = pl.GetProjectPosition(pbp);

                        double xOffset = pp.EastWest - surveyPoint.X;
                        double yOffset = pp.NorthSouth - surveyPoint.Y;
                        double zOffset = pp.Elevation - surveyPoint.Z;
                        try
                        {
                            {
                                parameter.Set(pl.Name);
                                Location location = linkInstance.Location;

                                location.Move(new XYZ(xOffset, yOffset, zOffset));

                                XYZ lp = new XYZ(xOffset, yOffset, zOffset);
                                XYZ cc = new XYZ(lp.X, lp.Y, lp.Z + 10);
                                Line axis = Line.CreateBound(lp, cc);
                                location.Rotate(axis, pp.Angle);

                                Transform currt = linkInstance.GetTotalTransform();
                                XYZ xAxis = XYZ.BasisX;
                                XYZ yAxis = XYZ.BasisY;
                                Transform t = pl.GetTotalTransform();
                                if (t.HasReflection == true)
                                {
                                    FilteredElementCollector collMirrored = new FilteredElementCollector(revitDoc);
                                    ICollection<ElementId> elementsToMirror = collMirrored.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(typeof(RevitLinkInstance)).ToElementIds();
                                    elementsToMirror.Clear();
                                    elementsToMirror.Add(linkInstance.Id);

                                    Plane mirrorplaneX = Plane.CreateByNormalAndOrigin(yAxis, lp);
                                    Plane mirrorplaneY = Plane.CreateByNormalAndOrigin(xAxis, lp);

                                    if (t.BasisX.X < 0 && currt.BasisX.X > 0)
                                    {
                                        ElementTransformUtils.MirrorElements(revitDoc, elementsToMirror, mirrorplaneY, false);
                                    }
                                    if (t.BasisX.X > 0 && currt.BasisX.X < 0)
                                    {
                                        ElementTransformUtils.MirrorElements(revitDoc, elementsToMirror, mirrorplaneY, false);
                                    }
                                    if (t.BasisY.Y < 0 && currt.BasisY.Y > 0)
                                    {
                                        ElementTransformUtils.MirrorElements(revitDoc, elementsToMirror, mirrorplaneX, false);
                                    }
                                    if (t.BasisY.Y > 0 && currt.BasisY.Y < 0)
                                    {
                                        ElementTransformUtils.MirrorElements(revitDoc, elementsToMirror, mirrorplaneX, false);
                                    }
                                }
                                //TaskDialog.Show("Old Location", "Location Name" + docum.ActiveProjectLocation.Name);
                                LinkElementId linkElementId = new LinkElementId(linkInstance.Id, pl.Id);
                                revitDoc.PublishCoordinates(linkElementId);
                                //TaskDialog.Show("New Location", "Location Name" + docum.ActiveProjectLocation.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Exception", "Exception " + ex.Message);
                        }
                    }
                }
                ElementId id = e.Id;
                revitDoc.Delete(id);
            }
        }

        private static void HandleDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            switch (args)
            {
                // (Konrad) Dismiss Unresolved References pop-up.
                case TaskDialogShowingEventArgs args2:
                    if (args2.DialogId == "TaskDialog_Location_Position_Changed")
                        args2.OverrideResult(1001);
                    break;
                default:
                    return;
            }
        }
    }
}

/// <summary>
/// change the current project location
/// </summary>
/// <param name="locationName"></param>

//if (projectLocation.Name == locationName)
//{
//m_application.ActiveUIDocument.Document.ActiveProjectLocation = projectLocation;
//currentLoc = projectLocation;
//    m_currentLocationName = locationName;
//    break;
//}

//void GetData()
//{
//    this.GetLocationData();
//}

//void GetLocationData()
//{
//m_locationnames.Clear();
////ProjectLocation currentLocation = m_application.ActiveUIDocument.Document.ActiveProjectLocation;
////get the current location name
//m_currentLocationName = currentLocation.Name;
////Retrieve all the project locations associated with this project
////ProjectLocationSet locations = m_application.ActiveUIDocument.Document.ProjectLocations;

//ProjectLocationSetIterator iter = linkLocations.ForwardIterator();
//iter.Reset();
//while (iter.MoveNext())
//{
//    ProjectLocation locationTransform = iter.Current as ProjectLocation;
//    string transformName = locationTransform.Name;
//    m_locationnames.Add(transformName); //add the location's name to the list
//}
//}

/// <summary>
/// get the offset values of the project position 
/// </summary>
/// <param name="locationName"></param>
//public void GetOffset(string locationName)
//{
//    ProjectLocationSet locationSet = m_application.ActiveUIDocument.Document.ProjectLocations;
//    foreach (ProjectLocation projectLocation in locationSet)
//    {
//        if (projectLocation.Name == locationName ||
//                    projectLocation.Name + " (current)" == locationName)
//        {
//            Autodesk.Revit.DB.XYZ origin = new Autodesk.Revit.DB.XYZ(0, 0, 0);
//            //get the project position
//            ProjectPosition pp = projectLocation.GetProjectPosition(origin);
//            m_angle = (pp.Angle /= Modulus); //convert to unit degree  
//            m_eastWest = pp.EastWest;     //East to West offset
//            m_northSouth = pp.NorthSouth; //north to south offset
//            m_elevation = pp.Elevation;   //Elevation above ground level
//            break;
//        }
//    }
//        //this.ChangePrecision();
//        //m_angle = UnitConversion.DealPrecision(m_angle, Precision);
//        //m_eastWest = UnitConversion.DealPrecision(m_eastWest, Precision);
//        //m_northSouth = UnitConversion.DealPrecision(m_northSouth, Precision);
//        //m_elevation = UnitConversion.DealPrecision(m_elevation, Precision);
//    }


//public void EditPosition(string locationName, double newAngle, double newEast,
//                       double newNorth, double newElevation)
//{
//    ProjectLocationSet locationSet = m_application.ActiveUIDocument.Document.ProjectLocations;
//    foreach (ProjectLocation location in locationSet)
//    {
//        if (location.Name == locationName ||
//                    location.Name + " (current)" == locationName)
//        {
//            //get the project position
//            Autodesk.Revit.DB.XYZ origin = new Autodesk.Revit.DB.XYZ(0, 0, 0);
//            ProjectPosition projectPosition = location.GetProjectPosition(origin);
//            //change the offset value of the project position
//            projectPosition.Angle = newAngle * Modulus; //convert the unit 
//            projectPosition.EastWest = newEast;
//            projectPosition.NorthSouth = newNorth;
//            projectPosition.Elevation = newElevation;
//            //set the value of the project position
//            location.SetProjectPosition(origin, projectPosition);
//        }
//    }
//}

/// <summary>
/// change the Precision of the value
/// </summary>
//private void ChangePrecision()
//{
//    m_angle = UnitConversion.DealPrecision(m_angle, Precision);
//    m_eastWest = UnitConversion.DealPrecision(m_eastWest, Precision);
//    m_northSouth = UnitConversion.DealPrecision(m_northSouth, Precision);
//    m_elevation = UnitConversion.DealPrecision(m_elevation, Precision);
//}


//docum.ActiveProjectLocation = pl;

//int i = projLoc.IndexOf(pl);
//docum.ActiveProjectLocation = projLoc.ElementAt(i);

//XYZ origin = new XYZ(0, 0, 0);
//ProjectPosition projectPosition = docum.ActiveProjectLocation.GetProjectPosition(origin);
//docum.ActiveProjectLocation.SetProjectPosition(origin, projectPosition);

//ProjectLocation projectLocation = docum.ActiveProjectLocation;
//ProjectPosition projectPosition = projectLocation.GetProjectPosition(origin);
//ProjectPosition newPP = pl.GetProjectPosition(origin);
//projectLocation.SetProjectPosition(origin, newPP);

//ProjectLocation projectLocation = docum.ActiveProjectLocation;
//ProjectPosition projectPosition = projectLocation.GetProjectPosition(origin);
//ProjectPosition plPosition = pl.GetProjectPosition(origin);

//projectPosition.NorthSouth = plPosition.NorthSouth;
//projectPosition.EastWest = plPosition.NorthSouth;
//projectPosition.Elevation = plPosition.NorthSouth;
//projectPosition.Angle = plPosition.Angle;

//ProjectPosition newPP = new ProjectPosition(projectPosition.EastWest, projectPosition.NorthSouth, projectPosition.Elevation, projectPosition.Angle);
//projectLocation.SetProjectPosition(origin, newPP);

//public ProjectLocation GetProjectLocation(Autodesk.Revit.DB.Document document)
//    {
//        XYZ origin = new XYZ(0, 0, 0);

//        ProjectLocation currentLocation = document.ActiveProjectLocation;
//        ProjectPosition projectPosition = currentLocation.GetProjectPosition(origin);

//        ProjectPosition newPosition = pl.GetProjectPosition(origin);
//        if (null != newPosition)
//        {
//            currentLocation.SetProjectPosition(origin, newPosition);
//        }
//        return currentLocation;
//    }


//foreach (Document linkedDoc in revitDoc.Application.Documents)
//{
//    if (linkedDoc.Title.Equals(link.Name))
//    {
//        FilteredElementCollector collLinked = new FilteredElementCollector(linkedDoc);
//        IList<Element> linkLocations = collLinked.OfClass(typeof(ProjectLocation)).ToElements();

//        foreach (Element elLocations in linkLocations)
//        {
//            ProjectLocation linkloc = elLocations as ProjectLocation;
//            s = s + "\n" + elLocations.Name;
//        }
//        TaskDialog.Show("Location Names", linkedDoc.PathName + " : " + s);
//    }
//}


//GEO_LOCATION BuiltinParameter
//The BasePoint whose IsShared property is true represents the survey point.
//Each BasePoint has a Location property that you can set to move the point.
//You cannot currently access the shared location of a Revit or DWG instance via the API.
//You cannot establish a shared - coordinates relationship with a link instance via the API. 
//If the link is already using shared coordinates, then moving the survey point will move the link.

//private void ModifySelectedDoors(UIApplication uiapp, String text, DoorOperation operation)
//        {
//            UIDocument uidoc = uiapp.ActiveUIDocument;

//            // check if there is anything selected in the active document

//            if ((uidoc != null) && (uidoc.Selection != null))
//            {
//                ICollection<ElementId> selElements = uidoc.Selection.GetElementIds();
//                if (selElements.Count > 0)
//                {
//                    // Filter out all doors from the current selection

//                    FilteredElementCollector collector = new FilteredElementCollector(uidoc.Document, selElements);
//                    ICollection<Element> doorset = collector.OfCategory(BuiltInCategory.OST_Doors).ToElements();

//                    if (doorset != null)
//                    {
//                        // Since we'll modify the document, we need a transaction
//                        // It's best if a transaction is scoped by a 'using' block
//                        using (Transaction trans = new Transaction(uidoc.Document))
//                        {
//                            // The name of the transaction was given as an argument

//                            if (trans.Start(text) == TransactionStatus.Started)
//                            {
//                                // apply the requested operation to every door

//                                foreach (FamilyInstance door in doorset)
//                                {
//                                    operation(door);
//                                }

//                                trans.Commit();
//                            }
//                        }
//                    }
//                }
//            }
//        }


//////////////////////////////////////////////////////////////////////////
//
// Helpers - simple delegates operating upon an instance of a door

//private void FlipHandAndFace(FamilyInstance e)
//{
//    e.flipFacing(); e.flipHand();
//}
//// Note: The door orientation [left/right] is according the common
//// conventions used by the building industry in the Czech Republic.
//// If the convention is different in your county (like in the U.S),
//// swap the code of the MakeRight and MakeLeft methods.

//private static void MakeLeft(FamilyInstance e)
//{
//    if (e.FacingFlipped ^ e.HandFlipped) e.flipHand();
//}

//private void MakeRight(FamilyInstance e)
//{
//    if (!(e.FacingFlipped ^ e.HandFlipped)) e.flipHand();
//}

//// Note: The In|Out orientation depends on the position of the
//// wall the door is in; therefore it does not necessary indicates
//// the door is facing Inside, or Outside, respectively.
//// The presented implementation is good enough to demonstrate
//// how to flip a door, but the actual algorithm will likely
//// have to be changes in a read-world application.

//private void TurnIn(FamilyInstance e)
//{
//    if (!e.FacingFlipped) e.flipFacing();
//}

//private void TurnOut(FamilyInstance e)
//{
//    if (e.FacingFlipped) e.flipFacing();
//}
//        }

//    }  // class

//}  // namespace


//public class Link : IExternalCommand
//{
//public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
//{
//    UIDocument uidoc = commandData.Application.ActiveUIDocument;
//    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
//    using (Transaction tr = new Transaction(revitDoc))
//    try
//    {
//    tr.Start("Link files");
//    UserControl userControl = new UserControl() ;

//    foreach (var a in userControl.LinkcheckboxesList.ToList())
//{
//    FileInfo filePath = new FileInfo(a.ChkName);

//    // debug ***********
//    MessageBox.Show("filePath.FullName.ToString() = " + filePath.FullName.ToString());
//    // debug ***********
//    
//        ModelPath mp = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath.FullName.ToString());
//        RevitLinkOptions rlo = new RevitLinkOptions(false);
//        var linkType = RevitLinkType.Create(revitDoc, mp, rlo);
//        var instance = RevitLinkInstance.Create(revitDoc, linkType.ElementId, ImportPlacement.Origin);
//    }
//    tr.Commit();
//    return Autodesk.Revit.UI.Result.Succeeded;
//    }
//    catch (Exception ex)
//    {
//        message = ex.Message;
//        return Result.Failed;
//    }
