using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Models
{
    //path download
    public class OfflinePath
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PathName { get; set; }
        public string Origin { get; set; }
        public string OriginPoint { get; set; }
        public string Destination { get; set; }
        public string DestinationPoint { get; set; }
        public virtual Summary Summary { get; set; }
        public virtual ICollection<PathStep> PathSteps { get; set; }
    }
    public class PathStep
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; init; }
        [ForeignKey("OfflinePath")]
        public int PathId { get; init; }
        public OfflinePath Path { get; init; }

        [ForeignKey("OfflineStep")]
        public int StepId { get; init; }
        public OfflineStep Step { get; init; }
    }
    public class OfflineStep
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Action { get; set; }
        public string Instruction { get; set; }
        public string GeometryWkt { get; set; }
        //public virtual PathStep PathStep { get; set; }
    }
    public class Summary
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double TotalDistance { get; set; }
        public double TotalDuration { get; set; }
        public double TotalFare { get; set; }
        public string PUVs { get; set; }

    }
}
