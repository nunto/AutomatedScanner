using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Saraff.Twain;
using PdfSharp;

namespace AutomatedScanner
{
    public partial class Form1 : Form
    {
        private const string _x86Aux = "Saraff.Twain.Aux_x86.exe"; // For twain 1.x
        private const string _msilAux = "Saraff.Twain.Aux_MSIL.exe"; // For twain 2.x
        private string destination = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Default destination (Currently Desktop)
        private List<Image> imageScans = new List<Image>(); // Stores the current set of scanned images
        private bool _isEnable = false; // Boolean for twain state changes

        /// <summary>
        /// Constructs the form window and opens the twain data source manager
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            try
            {
                // Opens the twain Data Source Manager
                this._twain.OpenDSM();
            } catch (Exception ex)
            {
                MessageBox.Show("Error opening data source manager", "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Lists all available data sources available and allows the user to select device upon clicking the Select DS button
        /// </summary>
        /// <param name="sender">Provides context regarding the object that raised the event</param>
        /// <param name="e">Contains the data pertaining to the event</param>
        private void btnDSOnClick(object sender, EventArgs e)
        {
            this._twain.CloseDataSource();
            // Opens a list for user to select their Data Source
            this._twain.SelectSource();
        }

        /// <summary>
        /// Initializes the scan using the selected data source upon clicking the scan button
        /// </summary>
        private void btnScanOnClick(object sender, EventArgs e)
        {
            try
            {
                // Scan the document
                this._twain.Acquire();
            } catch(Exception ex)
            {
                MessageBox.Show("Error scanning document", "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a folder browser and overrides the default destination with the selected path upon clicking the Destination button
        /// </summary>
        /// <param name="sender">Provides context regarding the object that raised the event</param>
        /// <param name="e">Contains the data pertaining to the event</param>
        private void btnDestOnClick(object sender, EventArgs e)
        {
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                // Saves the user's preferred save location
                destination = folderBrowser.SelectedPath;
            }
        }

        /// <summary>
        /// Actions to occur after a completed scan; previews the scan and saves it as a pdf
        /// </summary>
        /// <param name="sender">Provides context regarding the object that raised the event</param>
        /// <param name="e">Contains the data pertaining to the event</param>
        private void _twain_AcquireCompleted(object sender, EventArgs e)
        {
            try
            {
                if (this._twain.ImageCount > 0)
                {
                    // Previews an image of the current scan
                    this.pictureBox1.Image = this._twain.GetImage(this._twain.ImageCount - 1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying preview.", "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Adds the latest scan into the scans array
            imageScans.Add(this._twain.GetImage(this._twain.ImageCount - 1));
        }

        /// <summary>
        /// Saves the currently stored images as a pdf upon clicking the done button
        /// </summary>
        /// <param name="sender">Provides context regarding the object that raised the event</param>
        /// <param name="e">Contains the data pertaining to the event</param>
        private void btnDoneOnClick(object sender, EventArgs e)
        {
            // Initializes a PDF document
            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();

            // Adding all scans into the document (one per page)
            foreach (Image im in imageScans)
            {
                PdfSharp.Pdf.PdfPage page = document.AddPage();
                PdfSharp.Drawing.XGraphics xgfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                PdfSharp.Drawing.XImage ximg = PdfSharp.Drawing.XImage.FromGdiPlusImage(im);
                xgfx.DrawImage(ximg, 0, 0);
            }
            // Naming the document with a timestamp
            String timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHHmmss");
            string filename = timeStamp + "testScan";

            // Saving the document if it's not empty
            if (document.PageCount > 0)
            {
                document.Save(destination + "/" + filename + ".pdf");
            } else
            {
                MessageBox.Show("At least one page must be scanned before saving.", "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Close doc and clear the Scans from this session
            document.Close();
            imageScans.Clear();
        }

        /// <summary>
        /// Method for dealing with a twain state change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _twain_TwainStateChanged(object sender, Twain32.TwainStateEventArgs e)
        {
            try
            {
                if ((e.TwainState & Twain32.TwainStateFlag.DSEnabled) == 0 && this._isEnable)
                {
                    this._isEnable = false;
                }
                this._isEnable = (e.TwainState & Twain32.TwainStateFlag.DSEnabled) != 0;
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }
    }
}
