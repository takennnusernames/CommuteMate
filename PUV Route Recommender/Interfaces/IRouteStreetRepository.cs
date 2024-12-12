namespace CommuteMate.Interfaces
{
    public interface IRouteStreetRepository
    {
        Task<bool> InsertStreetRelation(RouteStreet routeStreet);
        Task<List<Street>> GetRelatedStreets(long osmId);
        bool CheckRelation(long streetId, long routeId);
    }
}
