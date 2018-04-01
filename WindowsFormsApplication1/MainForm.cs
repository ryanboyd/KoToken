using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Moda.Korean.TwitterKoreanProcessorCS;


namespace KoTokenApplication
{

    public partial class Form1 : Form
    {


        //this is what runs at initialization
        public Form1()
        {

            InitializeComponent();

            foreach(var encoding in Encoding.GetEncodings())
            {
                EncodingDropdown.Items.Add(encoding.Name);
            }
            EncodingDropdown.SelectedIndex = EncodingDropdown.FindStringExact(Encoding.Default.BodyName);

            


        }







        private void button1_Click(object sender, EventArgs e)
        {

            

            

            FolderBrowser.Description = "Please choose the location of your .txt files";
            FolderBrowser.ShowDialog();
            string TextFileFolder = FolderBrowser.SelectedPath.ToString();

            if (TextFileFolder != "")
            {

                FolderBrowser.Description = "Please choose your output location";
                FolderBrowser.ShowDialog();
                string OutputFolder = FolderBrowser.SelectedPath.ToString();

                if (OutputFolder != "")
                {

                    if (TextFileFolder != OutputFolder) { 

                        button1.Enabled = false;
                        ScanSubfolderCheckbox.Enabled = false;
                        EncodingDropdown.Enabled = false;
                        BgWorker.RunWorkerAsync(new string[] { TextFileFolder, OutputFolder });

                    }
                    else
                    {
                        MessageBox.Show("Your input folder cannot be the same as your output folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }

            } 

        }

        




        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {


            
            //selects the text encoding based on user selection
            Encoding SelectedEncoding = null;
            this.Invoke((MethodInvoker)delegate ()
            {
                SelectedEncoding = Encoding.GetEncoding(EncodingDropdown.SelectedItem.ToString());

            });



            //get the list of files
            var SearchDepth = SearchOption.TopDirectoryOnly;
            if (ScanSubfolderCheckbox.Checked)
            {
                SearchDepth = SearchOption.AllDirectories;
            }
            var files = Directory.EnumerateFiles(((string[])e.Argument)[0], "*.txt", SearchDepth);



            try {

                string outputdir = Path.Combine(((string[])e.Argument)[1]);

                Directory.CreateDirectory(outputdir);
                


                foreach (string fileName in files)
                {



                    //set up our variables to report
                    string Filename_Clean = Path.GetFileName(fileName);
                    


                    //report what we're working on
                    FilenameLabel.Invoke((MethodInvoker)delegate
                    {
                        FilenameLabel.Text = "Analyzing: " + Filename_Clean;
                    });

                    
                                            

                    //do stuff here
                    string readText = File.ReadAllText(fileName, SelectedEncoding).ToLower();

                    var TokenResults = TwitterKoreanProcessorCS.Tokenize(readText);

                    StringBuilder Builder = new StringBuilder();

                    int tokenCount = TokenResults.Count();

                    for (int i = 0; i < tokenCount; i++)
                    {
                        if (TokenResults.ElementAt(i).Pos != KoreanPos.Space) Builder.Append(TokenResults.ElementAt(i).Text + ' ');
                    }

                    using (System.IO.StreamWriter fileout =
                        new StreamWriter(Path.Combine(outputdir, Filename_Clean), false, SelectedEncoding))
                    {
                        fileout.Write(Builder.ToString());
                    }

                }



           }
            catch
           {
                MessageBox.Show("KoToken encountered a problem while trying to tokenize/write a file.");
            }



            
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            ScanSubfolderCheckbox.Enabled = true;
            EncodingDropdown.Enabled = true;
            FilenameLabel.Text = "Finished!";
            MessageBox.Show("KoToken has finished analyzing your texts.", "Analysis Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void WordWindowSizeTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void PhraseLengthTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void BigWordTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

    }
    


}
