namespace CommuteMate.Interfaces
{
    public interface IVehicleRepository
    {
        Vehicle GetVehicleById(int vehicle_ID);
        List<Vehicle> GetVehicles();
    }
}
