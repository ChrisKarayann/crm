/* #################################  - CMR - #################################
 * ###### SIMPLE UTILITY TO MANIPULATE DATA FROM FRUIT CUTTING MACHINES #######
 * ----------------------------------------------------------------------------
 * ----------------------- Chris Karayannidis - 2021 --------------------------
 * ######################################################################### */
 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;

// PROGRAM BEGINS

namespace d_interceptor
{
    public partial class Form1 : Form
    {
        string filename = ""; // The path of the file to capture - including the actual filename
        bool shouldStop = false; //stop button can not act by default
        public int counter = 0; // Set a counter - it counts lines from current.txt, then I split the info line (below)
        string newTime = "0";

        /* DECLARE THE VARIABLES WE NEED - CURRENT*/
        string Fruit, networkNumber, machineState;

        int currentFeedPerCent, 
            currentSpoonPerCent, 
            currentReworkPerCent, 
            peachesSinceLHHFileWritten, 
            machineCyclesSinceLHHFileWritten, 
            spoonPeachesSinceLHHFileWritten,
            ReworkPeachesSinceLHHFileWritten,
            machineRuntimeSinceLHHFileWritten;
        /*-------------------------------*/

        /* DECLARE THE OTHER VARIABLES WE NEED - COMPENSATED*/
        string compensatedFruit, compensatedNetworkNumber, compensatedMachineState;

        int compensatedCurrentFeedPerCent,
            compensatedCurrentSpoonPerCent,
            compensatedCurrentReworkPerCent,
            compensatedPeachesSinceLHHFileWritten,
            compensatedMachineCyclesSinceLHHFileWritten,
            compensatedSpoonPeachesSinceLHHFileWritten,
            compensatedReworkPeachesSinceLHHFileWritten,
            compensatedMachineRuntimeSinceLHHFileWritten;
        /*-------------------------------*/

        public Form1()
        {
            InitializeComponent();
            // Add other Initializers here  
            //dataXmlCreator(); // Create the predata.xml -  the file to compare
            xmlLinqCreator(); // Create the predata.xml -  the file to compare
            ///xmlAppender();

        }

        // The button that opens the dialog to browse the path to file
        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
            filename = openFileDialog2.FileName;
            if (filename != null)
            {
                FilePathTextBox.Text = filename;
            }
         }

