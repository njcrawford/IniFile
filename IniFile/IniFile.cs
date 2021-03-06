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
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace NJCrawford
{
    public class IniFile : IniBase
    {
        private string _filename = "";

        /// <summary>
        /// Creates an IniFile with the same name as the calling assembly.
        /// </summary>
        public IniFile()
        {
            doInit(Assembly.GetCallingAssembly().GetName().Name + ".ini");
        }

        /// <summary>
        /// Creates an IniFile with the specified file name.
        /// If file name does not contain any slashes or backslashes, appPath()
        /// is automatically prepended.
        /// </summary>
        public IniFile(string filename)
        {
            doInit(filename);
        }

        // Constructor helper function
        private void doInit(string filename)
        {
            _filename = filename;
            if (!_filename.Contains("\\") && !_filename.Contains("/"))
            {
                _filename = System.IO.Path.Combine(appPath(), _filename);
            }

            if (System.IO.File.Exists(_filename))
            {
                // inFile will be automatically closed at the end of the using block
                using (System.IO.StreamReader inFile = System.IO.File.OpenText(_filename))
                {
                    readValuesFromStream(inFile);
                }
            }
        }

        /// <summary>
        /// Sets the value of 'name' to 'value'. If name doesn't exist,
        /// it will be added. Sections are added as needed.
        /// </summary>
        public void setValue<T>(string sectionName, string valueName, T value)
        {
            setValueString(sectionName, valueName, value.ToString());
        }

        /// <summary>
        /// Sets the value of 'name' to 'value'. If name doesn't exist,
        /// it will be added. Uses default (no name) section.
        /// </summary>
        public void setValue<T>(string valueName, T value)
        {
            setValue("", valueName, value);
        }

        /// <summary>
        /// Writes all sections and name-value pairs to the file.
        /// Creates the file if it doesn't exist.
        /// </summary>
        public void save()
        {
            if (anyValuesChanged)
            {
                StreamReader origFile = null;
                // Open the existing file, if present
                if (File.Exists(_filename))
                {
                    origFile = new StreamReader(_filename);
                }
                string tempFilePath = Path.GetDirectoryName(_filename);
                tempFilePath = Path.Combine(tempFilePath, Path.GetFileNameWithoutExtension(_filename) + "." + Process.GetCurrentProcess().Id + Path.GetExtension(_filename));
                // Outfile will be automatically closed by end of using block
                using (StreamWriter tempFile = File.CreateText(tempFilePath))
                {
                    string buffer = null;
                    string section = "";
                    List<string> keysWritten = new List<string>();
                    List<string> sectionsWritten = new List<string>();

                    if (origFile != null)
                    {
                        buffer = origFile.ReadLine();
                    }
                    while (buffer != null)
                    {
                        if (buffer.TrimStart().StartsWith("[") && buffer.Contains("]"))
                        {
                            int start = buffer.IndexOf('[') + 1;
                            int length = buffer.IndexOf(']') - start;
                            // Found the start of another section, make sure we wrote all values from this section.
                            // We have to treat end of file same as start of new section, see below
                            // first, we find the section we're looking for
                            for (int i = 0; i < _sections.Count; i++)
                            {
                                if (_sections[i].name == section)
                                {
                                    // Then we check all it's values to see if each one has been written
                                    for (int x = 0; x < _sections[i].values.Count; x++)
                                    {
                                        if (!keysWritten.Contains(_sections[i].values[x].name))
                                        {
                                            //then write it, and mark it as written
                                            tempFile.WriteLine(_sections[i].values[x].name + "=" + _sections[i].values[x].value);
                                            keysWritten.Add(_sections[i].values[x].name);
                                        }
                                    }
                                    break;
                                }
                            }
                            sectionsWritten.Add(section);
                            section = buffer.Substring(start, length); ;
                            keysWritten = new List<string>();

                            //string newSection = buffer.Substring(start, length);
                            //if (!sectionsWritten.Contains(newSection))
                            //{
                            //    section = newSection;
                            //    valuesWritten = new List<string>();
                            //}
                            //else
                            //{
                            //    //skip to the next section
                            //    //may need different code later

                            //}
                        }
                        else if (!buffer.TrimStart().StartsWith("#") && !buffer.TrimStart().StartsWith(";") && buffer.Contains("="))
                        {
                            string key = buffer.Remove(buffer.IndexOf("="));
                            if (!keysWritten.Contains(key))
                            {
                                if (buffer.Length > key.Length + 1)//if it's less than or equal to, the current value is an empty string and can't be dropped
                                {
                                    //drop the current value
                                    buffer = buffer.Remove(key.Length + 1);
                                }
                                //tack on the new value
                                buffer = buffer + getValueOrNull(section, key);
                                keysWritten.Add(key);
                            }
                            else
                            {
                                //just skip doubled lines
                                //buffer = "#" + buffer;//comment doubled lines, this lets user see what line is really being used
                            }
                        }

                        tempFile.WriteLine(buffer);

                        buffer = origFile.ReadLine();
                    }

                    //we have to treat end of file same as start of new section, see above
                    //first, we find the section we're looking for
                    for (int i = 0; i < _sections.Count; i++)
                    {
                        if (_sections[i].name == section)
                        {
                            //then we check all it's values to see if each one has been written
                            for (int x = 0; x < _sections[i].values.Count; x++)
                            {
                                if (!keysWritten.Contains(_sections[i].values[x].name))
                                {
                                    //then write it, and mark it as written
                                    tempFile.WriteLine(_sections[i].values[x].name + "=" + _sections[i].values[x].value);
                                    keysWritten.Add(_sections[i].values[x].name);
                                }
                            }
                            break;
                        }
                    }
                    sectionsWritten.Add(section);

                    //write any new sections at the end of the file
                    for (int i = 0; i < _sections.Count; i++)
                    {
                        if (!sectionsWritten.Contains(_sections[i].name))
                        {
                            if (_sections[i].name != "")
                            {
                                tempFile.WriteLine("[" + _sections[i].name + "]");
                            }
                            sectionsWritten.Add(_sections[i].name);
                            for (int x = 0; x < _sections[i].values.Count; x++)
                            {
                                tempFile.WriteLine(_sections[i].values[x].name + "=" + _sections[i].values[x].value);
                            }
                        }
                    }

                    //for (int y = 0; y < _sections.Count; y++)
                    //{
                    //    if (y != 0)
                    //    {
                    //        outFile.WriteLine();
                    //        outFile.WriteLine("[" + _sections[y].name + "]");
                    //    }
                    //    for (int x = 0; x < _sections[y].values.Count; x++)
                    //    {
                    //        outFile.WriteLine(_sections[y].values[x].name + "=" + _sections[y].values[x].value);
                    //    }
                    //}

                    // Close the existing file, if needed
                    if (origFile != null)
                    {
                        origFile.Close();
                    }
                }
                // Copy the temp file to the old location and delete the temp file
                File.Copy(tempFilePath, _filename, true);
                File.Delete(tempFilePath);
            }
        }
    }
}
