using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
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
        Dictionary<string, string> HebrewBibleOSIS = new Dictionary<string, string>();

        List<string> OsisBooks = new List<string>();
        Dictionary<string, string> OsisToHebrewReferenceMap = new Dictionary<string, string>();

        public readonly Dictionary<string, BookDetails> bookCounts = new Dictionary<string, BookDetails>();

        HebrewBibleParser hebrewBibleParser;

        bool mergePsalmTitles = true;

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
        public OshbParser(HebrewBibleParser hebrewParser)
        {
            hebrewBibleParser = hebrewParser;
            string source = @"OSHB\OSHB.xml";

            Dictionary<string, BookDetails> tempBookCounts = new Dictionary<string, BookDetails>();
            string lastBook = string.Empty;
            int lastChapter = 0;
            int lastVerseNumber = 0;

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
                                HebrewBibleOSIS[verseReference] = verseTextBuilder.ToString().Replace("/", "").Trim(); // trim any leading or trailing spaces from the verse text    
                                string book = verseReference.Split('.')[0];
                                int chapter = int.Parse(verseReference.Split('.')[1]);
                                int verseNumber = int.Parse(verseReference.Split('.')[2]);

                                if (!OsisBooks.Contains(book))
                                {
                                    OsisBooks.Add(book);
                                }

                                // update tempBookCounts
                                if (!tempBookCounts.ContainsKey(book))
                                {
                                    // This is a new book,
                                    // before adding it to tempBookCounts with an empty list of chapter verses
                                    // wee need to fill in the chapter verses for the previous book if it exists
                                    if (!string.IsNullOrEmpty(lastBook))
                                    {
                                        // fill in chapter verses for lastBook
                                        BookDetails details = tempBookCounts[lastBook];
                                        // ensure chapter verses list has enough entries for all chapters
                                        while (details.ChapterVerses.Count < lastChapter)
                                        {
                                            details.ChapterVerses.Add(0);
                                        }
                                        // update verse count for last chapter
                                        if (details.ChapterVerses.Count > 0)
                                        {
                                            details.ChapterVerses[lastChapter - 1] = lastVerseNumber;
                                        }
                                        lastBook = book;
                                        lastChapter = 0;
                                        lastVerseNumber = 0;
                                    }
                                    tempBookCounts[book] = new BookDetails
                                    {
                                        BookName = book,
                                        ChapterVerses = new List<int>()
                                    };
                                }
                                else
                                {
                                    // This is an existing book, update chapter verses count
                                    BookDetails details = tempBookCounts[book];
                                    // ensure chapter verses list has enough entries for all chapters
                                    while (details.ChapterVerses.Count < chapter)
                                    {
                                        details.ChapterVerses.Add(0);
                                    }
                                    // update verse count for current chapter
                                    if (details.ChapterVerses.Count > 0)
                                    {
                                        details.ChapterVerses[chapter - 1] = Math.Max(details.ChapterVerses[chapter - 1], verseNumber);
                                    }
                                }

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

            // replace book names in BookDetails keys with the corresponding names in tahotBooks
            foreach (var kvp in tempBookCounts)
            {
                string oshbBookName = kvp.Key;
                BookDetails details = kvp.Value;
                int oshbBookIndex = OsisBooks.IndexOf(oshbBookName);
                if (oshbBookIndex < 0 || oshbBookIndex >= OsisBooks.Count)
                {
                    continue; // book not found
                }
                string hebrewBookName = hebrewBibleParser.HebrewBooks[oshbBookIndex];
                bookCounts[hebrewBookName] = details;
            }

            // copy the HebrewBibleOSIS dictionary to HebrewBible 
            // After translating the book names in verse references from OSIS name to the name used in the Hebrew Bible parser HebrewBooks
            // booksOsis count should be the same as HebrewBooks count and they should be in the same order
            // =====
            // Special Treatment for Psalms:
            // if mergePsalmTitles is true then
            // if in the book of Psalms, and the chapter is found in psalmsWithTitles list
            // append verse 2 text to verse 1
            // renumber the following verses by subtracting one

            for (int i = 0; i < OsisBooks.Count; i++)
            {
                string osisBook = OsisBooks[i];
                string hebrewBook = hebrewBibleParser.HebrewBooks[i];
                string verseOneText = string.Empty;
                foreach (var kvp in HebrewBibleOSIS)
                {
                    if (kvp.Key.StartsWith(osisBook + "."))
                    {
                        int chapter = int.Parse(kvp.Key.Split('.')[1]);
                        int verseNumber = int.Parse(kvp.Key.Split('.')[2]);
                        string verseText = kvp.Value;
                        if (mergePsalmTitles && kvp.Key.StartsWith("Ps"))
                        {
                             if(psalmsWithTitles.Contains(chapter))
                             {
                                if (verseNumber == 1)
                                {
                                    verseOneText = verseText;
                                    continue;
                                }
                                else if (verseNumber == 2)
                                {
                                    verseText = $"{verseOneText} {verseText}";
                                    verseOneText = string.Empty;
                                }
                                verseNumber--;
                            }
                        }
                        string hebrewReference = $"{hebrewBook}.{chapter}.{verseNumber}";
                        if(HebrewBible.ContainsKey(hebrewReference))
                        {
                            throw new Exception($"Duplicate verse: {hebrewReference}");
                        }
                        HebrewBible[hebrewReference] = verseText;
                    }
                }
            }

            // output the book counts from the bookCounts dictionary in the format
            // book
            // {
            //    verse counts max 10 ber line
            //    ...
            // }
            // book
            //
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in bookCounts)
            {
                List<int> chapters = bookCounts[kvp.Key].ChapterVerses;
                sb.AppendLine(kvp.Key);
                sb.AppendLine("{");
                // append the chapters, seperated by commas
                // each line starts with a tab and has a maximum of ten chapters
                // the numbers should be formated as 3 digits with leading zeroes
                int lineCounter = 0;
                sb.Append("\t"); // atart of fist line
                for (int i=0; i < chapters.Count; i++)
                {
                    int ch = chapters[i];
                    lineCounter++;
                    bool endOfLine = lineCounter % 10 == 0;
                    bool last= i >= chapters.Count - 1;
                    if (endOfLine && !last)
                        sb.Append($"{ch},");
                    else if(last)
                        sb.Append($"{ch}");
                    else
                        sb.Append($"{ch}, ");

                    if (endOfLine && !last)
                    {
                        // 10 chapters written and not the last
                        sb.AppendLine();
                        sb.Append("\t"); // atart of the
                    }
                }
                sb.AppendLine();
                sb.AppendLine("}");
            }
            File.WriteAllText(@"OSHB\OSHB_VerseCounts.txt", sb.ToString());

            // output the Bible verses to a text file with each line in the format "verse reference: verse text"
            sb = new StringBuilder();
            foreach (var kvp in HebrewBible)
            {
                sb.AppendLine($"{kvp.Key} {kvp.Value}");
            }
            File.WriteAllText(@"OSHB\OSHB_verses.txt", sb.ToString());

            //BuildMap();
        }


        public Dictionary<string, string>? GetAttributes(XmlReader reader)
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

        /// <summary>
        /// The file OSHB\MT_Map.txt contains a mapping of 
        /// verse references in the HebrewBibleOSIS to 
        /// verse references in the HebrewBibleParser.HebrewBible
        /// 
        /// All book names in the mapping file are in the OSIS format, 
        /// so we need to translate them to the Hebrew Bible parser format 
        /// using the OsisBooks and HebrewBooks lists before we can use the mapping to build 
        /// the OsisToHebrewReferenceMap dictionary.
        /// references may have suffixes with "!a" or "!b" ... 
        /// we need to ignore the suffixes when building the mapping.
        /// 
        /// each line in the mapping file is in the format one of the following two formats:
        /// Single verse mapping:
        /// e.g. "1Sam.21.1=1Sam.20.42"
        /// Range verse mapping:
        /// e.g. "Num.30.1-Num.30.17=Num.29.40-Num.30.16"
        /// The range should be expanded to individual verse mappings before adding to the OsisToHebrewReferenceMap dictionary.
        /// </summary>
        private void BuildMap()
        {
            string source = @"OSHB\MT_Map.txt";
            var mappingLines = File.ReadAllLines(source);
            foreach (var mappingLine in mappingLines)
            {
                string[] parts = mappingLine.Split('=');
                string leftSide = parts[0];
                string rightSide = parts[1];
                // is this a range mapping or a single verse mapping?
                if (leftSide.Contains('-') && rightSide.Contains('-'))
                {
                    // range mapping
                    string[] leftRangeParts = leftSide.Split('-');
                    string[] rightRangeParts = rightSide.Split('-');
                    string leftStart = leftRangeParts[0];
                    string leftEnd = leftRangeParts[1];
                    string rightStart = rightRangeParts[0];
                    string rightEnd = rightRangeParts[1];

                    // we need to expand the range to individual verse mappings
                    // we can do this by parsing the verse references and then iterating through the verses in the range
                    // we can parse the verse references by splitting them into book, chapter, and verse parts
                    string leftStartBook = TranslateBookName(leftStart.Split('.')[0]);
                    int leftStartChapter = int.Parse(leftStart.Split('.')[1]);
                    int leftStartVerse = int.Parse(leftStart.Split('.')[2].Split('!')[0]); // ignore any suffixes after "!"

                    // if the start book is Psalms and the verse is 1 nad the chapter is in psalmsWithTitles
                    // skip this 
                    if (mergePsalmTitles)
                    {
                        // if the start book is Psalms and the verse is 1 nad the chapter is in psalmsWithTitles
                        // skip this mapping
                        if (leftStartBook.StartsWith("Ps") && leftStartVerse == 1 && psalmsWithTitles.Contains(leftStartChapter))
                            continue;
                    }

                    string leftEndBook = TranslateBookName(leftEnd.Split('.')[0]);
                    int leftEndChapter = int.Parse(leftEnd.Split('.')[1]);
                    int leftEndVerse = int.Parse(leftEnd.Split('.')[2].Split('!')[0]); // ignore any suffixes after "!"

                    string rightStartBook = TranslateBookName(rightStart.Split('.')[0]);
                    int rightStartChapter = int.Parse(rightStart.Split('.')[1]);
                    int rightStartVerse = int.Parse(rightStart.Split('.')[2].Split('!')[0]); // ignore any suffixes after "!"

                    string rightEndBook = TranslateBookName(rightEnd.Split('.')[0]);
                    int rightEndChapter = int.Parse(rightEnd.Split('.')[1]);
                    int rightEndVerse = int.Parse(rightEnd.Split('.')[2].Split('!')[0]); // ignore any suffixes after "!"

                    leftEnd = $"{leftEndBook}.{leftEndChapter}.{leftEndVerse}";
                    rightEnd = $"{rightEndBook}.{rightEndChapter}.{rightEndVerse}";

                    // we don't support ranges that span multiple books.
                    if (leftStartBook != leftEndBook || rightStartBook != rightEndBook)
                    {
                        throw new Exception($"Invalid mapping: {mappingLine}. Ranges that span multiple books are not supported.");
                    }

                    string leftReference = $"{leftStartBook}.{leftStartChapter}.{leftStartVerse}";
                    string rightReference = $"{rightStartBook}.{rightStartChapter}.{rightStartVerse}";

                    while (true)
                    {
                        OsisToHebrewReferenceMap[leftReference] = rightReference;
                        // if the left reference is not in the HebrewBibleOSIS dictionary,
                        // or the right reference is not in the HebrewBibleParser.HebrewBible dictionary,
                        // throw an exception since this means there is an error in the mapping file
                        if (!HebrewBible.ContainsKey(leftReference))
                        {
                            throw new Exception($"Invalid mapping: {mappingLine}. Reference {leftReference} not found in HebrewBibleOSIS.");
                        }
                        if (!hebrewBibleParser.referenceIndices.ContainsKey(rightReference))
                        {
                            throw new Exception($"Invalid mapping: {mappingLine}. Reference {rightReference} not found in HebrewBibleParser.HebrewBible.");
                        }

                        if (leftReference == leftEnd && rightReference == rightEnd)
                        {
                            break; // end of range
                        }

                        // increment the left reference
                        // if the left reference verse is equal to the last verse of the chapter,
                        // then we need to move to the next chapter and reset the verse number to 1
                        string bookName = leftReference.Split('.')[0];
                        if (leftStartVerse == bookCounts[bookName].ChapterVerses[leftStartChapter - 1])
                        {
                            leftStartVerse = 1;
                            leftStartChapter++;
                        }
                        else
                        {
                            leftStartVerse++;
                        }
                        
                        leftReference = $"{leftStartBook}.{leftStartChapter}.{leftStartVerse}";


                        // increment right reference
                        // if the right reference verse is equal to the last verse of the hebrew bible chapter,
                        // then we need to move to the next chapter and reset the verse number to 1
                        bookName = rightReference.Split(".")[0];
                        if (rightStartVerse == hebrewBibleParser.bookCounts[bookName].ChapterVerses[rightStartChapter - 1])
                        {
                            rightStartVerse = 1;
                            rightStartChapter++;
                        }
                        else
                        {
                            rightStartVerse++;
                        }

                        rightReference = $"{rightStartBook}.{rightStartChapter}.{rightStartVerse}";
                    }
                }
                else
                {
                    // single verse mapping
                    string leftReference = TranslateBookName(leftSide.Split('.')[0]) + "." + leftSide.Substring(leftSide.IndexOf('.') + 1).Split('!')[0]; // ignore any suffixes after "!"
                    string rightReference = TranslateBookName(rightSide.Split('.')[0]) + "." + rightSide.Substring(rightSide.IndexOf('.') + 1).Split('!')[0]; // ignore any suffixes after "!"
                    OsisToHebrewReferenceMap[leftReference] = rightReference;
                    // if the left reference is not in the HebrewBibleOSIS dictionary,
                    // or the right reference is not in the HebrewBibleParser.HebrewBible dictionary,
                    // throw an exception since this means there is an error in the mapping file
                    if (!HebrewBible.ContainsKey(leftReference))
                    {
                        //throw new Exception($"Invalid mapping: {mappingLine}. Reference {leftReference} not found in HebrewBibleOSIS.");
                        int x = 0;
                    }
                }
            }
        }

        private string TranslateBookName(string v)
        {
            if (string.IsNullOrEmpty(v))
                return v;

            return hebrewBibleParser.HebrewBooks[OsisBooks.IndexOf(v)];
        }

        private List<int> psalmsWithTitles = new List<int>()
        { 3, 4, 5, 6, 7, 8, 9, 11, 12, 13,
          14, 15, 16, 17, 18, 19, 20, 21, 22, 23,
          24, 25, 26, 27, 28, 29, 30, 31, 32, 34,
          35, 36, 37, 38, 39, 40, 41, 42, 44, 45,
          46, 47, 48, 49, 50, 51, 52, 53, 54, 55,
          56, 57, 58, 59, 60, 61, 62, 63, 64, 65,
          66, 67, 68, 69, 70, 72, 73, 74, 75, 76,
          77, 78, 79, 80, 81, 82, 83, 84, 85, 86,
          87, 88, 89, 90, 92, 98, 100, 101, 102, 103,
          108, 109, 110, 120, 121, 122, 123, 124, 125, 126,
          127, 128, 129, 130, 131, 132, 133, 134, 138, 139,
          140, 141, 142, 143, 144, 145 
        };
    }
}
