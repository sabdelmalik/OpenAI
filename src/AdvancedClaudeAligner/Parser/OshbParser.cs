using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;

namespace AdvancedAligner
{
    public class OshbParser
    {
        /// <summary>
        /// key: verse reference in the format "Book.Chapter.Verse", e.g. "Genesis.1.1"
        /// value: the hebrew verse text
        /// </summary>
        Dictionary<string, string> HebrewBible = new Dictionary<string, string>();


        /// <summary>
        /// the xml file is structured as follows:
        /// The complete Bible is contained in a <osisText> element.
        /// the <osisText> element contains a <header> element which we ignore,followed by a set of <div> elements, each of which represents a book of the Bible. 
        /// Each <div> element has a "type" attribute which contains the name of the book, e.g. "Genesis".
        /// Each <div> element contains a set of <chapter> elements, each of which represents a chapter of the book.
        /// each <chapter> element has a "osisID" attribute which contains the chapter reference in the format "Book.Chapter", e.g. "Genesis.1".
        /// each <chapter> element contains a set of <verse> elements, each of which represents a verse of the chapter.
        /// each <verse> element has a "osisID" attribute which contains the verse reference in the format "Book.Chapter.Verse", e.g. "Genesis.1.1".
        /// each <verse> element contains 
        ///   - a set <w> elements, each of which represents a word of the verse.
        ///   - <note> elements which we ignore
        ///   - other elements    
        /// /// </summary>
        public OshbParser()
        {
            string source = @"OSHB\OSHB.xml";

            // Intially we need to go throw the file and add all the other elements inside the verse elements to the other elements list.
            var settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = false,
                IgnoreComments = true

            };
            //settings.CheckCharacters = false;
            using (XmlReader reader = XmlReader.Create(source, settings))
            {

                while (reader.Read())
                {

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "verse")
                    {
                        // we are inside a verse element
                        // The verse reference is in the osisID attribute of the verse element
                        //
                        // We ignore any <note> elements
                        // The verse element contains <w> elements and <seg> elements the=st represent the text of the verse.
                        // We ignore the attributes of <w> elements
                        // The <w> elements contain the text of the verse, and my contain <seg> elements with specic type atribut
                        // If the <w> element a <seg> element with type="x-suspended" or "x-large" or "x-small" attribute, then its containd text is one character that is considered part of the verse text, but it is not a word, so we ignore the <w> element and add the text of the <seg> element to the verse text.
                        // The <seg> elements inside the verse are treated differently based on their type attribute as follows
                        // type=x-sof-pasuq         always at the end of the verse there should be no space before but may be after it
                        // type = x-maqqef          used between two words and should be no space before or after it
                        // type = x-maqqef          used between two words and should be no space before or after it
                        // type = x-paseq           is treated as a word and is included in the verse text with a space before and after it.
                        // type = x-pe              is treated as a word and is included in the verse text with a space before it.
                        // type = x-samekh          is treated as a word and is included in the verse text with a space before it.
                        // type = x-reversednun     is treated as a word and is included in the verse text with a space before and after it.

                        StringBuilder verseTextBuilder = new StringBuilder();
                        string verseReference = reader.GetAttribute("osisID");

                        bool skip = false;

                        while (true)
                        {
                            bool success = false;
                            if (skip)
                            {
                                skip = false;
                                success = true;
                                //reader.Skip();
                            }
                            else
                                success = reader.Read();

                            if (!success)
                                break;

                            // ignore <note> elements
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "note")
                            {
                                reader.Skip();
                                skip = true; // skip any nodes inside a <seg> element
                                continue;
                            }

                            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "verse")
                            {
                                HebrewBible[verseReference] = verseTextBuilder.ToString().Replace("/", "").Trim(); // trim any leading or trailing spaces from the verse text    
                                break; // end of the <verse> element
                            }
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "w")
                            {
                                // we are inside a word element
                                // we need to check if it contains a seg element with type="x-suspended" or "x-large" or "x-small"
                                string wordText = "";
                                if (reader.IsEmptyElement)
                                {
                                    continue; // skip empty <w> elements
                                }
                                bool wSkip = false; // flag to indicate whether to skip the next read after processing a <seg> element inside a <w> element
                                while (true)
                                {
                                    bool segSuccess = false;
                                    if (wSkip)
                                    {
                                        wSkip = false;
                                        segSuccess = true;
                                    }
                                    else
                                        segSuccess = reader.Read();

                                    if (!success)
                                    {
                                        break;
                                    }
                                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "seg")
                                    {
                                        string segType = reader.GetAttribute("type");
                                        if (segType == "x-suspended" || segType == "x-large" || segType == "x-small")
                                        {
                                            wordText += reader.ReadElementContentAsString();
                                            wSkip = true;
                                        }
                                        else
                                        {
                                            wordText += reader.ReadElementContentAsString();
                                        }
                                    }
                                    else if (reader.NodeType == XmlNodeType.Text)
                                    {
                                        wordText += reader.Value;
                                    }
                                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "w")
                                    {

                                        break; // end of the <w> element
                                    }
                                }
                                verseTextBuilder.Append(wordText);
                                verseTextBuilder.Append(' '); // add a space after each word
                            }
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "seg")
                            {
                                string segType = reader.GetAttribute("type");
                                if (segType == "x-sof-pasuq")
                                {
                                    // remove last space if it exists before adding the sof pasuq text
                                    if (verseTextBuilder.Length > 0 && verseTextBuilder[verseTextBuilder.Length - 1] == ' ')
                                    {
                                        verseTextBuilder.Length--; // remove the last space
                                    }
                                    verseTextBuilder.Append(reader.ReadElementContentAsString());
                                    verseTextBuilder.Append(' '); // add a space after the seg text
                                }
                                else if (segType == "x-paseq" || segType == "x-reversednun")
                                {
                                    verseTextBuilder.Append(reader.ReadElementContentAsString());
                                    verseTextBuilder.Append(' '); // add a space after the seg text
                                }
                                else if (segType == "x-maqqef")
                                {
                                    // remove last space if it exists before adding the maqqef text
                                    if (verseTextBuilder.Length > 0 && verseTextBuilder[verseTextBuilder.Length - 1] == ' ')
                                    {
                                        verseTextBuilder.Length--; // remove the last space
                                    }
                                    verseTextBuilder.Append(reader.ReadElementContentAsString());
                                }
                                else if (segType == "x-pe" || segType == "x-samekh")
                                {
                                    verseTextBuilder.Append(reader.ReadElementContentAsString());
                                }
                                else
                                {
                                    verseTextBuilder.Append(reader.ReadElementContentAsString());
                                }
                                skip = true; // skip next read since we already read the content of the <seg> element
                            }
                        }

                    }
                }
            }

            // output the Bible verses to a text file with each line in the format "verse reference: verse text"
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in HebrewBible)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            File.WriteAllText(@"OSHB\OSHB_verses.txt", sb.ToString());
        }


        public static Dictionary<string, string>? GetAttributes(XmlReader reader)
        {
            Dictionary<string, string>? attributes = null;
            if (reader.HasAttributes)
            {
                attributes = new Dictionary<string, string>();
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    attributes[reader.Name] = reader.Value;
                }
                // move back to the element node that contains
                // the attributes we just traversed
                reader.MoveToElement();

            }

            return attributes;
        }
    }
}
