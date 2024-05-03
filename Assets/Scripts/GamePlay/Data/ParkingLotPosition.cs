namespace GamePlay.Data
{
    public class ParkingLotPosition
    {
        private readonly int _gridGroupIndex = 0;
        private readonly int _gridLineIndex = 0;
        private readonly int _parkingLotIndex = 0;
        
        public ParkingLotPosition(int gridGroupIndex, int gridLineIndex, int parkingLotIndex)
        {
            _gridGroupIndex = gridGroupIndex;
            _gridLineIndex = gridLineIndex;
            _parkingLotIndex = parkingLotIndex;
        }
        
        public int GetGridGroupIndex()
        {
            return _gridGroupIndex;
        }
        
        public int GetGridLineIndex()
        {
            return _gridLineIndex;
        }
        
        public int GetParkingLotIndex()
        {
            return _parkingLotIndex;
        }
    }
}