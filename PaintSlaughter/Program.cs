namespace PaintKiller
{
#if WINDOWS || XBOX
    internal static class Program
    {
        private static void Main(string[] args)
        {
            using (PaintKiller game = new PaintKiller()) game.Run();
        }
    }
#endif
}
