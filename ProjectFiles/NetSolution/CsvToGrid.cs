#region Using directives
using System;
using FTOptix.Core;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.OPCUAServer;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.NetLogic;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FTOptix.CommunicationDriver;
using FTOptix.Modbus;
#endregion

public class CsvToGrid : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void saveCSVfile()
    {
        string fileSeparator = LogicObject.GetVariable("CsvSeparator").Value;
        var filePath = LogicObject.GetVariable("CsvFile");
        FTOptix.Core.ResourceUri filePathValue = new FTOptix.Core.ResourceUri(filePath.Value);
        ColumnLayout myVL = Owner.Find<ColumnLayout>("VerticalLayout1");
        RowLayout myRL = null;
        EditableLabel myCell;
        int numRow = 0;
        int numCol = 0;
        string fileRowValue = "";
        
        using (var writer = new StreamWriter(filePathValue.Uri))
        {
            foreach (var itemCol in myVL.Children.OfType<RowLayout>())
            {
                if (numRow > 0)
                {
                    numCol = 0;
                    myRL = itemCol;
                    foreach (var itemRow in myRL.Children.OfType<Rectangle>())
                    {
                        if (numCol > 0)
                        {
                            myCell = (EditableLabel)itemRow.Find("CGEL" + numRow.ToString() + "R" + numCol.ToString());
                            if (numCol > 1)
                                fileRowValue = fileRowValue + fileSeparator + myCell.Text;
                            else
                                fileRowValue = myCell.Text;
                        }
                        numCol++;
                    }
                    writer.WriteLine(fileRowValue);
                }
                numRow++;
            }
            writer.Close();
        }
    }

    [ExportMethod]
    public void readCsvFile()
    {
        FTOptix.UI.Rectangle CellaGriglia;
        FTOptix.UI.EditableLabel CellaGrigliaEditableLabel;
        FTOptix.UI.Label CellaGrigliaLabel;
        FTOptix.UI.RowLayout RigaGriglia;
        ColumnLayout myVL = Owner.Find<ColumnLayout>("VerticalLayout1");
        ScrollView mySV = Owner.Find<ScrollView>("ScrollView1");
        //mySV.Visible = false;
        myVL.Children.Clear();

        var filePath = LogicObject.GetVariable("CsvFile");
        FTOptix.Core.ResourceUri filePathValue = new FTOptix.Core.ResourceUri(filePath.Value);
        string fileSeparator = LogicObject.GetVariable("CsvSeparator").Value;
        char[] characters = fileSeparator.ToCharArray();
        
        int rowNumber = 0;
        int colNumber = 0;
        
        var lineCount = File.ReadLines(filePathValue.Uri).Count();
        myVL.Height = (lineCount + 1) * 30;
        
        using (var reader = new StreamReader(filePathValue.Uri))
        {

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(characters[0]);
                colNumber = values.Length;

                if (rowNumber == 0)
                {
                    myVL.Width = (colNumber + 1) * 100;
                    RigaGriglia = InformationModel.MakeObject<RowLayout>("RigaGriglia" + rowNumber.ToString());
                    for (int j = 0; j <= colNumber; j++)
                    {
                        CellaGriglia = InformationModel.MakeObject<Rectangle>("COL " + j.ToString());
                        CellaGriglia.FillColor = Colors.White;
                        CellaGriglia.Width = 100;
                        CellaGriglia.Height = 30;
                        CellaGriglia.BorderThickness = 1;
                        CellaGriglia.Visible = true;
                        CellaGrigliaLabel = InformationModel.MakeObject<Label>("COL " + j.ToString());
                        if (j == 0)
                            CellaGrigliaLabel.Text = "---";
                        else
                            CellaGrigliaLabel.Text = "COL " + j.ToString();
                        CellaGrigliaLabel.VerticalAlignment = VerticalAlignment.Center;
                        CellaGrigliaLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        CellaGriglia.Children.Add(CellaGrigliaLabel);
                        RigaGriglia.Children.Add(CellaGriglia);
                    }
                    myVL.Children.Add(RigaGriglia);
                }
                RigaGriglia = InformationModel.MakeObject<RowLayout>("RigaGriglia" + rowNumber.ToString());
                rowNumber++;
                for (int j = 0; j <= colNumber; j++)
                {
                    CellaGriglia = InformationModel.MakeObject<Rectangle>("CG" + rowNumber.ToString() + "R" + j.ToString());
                    CellaGriglia.FillColor = Colors.White;
                    CellaGriglia.Width = 100;
                    CellaGriglia.Height = 30;
                    CellaGriglia.BorderThickness = 1;
                    CellaGriglia.Visible = true;
                    if (j == 0)
                    {
                        CellaGrigliaLabel = InformationModel.MakeObject<Label>("CGEL" + rowNumber.ToString() + "R" + j.ToString());
                        CellaGrigliaLabel.Text = "ROW " + rowNumber.ToString();
                        CellaGrigliaLabel.VerticalAlignment = VerticalAlignment.Center;
                        CellaGrigliaLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        CellaGriglia.Children.Add(CellaGrigliaLabel);
                        RigaGriglia.Children.Add(CellaGriglia);
                    }
                    else
                    {
                        CellaGrigliaEditableLabel = InformationModel.MakeObject<EditableLabel>("CGEL" + rowNumber.ToString() + "R" + j.ToString());
                        CellaGrigliaEditableLabel.Text = values[j - 1];
                        CellaGrigliaEditableLabel.VerticalAlignment = VerticalAlignment.Center;
                        CellaGrigliaEditableLabel.HorizontalAlignment = HorizontalAlignment.Center;
                        CellaGriglia.Children.Add(CellaGrigliaEditableLabel);
                        RigaGriglia.Children.Add(CellaGriglia);
                    }
                    
                }
                myVL.Children.Add(RigaGriglia);
            }
        }
    }
}
