namespace PictureHandler
{
    public class ServiceInfoMedianFiltering
    {
        public int InitialHeight { get; }
        public int FiniteHeight { get; }

        public ServiceInfoMedianFiltering(int initialHeight, int finiteHeight)
        {
            InitialHeight = initialHeight;
            FiniteHeight = finiteHeight;
        }
    }
}