        // The button that starts gathering data - if some conditions are met
        private void StartButton_Click(object sender, EventArgs e)
        {
            //richTextBox2.Text = "output";
            timer1.Enabled = true; // THE TIMER THAT POLLS THE CURRENT.TXT FOR NEW DATA ENTRIES
            DataOutputTextBox.AppendText("started..." + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
            compensatedDataTextBox.AppendText("started...waiting for incoming data to calculate..." + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
            // Lock the buttons that can confuse us
            OpenFileButton.Enabled = false; 
            StartButton.Enabled = false;
        }

        // The timer that pools the file every n intervals - The actual Interceptor
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                // Open a filestreamer without locking the file
                using (StreamReader reader = new StreamReader(new FileStream(@filename,
                                 FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {

                    string line = "";

                    if ((line = reader.ReadLine()) != null)
                    {
                        counter = 0; // Reset the counter
                        char[] delimiters = new char[] { '\t' }; // Tell me...what is the delimiter character
                        string[] infoLineParts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries); // Split the info in tabs
                        newTime = infoLineParts[2];

                        if (newTime != timeOfLastCaptureLabel.Text) // Is the time read newer than the last one?
                        {
                            DataOutputTextBox.AppendText("TIME: " + infoLineParts[2] + " --- " + "DATE: " + infoLineParts[1] + "\n"); // Output date and time for ease of visual reference
                            compensatedDataTextBox.AppendText("- Difference to previous minute -" + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
                            timeOfLastCaptureLabel.Text = infoLineParts[2]; // Report time in label and also for reference
                            counter++; // Increment the counter by one to skip the timestamp of current.txt - Skip to actual data lines

                            //xmlResetter(); // Reset the predata.xml - clean all entries

                            // FROM THIS LINE ON THE DATA READ ARE THE ACTUAL HEADS DATA - TIMESTAMP SKIPPED!
                            while ((line = reader.ReadLine()) != null)
                            {
                                char[] adataDelimiters = new char[] { '\t' };
                                string[] actualDataParts = line.Split(adataDelimiters, StringSplitOptions.RemoveEmptyEntries);

                                /* ====== MY DATA TO WORK WITH ====== */
                                Fruit = actualDataParts[0];
                                networkNumber = actualDataParts[1];
                                machineState = actualDataParts[2];
                                currentFeedPerCent = Int32.Parse(actualDataParts[3]);
                                currentSpoonPerCent = Int32.Parse(actualDataParts[4]);
                                currentReworkPerCent = Int32.Parse(actualDataParts[5]);
                                peachesSinceLHHFileWritten = Int32.Parse(actualDataParts[6]);
                                machineCyclesSinceLHHFileWritten = Int32.Parse(actualDataParts[7]);
                                spoonPeachesSinceLHHFileWritten = Int32.Parse(actualDataParts[8]);
                                ReworkPeachesSinceLHHFileWritten = Int32.Parse(actualDataParts[9]);
                                machineRuntimeSinceLHHFileWritten = Int32.Parse(actualDataParts[10]);
                                /* ================================== */

                                // FIRST APPEND THE DATA TO CURRENT DATA OUTPUT BOX
                                DataOutputTextBox.AppendText("F: " + Fruit + "  |  "  + 
                                    "NN: " + networkNumber + "  |  " +
                                    "MS: " + machineState + "  ||  " + 
                                    "CPF: " + currentFeedPerCent + "   " + 
                                    "CPS: " + currentSpoonPerCent + "   " + 
                                    "CPR: " + currentReworkPerCent + "   " + 
                                    "PSLH: " + peachesSinceLHHFileWritten + "   " + 
                                    "MCSLH: " + machineCyclesSinceLHHFileWritten + "   " +
                                    "SPSLH: " + spoonPeachesSinceLHHFileWritten + "   " + 
                                    "RPSLH: " + ReworkPeachesSinceLHHFileWritten + "   " + 
                                    "MRTSLH: " + machineRuntimeSinceLHHFileWritten + "   " 
                                    + "\n");
                                DataOutputTextBox.ScrollToCaret();

                                /*
                                // ...THEN APPEND THE DATA -COMPENSATED (DIFFERENTIATED) TO THE COMPENSATED DATA BOX
                                compensatedDataTextBox.AppendText("F: " + Fruit + "   " +
                                   "NN: " + networkNumber + "   " +
                                   "MS: " + machineState + "   " +
                                   "CPF: " + currentFeedPerCent + "   " +
                                   "CPS: " + currentSpoonPerCent + "   " +
                                   "CPR: " + currentReworkPerCent + "   " +
                                   "PSLH: " + peachesSinceLHHFileWritten + "   " +
                                   "MCSLH: " + machineCyclesSinceLHHFileWritten + "   " +
                                   "SPSLH: " + spoonPeachesSinceLHHFileWritten + "   " +
                                   "RPSLH: " + ReworkPeachesSinceLHHFileWritten + "   " +
                                   "MRTSLH: " + machineRuntimeSinceLHHFileWritten + "   "
                                   + "\n");
                                   */
                                // DO WHATEVER THEN
                                xmlAppender(); // After calculations is done , no more we need the old data
                                counter++;
                            }
                            numberOfHeadsLabel.Text = (counter -1).ToString();
                        }

                    }
                    reader.Close();  // Release the system
                }

                shouldStop = true; // Procedure has started succesfully and stop button can act
            }

            catch
            {
                timer1.Enabled = false;
                DataOutputTextBox.AppendText("stopped..." + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
                compensatedDataTextBox.AppendText("stopped..." + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
                MessageBox.Show("Path cannot be empty");
                // Unlock the buttons and reset 
                OpenFileButton.Enabled = true;
                StartButton.Enabled = true;
            }
            
        }

        // The stop procedure - capturing button
        private void StopButton_Click(object sender, EventArgs e)
        {
            if (shouldStop) // Can stop button be pressed?
            {
                timer1.Enabled = false; // Stop the timer thread
                DataOutputTextBox.AppendText("stopped..." + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
                compensatedDataTextBox.AppendText("stopped..." + DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "\n");
                shouldStop = false; 
                timeOfLastCaptureLabel.Text = "HHMM";
                numberOfHeadsLabel.Text = "-";
                // Re-enable starting procedure buttons 
                OpenFileButton.Enabled = true;
                StartButton.Enabled = true;
            } else
            {
                MessageBox.Show("Can't stop what never started!"); // Why to press stop if never started?
            }
            
        }

        // Exits all and close program thread 
        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /* WILL NOT BE USED
        // Create an xml document that holds the previous minute data 
        // in order to compare it when receive the "fresh" data
        // CREATED WHEN PROGRAM STARTS!
        private void dataXmlCreator()
        {

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            XmlWriter writer = XmlWriter.Create("predata.xml", xmlSettings);

            writer.WriteStartDocument();
           {
                writer.WriteComment("This is an application(CMR) generated file holding the immediately previous data to");
                writer.WriteComment("compare them with the last captured data" );
                writer.WriteComment("For each machine network number there is a corresponding");
                writer.WriteComment("entry to be used as ID");
                writer.WriteStartElement("Entries");
                writer.WriteEndElement();
                writer.Flush();
                writer.Close();
           }
            
            
        }
        */

        // Every capture, after xmlResetter() I append the new data and hold them until 
        // the next capture.
        private void xmlAppender()
        {
            XDocument predata = XDocument.Load("predata.xml");

            bool machineExists = predata.Descendants("MACHINE" + counter.ToString()).Any(); // If there is ANY number of machine entries...delete

            if (!machineExists) // If ANY number of machines DO NOT exists, add them! This will happen once when the program first starts for example.
            {
                /* --- SET VALUES TO COMPENSATED DATA BY CALCULATING NOTHING CAUSE IT'S THE FIRST READING - ALL COMPENSATIONS SHOULD BE ZERO - SELF SUBTRACT --- */
                compensatedFruit = Fruit;
                compensatedNetworkNumber = networkNumber;
                compensatedMachineState = machineState;
                compensatedCurrentFeedPerCent = currentFeedPerCent - currentFeedPerCent;
                compensatedCurrentSpoonPerCent = currentSpoonPerCent - currentSpoonPerCent;
                compensatedCurrentReworkPerCent = currentReworkPerCent - currentReworkPerCent;
                compensatedPeachesSinceLHHFileWritten = peachesSinceLHHFileWritten - peachesSinceLHHFileWritten;
                compensatedMachineCyclesSinceLHHFileWritten = machineCyclesSinceLHHFileWritten - machineCyclesSinceLHHFileWritten;
                compensatedSpoonPeachesSinceLHHFileWritten = spoonPeachesSinceLHHFileWritten - spoonPeachesSinceLHHFileWritten;
                compensatedReworkPeachesSinceLHHFileWritten = ReworkPeachesSinceLHHFileWritten - ReworkPeachesSinceLHHFileWritten;
                compensatedMachineRuntimeSinceLHHFileWritten = machineRuntimeSinceLHHFileWritten - machineRuntimeSinceLHHFileWritten;
                /* -------------------------------------------------------------------------------------------- */

                // ...THEN APPEND THE DATA -COMPENSATED (DIFFERENTIATED) TO THE COMPENSATED DATA BOX
                compensatedDataTextBox.AppendText("F: " + compensatedFruit + "  |  " +
                   "NN: " + compensatedNetworkNumber + "  |  " +
                   "MS: " + compensatedMachineState + "  ||  " +
                   "CPF: " + compensatedCurrentFeedPerCent + "   " +
                   "CPS: " + compensatedCurrentSpoonPerCent + "   " +
                   "CPR: " + compensatedCurrentReworkPerCent + "   " +
                   "PSLH: " + compensatedPeachesSinceLHHFileWritten + "   " +
                   "MCSLH: " + compensatedMachineCyclesSinceLHHFileWritten + "   " +
                   "SPSLH: " + compensatedSpoonPeachesSinceLHHFileWritten + "   " +
                   "RPSLH: " + compensatedReworkPeachesSinceLHHFileWritten + "   " +
                   "MRTSLH: " + compensatedMachineRuntimeSinceLHHFileWritten + "   "
                   + "\n");
                compensatedDataTextBox.ScrollToCaret();

                XElement entries = predata.Element("Entries");
                entries.Add(new XElement("MACHINE" + counter.ToString(),
                new XElement("F", Fruit),
                new XElement("NN", networkNumber),
                new XElement("MS", machineState),
                new XElement("CPF", currentFeedPerCent),
                new XElement("CPS", currentSpoonPerCent),
                new XElement("CPR", currentReworkPerCent),
                new XElement("PSLH", peachesSinceLHHFileWritten),
                new XElement("MCSLH", machineCyclesSinceLHHFileWritten),
                new XElement("SPSLH", spoonPeachesSinceLHHFileWritten),
                new XElement("RPSLH", ReworkPeachesSinceLHHFileWritten),
                new XElement("MRTSLH", machineRuntimeSinceLHHFileWritten),
                new XElement("TIME", newTime)));

                predata.Save("predata.xml");

            } else //...we have some records here so first I calculate their values to get compensated data, then replace the values in the nodes with the fresh ones.
            {
                /* --- SET VALUES TO COMPENSATED DATA BY CALCULATING XML DATA (OLD) WITH FRESH INCOMING DATA --- */
                compensatedFruit = Fruit;
                compensatedNetworkNumber = networkNumber;
                compensatedMachineState = machineState;
                compensatedCurrentFeedPerCent = currentFeedPerCent - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("CPF").First().Value);
                compensatedCurrentSpoonPerCent = currentSpoonPerCent - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("CPS").First().Value);
                compensatedCurrentReworkPerCent = currentReworkPerCent - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("CPR").First().Value);
                compensatedPeachesSinceLHHFileWritten = peachesSinceLHHFileWritten - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("PSLH").First().Value);
                compensatedMachineCyclesSinceLHHFileWritten = machineCyclesSinceLHHFileWritten - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("MCSLH").First().Value);
                compensatedSpoonPeachesSinceLHHFileWritten = spoonPeachesSinceLHHFileWritten - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("SPSLH").First().Value);
                compensatedReworkPeachesSinceLHHFileWritten = ReworkPeachesSinceLHHFileWritten - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("RPSLH").First().Value);
                compensatedMachineRuntimeSinceLHHFileWritten = machineRuntimeSinceLHHFileWritten - Int32.Parse(predata.Elements("Entries").Elements("MACHINE" + counter.ToString()).Elements("MRTSLH").First().Value);
                /* -------------------------------------------------------------------------------------------- */

                // ...THEN APPEND THE DATA -COMPENSATED (DIFFERENTIATED) TO THE COMPENSATED DATA BOX
                compensatedDataTextBox.AppendText("F: " + compensatedFruit + "   " +
                   "NN: " + compensatedNetworkNumber + "   " +
                   "MS: " + compensatedMachineState + "   " +
                   "CPF: " + compensatedCurrentFeedPerCent + "   " +
                   "CPS: " + compensatedCurrentSpoonPerCent + "   " +
                   "CPR: " + compensatedCurrentReworkPerCent + "   " +
                   "PSLH: " + compensatedPeachesSinceLHHFileWritten + "   " +
                   "MCSLH: " + compensatedMachineCyclesSinceLHHFileWritten + "   " +
                   "SPSLH: " + compensatedSpoonPeachesSinceLHHFileWritten + "   " +
                   "RPSLH: " + compensatedReworkPeachesSinceLHHFileWritten + "   " +
                   "MRTSLH: " + compensatedMachineRuntimeSinceLHHFileWritten + "   "
                   + "\n");
                compensatedDataTextBox.ScrollToCaret();

                // ... THEN RESET THE XML AND START OVER
                predata.Descendants("Entries").Elements(). //...Go to "Entries" node
                Where(r => r.Name == "MACHINE" + counter.ToString()).Remove(); //...Search for each "MACHINE"+No element and delete it


                XElement entries = predata.Element("Entries");
                entries.Add(new XElement("MACHINE" + counter.ToString(),
                new XElement("F", Fruit),
                new XElement("NN", networkNumber),
                new XElement("MS", machineState),
                new XElement("CPF", currentFeedPerCent),
                new XElement("CPS", currentSpoonPerCent),
                new XElement("CPR", currentReworkPerCent),
                new XElement("PSLH", peachesSinceLHHFileWritten),
                new XElement("MCSLH", machineCyclesSinceLHHFileWritten),
                new XElement("SPSLH", spoonPeachesSinceLHHFileWritten),
                new XElement("RPSLH", ReworkPeachesSinceLHHFileWritten),
                new XElement("MRTSLH", machineRuntimeSinceLHHFileWritten),
                new XElement("TIME", newTime)));

                predata.Save("predata.xml");

            }

        }

        /* WILL NOT BE USED!!!
        // Here I reset the predata.xml to be empty and ready to accept the new incoming data - 
        // I delete all nodes from Root "Entries" node and on.
        private void xmlResetter()
        {
            XDocument predata = XDocument.Load("predata.xml");

            bool machineExists = predata.Descendants("Machine").Any(); // If there is ANY number of machine entries...delete

            if (machineExists)
            {
                predata.Descendants("Entries").Elements(). //...Go to "Entries" node
                Where(r => r.Name == "Machine").Remove(); //...Search for "Machine" elements and delete them
                predata.Save("predata.xml");
            }
        }
        */
        
        private void xmlLinqCreator()
        {
            XDocument predata = new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XComment("the comment goes here"),
                new XElement("Entries")
                );
            predata.Save("predata.xml");
        }
    }
}

