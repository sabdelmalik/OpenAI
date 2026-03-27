using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedAligner
{
    public class BookDetails
    {
        public string BookName { get; set; }
        /// <summary>
        /// Alist of verse counts for each chapter in the book. 
        /// The index of the list corresponds to the chapter number - 1, 
        /// and the value at that index is the count of verses in the chapter.
        /// </summary>
        public List<int> ChapterVerses { get; set; }
    }
}
