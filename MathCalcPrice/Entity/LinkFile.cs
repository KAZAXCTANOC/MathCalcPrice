using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MathCalcPrice.Entity
{
    public class LinkFile
    {
        public Document Doc { get; set; }

        public string Name { get { return Path.GetFileName(Doc.PathName); } }

        public bool IsChecked { get; set; }

        public LinkFile(Document doc)
        {
            Doc = doc;
            IsChecked = true;
        }
        private IEnumerable<ExternalFileReference> GetLinkedFileReferences()
        {
            var collector = new FilteredElementCollector(Doc);
            var linkedElements = collector
                .OfClass(typeof(RevitLinkType))
                .Select(x => x.GetExternalFileReference())
                .ToList();

            return linkedElements;
        }

        private IEnumerable<Document> GetLinkedDocuments()
        {
            var linkedfiles = GetLinkedFileReferences();

            var linkedFileNames = linkedfiles
                .Select(x => ModelPathUtils.ConvertModelPathToUserVisiblePath(x.GetAbsolutePath()))
                .ToList();

            return Doc.Application.Documents
                .Cast<Document>()
                .Where(doc => linkedFileNames
                    .Any(fileName => doc.PathName.Equals(fileName)));
        }

        public List<Document> GetDocuments(bool includelinkFiles)
        {
            List<Document> docs = includelinkFiles
                ? GetLinkedDocuments().ToList()
                : new List<Document>();

            if (!docs.Contains(Doc))
                docs.Add(Doc);

            return docs;
        }

    }
}
