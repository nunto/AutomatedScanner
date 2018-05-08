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
        private string destination = "./"; // Default destination
        private bool _isEnable = false;

        /// <summary>
        /// Constructs the form window and opens the twain data source manager
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            try
            {
                this._twain.OpenDSM();
            } catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\n\n{1}", ex.Message, ex.StackTrace), "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Lists all available data sources available and allows the user to select device
        /// </summary>
        /// <param name="sender">Provides context regarding the object that raised the event</param>
        /// <param name="e">Contains the data pertaining to the event</param>
        private void btnDS_Click(object sender, EventArgs e)
        {
            this._twain.CloseDataSource();
            this._twain.SelectSource();
        }

        /// <summary>
        /// Initializes the scan using the selected data source
        /// </summary>
        private void btnScan_Click(object sender, EventArgs e)
        {
            try
            {
                this._twain.Acquire();
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens a folder browser and overrides the default destination with the selected path
        /// </summary>
        /// <param name="sender">Provides context regarding the object that raised the event</param>
        /// <param name="e">Contains the data pertaining to the event</param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
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
                if (this.pictureBox1.Image != null)
                {
                    this.pictureBox1.Dispose();
                }
                if (this._twain.ImageCount > 0)
                {
                    this.pictureBox1.Image = this._twain.GetImage(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SaveScan(destination, this._twain.GetImage(0));
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
                MessageBox.Show(ex.Message, "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Saves the returned image as a pdf
        /// </summary>
        /// <param name="destination">Location where the pdf will be saved</param>
        /// <param name="originalImage">Image returned from the scan</param>
        private void SaveScan(string destination, System.Drawing.Image originalImage)
        {
            PdfSharp.Pdf.PdfDocument document = new PdfSharp.Pdf.PdfDocument();
            PdfSharp.Pdf.PdfPage page = document.AddPage();
            PdfSharp.Drawing.XGraphics xgfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            PdfSharp.Drawing.XImage ximg = PdfSharp.Drawing.XImage.FromGdiPlusImage(originalImage);


            xgfx.DrawImage(ximg, 0, 0);
            string filename = "testScan.pdf";
            document.Save(destination + "/" + filename);
            document.Close();
            Console.WriteLine(Environment.CurrentDirectory);
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
