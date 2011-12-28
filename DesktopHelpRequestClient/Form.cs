// Author: Marie West
// Email: marie@mariewest.me
// Website: http://mariewest.me
// Date: 2011-12-07
// Summary: User submission form - upon submission, form and system information will be posted to a specified URL
//          (Please see App.config for the config file)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DesktopHelpRequestClient
{
    public partial class SubmitForm : Form
    {
        public SubmitForm()
        {
            InitializeComponent();

            // Set text according to the config file
            this.Text = System.Configuration.ConfigurationSettings.AppSettings["appTitle"];
            instructionsLabel.Text = System.Configuration.ConfigurationSettings.AppSettings["instructions"];
            commentsLabel.Text = System.Configuration.ConfigurationSettings.AppSettings["commentsLabel"];

            // Hide form fields
            refLabel.Hide();
            responseLabel.Hide();
            okButton.Hide();
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            string referenceID;

            // Hide form fields
            instructionsLabel.Hide();
            nameLabel.Hide();
            nameText.Hide();
            emailLabel.Hide();
            emailText.Hide();
            commentsLabel.Hide();
            commentsText.Hide();
            submitButton.Hide();

            // Show form fields
            responseLabel.Show();
            okButton.Show();

            // Submit the information
            SystemInfo mySystem = new SystemInfo();
            mySystem.GatherInfo();
            referenceID = mySystem.PostInfo(nameText.Text, emailText.Text, commentsText.Text);

            // Notify user whether or not the information was sent successfully
            if (referenceID != "")
            {
                // Show reference number (if enabled)
                if (System.Configuration.ConfigurationSettings.AppSettings["showRefNumber"] == "true")
                {
                    refLabel.Text = "Reference #: " + referenceID;
                    refLabel.Show();
                }
                
                responseLabel.Text = System.Configuration.ConfigurationSettings.AppSettings["successMsg"];
            }
            else
            {
                responseLabel.Text = System.Configuration.ConfigurationSettings.AppSettings["failureMsg"];
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
