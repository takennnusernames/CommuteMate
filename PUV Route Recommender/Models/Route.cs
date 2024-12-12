using NetTopologySuite.Geometries;
using SQLite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CommuteMate.Models
{
    //db tables
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        public int RouteId { get; set; }
        [Unique]
        public long OsmId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool StreetNameSaved { get; set; }
        public virtual ICollection<Street> Streets { get; set; }
        public virtual ICollection<RouteStreet> RouteStreets { get; set; }

    }
    public class Street
    {
        [PrimaryKey, AutoIncrement]
        public int StreetId { get; set; }
        [Unique]
        public long OsmId { get; set; }
        public string Name { get; set; }
        public string GeometryWKT { get; set; }

        [NotMapped]
        public virtual ICollection<Route> Routes { get; set; }
        public virtual ICollection<RouteStreet> RouteStreets { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var otherStreet = (Street)obj;
            return Routes == otherStreet.Routes;
            // Add other relevant property comparisons if needed
        }

        public override int GetHashCode()
        {
            return StreetId.GetHashCode() ^ OsmId.GetHashCode();
            // Combine hash codes of relevant properties
        }
    }
    public class RouteStreet
    {
        [PrimaryKey, AutoIncrement]
        public int RelationId { get; init; }

        [ForeignKey("Route")]
        public long RouteOsmId { get; init; }
        public Route Route { get; init; }

        [ForeignKey("Street")]
        public long StreetOsmId { get; init; }
        public Street Street { get; init; }
    }

    
    
    //ViewModel
    public class RouteView : BindableObject
    {
        public long Osm_Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        private bool isStreetFrameVisible;
        public bool IsStreetFrameVisible
        {
            get { return isStreetFrameVisible; }
            set
            {
                isStreetFrameVisible = value;
                OnPropertyChanged(nameof(IsStreetFrameVisible));
            }
        }

        private bool isDownloaded;
        public bool IsDownloaded
        {
            get { return isDownloaded; }
            set
            {
                isDownloaded = value;
                OnPropertyChanged(nameof(IsDownloaded));
            }
        }
        public bool IsNotDownloaded => !IsDownloaded;
        private ObservableCollection<string> streets = [];
        public ObservableCollection<string> Streets
        {
            get { return streets; }
            set
            {
                streets = value;
                OnPropertyChanged(nameof(streets));
            }
        }
    }

}
