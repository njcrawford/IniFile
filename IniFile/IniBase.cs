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

namespace NJCrawford
{
    public class IniBase
    {
        protected class StringPair
        {
            public string value;
            public string name;
            public StringPair()
            {
                value = "";
                name = "";
            }
        }

        protected class FileSection
        {
            public string name;
            public List<StringPair> values = new List<StringPair>();

            public FileSection()
            {
                name = "";
                values = new List<StringPair>();
            }
        }

        protected List<FileSection> _sections = new List<FileSection>();

        public static string appPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location);
        }

        /** Returns the value associated with name if it exists,
         * returns a null string if it doesn't. */
        public string getValue(string name)
        {
            return getValue("", name);
        }

        public string getValue(string sectionName, string valueName)
        {
            string retval = null;
            for (int y = 0; y < _sections.Count; y++)
            {
                if (_sections[y].name.Equals(sectionName))
                {
                    for (int x = 0; x < _sections[y].values.Count; x++)
                    {
                        if (_sections[y].values[x].name.Equals(valueName))
                        {
                            retval = _sections[y].values[x].value;
                            break;
                        }
                    }
                }
            }
            return retval;
        }

        /* Returns the number of sections. */
        public int getSectionCount()
        {
            return _sections.Count;
        }

        /** Returns the name of _sections[index] if it exists,
         * returns a null string if it doesn't. */
        public string getSectionName(int sectionIndex)
        {
            string retval = null;
            if (sectionIndex < _sections.Count)
            {
                retval = _sections[sectionIndex].name;
            }
            return retval;
        }

        /* 
         * Gets the number of values in sectionIndex.
         * Returns -1 if sectionIndex is out of range.
         */
        public int getValueCount(int sectionIndex)
        {
            int retval = -1;
            if (sectionIndex < _sections.Count)
            {
                retval = _sections[sectionIndex].values.Count;
            }
            return retval;
        }

        /** Returns the value of _values[index] if it exists,
         * returns a null string if it doesn't. */
        public string getValue(int sectionIndex, int valueIndex)
        {
            string retval = null;
            if (sectionIndex < _sections.Count && valueIndex < _sections[sectionIndex].values.Count)
            {
                retval = _sections[sectionIndex].values[valueIndex].value;
            }
            return retval;
        }

        /** Returns the name of _values[index] if it exists,
         * returns a null string if it doesn't. */
        public string getName(int sectionIndex, int valueIndex)
        {
            string retval = null;
            if (sectionIndex < _sections.Count && valueIndex < _sections[sectionIndex].values.Count)
            {
                retval = _sections[sectionIndex].values[valueIndex].name;
            }
            return retval;
        }

        /** Sets the value of 'name' to 'value'. If name doesn't exist,
         * it will be added. Sections are added as needed. */
        protected void _setValue(string sectionName, string valueName, string value)
        {
            bool foundValue = false;
            bool foundSection = false;

            for (int y = 0; y < _sections.Count; y++)
            {
                if (_sections[y].name.Equals(sectionName))
                {
                    foundSection = true;
                    for (int x = 0; x < _sections[y].values.Count; x++)
                    {
                        if (_sections[y].values[x].name.Equals(valueName))
                        {
                            StringPair tmp = new StringPair();
                            tmp.name = valueName;
                            tmp.value = value;
                            foundValue = true;
                            _sections[y].values[x] = tmp;
                            break;
                        }
                    }
                    if (!foundValue)
                    {
                        StringPair tmp = new StringPair();
                        tmp.name = valueName;
                        tmp.value = value;
                        _sections[y].values.Add(tmp);
                    }
                }

            }
            if (!foundSection)
            {
                FileSection tmpSection = new FileSection();
                StringPair tmpValue = new StringPair();
                tmpSection.name = sectionName;
                tmpValue.name = valueName;
                tmpValue.value = value;
                _sections.Add(tmpSection);
                _sections[_sections.Count - 1].values.Add(tmpValue);
            }
        }

        //<summary>
        /** Reads name-value pairs from a stream. Uses setValue()
         * to add name-value pairs to _sections[n].values. If
         * the value already exists it will *NOT* be overwritten,
         * if not it will be added. (changed in v. 1.1.0.8)
         * This is so the first value in the stream has priority.
         * Any line without an equals sign or beginning with
         * # or ; will be discarded. Exception: lines that start 
         * with a '[' and have a ']' later will be read as section 
         * headers. Whitespace before and after will be eaten.*/
        //</summary>
        protected void readValuesFromStream(TextReader s)
        {

            string inLine;
            string section = "";
            _sections.Add(new FileSection()); //make sure the file has a default section, and it is the first

            inLine = s.ReadLine();
            while (inLine != null)
            {
                if (inLine.Trim().StartsWith("[") && inLine.Contains("]"))
                {
                    int start = inLine.IndexOf('[') + 1;
                    int length = inLine.IndexOf(']') - start;
                    section = inLine.Substring(start, length);
                }
                else if (!inLine.StartsWith("#") && !inLine.StartsWith(";") && inLine.Contains("="))
                {
                    string tmpName = inLine.Remove(inLine.IndexOf("="));
                    string tmpValue = inLine.Substring(inLine.IndexOf("=") + 1);
                    if (getValue(section, tmpName) == null)
                    {
                        _setValue(section, tmpName, tmpValue);
                    }
                }
                inLine = s.ReadLine();
            }
        }
    }
}
