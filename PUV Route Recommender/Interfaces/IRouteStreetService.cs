namespace CommuteMate.Interfaces
{
    public interface IRouteStreetService
    {
        Task AddRouteStreetAsync(int routeId, int streetId);
        //Task<List<Street>> GetRouteStreetsAsync(int routeId);
        //Task<List<Route>> GetStreetRoutesAsync(int streetId);
    }
}
