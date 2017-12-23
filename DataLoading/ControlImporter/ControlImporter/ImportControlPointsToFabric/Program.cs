/*
 Copyright 1995-2017 Esri

 All rights reserved under the copyright laws of the United States.

 You may freely redistribute and use this sample code, with or without modification.

 Disclaimer: THE SAMPLE CODE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED 
 WARRANTIES, INCLUDING THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
 FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESRI OR 
 CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 INTERRUPTION) SUSTAINED BY YOU OR A THIRD PARTY, HOWEVER CAUSED AND ON ANY 
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT ARISING IN ANY 
 WAY OUT OF THE USE OF THIS SAMPLE CODE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 SUCH DAMAGE.

 For additional information contact: Environmental Systems Research Institute, Inc.

 Attn: Contracts Dept.

 380 New York Street

 Redlands, California, U.S.A. 92373 

 Email: contracts@esri.com
*/

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Cadastral;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.GeoDatabaseExtensions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;

using System;
using System.Collections.Generic;
using System.Text;

namespace ImportControlPointsToFabric
{
  class Program
  {
    private static LicenseInitializer m_AOLicenseInitializer = new ImportControlPointsToFabric.LicenseInitializer();

    [STAThread()]
    static void Main(string[] args)
    {

      //arg 0 = source layer string file gdb string with feature class name
      //arg 1 = target layer path for fabric layer
      //arg 2 = control point tolerance (in projection units) to match with existing fabric points, -1 means don't do it
      //arg 3 = merge tolerance. Merge with existing control points if within tolerance, -1 means turn off merging
      //arg 4 = control merging choices for attributes [KeepExistingAttributes | UpdateExistingAttributes]
      //arg 5 = must have same name to merge if within the tolerance? [NamesMustMatchToMerge | IgnoreNames] 
      //arg 6 = if control is merged keep existing names or update with incoming names? [KeepExistingNames | UpdateExistingNames]
      //.....(arg 6 is ignored if arg 5 = NamesMustMatchToMerge) 
      //arg 7 = control merging choices for coordinates [UpdateXY | UpdateXYZ | UpdateZ | KeepExistingXYZ]
      //arg 8 = create a log file at the same location as the executable? [LoggingOn | LoggingOff]
      //.....(a log file is always generated unless LoggingOff is explcitly used) 

      //ESRI License Initializer generated code.
      m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeStandard, esriLicenseProductCode.esriLicenseProductCodeAdvanced },
      new esriLicenseExtensionCode[] { });
      //ESRI License Initializer generated code.
      //Do not make any call to ArcObjects after ShutDownApplication()
      int iLen = args.Length;
      if (iLen < 2)
      {
        UsageMessage();
        m_AOLicenseInitializer.ShutdownApplication();
        return;
      }

      ITrackCancel pTrkCan = new CancelTrackerClass();
      // Create and display the Progress Dialog
      IProgressDialogFactory pProDlgFact = new ProgressDialogFactoryClass();
      IProgressDialog2 pProDlg = pProDlgFact.Create(pTrkCan, 0) as IProgressDialog2;

