using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC
{
    public enum FieldType
    {
        gdsTextBox,
        gdsTextArea
    }
    public class Page
    {
        public string Name { get; set; }
        public string Question { get; set; }

        [JsonProperty("error-massage")]
        public string ErrorMassage { get; set; }
        public FieldType FieldType { get; set; }
        public string FieldName { get; set; }
        public bool Mandatory { get; set; }

        [JsonProperty("accordian-reference")]
        public string AccordianReference { get; set; }

        [JsonProperty("max-length")]
        public int? MaxLength { get; set; }
    }

    public class Sections
    {
        public List<Page> Pages { get; set; }
    }

    public class Application
    {
        public string Name { get; set; }
        public Sections Sections { get; set; }
    }

    public class ApplicationDefinition
    {
        public Application Application { get; set; }
    }

    public class ApplicationContent
    {
        public Application Application { get; set; }

        //
        // Summary:
        //     The primary key in the database.
        public int Id { get; set; }
        //
        // Summary:
        //     The logical identifier of the content item across versions.
        public string ContentItemId { get; set; }
        //
        // Summary:
        //     The logical identifier of the versioned content item.
        public string ContentItemVersionId { get; set; }
        //
        // Summary:
        //     The content type of the content item.
        public string ContentType { get; set; }
        //
        // Summary:
        //     Whether the version is published or not.
        public bool Published { get; set; }
        //
        // Summary:
        //     Whether the version is the latest version of the content item.
        public bool Latest { get; set; }
        //
        // Summary:
        //     When the content item version has been updated.
        public DateTime? ModifiedUtc { get; set; }
        //
        // Summary:
        //     When the content item has been published.
        public DateTime? PublishedUtc { get; set; }
        //
        // Summary:
        //     When the content item has been created or first published.
        public DateTime? CreatedUtc { get; set; }
        //
        // Summary:
        //     The user id of the user who first created this content item version.
        public string Owner { get; set; }
        //
        // Summary:
        //     The name of the user who last modified this content item version.
        public string Author { get; set; }
        //
        // Summary:
        //     The text representing this content item.
        public string DisplayText { get; set; }
    }
}
