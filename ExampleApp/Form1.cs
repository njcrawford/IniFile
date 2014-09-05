/*
IniFile - Parser for .ini files

Copyright (C) 2014 Nathan Crawford
 
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
 
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA
02111-1307, USA.

A copy of the full GPL 2 license can be found in the docs directory.
You can contact me at http://www.njcrawford.com/contact/.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using nc_settings;

namespace nc_settings
{
    public partial class Form1 : Form
    {
        private NJCrawford.IniFile settings = new NJCrawford.IniFile("testsettings.ini");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Int32 test;
            test = settings.getValue<Int32>("location", "top", -1);
            if (test != -1)
            {
                this.Top = test;
            }

            test = settings.getValue<Int32>("location", "left", -1);
            if (test != -1)
            {
                this.Left = test;
            }

            test = settings.getValue<Int32>("location", "width", -1);
            if (test != -1)
            {
                this.Width = test;
            }

            test = settings.getValue<Int32>("location", "height", -1);
            if (test != -1)
            {
                this.Height = test;
            }

            lblVersion.Text = Application.ProductVersion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            settings.setValue("location", "top", this.Top);
            settings.setValue("location", "left", this.Left);
            settings.setValue("location", "width", this.Width);
            settings.setValue("location", "height", this.Height);
            settings.setValue("this is cool", "Yes");
            settings.setValue("other junk", "way cool", "maybe");
            settings.save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string test = settings.getValue("this is cool", "bad string");
            if (test != "bad string")
            {
                MessageBox.Show(test);
            }
            else
            {
                MessageBox.Show("Well, it would be if you had saved the settings.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int sectionCount = settings.getSectionCount();
            System.IO.StringWriter message = new System.IO.StringWriter();
            for (int i = 0; i < sectionCount; i++)
            {
                string sectionName = settings.getSectionName(i);
                if (sectionName == "")
                {
                    sectionName = "(default)";
                }
                else
                {
                    // Add a line break for sections after the first, for visual separation
                    message.WriteLine();
                }
                message.WriteLine("Section: " + sectionName);
                int valueCount = settings.getValueCount(i);
                for (int x = 0; x < valueCount; x++)
                {
                    message.WriteLine("Name: " + settings.getNameOrNull(i, x) + ", Value: " + settings.getValueOrNull(i, x));
                }
            }
            MessageBox.Show(message.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.IO.File.SetAttributes(System.IO.Path.Combine(Application.StartupPath, "testsettings.ini"), System.IO.FileAttributes.ReadOnly);
            settings.save();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            System.IO.File.SetAttributes(System.IO.Path.Combine(Application.StartupPath, "testsettings.ini"), System.IO.FileAttributes.Normal);
        }

    }
}