      try
      {
        ICadastralControlImporter pControlPointCadastralImp = new CadastralControlImporterClass();

        // args[0] ============================================
        string PathToFileGDBandFeatureClass = args[0];

        if (args[0].Contains(":") && args[0].Contains("\\"))
        {
          PathToFileGDBandFeatureClass = args[0];
        }
        else
        {
          string[] sExecPathArr = System.Reflection.Assembly.GetEntryAssembly().Location.Split('\\');
          string x = "";
          for (int i = 0; i < (sExecPathArr.Length - 1); i++)
            x += sExecPathArr[i] + "\\";
          
          PathToFileGDBandFeatureClass = x + PathToFileGDBandFeatureClass;
        }

        string[] PathToFileGDBArray = PathToFileGDBandFeatureClass.Split('\\');
        string NameOfSourceFeatureClass = PathToFileGDBArray[PathToFileGDBArray.Length - 1];
        string PathToFileGDB = "";

        for (int i = 0; i < (PathToFileGDBArray.Length - 1); i++)
          PathToFileGDB += PathToFileGDBArray[i] + "\\";

        PathToFileGDB = PathToFileGDB.TrimEnd('\\');

        if (!System.IO.Directory.Exists(PathToFileGDB))
        { throw new Exception("File does not exist. [" + PathToFileGDB + "]"); }


        // args[1] ============================================

        string layerFilePathToFabric = args[1];

        if (!(layerFilePathToFabric.Contains(":") && layerFilePathToFabric.Contains("\\")))
        {
          string[] sExecPathArr = System.Reflection.Assembly.GetEntryAssembly().Location.Split('\\');
          string x = "";
          for (int i = 0; i < (sExecPathArr.Length - 1); i++)
            x += sExecPathArr[i] + "\\";

          layerFilePathToFabric = x + layerFilePathToFabric;
        }
        
        if (!System.IO.File.Exists(layerFilePathToFabric))
        { throw new Exception("File does not exist. [" + layerFilePathToFabric + "]"); }

        ILayerFile layerFileToTargetFabric = new LayerFileClass();
        layerFileToTargetFabric.Open(layerFilePathToFabric);
        ILayer pLayer = layerFileToTargetFabric.Layer;
        ICadastralFabricLayer pParcelFabLyr = pLayer as ICadastralFabricLayer;
        ICadastralFabric pParcelFabric = pParcelFabLyr.CadastralFabric;
        IDataset pDS = pParcelFabric as IDataset;
        IName pDSName = pDS.FullName;
        ICadastralFabricName pCadastralFabricName = pDSName as ICadastralFabricName;

        IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactory();
        IFeatureWorkspace pFWS = (IFeatureWorkspace)workspaceFactory.OpenFromFile(PathToFileGDB, 0);
        pDS = (IDataset)pFWS.OpenFeatureClass(NameOfSourceFeatureClass);
        IName pSourceFeatClassName = pDS.FullName;

        //  args[2] ============================================
        bool bHasControlTolerance = iLen > 2;
        double dControlToFabricPointMatchTolerance = -1;
        if (bHasControlTolerance)
        {
          if (!Double.TryParse(args[2], out dControlToFabricPointMatchTolerance))
          { throw new Exception("The third parameter should be a numeric value. [" + args[2] + "]"); }
        }
        pControlPointCadastralImp.ControlPointTolerance = dControlToFabricPointMatchTolerance;
        //'***** performance, -1 means that matching to existing fabric points is turned off
        pControlPointCadastralImp.UseShapeField = true;

        //============= Arguments for merging control points ===============
        // args[3] [4] [5] [6] [7] ============================================
        bool bIsMergingControlPoints = (iLen == 7);
        if (bIsMergingControlPoints)
        {
          //arg 3 = merge tolerance. Merge with existing control points if within tolerance, -1 means turn off merging
          //arg 4 = control merging choices for attributes [KeepExistingAttributes | UpdateExistingAttributes]
          //arg 5 = must have same name to merge if within the tolerance? [NamesMustMatchToMerge | IgnoreNames] 
          //arg 6 = if control is merged keep existing names or update with incoming names? [KeepExistingNames | UpdateExistingNames]
          //.....(arg 6 is ignored if arg 5 = NamesMustMatchToMerge) 
          //arg 7 = control merging choices for coordinates [UpdateXY | UpdateXYZ | UpdateZ | KeepExistingXYZ]

          double dControlPointMergingTolerance = -1;
          if(!Double.TryParse(args[3], out dControlPointMergingTolerance))
          {
            { throw new Exception("The fourth parameter should be a numeric value. [" + args[3] + "]"); }
          }
          ICadastralControlImporterMerging pImpMerge = pControlPointCadastralImp as ICadastralControlImporterMerging;
          pImpMerge.MergeCloseControl = dControlPointMergingTolerance > 0;
          pImpMerge.CloseControlTolerance = dControlPointMergingTolerance;
          if(args[4].ToLower().Contains("updateexistingattributes"))
            pImpMerge.MergeAttributesOption = esriCFControlMergingAttributes.esriCFControlMergingUpdateAttributes;
          else
            pImpMerge.MergeAttributesOption = esriCFControlMergingAttributes.esriCFControlMergingKeepAttributes;

          pImpMerge.MergeControlNameCaseSensitive = false;

          pImpMerge.MergeControlSameName = args[5].ToLower().Contains("namesmustmatchtomerge");

          if (args[6].ToLower().Contains("updateexistingnames") && !args[5].ToLower().Contains("namesmustmatchtomerge"))
            pImpMerge.MergeNameOption = esriCFControlMergingName.esriCFControlMergingUpdateExistingNames;
          else
            pImpMerge.MergeNameOption = esriCFControlMergingName.esriCFControlMergingKeepExistingNames;

          if (args[7].ToLower()=="updatexy")
            pImpMerge.MergeCoordinateOption = esriCFControlMergingCoordinate.esriCFControlMergingUpdateXY;
          else if(args[7].ToLower()=="updatexyz")
            pImpMerge.MergeCoordinateOption = esriCFControlMergingCoordinate.esriCFControlMergingUpdateXYZ;
          else if (args[7].ToLower() == "updatez")
            pImpMerge.MergeCoordinateOption = esriCFControlMergingCoordinate.esriCFControlMergingUpdateZ;
          else if (args[7].ToLower() == "keepexistingxyz")
            pImpMerge.MergeCoordinateOption = esriCFControlMergingCoordinate.esriCFControlMergingKeepExistingCoordinates;

        }

        ICadastralImporter pControlImporter = (ICadastralImporter)pControlPointCadastralImp;

        bool bHasExplicitLogFileParameter = (iLen > 8);
        bool bExplicitTurnLoggingOff = false;
        /// Argument for logging importer results
        //arg 8 = create a log file at the same location as the executable? [LoggingOn | LoggingOff | <path to logfile>]
        //.....(a log file is always generated unless LoggingOff is explcitly used) 

        string sLogFilePath = "LogControlImport";
        if (bHasExplicitLogFileParameter)
        {
          if (args[8].ToLower() == "loggingoff")
            bExplicitTurnLoggingOff = true;
          if (args[8].ToLower() != "loggingon" && !bExplicitTurnLoggingOff)
          {
            if (args[8].Contains(":") && args[8].Contains("\\")) //if (args[8].ToLower() != "loggingon")
              sLogFilePath = args[8];
            else
            {
              string[] sExecPathArr = System.Reflection.Assembly.GetEntryAssembly().Location.Split('\\');
              string x = "";
              for (int i = 0; i < (sExecPathArr.Length - 1); i++)
                x += sExecPathArr[i] + "\\";
              sLogFilePath = x + args[8];
            }            
          }
        }
        else
        {
          string[] sExecPathArr = System.Reflection.Assembly.GetEntryAssembly().Location.Split('\\');
          string x = "";
          for (int i = 0; i < (sExecPathArr.Length - 1); i++)
            x += sExecPathArr[i] + "\\";
          sLogFilePath = x + sLogFilePath;
        }
        sLogFilePath += ".log";

        if (!bExplicitTurnLoggingOff)
          pControlImporter.OutputLogfile = sLogFilePath; //default location is same as executable
        

        //Set the properties of the Progress Dialog
        pProDlg.CancelEnabled = false;
        pProDlg.Description = "Importing Control Point data ...";
        pProDlg.Title = "Importing Control Points";
        pProDlg.Animation = esriProgressAnimationTypes.esriProgressGlobe;

        Console.WriteLine("Starting data load...");
        pControlImporter.Import(pSourceFeatClassName, pCadastralFabricName, pTrkCan); //BUG fails if the TrackCancel is null
        Console.WriteLine("Finished data load...");
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        //UsageMessage();
      }
      finally
      {
        if (pProDlg != null)
          pProDlg.HideDialog();
        m_AOLicenseInitializer.ShutdownApplication();
      }

    }


    static void UsageMessage()
    {
      Console.WriteLine("-----------------------------------------------------------------");
      Console.WriteLine("Syntax:");
      Console.WriteLine("ImportControlPointsToFabric [0] [1] [2] [3] [4] [5] [6] [7] [8]");
      Console.WriteLine("-----------------------------------------------------------------");
      Console.WriteLine("Expecting parameters as follows:");
      Console.WriteLine("");
      Console.WriteLine("[0] = source file gdb string path with feature class name (required)");
      Console.WriteLine("Example->  c:" + "\\" + "myfgdb.gdb" + "\\" + "myfeatureclass");
      Console.WriteLine("");
      Console.WriteLine("[1] = target layer path for fabric layer (required)");
      Console.WriteLine("Example-> c:" + "\\" + "myfabriclayer.lyr");
      Console.WriteLine("");
      Console.WriteLine("[2] = control point tolerance (in projection units) to match with existing fabric points (optional)");
      Console.WriteLine("-1 means don't associate incoming control to fabric points");
      Console.WriteLine("");
      Console.WriteLine("[3] = merge tolerance. Merge with existing control points if within tolerance (optional)");
      Console.WriteLine("Leave off parameters 3, 4, 5, 6, 7, 8 if not merging");
      Console.WriteLine("");
      Console.WriteLine("[4] = attribute merging choices (required if parameter 3 exists)");
      Console.WriteLine("[KeepExistingAttributes | UpdateExistingAttributes]");
      Console.WriteLine("");
      Console.WriteLine("[5] = choice for how control name matching is used (required if parameter 3 exists)");
      Console.WriteLine("[NamesMustMatchToMerge | IgnoreNames]");
      Console.WriteLine("");
      Console.WriteLine("[6] = if control is merged keep existing names or update with incoming names?");
      Console.WriteLine("[KeepExistingNames | UpdateExistingNames]");
      Console.WriteLine("");
      Console.WriteLine(".....(arg 6 is ignored if arg 5 = NamesMustMatchToMerge)");
      Console.WriteLine("-----------------------------------------------------------------");
      Console.WriteLine("");
      Console.WriteLine("[7] = control merging choices for coordinates (required if parameter 3 exists)");
      Console.WriteLine("[UpdateXY | UpdateXYZ | UpdateZ | KeepExistingXYZ]");
      Console.WriteLine("-----------------------------------------------------------------");
      Console.WriteLine("");
      Console.WriteLine("[8] =  create a log file at the same location as the executable (optional: default creates a log file at location of executable)");
      Console.WriteLine("[LoggingOn| LoggingOff| <string path to logfile including its name>]");
      Console.WriteLine("");
      Console.WriteLine(".....(a log file is always generated unless LoggingOff is explcitly used)");
      Console.WriteLine("-----------------------------------------------------------------");
      Console.WriteLine("");
      Console.WriteLine("Example 1: ImportControlPointsToFabric c:\\" + "myfgdb.gdb" + "\\" + "MycontrolPointsFC c:" + "\\" + "myfabriclayer.lyr");
      Console.WriteLine("");
      Console.WriteLine("Example 2: ImportControlPointsToFabric c:\\" + "myfgdb.gdb" + "\\" + "MycontrolPointsFC c:" + "\\" + "myfabriclayer.lyr -1 0.2 UpdateExistingAttributes NamesMustMatchToMerge KeepExistingNames UpdateXY");
      Console.WriteLine("");
      Console.WriteLine("Example 3: ImportControlPointsToFabric c:\\" + "myfgdb.gdb" + "\\" + "MycontrolPointsFC c:" + "\\" + "myfabriclayer.lyr -1 0.2 KeepExistingAttributes NamesMustMatchToMerge KeepExistingNames UpdateXYZ c:\\" + "mylogfile");
      Console.WriteLine("-----------------------------------------------------------------");
    }

  }
}
