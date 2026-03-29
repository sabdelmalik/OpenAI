using AdvancedAligner.ExampleEditor;
using System.Windows.Forms;

namespace BibleTaggingUtil.Editor
{

    public class DragData
    {
        public List<DragDataItem> Data {get;set;} = new List<DragDataItem>();

        public void Add(int rowIndex, int columnIndex,
            string index, string gloss, string word, string lemma, string pos, string morph, string strong,
            DataGridView source)
        {
            Data.Add(new DragDataItem(rowIndex, columnIndex,  
            index, gloss, word, lemma, pos, morph, strong,
            source));
        }
    }
    
    public class DragDataItem
    {
        public DragDataItem(int rowIndex, int columnIndex, 
            string index, string gloss, string word, string lemma, string pos, string morph, string strong,
            DataGridView source)
        {
            ColumnIndex = columnIndex;
            RowIndex = rowIndex;
            Index = index;
            Gloss = gloss;
            Word = word;
            Lemma = lemma;
            POS = pos;
            Morph = morph;
            Strong = strong;
            Source = source;
        }

        public string Index { get; private set; }
        public string Gloss { get; private set; }
        public string Word { get; private set; }
        public string Lemma { get; private set; }
        public string POS { get; private set; }
        public string Morph { get; private set; }
        public string Strong { get; private set; }
        public int ColumnIndex { get; private set; }
        public int RowIndex { get; private set; }
        public DataGridView Source { get; private set; }

    }

}
