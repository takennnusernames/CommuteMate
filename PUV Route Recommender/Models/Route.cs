using NetTopologySuite.Geometries;
using SQLite;
using System.ComponentModel.DataAnnotations.Schema;


namespace CommuteMate.Models
{
    //db tables
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        public int RouteId { get; set; }
        [Unique]
        public long Osm_Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool StreetNameSaved { get; set; }
        public virtual ICollection<Street> Streets { get; set; }

    }
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
    public class Street
    {
        [PrimaryKey, AutoIncrement]
        public int StreetId { get; set; }
        [Unique]
        public long RouteId { get; set; }
        public string Name { get; set; }
        public string GeometryWKT { get; set; }

        [NotMapped]
        public List<Coordinate> Coordinates { get; set; }
        //public RoutesViewModel ParentViewModel { get; set; }
        //public Street(RoutesViewModel parent)
        //{
        //    ParentViewModel = parent;
        //}
        public virtual ICollection<Route> Routes { get; set; }
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
            return StreetId.GetHashCode() ^ RouteId.GetHashCode();
            // Combine hash codes of relevant properties
        }
    }

    //project objects]
    public class StreetWithCoordinates
    {
        public int StreetId { get; set; }
        public long Osm_Id { get; set; }
        public string Role { get; set; }
        public List<Coordinate> Coordinates { get; set; }
    }

}
