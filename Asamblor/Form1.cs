using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Asamblor
{
    public partial class Form1 : Form
    {
        String fileNameWithPath = "";
        String filename = "";
        /* List which will store each token (element) read from ASM file */
        List<String> asmElements = new List<String>();

        Dictionary<string, Int16> registri = new Dictionary<string, Int16>()
            {
                                    {"R0",  0b0000000000000000},
                                    {"R1",  0b0000000000000001},
                                    {"R2",  0b0000000000000010},
                                    {"R3",  0b0000000000000011},
                                    {"R4",  0b0000000000000100},
                                    {"R5",  0b0000000000000101},
                                    {"R6",  0b0000000000000110},
                                    {"R7",  0b0000000000000111},
                                    {"R8",  0b0000000000001000},
                                    {"R9",  0b0000000000001001},
                                    {"R10", 0b0000000000001010},
                                    {"R11", 0b0000000000001011},
                                    {"R12", 0b0000000000001100},
                                    {"R13", 0b0000000000001101},
                                    {"R14", 0b0000000000001110},
                                    {"R15", 0b0000000000001111}
            };

        Dictionary<string, Int16> moduriAdresare = new Dictionary<string, Int16>()
            {
                { "AIMEDIATA", 00 },
                { "ADIRECTA", 01 },
                { "AINDIRECTA", 10 },
                { "AINDEXATA", 11 },
            };

        Dictionary<string, Int16> instructiuniClasa1 = new Dictionary<string, Int16>() {
                    { "MOV", 0b0000000000000000 },
                    { "ADD", 0b0001000000000000 },
                    { "SUB", 0b0010000000000000 },
                    { "CMP", 0b0011000000000000 },
                    { "AND", 0b0100000000000000 },
                    { "OR",  0b0101000000000000 },
                    { "XOR", 0b0111000000000000 }
            };

        public Form1()
        {
            InitializeComponent();
        }

        // Functie care ne returneaza path-ul 
        private String getFileName(String filter)
        {
            try
            {  
                /* Instantiate an OpenFileDialog */
                OpenFileDialog of = new OpenFileDialog();
                /* Set the filter */
                of.Filter = filter;
                /* Get the working directory */
                of.InitialDirectory = Path.GetFullPath("..\\Proiec ASC");
                of.RestoreDirectory = true;
                /* Display the Open File dialog */
                if (of.ShowDialog() == DialogResult.OK)
                {
                    fileNameWithPath = of.FileName;         //filename cu path
                    filename = of.SafeFileName;             //filename fara path
                }
                return fileNameWithPath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                outputTextBox.Text = "";
                /* Take the filename selected by user */
                filename = getFileName("ASM file for didactical processor(*.asm)|*.asm");
                /* Display the filename in ASMFileTextBox */
                pathTextBox.Text = filename != null ? filename : pathTextBox.Text;
                /* Enable/Disable the ParseFileButton depending of user choice */
                if (!filename.Equals(""))
                {
                    btnExecute.Enabled = true;
                }
                else
                {
                    btnExecute.Enabled = false;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                /* local variable used for debugging only */
                int lineCounter = 0;

                /* Create a parser object used for ASM file
                    REMEMBER: this parser can be used for all kind of text files!!!
                 */
                TextFieldParser parser = new TextFieldParser(filename);
                /* Reinitialize the Text property of OutputTextBox */
                outputTextBox.Clear();
                /* Define delimiters in ASM file */
                String[] delimiters = { ":", ",", " ", "(", ")" };
                /* Specify that the elements in ASM file are delimited by some characters */
                parser.TextFieldType = FieldType.Delimited;
                /* Set-up the specified delimiters */
                parser.SetDelimiters(delimiters);
                /* Parse the entire ASM file based on previous specifications*/
                while (!parser.EndOfData)
                {
                    /* Read an entire line in ASM file
                       and split this line in many strings delimited by delimiters */
                    string[] asmFields = parser.ReadFields();
                    /* Store each string as a single element in the list
                       if this string is not empty */
                    foreach (string s in asmFields)
                    {
                        if (!s.Equals(""))
                        {
                            asmElements.Add(s);
                        }
                    }

                    //Adaugam un NL pt a indica ca am trecut la un rand nou
                    asmElements.Add("NL");
                    /* Counting the number of lines stored in ASM file */
                    lineCounter++;
                }

                /* Close the parser */
                parser.Close();
                /* If the file is empty, trigger a new exception which
                   in turn will display an error message */
                if (lineCounter == 0)
                {
                    Exception exc = new Exception("The ASM file is empty!");
                    throw exc;
                }
                else
                {
                    /* Display every token in OutputTextBox */
                    outputTextBox.Clear();
                    foreach (String s in asmElements)
                    {
                        if (s == "NL")
                        {
                            outputTextBox.Text += Environment.NewLine;
                        }
                        else {
                            outputTextBox.Text += s + " ";
                        }
                    }
                    /* Display an information about the process completion */
                    MessageBox.Show("Parsing is completed!", "Assembler information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTransform_Click(object sender, EventArgs e)
        {
            transformTextBox.Clear();

            List<List<String>> listaInstructiuni = new List<List<String>>();
            List<String> instructiuneTemporara = new List<String>();
            foreach (String elem in asmElements)
            {
                if (elem != "NL")
                {
                    instructiuneTemporara.Add(elem);
                }
                else {
                    listaInstructiuni.Add(instructiuneTemporara.ToList());
                    instructiuneTemporara.Clear();
                }
            }
            Int16 varTemp;

            foreach (List<String> instructiune in listaInstructiuni)
            {
                varTemp = 0;
                if (Regex.IsMatch(instructiune[1], @"\b[R]\w+") && Regex.IsMatch(instructiune[2], @"\b[R]\w+"))   //Mod adresare directa 
                {
                    transformTextBox.Text += "Adresare Directa: " + Environment.NewLine;    
                    varTemp = (short)(varTemp | (instructiuniClasa1[instructiune[0]]));
                    varTemp = (short)(varTemp | (moduriAdresare["ADIRECTA"] << 4));         
                    varTemp = (short)(varTemp | (moduriAdresare["ADIRECTA"] << 10));
                    varTemp = (short)(varTemp | (registri[instructiune[1]] << 6));
                    varTemp = (short)(varTemp | registri[instructiune[2]]);
                    transformTextBox.Text += Convert.ToString(varTemp,2).PadLeft(16, '0') + " Completa" + Environment.NewLine;
                }
                else if (Regex.IsMatch(instructiune[1], @"\b[R]\w+") && Regex.IsMatch(instructiune[2], @"\d")) // Mod adresare imediata
                {
                    transformTextBox.Text += "Adresare Imediata: " + Environment.NewLine;   
                    varTemp = (short)(varTemp | (instructiuniClasa1[instructiune[0]]));
                    varTemp = (short)(varTemp | (moduriAdresare["AIMEDIATA"] << 4));         
                    varTemp = (short)(varTemp | (moduriAdresare["AIMEDIATA"] << 10));
                    varTemp = (short)(varTemp | (registri[instructiune[1]] << 6));
                    varTemp = (short)(varTemp | Convert.ToInt16(instructiune[2]));
                    transformTextBox.Text += Convert.ToString(varTemp, 2).PadLeft(16, '0') + " Completa" + Environment.NewLine;
                }

                foreach (String elem in instructiune)
                {
                    if (instructiuniClasa1.ContainsKey(elem))
                    {
                        transformTextBox.Text += Convert.ToString(instructiuniClasa1[elem], 2).PadLeft(16, '0') + Environment.NewLine;
                    }
                    else if (registri.ContainsKey(elem))
                    {
                        transformTextBox.Text += Convert.ToString(registri[elem], 2).PadLeft(16, '0') + Environment.NewLine;
                    }
                    else if (Regex.IsMatch(elem, @"\d"))
                    {
                        transformTextBox.Text += Convert.ToString(Convert.ToInt16(elem), 2).PadLeft(16, '0') + Environment.NewLine;
                    }
                    else
                    {
                        transformTextBox.Text += elem + "$" + Environment.NewLine;
                    }
                }
                transformTextBox.Text += "-------------------------------" + Environment.NewLine;
            }    
        }
    }
}
